using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

// 거실로 나가면 → 대사 "...어" → 일러스트 3장 → 하얀 배경 내레이션 → 일러스트 2장 → 게임 시작 씬
// 모든 전환은 크로스 페이드. 각 장면은 시간이 지나거나 클릭하면 넘어감.
// 사용법: 거실 입구를 덮는 Box Collider 2D(Is Trigger) 오브젝트에 붙이고,
//        PhotoPopupEvent → On Finished에서 SetReady() 연결
[RequireComponent(typeof(Collider2D))]
public class EndingSequence : MonoBehaviour
{
    [Header("사용 가능 여부")]
    public bool startReady = false;

    [Header("대사")]
    public DialogueDataSO enterDialogue;        // "...어"

    [Header("컷씬 UI")]
    public GameObject cutsceneRoot;             // 컷씬 전체 부모 (시작 시 꺼짐, Canvas 맨 위)
    public Image blackBackground;               // 화면 전체 검은 배경
    public Image layerA;                        // 일러스트 표시용 (크로스 페이드용으로 2장)
    public Image layerB;
    public Image whitePanel;                    // 내레이션용 하얀 배경
    public TMP_Text narrationText;

    [Header("일러스트 / 내레이션")]
    public Sprite[] firstIllustrations;         // 1차 일러스트 3장
    [TextArea] public string[] narrationLines =
    {
        "내가 우울해할수록 영롱이는 슬퍼하겠지?",
        "우울해하지 않을게 영롱아"
    };
    public Sprite[] secondIllustrations;        // 2차 일러스트 2장

    [Header("시간")]
    public float fadeTime = 1f;                 // 크로스 페이드 시간
    public float slideTime = 3f;                // 한 장 보여주는 시간 (클릭하면 바로 넘어감)
    public float lineTime = 2.5f;               // 내레이션 한 줄 시간

    [Header("게임 종료")]
    public string endSceneName = "MainScene";   // 게임 시작 씬 이름
    public bool clearInventory = true;          // 다시 시작할 때를 위해 인벤토리 비우기

    private bool isReady;
    private bool isPlaying = false;
    private PlayerController player;
    private TextTyping textTyping;

    private void Start()
    {
        GetComponent<Collider2D>().isTrigger = true;
        isReady = startReady;
        player = MemoryUI.FindPlayer();
        textTyping = FindAnyObjectByType<TextTyping>(FindObjectsInactive.Include);
        if (cutsceneRoot != null) cutsceneRoot.SetActive(false);
    }

    public void SetReady()
    {
        isReady = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isReady || isPlaying) return;
        if (!other.CompareTag("Player")) return;
        StartCoroutine(Play());
    }

    private IEnumerator Play()
    {
        isPlaying = true;
        MemoryUI.IsOpen = true;
        if (player != null) player.canMove = false;

        // 1. 대사 "...어"
        if (textTyping != null && enterDialogue != null)
        {
            textTyping.StartDialogue(enterDialogue);
            yield return null;
            if (textTyping.darkUIEffect != null)
                yield return new WaitUntil(() => !textTyping.darkUIEffect.isStory);
        }

        // 2. 컷씬 시작 (검은 화면 페이드 인)
        cutsceneRoot.SetActive(true);
        SetAlpha(layerA, 0f); SetAlpha(layerB, 0f);
        SetAlpha(whitePanel, 0f);
        if (narrationText != null) narrationText.alpha = 0f;
        SetAlpha(blackBackground, 0f);
        yield return Fade(blackBackground, 1f);

        // 3. 1차 일러스트
        lastShown = null;
        foreach (Sprite s in firstIllustrations)
            yield return ShowSlideRoutine(s, lastShown);
        Image current = lastShown;

        // 4. 하얀 배경 + 내레이션 (일러스트 위로)
        if (whitePanel != null) whitePanel.transform.SetAsLastSibling();
        yield return Fade(whitePanel, 1f);
        if (current != null) SetAlpha(current, 0f);

        if (narrationText != null)
        {
            narrationText.transform.SetAsLastSibling();
            foreach (string line in narrationLines)
            {
                narrationText.text = line;
                yield return FadeText(1f);
                yield return WaitOrClick(lineTime);
                yield return FadeText(0f);
            }
        }

        // 5. 2차 일러스트 (하얀 배경 위로 크로스 페이드)
        lastShown = null;
        bool first = true;
        foreach (Sprite s in secondIllustrations)
        {
            yield return ShowSlideRoutine(s, lastShown);
            if (first) { SetAlpha(whitePanel, 0f); first = false; }
        }

        // 6. 검게 페이드 아웃 → 게임 시작 씬
        if (lastShown != null) yield return Fade(lastShown, 0f);

        if (clearInventory && Inventory.Instance != null) Inventory.Instance.ClearItem();
        MemoryUI.IsOpen = false;

        LoadingSceneController.LoadScene(endSceneName);
    }

    // ───────── 슬라이드 ─────────
    private Image lastShown;

    private IEnumerator ShowSlideRoutine(Sprite sprite, Image previous)
    {
        if (sprite == null) yield break;

        Image next = (previous == layerA) ? layerB : layerA;
        next.sprite = sprite;
        next.transform.SetAsLastSibling();
        SetAlpha(next, 0f);

        // 크로스 페이드: 다음 장은 나타나고, 이전 장은 사라짐
        float t = 0f;
        float startPrev = previous != null ? previous.color.a : 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float p = t / fadeTime;
            SetAlpha(next, p);
            if (previous != null) SetAlpha(previous, Mathf.Lerp(startPrev, 0f, p));
            yield return null;
        }
        SetAlpha(next, 1f);
        if (previous != null) SetAlpha(previous, 0f);

        lastShown = next;
        yield return WaitOrClick(slideTime);
    }

    private IEnumerator WaitOrClick(float time)
    {
        yield return null;
        float t = 0f;
        while (t < time)
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) yield break;
            t += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator Fade(Image img, float target)
    {
        if (img == null) yield break;
        float start = img.color.a;
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            SetAlpha(img, Mathf.Lerp(start, target, t / fadeTime));
            yield return null;
        }
        SetAlpha(img, target);
    }

    private IEnumerator FadeText(float target)
    {
        float start = narrationText.alpha;
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            narrationText.alpha = Mathf.Lerp(start, target, t / fadeTime);
            yield return null;
        }
        narrationText.alpha = target;
    }

    private void SetAlpha(Image img, float a)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}
