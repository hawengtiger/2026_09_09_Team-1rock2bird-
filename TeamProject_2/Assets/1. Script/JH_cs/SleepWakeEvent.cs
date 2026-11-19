using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// 침대에서 F → 잠들기(암전) → 서서히 밝아지며 눈을 깜빡이는 연출 → 대사 → 거실로 유도
// 사용법: 침대 앞에 Box Collider 2D(Is Trigger)가 있는 오브젝트를 만들고 이 스크립트를 붙임
// 영양제 이벤트가 끝난 뒤에만 쓸 수 있게 하려면 Start Ready를 끄고,
// NutrientGiveEvent → On Finished에서 이 스크립트의 SetReady()를 연결
[RequireComponent(typeof(Collider2D))]
public class SleepWakeEvent : MonoBehaviour
{
    [System.Serializable]
    public class BlinkStep
    {
        [Range(0f, 1f)] public float alpha = 1f;   // 1 = 완전 깜깜, 0 = 완전 밝음
        public float duration = 1f;                 // 이 밝기까지 걸리는 시간
    }

    [Header("사용 가능 여부")]
    public bool startReady = false;         // 체크하면 처음부터 사용 가능 (테스트용)

    [Header("상호작용 F 표시")]
    public GameObject interactPrompt;       // 침대 위 F 표시 (사용 가능할 때만 켜짐)

    [Header("플레이어")]
    public Transform player;                // 비워두면 Player 태그로 자동 검색
    public PlayerController playerController;
    public Transform sleepPoint;            // (선택) 잠들 때 플레이어를 옮길 위치

    [Header("화면 연출")]
    public Image blinkOverlay;              // 화면 전체를 덮는 검은 Image (Canvas 맨 위)
    public List<BlinkStep> blinkSteps = new List<BlinkStep>()
    {
        new BlinkStep { alpha = 1f,    duration = 1.0f },  // 암전
        new BlinkStep { alpha = 1f,    duration = 1.5f },  // 잠깐 깜깜하게 유지
        new BlinkStep { alpha = 0.6f,  duration = 1.2f },  // 서서히 실눈
        new BlinkStep { alpha = 1f,    duration = 0.25f }, // 깜빡 (감기)
        new BlinkStep { alpha = 0.35f, duration = 0.8f },  // 조금 더 뜨기
        new BlinkStep { alpha = 1f,    duration = 0.2f },  // 깜빡 (감기)
        new BlinkStep { alpha = 0f,    duration = 1.2f },  // 완전히 뜨기
    };

    [Header("Blink Overlay가 비어 있을 때 (팀원 FadeManager 검은 화면 사용)")]
    public int fadeBlinkCount = 2;          // 눈 뜬 뒤 깜빡이는 횟수

    [Header("대사")]
    public DialogueDataSO wakeUpDialogue;   // "하아.. 물을 마셔야겠다."
    public float delayBeforeDialogue = 0.3f;

    [Header("길막 풀기 (선택)")]
    public DialogueDataSO[] unlockTriggerDialogues;

    [Header("끝났을 때 실행할 것 (선택)")]
    public UnityEvent onFinished;

    private TextTyping textTyping;
    private bool isReady;
    private bool isPlayerInside = false;
    private bool isPlaying = false;
    private bool isDone = false;

    private void Start()
    {
        GetComponent<Collider2D>().isTrigger = true;
        isReady = startReady;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        if (playerController == null && player != null)
            playerController = player.GetComponentInChildren<PlayerController>();

        textTyping = FindAnyObjectByType<TextTyping>(FindObjectsInactive.Include);

        if (blinkOverlay != null)
        {
            SetOverlayAlpha(0f);
            blinkOverlay.gameObject.SetActive(false);
        }
    }

    // NutrientGiveEvent → On Finished에 연결
    public void SetReady()
    {
        isReady = true;
    }

