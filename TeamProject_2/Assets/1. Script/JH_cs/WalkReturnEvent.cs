using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// 산책을 다녀와서 거실로 돌아왔을 때 실행되는 이벤트
// 1) 산책길의 체크 지점(전봇대 등)에 다녀왔는지 확인
// 2) 거실로 돌아와 이 트리거에 들어오면: 영롱이를 내려놓고 소파로 이동
// 3) 소파에 도착하면 대사 출력 ("아, 이제 영양제 줘야지")
// 4) 대사가 끝나면 주방 쪽 안내 표시를 켜서 플레이어를 유도
//
// 사용법: 거실 현관문 안쪽에 Box Collider 2D(Is Trigger 체크)를 가진 오브젝트를 만들고 이 스크립트를 붙임
[RequireComponent(typeof(Collider2D))]
public class WalkReturnEvent : MonoBehaviour
{
    [Header("산책 완료 체크")]
    public Transform walkCheckPoint;        // 산책길에서 플레이어가 도착해야 하는 지점 (예: TelephonePole)
    public float walkCheckDistance = 2f;    // 이 거리 안에 들어오면 산책한 것으로 인정
    public bool skipWalkCheck = false;      // 테스트용: 체크하면 산책 안 가도 바로 실행

    [Header("플레이어")]
    public Transform player;                // 비워두면 Player 태그로 자동 검색
    public PlayerController playerController;
    public Animator playerAnimator;         // 영롱이를 안고 있는 애니메이션(isHug)을 끄기 위해 사용
    public string hugBoolName = "isHug";

    [Header("영롱이")]
    public Transform dog;                   // 영롱이 오브젝트 (DogMove가 붙은 것)
    public DogMove dogMove;
    public SpriteRenderer dogSpriteRenderer;// 안고 있을 때 숨겨져 있던 영롱이 스프라이트
    public Vector3 dropOffset = new Vector3(1f, 0f, 0f); // 내려놓을 때 플레이어 기준 위치

    [Header("소파")]
    public Transform sofaPoint;             // 영롱이가 누울 소파 위치 (비워두면 영롱이가 지금 있는 자리 = DogMedicineAction이 보낸 소파)
    public bool walkToSofa = false;         // 체크: 플레이어 옆에 내려놓고 소파까지 걸어감 / 해제: 소파에 바로 나타남
    public float arriveDistance = 0.2f;
    public float maxWaitTime = 5f;          // 이동이 막혀도 이 시간이 지나면 소파로 순간이동
    public Sprite lyingSprite;              // (선택) 누운 모습 스프라이트
    public Animator dogAnimator;            // (선택) 누운 스프라이트를 쓸 때 끌 영롱이 Animator

    [Header("대사")]
    public DialogueDataSO returnDialogue;   // "아, 이제 영양제 줘야지"
    public float delayBeforeDialogue = 0.3f;

    [Header("주방 유도")]
    public GameObject kitchenGuide;         // 주방 방향 화살표 / 표시 (시작 시 꺼짐)
    public Transform kitchenPoint;          // (선택) 플레이어가 여기 도착하면 안내 표시 끄기
    public float kitchenArriveDistance = 1.5f;

    [Header("끝났을 때 실행할 것 (선택)")]
    public UnityEvent onFinished;

    private TextTyping textTyping;
    private bool hasWalked = false;
    private bool isPlayed = false;
    private bool isGuiding = false;

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

        if (dogMove == null && dog != null)
            dogMove = dog.GetComponent<DogMove>();

        textTyping = FindAnyObjectByType<TextTyping>(FindObjectsInactive.Include);

