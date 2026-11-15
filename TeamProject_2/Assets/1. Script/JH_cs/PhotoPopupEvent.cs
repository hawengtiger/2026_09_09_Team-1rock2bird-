using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// 바닥에 떨어진 사진 F → 네컷 사진 팝업 (아래에 영롱이 날짜)
// → 아무 곳이나 클릭하면 닫힘 → 바로 대사 출력 ("아... 맞다." ...)
// 바닥 사진의 RangeInteract → On Interact에서 Open()을 부름
public class PhotoPopupEvent : MonoBehaviour
{
    [Header("팝업")]
    public GameObject popupPanel;               // 네컷 사진 + 날짜 + 안내 문구 (시작 시 꺼짐)

    [Header("닫은 뒤 대사")]
    public DialogueDataSO afterDialogue;        // "아... 맞다." / "..오늘 영롱이 49재지.." / ...

    [Header("끝났을 때 실행")]
    public UnityEvent onFinished;               // 거실 엔딩 트리거 켜기 등

    private PlayerController player;
    private TextTyping textTyping;
    private bool isOpen = false;

    private void Start()
    {
        player = MemoryUI.FindPlayer();
        textTyping = FindAnyObjectByType<TextTyping>(FindObjectsInactive.Include);
        if (popupPanel != null) popupPanel.SetActive(false);
    }

    public void Open()
    {
        if (isOpen) return;
        StartCoroutine(PopupSequence());
    }

    private IEnumerator PopupSequence()
    {
        isOpen = true;
        MemoryUI.IsOpen = true;
        if (player != null) player.canMove = false;

        popupPanel.SetActive(true);
        yield return null;   // F 누른 프레임에 바로 닫히지 않게

        // 아무 곳이나 클릭
        yield return new WaitUntil(() => Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

        popupPanel.SetActive(false);
        MemoryUI.IsOpen = false;
        yield return null;   // 같은 클릭이 대사를 넘기지 않게

        // 지연 없이 대사
        if (textTyping != null && afterDialogue != null)
        {
            textTyping.StartDialogue(afterDialogue);
            yield return null;
            if (textTyping.darkUIEffect != null)
                yield return new WaitUntil(() => !textTyping.darkUIEffect.isStory);
        }

        if (player != null) player.canMove = true;
        isOpen = false;

        onFinished?.Invoke();
    }
}
