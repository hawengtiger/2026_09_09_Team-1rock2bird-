using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// 일정 거리 안에 들어오면 (닿지 않아도) F 표시가 뜨고, F를 누르면 상호작용
// - 아이템 줍기: Add Item ID에 인벤토리 ID를 넣으면 F로 인벤토리에 들어가고 사라짐
// - 조건: Required Item IDs를 넣으면 그 아이템들을 다 가지고 있을 때만 F 표시 (옷장)
// - 그 외 동작은 On Interact에 연결 (퍼즐 열기, 사진 팝업 등)
public class RangeInteract : MonoBehaviour
{
    [Header("F 표시")]
    public GameObject prompt;                   // 머리 위 F 표시
    public float range = 4f;                    // 이 거리 안이면 F 표시 (Scene에 노란 원)

    [Header("아이템 줍기 (선택)")]
    public int addItemID = -1;                  // -1이면 줍지 않음
    public bool hideOnUse = true;               // 상호작용 후 오브젝트 숨기기

    [Header("조건 (선택)")]
    public int[] requiredItemIDs;               // 이 아이템들을 모두 가지고 있어야 F 표시

    [Header("상호작용 시 실행")]
    public UnityEvent onInteract;

    private Transform player;
    private bool used = false;
    private float shownTime = 0f;               // F 표시가 떠 있던 시간 (뜨자마자 눌리는 것 방지)
    private const float minShowTime = 0.3f;

    private void OnEnable()
    {
        shownTime = 0f;
    }

    private void Start()
    {
        PlayerController pc = MemoryUI.FindPlayer();
        if (pc != null) player = pc.transform;

        if (prompt != null) prompt.SetActive(false);
    }

    private void OnDisable()
    {
        if (prompt != null) prompt.SetActive(false);
    }

    private bool HasRequiredItems()
    {
        if (requiredItemIDs == null || requiredItemIDs.Length == 0) return true;
        if (Inventory.Instance == null) return false;

        foreach (int id in requiredItemIDs)
            if (Inventory.Instance.GetItemCount(id) <= 0) return false;

        return true;
    }

    private void Update()
    {
        if (player == null) return;

        bool canUse = !used &&
                      !MemoryUI.IsOpen &&
                      !MemoryUI.IsStory() &&
                      HasRequiredItems() &&
                      Vector2.Distance(player.position, transform.position) <= range;

        if (prompt != null && prompt.activeSelf != canUse)
            prompt.SetActive(canUse);

        shownTime = canUse ? shownTime + Time.deltaTime : 0f;

        // F 표시가 0.3초 이상 보인 뒤에 누른 F만 인정
        if (canUse && shownTime >= minShowTime &&
            Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            Interact();
    }

    private void Interact()
    {
        if (addItemID >= 0 && Inventory.Instance != null)
            Inventory.Instance.AddItem(addItemID, 1);

        if (prompt != null) prompt.SetActive(false);

        onInteract?.Invoke();

        if (hideOnUse)
        {
            used = true;
            gameObject.SetActive(false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
