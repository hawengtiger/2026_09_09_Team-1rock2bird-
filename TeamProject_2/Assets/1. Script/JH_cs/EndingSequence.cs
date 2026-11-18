using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

// [엔딩] 네컷 사진 대사가 끝나면(SetReady) 방 문(RoomDoor)에서 F → 거실로 나감
// → 대사 "...어" → 일러스트 3장 → 하얀 배경 내레이션 → 일러스트 2장 → 게임 시작 씬
// 일러스트가 비어 있으면 그 자리는 흰 화면으로 대신 보여줌 (나중에 그림만 넣으면 됨)
// 퍼즐(사진 대사)이 끝나기 전에는 문 쪽은 원래(팀원) 동작 그대로
[RequireComponent(typeof(Collider2D))]
public class EndingSequence : MonoBehaviour
{
    [Header("사용 가능 여부")]
    public bool startReady = false;

    [Header("문으로 나가기")]
    public string doorObjectName = "RoomDoor";  // 방 문 오브젝트 이름
    public float doorRange = 2.5f;              // 이 거리 안에서 F
    public Transform exitPoint;                 // 나갔을 때 설 위치 (비우면 문 DoorAction의 Target Door)

    [Header("대사")]
    public DialogueDataSO enterDialogue;        // "...어"

    [Header("컷씬 UI")]
    public GameObject cutsceneRoot;
    public Image blackBackground;
    public Image layerA;
    public Image layerB;
    public Image whitePanel;
    public TMP_Text narrationText;

    [Header("일러스트 (비우면 흰 화면으로 대신)")]
    public Sprite[] firstIllustrations;
    public int firstCount = 3;                  // 그림이 없을 때 흰 화면 장 수
    [TextArea] public string[] endingLines =
    {
        "내가 우울해 할 수록 영롱이는 슬퍼하겠지?",
        "나 다시 힘내볼게 영롱아"
    };
    public Sprite[] secondIllustrations;
    public int secondCount = 2;

    [Header("시간")]
    public float fadeTime = 1f;
    public float slideTime = 3f;
    public float lineTime = 2.5f;

    [Header("게임 종료")]
    public string endSceneName = "MainScene";
    public bool clearInventory = true;

    private bool isReady;
    private bool isPlaying = false;
    private PlayerController player;
    private TextTyping textTyping;
    private GameObject door;
    private GameObject doorPrompt;

    private void Start()
    {
        GetComponent<Collider2D>().isTrigger = true;
        player = MemoryUI.FindPlayer();
        textTyping = FindAnyObjectByType<TextTyping>(FindObjectsInactive.Include);
        if (cutsceneRoot != null) cutsceneRoot.SetActive(false);

        door = GameObject.Find(doorObjectName);
        if (door != null)
        {
            PickupItem pi = door.GetComponent<PickupItem>();
            if (pi != null) doorPrompt = pi.PickupState;
            if (exitPoint == null)
            {
                DoorAction da = door.GetComponent<DoorAction>();
                if (da != null) exitPoint = da.targetDoor;
            }
        }

        if (startReady) SetReady();
    }

    // PhotoPopupEvent → On Finished 에서 호출 (퍼즐 + 사진 대사 끝 = 나갈 수 있음)
    public void SetReady()
    {
        isReady = true;

        // 이제부터 문은 이 스크립트가 담당 (팀원 문 동작/대사와 겹치지 않게)
        if (door != null)
            foreach (Collider2D c in door.GetComponents<Collider2D>()) c.enabled = false;
    }

    private void Update()
    {
        if (!isReady || isPlaying || door == null || player == null) return;

        bool canUse = !MemoryUI.IsOpen && !MemoryUI.IsStory() &&
                      Vector2.Distance(player.transform.position, door.transform.position) <= doorRange;

        if (doorPrompt != null && doorPrompt.activeSelf != canUse) doorPrompt.SetActive(canUse);

        if (canUse && Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            StartCoroutine(ExitAndPlay());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isReady || isPlaying) return;
        if (!other.CompareTag("Player")) return;
        StartCoroutine(Play());
    }

