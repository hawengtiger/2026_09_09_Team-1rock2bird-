using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private InventorySlotUI slot;

    private Inventory.ItemData displayedItem;
    private float displayedItemCount = -1f;

    private void Start()
    {
        RefreshHighestItem();
    }

    private void Update()
    {
        RefreshHighestItem();
    }

    private void RefreshHighestItem()
    {
        if (Inventory.Instance == null ||
            Inventory.Instance.itemData == null ||
            Inventory.Instance.itemData.Count == 0 ||
            slot == null)
        {
            return;
        }

        Inventory.ItemData highestItem = Inventory.Instance.itemData[0];

        foreach (Inventory.ItemData item in Inventory.Instance.itemData)
        {
            if (item.itemCount > highestItem.itemCount)
            {
                highestItem = item;
            }
        }

        // 가장 높은 아이템이 변경되었을 때만 슬롯 갱신
        if (displayedItem == highestItem &&
            displayedItemCount == highestItem.itemCount)
        {
            return;
        }

        displayedItem = highestItem;
        displayedItemCount = highestItem.itemCount;

        slot.SetItem(highestItem);
    }
}
