using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// 자고 일어난 뒤 → 거실에 나오면 영롱이 머리 위 물방울(땀) 표시
// → 영롱이 근처에 가면 자동 대사 ("또.. 호흡이..." / "빨리 병원 가야겠다..")
// → 영롱이 근처 F → 화면 깜빡임 + 영롱이 안기
// → 현관문 근처 F → 병원(진료소)으로 이동
//
// 사용법: 거실(방에서 나오면 도착하는 자리)을 덮는 Box Collider 2D(Is Trigger) 오브젝트에 붙임
// SleepWakeEvent → On Finished에서 이 스크립트의 SetReady()를 연결
[RequireComponent(typeof(Collider2D))]
public class HospitalRushEvent : MonoBehaviour
{
    private enum Step { Waiting, Ready, Sweating, Talking, CanCarry, Carrying, Moving, Done }

    [Header("사용 가능 여부")]
    public bool startReady = false;         // 테스트용: 체크하면 처음부터 진행 가능

    [Header("플레이어")]
    public Transform player;                // 비워두면 Player 태그로 자동 검색
    public PlayerController playerController;
    public Animator playerAnimator;         // 영롱이 안는 애니메이션(isHug)용
    public string hugBoolName = "isHug";

    [Header("영롱이")]
    public Transform dog;
    public SpriteRenderer dogSpriteRenderer;    // 안으면 숨김
    public GameObject sweatIcon;                // 영롱이 머리 위 물방울(땀) 표시
    public float dogTalkDistance = 3f;          // 이 거리 안에 오면 자동 대사
    public float dogCarryDistance = 2.5f;       // 이 거리 안에서 F로 안기
    public GameObject dogPrompt;                // 영롱이 머리 위 F 표시 (전용 복제본)

    [Header("대사")]
    public DialogueDataSO dogDialogue;          // "또.. 호흡이..." / "빨리 병원 가야겠다.."

    [Header("안을 때 깜빡임")]
    public int carryBlinkCount = 1;

    [Header("현관문 → 병원")]
    public Transform frontDoorPoint;            // 현관문 위치
    public float doorDistance = 2f;             // 이 거리 안에서 F로 이동
    public GameObject doorPrompt;               // 현관문 위 F 표시 (전용 복제본)
    public Collider2D[] disableOnCarry;         // 안은 뒤 꺼둘 콜라이더 (기존 현관문 등, 산책길로 가는 것 막기)
    public Transform clinicPoint;               // 병원(진료소)에서 플레이어가 나타날 위치
    public Transform dogClinicPoint;            // (선택) 영롱이를 옮겨 둘 위치

    [Header("끝났을 때 실행할 것 (선택)")]
    public UnityEvent onArrivedClinic;

    private Step step = Step.Waiting;
    private TextTyping textTyping;
    private bool wantDogPrompt = false;
    private bool wantDoorPrompt = false;

    private void Start()
    {
        GetComponent<Collider2D>().isTrigger = true;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        if (playerController == null && player != null)
            playerController = player.GetComponentInChildren<PlayerController>();

        textTyping = FindAnyObjectByType<TextTyping>(FindObjectsInactive.Include);

        // 시작할 때 표시를 끄지 않음 (팀원 F를 같이 쓰는 경우를 위해)

        if (startReady) step = Step.Ready;
    }

    // SleepWakeEvent → On Finished에 연결
    public void SetReady()
    {
        if (step == Step.Waiting) step = Step.Ready;
    }

    // 1. 거실에 들어오면 물방울 표시
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (step != Step.Ready) return;
        if (!other.CompareTag("Player")) return;