    // 문 F → 페이드 → 거실로 이동 → 엔딩
    private IEnumerator ExitAndPlay()
    {
        isPlaying = true;
        MemoryUI.IsOpen = true;
        if (doorPrompt != null) doorPrompt.SetActive(false);
        player.canMove = false;

        bool dark = false;
        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.FadeOut(0.5f, DG.Tweening.Ease.InQuad, () => dark = true);
            float t = 0f;
            while (!dark && t < 2f) { t += Time.deltaTime; yield return null; }
        }

        if (exitPoint != null) MovePlayer(exitPoint.position);
        yield return new WaitForSeconds(0.3f);

        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.FadeIn(0.5f, DG.Tweening.Ease.InQuad);
            yield return new WaitForSeconds(0.6f);
        }

        yield return PlaySequence();
    }

    private void MovePlayer(Vector3 dest)
    {
        GameObject root = GameObject.Find("PlayerOB");
        Transform target = root != null ? root.transform : player.transform;
        Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
        if (rb != null) { rb.position = dest; rb.linearVelocity = Vector2.zero; }
        target.position = new Vector3(dest.x, dest.y, target.position.z);
    }

    private IEnumerator Play()
    {
        isPlaying = true;
        yield return PlaySequence();
    }

    private IEnumerator PlaySequence()
    {
        MemoryUI.IsOpen = true;
        if (player != null) player.canMove = false;

        // 1. 대사 "...어"
        if (textTyping != null && enterDialogue != null)
        {
            textTyping.StartDialogue(enterDialogue);
            yield return null;
            if (textTyping.darkUIEffect != null)
                while (textTyping.darkUIEffect.isStory)
                {
                    if (player != null) player.canMove = false;
                    yield return null;
                }
        }

        // 2. 컷씬 시작
        cutsceneRoot.SetActive(true);
        SetAlpha(layerA, 0f); SetAlpha(layerB, 0f);
        SetAlpha(whitePanel, 0f);
        if (narrationText != null) narrationText.alpha = 0f;
        SetAlpha(blackBackground, 0f);
        yield return Fade(blackBackground, 1f);

        // 3. 1차 일러스트 (없으면 흰 화면)
        lastShown = null;
        foreach (Sprite s in Slides(firstIllustrations, firstCount))
            yield return ShowSlideRoutine(s, lastShown);
        Image current = lastShown;

        // 4. 하얀 배경 + 내레이션
        if (whitePanel != null) whitePanel.transform.SetAsLastSibling();
        yield return Fade(whitePanel, 1f);
        if (current != null) SetAlpha(current, 0f);

        if (narrationText != null)
        {
            narrationText.transform.SetAsLastSibling();
            foreach (string line in endingLines)
            {
                narrationText.text = line;
                yield return FadeText(1f);
                yield return WaitOrClick(lineTime);
                yield return FadeText(0f);
            }
        }

        // 5. 2차 일러스트 (없으면 흰 화면)
        lastShown = null;
        bool first = true;
        foreach (Sprite s in Slides(secondIllustrations, secondCount))
        {
            yield return ShowSlideRoutine(s, lastShown);
            if (first) { SetAlpha(whitePanel, 0f); first = false; }
        }

        // 6. 페이드 아웃 → 게임 시작 씬
        if (lastShown != null) yield return Fade(lastShown, 0f);

        if (clearInventory && Inventory.Instance != null) Inventory.Instance.ClearItem();
        MemoryUI.IsOpen = false;

        LoadingSceneController.LoadScene(endSceneName);
    }

    // 그림이 하나도 없으면 count만큼 빈(=흰 화면) 슬라이드
    private Sprite[] Slides(Sprite[] arr, int count)
    {
        if (arr != null && arr.Length > 0) return arr;
        return new Sprite[Mathf.Max(0, count)];
    }

    // ───────── 슬라이드 ─────────
    private Image lastShown;

    private IEnumerator ShowSlideRoutine(Sprite sprite, Image previous)
    {
        Image next = (previous == layerA) ? layerB : layerA;
        next.sprite = sprite;                    // null이면 흰 화면
        Color c = Color.white; c.a = 0f; next.color = c;
        next.transform.SetAsLastSibling();

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