        if (kitchenGuide != null) kitchenGuide.SetActive(false);
    }

    private void Update()
    {
        // 산책길 체크 지점에 다녀왔는지 확인
        if (!hasWalked && walkCheckPoint != null && player != null)
        {
            if (Vector2.Distance(player.position, walkCheckPoint.position) <= walkCheckDistance)
                hasWalked = true;
        }

        // 주방에 도착하면 안내 표시 끄기
        if (isGuiding && kitchenPoint != null && player != null)
        {
            if (Vector2.Distance(player.position, kitchenPoint.position) <= kitchenArriveDistance)
            {
                isGuiding = false;
                if (kitchenGuide != null) kitchenGuide.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayed) return;
        if (!other.CompareTag("Player")) return;
        if (!hasWalked && !skipWalkCheck) return;

        // 영롱이를 안고 있을 때만 실행
        if (playerAnimator != null && !playerAnimator.GetBool(hugBoolName)) return;

        isPlayed = true;
        StartCoroutine(ReturnSequence());
    }

    private IEnumerator ReturnSequence()
    {
        // 페이드(문 이동)가 끝날 때까지 기다리기
        if (FadeManager.Instance != null)
            yield return new WaitUntil(() => !FadeManager.Instance.isFading);

        // 1. 플레이어 멈추기
        if (playerController != null) playerController.canMove = false;

        // 2. 영롱이 내려놓기
        if (playerAnimator != null) playerAnimator.SetBool(hugBoolName, false);

        if (walkToSofa && dog != null && player != null)
        {
            // 높이(Y)는 영롱이가 원래 서 있던 바닥 높이를 유지 → 위에서 떨어지지 않게
            Vector3 drop = new Vector3(player.position.x + dropOffset.x,
                                       dog.position.y + dropOffset.y,
                                       dog.position.z);
            dog.position = drop;
        }
        if (dogSpriteRenderer != null) dogSpriteRenderer.enabled = true;

        // 3. 소파로 (바로 나타나기 / 걸어가기)
        if (!walkToSofa && dog != null && sofaPoint != null)
        {
            Vector3 sofaNow = sofaPoint.position;
            sofaNow.z = dog.position.z;
            dog.position = sofaNow;
        }
        else if (walkToSofa && dog != null && sofaPoint != null)
        {
            if (dogMove != null)
            {
                // DogMove는 도착하면 목표 오브젝트를 지우기 때문에 복사본을 만들어서 넘김
                GameObject target = new GameObject("SofaTarget_Temp");
                target.transform.position = sofaPoint.position;
                dogMove.AddTargetPoint(target.transform);

                float timer = 0f;
                yield return new WaitUntil(() =>
                {
                    timer += Time.deltaTime;
                    return Vector2.Distance(dog.position, sofaPoint.position) <= dogMove.stopDistance + arriveDistance
                           || timer >= maxWaitTime;
                });

                if (target != null) Destroy(target);

                // 시간 안에 못 가면(막혔을 때만) 소파로 순간이동
                if (timer >= maxWaitTime)
                {
                    Vector3 sofa = sofaPoint.position;
                    sofa.z = dog.position.z;
                    dog.position = sofa;
                }
            }
            else
            {
                Vector3 sofa = sofaPoint.position;
                sofa.z = dog.position.z;
                dog.position = sofa;
            }
        }

        // 4. 누운 모습으로 바꾸기 (선택)
        if (lyingSprite != null && dogSpriteRenderer != null)
        {
            if (dogAnimator != null) dogAnimator.enabled = false;
            dogSpriteRenderer.sprite = lyingSprite;
        }

        yield return new WaitForSeconds(delayBeforeDialogue);

        // 5. 대사 출력 후 끝날 때까지 기다리기
        if (textTyping != null && returnDialogue != null)
        {
            textTyping.StartDialogue(returnDialogue);
            yield return null;

            if (textTyping.darkUIEffect != null)
                yield return new WaitUntil(() => !textTyping.darkUIEffect.isStory);
        }

        // 6. 플레이어 다시 움직이게 + 주방 유도
        if (playerController != null) playerController.canMove = true;

        if (kitchenGuide != null)
        {
            kitchenGuide.SetActive(true);
            isGuiding = true;
        }

        onFinished?.Invoke();
    }

    private void OnDrawGizmosSelected()
    {
        if (walkCheckPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(walkCheckPoint.position, walkCheckDistance);
        }
        if (sofaPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(sofaPoint.position, 0.3f);
        }
        if (kitchenPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(kitchenPoint.position, kitchenArriveDistance);
        }
    }
}
