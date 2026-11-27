using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

// 상자 렌즈 기믹 (공간 맞추기 퍼즐의 UI 버전)
// 클릭 = 놓을 자리 선택 / 1~2 = 인벤토리(오른쪽 위) 슬롯 선택 / E = 놓기 / 우클릭·ESC = 닫기
// 아이템 선택은 팀원이 만든 InventoryUI의 선택 슬롯을 그대로 읽어서 씀 (InventoryUI 스크립트는 안 건드림)
// 옷장의 RangeInteract → On Interact에서 Open()을 부름
public class BoxLensPuzzle : MonoBehaviour
{
    [System.Serializable]
    public class Spot
    {
        public RectTransform area;              // 클릭할 자리 (예: 방석, 렌즈 구멍)
        public int requiredItemID;              // 이 자리에 맞는 아이템 ID
        public GameObject placedImage;          // 놓았을 때 보일 그림 (시작 시 꺼짐)
        public Image highlight;                 // 선택했을 때 보일 테두리 (선택)
        [HideInInspector] public bool filled;
    }

    [System.Serializable]
    public class ItemSlot
    {
        public int itemID;                      // 1번 키 = 첫 번째, 2번 키 = 두 번째
        public string itemName = "아이템";
        public Image slotImage;                 // (선택) 퍼즐 화면 안 아이템 칸 - 비워둬도 됨
    }

    [Header("퍼즐 화면")]
    public GameObject puzzlePanel;              // 상자 안 그림이 있는 UI (시작 시 꺼짐)
    public Spot[] spots;
    public ItemSlot[] items;

    [Header("아이템 칸 색")]
    public Color selectedColor = Color.yellow;
    public Color normalColor = Color.white;
    public Color usedColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

    [Header("안내 / 메시지")]
    public TMP_Text messageText;
    public string wrongMessage = "여기에 맞지 않는 것 같다.";
    public string selectSpotMessage = "먼저 놓을 곳을 클릭하자.";
    public string solvedMessage = "";
    public float messageTime = 1.5f;
    public float closeDelay = 1f;               // 다 맞춘 뒤 닫히기까지 시간

    [Header("다 풀었을 때 실행")]
    public UnityEvent onSolved;                 // 방 정상화, 사진 떨어뜨리기 등

    private int selectedItem = 0;
    private Spot selectedSpot;
    private bool isOpen = false;
    private bool isSolved = false;
    private float messageTimer = 0f;
    private PlayerController player;
    private InventoryUI inventoryUI;
    private FieldInfo selectedSlotField;
    private FieldInfo slotItemsField;

    private void Start()
    {
        player = MemoryUI.FindPlayer();
        inventoryUI = FindAnyObjectByType<InventoryUI>(FindObjectsInactive.Include);
        selectedSlotField = typeof(InventoryUI).GetField("selectedSlot", BindingFlags.NonPublic | BindingFlags.Instance);
        slotItemsField = typeof(InventoryUI).GetField("slotItems", BindingFlags.NonPublic | BindingFlags.Instance);
        if (puzzlePanel != null) puzzlePanel.SetActive(false);

        foreach (Spot s in spots)
        {
            if (s.placedImage != null) s.placedImage.SetActive(false);
            if (s.highlight != null) s.highlight.enabled = false;
        }
        if (messageText != null) messageText.text = "";
    }

    // 옷장 F → 열기
    public void Open()
    {
        if (isOpen || isSolved) return;

        isOpen = true;
        MemoryUI.IsOpen = true;
        if (player != null) player.canMove = false;

        puzzlePanel.SetActive(true);
        selectedSpot = null;
        selectedItem = 0;
        UpdateUI();
    }

    public void Close()
    {
        isOpen = false;
        MemoryUI.IsOpen = false;
        if (player != null) player.canMove = true;
        puzzlePanel.SetActive(false);
    }

    // 인벤토리 Item Data → On Use에 연결하면, 퍼즐 밖에서 F로 잘못 써도 아이템이 돌아옴
    public void ReturnItem(int itemID)
    {
        if (Inventory.Instance != null) Inventory.Instance.AddItem(itemID, 1);
    }

