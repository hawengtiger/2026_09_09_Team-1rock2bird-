using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

// 소파에 누운 영롱이에게 영양제를 주는 이벤트
// 사용법: Inventory → Item Data → 영양제 → On Use()에 이 스크립트의 UseNutrient()를 연결
// 1) 영롱이 근처에서 영양제를 사용하면
// 2) 영롱이 말풍선이 잠깐 뜨고
// 3) 플레이어 대사 ("이제 방으로 가서 누워야겠다.")
// 4) 끝나면 On Finished 실행 (방 이동 길 열기 등)
public class NutrientGiveEvent : MonoBehaviour
{
    [Header("영양제 아이템")]
    public int nutrientItemID = 4;          // Inventory에 등록한 영양제 Item ID

    [Header("거리 체크")]
    public Transform player;                // 비워두면 Player 태그로 자동 검색
    public PlayerController playerController;
    public Transform dog;                   // 영롱이
    public float giveDistance = 2.5f;       // 이 거리 안에서만 줄 수 있음

    [Header("상호작용 F 표시")]
    public GameObject interactPrompt;       // 영롱이 머리 위 F 표시 (영양제를 줄 수 있을 때만 켜짐)

    [Header("영롱이 말풍선")]
    public GameObject dogBubble;            // 영롱이 머리 위 말풍선 오브젝트 (시작 시 꺼짐)
    public TMP_Text dogBubbleText;          // (선택) 말풍선 안 글자
    public string dogBubbleMessage = "꿀꺽꿀꺽";
    public float dogBubbleDuration = 1.5f;

    [Header("대사")]
    public DialogueDataSO afterDialogue;    // "이제 방으로 가서 누워야겠다."
    public DialogueDataSO tooFarDialogue;   // (선택) 멀리서 썼을 때 대사

    [Header("길막 풀기")]
    // 여기에 넣은 대사를 쓰는 TriggerDialogue(길막)를 씬에서 전부 찾아 충돌 판정을 끔
    public DialogueDataSO[] unlockTriggerDialogues;

    [Header("끝났을 때 실행할 것 (선택)")]
    public UnityEvent onFinished;

    private TextTyping textTyping;
    private bool isDone = false;
    private bool isPlaying = false;

    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        if (playerController == null && player != null)
            playerController = player.GetComponentInChildren<PlayerController>();

        textTyping = FindAnyObjectByType<TextTyping>(FindObjectsInactive.Include);

        if (dogBubble != null) dogBubble.SetActive(false);
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    private void Update()
    {
        if (interactPrompt == null) return;

        // 영양제를 가지고 영롱이 근처에 있을 때만 F 표시
        bool hasNutrient = Inventory.Instance != null &&
                           Inventory.Instance.GetItemCount(nutrientItemID) > 0;

        bool nearDog = player != null && dog != null &&
                       Vector2.Distance(player.position, dog.position) <= giveDistance;

        bool show = !isDone && !isPlaying && hasNutrient && nearDog;

        if (interactPrompt.activeSelf != show)
            interactPrompt.SetActive(show);
    }

    // Inventory의 영양제 On Use()에 연결
    public void UseNutrient()
    {
        // 인벤토리는 이 함수를 부르기 전에 아이템을 이미 1개 뺀 상태
        if (isDone || isPlaying)
        {
            RefundItem();
            return;
        }

        float dist = (player != null && dog != null)
            ? Vector2.Distance(player.position, dog.position) : 9999f;
        bool nearDog = dist <= giveDistance;

        if (!nearDog)
        {
            // 멀리서 쓰면 아이템 돌려주기
            RefundItem();

            if (textTyping != null && tooFarDialogue != null)
                textTyping.StartDialogue(tooFarDialogue);
            return;
        }

        StartCoroutine(GiveSequence());
    }

    private void RefundItem()
    {
        if (Inventory.Instance != null)
            Inventory.Instance.AddItem(nutrientItemID, 1);
    }

    private IEnumerator GiveSequence()
    {
        isPlaying = true;

        // 1. 플레이어 멈추기
        if (playerController != null) playerController.canMove = false;

        // 2. 영롱이 말풍선
        if (dogBubble != null)
        {
            if (dogBubbleText != null) dogBubbleText.text = dogBubbleMessage;
            dogBubble.SetActive(true);
            yield return new WaitForSeconds(dogBubbleDuration);
            dogBubble.SetActive(false);
        }

        // 3. 플레이어 대사
        if (textTyping != null && afterDialogue != null)
        {
            textTyping.StartDialogue(afterDialogue);
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

    // 지정한 대사를 쓰는 길막 트리거를 모두 찾아서 충돌 판정 끄기
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

    private void OnDrawGizmosSelected()
    {
        if (dog == null) return;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(dog.position, giveDistance);
    }
}