    private void Update()
    {
        bool canUse = isReady && !isPlaying && !isDone && isPlayerInside;

        // 준비되기 전에는 F 표시를 건드리지 않음 (팀원 침대가 같은 F를 써도 괜찮게)
        if (isReady && interactPrompt != null && interactPrompt.activeSelf != canUse)
            interactPrompt.SetActive(canUse);

        if (canUse && Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            StartCoroutine(SleepSequence());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerInside = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerInside = false;
    }

    private IEnumerator SleepSequence()
    {
        isPlaying = true;
        if (interactPrompt != null) interactPrompt.SetActive(false);

        // 1. 플레이어 멈추기
        if (playerController != null) playerController.canMove = false;

        // 2. 눈 감기 → 깜빡이며 뜨기
        if (blinkOverlay != null)
        {
            blinkOverlay.gameObject.SetActive(true);
            SetOverlayAlpha(0f);

            for (int i = 0; i < blinkSteps.Count; i++)
            {
                yield return FadeOverlay(blinkSteps[i].alpha, blinkSteps[i].duration);

                // 첫 암전이 끝나면 (화면이 깜깜할 때) 침대 위치로 옮기기
                if (i == 0 && sleepPoint != null && player != null)
                    MovePlayer(sleepPoint.position);
            }

            SetOverlayAlpha(0f);
            blinkOverlay.gameObject.SetActive(false);
        }
        else if (FadeManager.Instance != null)
        {
            // 검은 Image가 없으면 기존 FadeManager로 간단하게 암전 → 밝아지기
            yield return new WaitUntil(() => !FadeManager.Instance.isFading);
            FadeManager.Instance.FadeOut(1f, DG.Tweening.Ease.InQuad);
            yield return new WaitUntil(() => !FadeManager.Instance.isFading);

            if (sleepPoint != null && player != null) MovePlayer(sleepPoint.position);
            yield return new WaitForSeconds(1.5f);

            FadeManager.Instance.FadeIn(1.5f, DG.Tweening.Ease.OutQuad);
            yield return new WaitUntil(() => !FadeManager.Instance.isFading);

            // 같은 검은 화면으로 눈 깜빡이기
            for (int i = 0; i < fadeBlinkCount; i++)
            {
                yield return new WaitForSeconds(0.3f);

                FadeManager.Instance.FadeOut(0.15f, DG.Tweening.Ease.InQuad);
                yield return new WaitUntil(() => !FadeManager.Instance.isFading);

                FadeManager.Instance.FadeIn(0.4f, DG.Tweening.Ease.OutQuad);
                yield return new WaitUntil(() => !FadeManager.Instance.isFading);
            }
        }

        yield return new WaitForSeconds(delayBeforeDialogue);

        // 3. 대사
        if (textTyping != null && wakeUpDialogue != null)
        {
            textTyping.StartDialogue(wakeUpDialogue);
            yield return null;

            if (textTyping.darkUIEffect != null)
                yield return new WaitUntil(() => !textTyping.darkUIEffect.isStory);
        }

        // 4. 다시 움직이기
        if (playerController != null) playerController.canMove = true;

        isDone = true;
        isPlaying = false;

        UnlockTriggers();
        onFinished?.Invoke();
    }

    private IEnumerator FadeOverlay(float target, float duration)
    {
        float start = blinkOverlay.color.a;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            SetOverlayAlpha(Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t / duration)));
            yield return null;
        }

        SetOverlayAlpha(target);
    }

    private void SetOverlayAlpha(float a)
    {
        if (blinkOverlay == null) return;
        Color c = blinkOverlay.color;
        c.a = a;
        blinkOverlay.color = c;
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

    private void UnlockTriggers()
    {
        if (unlockTriggerDialogues == null || unlockTriggerDialogues.Length == 0) return;

        TriggerDialogue[] triggers = FindObjectsByType<TriggerDialogue>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (TriggerDialogue t in triggers)
        {
            if (t == null || t.dialogue == null) continue;

            foreach (DialogueDataSO d in unlockTriggerDialogues)
            {
                if (d == null || t.dialogue != d) continue;

                foreach (Collider2D c in t.GetComponents<Collider2D>())
                    c.enabled = false;
                break;
            }
        }
    }
}