        step = Step.Sweating;
    }

    private void Update()
    {
        if (player == null) return;

        bool fPressed = Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame;
        float dogDist = dog != null ? Vector2.Distance(player.position, dog.position) : 9999f;

        switch (step)
        {
            // 2. 영롱이 근처에 가면 자동 대사
            case Step.Sweating:
                if (dogDist <= dogTalkDistance)
                    StartCoroutine(TalkSequence());
                break;

            // 3. 영롱이 근처 F → 안기
            case Step.CanCarry:
                bool nearDog = dogDist <= dogCarryDistance;
                wantDogPrompt = nearDog;

                if (nearDog && fPressed)
                    StartCoroutine(CarrySequence());
                break;

            // 4. 현관문 근처 F → 병원
            case Step.Carrying:
                bool nearDoor = frontDoorPoint != null &&
                                Vector2.Distance(player.position, frontDoorPoint.position) <= doorDistance;
                wantDoorPrompt = nearDoor;

                if (nearDoor && fPressed)
                    StartCoroutine(MoveToClinic());
                break;
        }
    }

    // 표시는 다른 스크립트(팀원 F 표시 등)가 끄고 난 뒤인 LateUpdate에서 마지막으로 적용
    private void LateUpdate()
    {
        // 이 이벤트가 진행 중일 때만 표시를 건드림 (그 전/후에는 팀원 F 표시가 그대로 동작)
        if (step == Step.Waiting || step == Step.Ready || step == Step.Done) return;

        bool showSweat = step == Step.Sweating || step == Step.Talking || step == Step.CanCarry;
        SetPrompt(sweatIcon, showSweat);
        SetPrompt(dogPrompt, step == Step.CanCarry && wantDogPrompt);
        SetPrompt(doorPrompt, step == Step.Carrying && wantDoorPrompt);
    }

    private IEnumerator TalkSequence()
    {
        step = Step.Talking;
        if (playerController != null) playerController.canMove = false;

        if (textTyping != null && dogDialogue != null)
        {
            textTyping.StartDialogue(dogDialogue);
            yield return null;

            if (textTyping.darkUIEffect != null)
                yield return new WaitUntil(() => !textTyping.darkUIEffect.isStory);
        }

        if (playerController != null) playerController.canMove = true;
        step = Step.CanCarry;
    }

    private IEnumerator CarrySequence()
    {
        step = Step.Moving;
        SetPrompt(dogPrompt, false);
        if (playerController != null) playerController.canMove = false;

        for (int i = 0; i < Mathf.Max(1, carryBlinkCount); i++)
        {
            yield return FadeOut(0.15f);

            // 첫 번째로 깜깜해졌을 때 안는 모습으로 바꾸기
            if (i == 0)
            {
                if (playerAnimator != null) playerAnimator.SetBool(hugBoolName, true);
                if (dogSpriteRenderer != null) dogSpriteRenderer.enabled = false;
                if (sweatIcon != null) sweatIcon.SetActive(false);
            }

            yield return FadeIn(0.35f);
        }

        // 기존 현관문(산책길 이동) 등 끄기
        if (disableOnCarry != null)
            foreach (Collider2D c in disableOnCarry)
                if (c != null) c.enabled = false;

        if (playerController != null) playerController.canMove = true;
        step = Step.Carrying;
    }

    private IEnumerator MoveToClinic()
    {
        step = Step.Moving;
        SetPrompt(doorPrompt, false);
        if (playerController != null) playerController.canMove = false;

        yield return FadeOut(0.5f);

        if (clinicPoint != null) MovePlayer(clinicPoint.position);

        if (dog != null && dogClinicPoint != null)
        {
            Vector3 d = dogClinicPoint.position;
            d.z = dog.position.z;
            dog.position = d;
        }

        yield return new WaitForSeconds(0.5f);
        yield return FadeIn(0.5f);

        if (playerController != null) playerController.canMove = true;
        step = Step.Done;

        onArrivedClinic?.Invoke();
    }

    // ───────── 공통 ─────────
    private IEnumerator FadeOut(float time)
    {
        if (FadeManager.Instance == null) yield break;
        yield return new WaitUntil(() => !FadeManager.Instance.isFading);
        FadeManager.Instance.FadeOut(time, DG.Tweening.Ease.InQuad);
        yield return new WaitUntil(() => !FadeManager.Instance.isFading);
    }

    private IEnumerator FadeIn(float time)
    {
        if (FadeManager.Instance == null) yield break;
        yield return new WaitUntil(() => !FadeManager.Instance.isFading);
        FadeManager.Instance.FadeIn(time, DG.Tweening.Ease.OutQuad);
        yield return new WaitUntil(() => !FadeManager.Instance.isFading);
    }

    private void SetPrompt(GameObject prompt, bool on)
    {
        if (prompt == null) return;

        if (on)
        {
            // 영롱이 Canvas 안에 있는 UI라면, 꺼져 있는 부모(Canvas 등)도 같이 켜서 보이게 함
            Transform t = prompt.transform.parent;
            while (t != null)
            {
                if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
                if (dog != null && t == dog) break;
                t = t.parent;
            }
        }

        if (prompt.activeSelf != on) prompt.SetActive(on);
    }

    private void MovePlayer(Vector3 pos)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.position = new Vector2(pos.x, pos.y);
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            pos.z = player.position.z;
            player.position = pos;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (dog != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(dog.position, dogTalkDistance);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(dog.position, dogCarryDistance);
        }
        if (frontDoorPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(frontDoorPoint.position, doorDistance);
        }
    }
}
