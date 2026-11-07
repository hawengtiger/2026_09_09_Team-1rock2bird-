
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryUI : MonoBehaviour
{
    [Header("인벤토리 슬롯 2개")]
    [SerializeField] private InventorySlotUI[] slots = new InventorySlotUI[2];

    // 각 슬롯에 고정 배정된 아이템
    private Inventory.ItemData[] slotItems = new Inventory.ItemData[2];

    // 현재 선택된 슬롯
    private int selectedSlot = -1;

    // 이번 프레임에 아이템을 사용했는지
    public static int LastItemUseFrame = -1;

    private void Start()
    {
        RefreshInventory();
    }

    private void Update()
    {
        RefreshInventory(); //나중에 아이템 더하고빼는 로직에 개별참조하기

        if (Inventory.Instance == null)
            return;

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (keyboard.digit1Key.wasPressedThisFrame ||
            keyboard.numpad1Key.wasPressedThisFrame)
        {
            SelectSlot(0);
        }

        if (keyboard.digit2Key.wasPressedThisFrame ||
            keyboard.numpad2Key.wasPressedThisFrame)
        {
            SelectSlot(1);
        }

        // F키로 선택한 아이템 사용
        if (keyboard.fKey.wasPressedThisFrame)
        {
            UseSelectedItem();
        }
    }

    // 슬롯 선택
    private void SelectSlot(int slotIndex)
    {
        if (slotItems[slotIndex] == null ||
            slotItems[slotIndex].itemCount <= 0)
        {
            Debug.Log("선택할 아이템이 없습니다.");
            return;
        }

        selectedSlot = slotIndex;

        RefreshInventory();

        Debug.Log($"슬롯 {slotIndex + 1} 선택");
    }

    // 선택한 아이템 사용
    private void UseSelectedItem()
    {
        if (selectedSlot < 0)
            return;

        Inventory.ItemData item = slotItems[selectedSlot];

        if (item == null || item.itemCount <= 0)
            return;

        int itemID = (int)item.itemID;

        // 약(ID 1)은 영롱이를 업은 상태에서만 사용 가능
        if (itemID == 1)
        {
            if (DogMedicineAction.Current == null ||
                !DogMedicineAction.Current.CanUseMedicine())
            {
                Debug.Log("영롱이를 업은 후에 약을 사용할 수 있습니다.");
                return;
            }
        }

        if (Inventory.Instance.RemoveItem(itemID, 1))
        {
            LastItemUseFrame = Time.frameCount;

            item.onUse?.Invoke();

            Debug.Log($"아이템 사용: {item.itemName}");

            if (item.itemCount <= 0)
            {
                slotItems[selectedSlot] = null;
                selectedSlot = -1;
            }
        }

        RefreshInventory();
    }

    // 인벤토리 최초 배치 및 갱신
    public void RefreshInventory()
    {
        if (Inventory.Instance == null ||
            Inventory.Instance.itemData == null)
            return;

        // 비어 있는 슬롯에만 새로운 아이템 배정
        for (int i = 0; i < slots.Length; i++)
        {
            if (slotItems[i] == null)
            {
                foreach (Inventory.ItemData item in Inventory.Instance.itemData)
                {
                    if (item.itemCount >= 1 &&
                        !IsAlreadyAssigned(item))
                    {
                        slotItems[i] = item;
                        break;
                    }
                }
            }
        }

        // UI 갱신
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
                continue;

            Inventory.ItemData item = slotItems[i];

            if (item != null && item.itemCount >= 1)
            {
                slots[i].SetItem(item);
                slots[i].SetSelected(selectedSlot == i);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }
    }

    // 이미 다른 슬롯에 배정된 아이템인지 확인
    private bool IsAlreadyAssigned(Inventory.ItemData item)
    {
        foreach (Inventory.ItemData assignedItem in slotItems)
        {
            if (assignedItem == item)
                return true;
        }

        return false;
    }
}