    private void Update()
    {
        if (messageTimer > 0f)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0f && messageText != null) messageText.text = "";
        }

        if (!isOpen || isSolved) return;
        Keyboard kb = Keyboard.current;
        Mouse mouse = Mouse.current;
        if (kb == null || mouse == null) return;

        // 1~2 아이템 선택
        if (kb.digit1Key.wasPressedThisFrame) { selectedItem = 0; UpdateUI(); }
        if (kb.digit2Key.wasPressedThisFrame && items.Length > 1) { selectedItem = 1; UpdateUI(); }

        // 클릭 = 자리 선택
        if (mouse.leftButton.wasPressedThisFrame)
        {
            Vector2 pos = mouse.position.ReadValue();
            foreach (Spot s in spots)
            {
                if (!s.filled && s.area != null &&
                    RectTransformUtility.RectangleContainsScreenPoint(s.area, pos, null))
                {
                    selectedSpot = s;
                    UpdateUI();
                    break;
                }
            }
        }

        // E = 놓기
        if (kb.eKey.wasPressedThisFrame) TryPlace();

        // 우클릭 / ESC = 닫기
        if (mouse.rightButton.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame) Close();
    }

    // 인벤토리 UI(오른쪽 위)에서 지금 선택된 아이템 ID. 없으면 -1
    private int GetInventorySelectedID()
    {
        if (inventoryUI == null || selectedSlotField == null || slotItemsField == null) return -1;

        int slot = (int)selectedSlotField.GetValue(inventoryUI);
        Inventory.ItemData[] slotItems = slotItemsField.GetValue(inventoryUI) as Inventory.ItemData[];
        if (slot < 0 || slotItems == null || slot >= slotItems.Length) return -1;

        Inventory.ItemData data = slotItems[slot];
        if (data == null || data.itemCount <= 0) return -1;
        return (int)data.itemID;
    }

    private void TryPlace()
    {
        if (selectedSpot == null) { ShowMessage(selectSpotMessage); return; }

        int itemID = GetInventorySelectedID();
        if (itemID < 0 && inventoryUI == null && items != null && items.Length > 0)
            itemID = items[selectedItem].itemID;   // InventoryUI가 없을 때만 예비용

        if (itemID < 0 || Inventory.Instance == null || Inventory.Instance.GetItemCount(itemID) <= 0)
        {
            ShowMessage("1·2번 키로 인벤토리 아이템을 먼저 고르자.");
            return;
        }

        if (itemID != selectedSpot.requiredItemID)
        {
            ShowMessage(wrongMessage);
            return;
        }

        // 배치 성공 → 인벤토리에서 빼기
        Inventory.Instance.RemoveItem(itemID, 1);
        selectedSpot.filled = true;
        if (selectedSpot.placedImage != null) selectedSpot.placedImage.SetActive(true);
        selectedSpot = null;
        UpdateUI();

        foreach (Spot s in spots)
            if (!s.filled) return;

        StartCoroutine(SolvedSequence());
    }

    private IEnumerator SolvedSequence()
    {
        isSolved = true;
        if (!string.IsNullOrEmpty(solvedMessage)) ShowMessage(solvedMessage);

        yield return new WaitForSeconds(closeDelay);

        Close();
        onSolved?.Invoke();
    }

    private void UpdateUI()
    {
        foreach (Spot s in spots)
            if (s.highlight != null) s.highlight.enabled = (s == selectedSpot);

        if (items == null) return;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].slotImage == null) continue;

            bool has = Inventory.Instance != null && Inventory.Instance.GetItemCount(items[i].itemID) > 0;
            if (!has) items[i].slotImage.color = usedColor;
            else if (i == selectedItem) items[i].slotImage.color = selectedColor;
            else items[i].slotImage.color = normalColor;
        }
    }

    private void ShowMessage(string text)
    {
        if (messageText == null) return;
        messageText.text = text;
        messageTimer = messageTime;
    }
}
