
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    public Image icon;
    [Header("슬롯 선택 이미지")]
    public Image slotBackground;

    public Sprite normalImage;
    public Sprite selectedImage;

    // 아이템 표시
    public void SetItem(Inventory.ItemData item)
    {
        if (item == null)
        {
            ClearSlot();
            return;
        }

        icon.sprite = item.itemSprite;
        icon.enabled = item.itemSprite != null;
    }

    // 슬롯 비우기
    public void ClearSlot()
    {
        icon.sprite = null;
        icon.enabled = false;

        SetSelected(false);
    }

    // 슬롯 선택 이미지 변경
    public void SetSelected(bool selected)
    {
        if (slotBackground == null)
            return;

        slotBackground.sprite =
            selected ? selectedImage : normalImage;
    }
}
