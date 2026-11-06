
using UnityEngine;

public class DogMedicineAction : MonoBehaviour
{
    [Header("상호작용 UI")]
    public PickupItem pickupItem;

    public static DogMedicineAction Current;

    [Header("약 사용 다이얼로그")]
    public DialogueDataSO medicineDialogue;

    [Header("강아지 퍼즐")]
    public DogFacePuzzleAction dogPuzzleAction;

    private TextTyping textTyping;

    private bool isPlayerNearby = false;
    private bool isCompleted = false;



    private void Awake()
    {
        Current = this;
    }

    private void Start()
    {
        textTyping = FindAnyObjectByType<TextTyping>();
    }

    private void Update()
    {
        if (pickupItem == null || pickupItem.PickupState == null)
            return;

        // 약(ID 1)을 보유하고 있는지 확인
        bool hasMedicine = Inventory.Instance != null &&
                           Inventory.Instance.GetItemCount(1) > 0;

        // 강아지 근처 + 약 보유 + 아직 사용하지 않은 상태
        bool showPickup = isPlayerNearby && hasMedicine && !isCompleted;

        pickupItem.PickupState.SetActive(showPickup);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }

    // 약 사용 가능 여부
    public bool CanUseMedicine()
    {
        return isPlayerNearby && !isCompleted;
    }

    // 인벤토리 onUse에서 호출
    public void UseMedicine()
    {
        if (!CanUseMedicine())
            return;

        isCompleted = true;

        Debug.Log("강아지에게 약을 먹였습니다.");

        // 약 사용 다이얼로그 실행
        if (textTyping != null && medicineDialogue != null)
        {
            textTyping.StartDialogue(medicineDialogue);
        }

        // 다음 강아지 퍼즐 활성화
        if (dogPuzzleAction != null)
        {
            dogPuzzleAction.EnableInteraction();
        }
    }

    private void OnDestroy()
    {
        if (Current == this)
            Current = null;
    }
}
