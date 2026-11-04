
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [System.Serializable]
    public class ItemData
    {
        public float itemID;
        public float itemCount;
        public string itemName;
        public Sprite itemSprite;

        // 아이템별 사용 함수
        public UnityEvent onUse;
    }

    public List<ItemData> itemData;

    public int GetItemCount(int itemID)
    {
        foreach (ItemData item in itemData)
        {
            if ((int)item.itemID == itemID)
                return (int)item.itemCount;
        }

        return 0;
    }

    public void AddItem(int itemID, int count)
    {
        foreach (ItemData item in itemData)
        {
            if ((int)item.itemID == itemID)
            {
                item.itemCount += count;
                return;
            }
        }

        Debug.LogError($"ItemID {itemID}를 가진 아이템을 찾을 수 없음");
    }

    public bool RemoveItem(int itemID, int count)
    {
        foreach (ItemData item in itemData)
        {
            if ((int)item.itemID == itemID)
            {
                if (item.itemCount < count)
                    return false;

                item.itemCount -= count;
                return true;
            }
        }

        return false;
    }

    public void ClearItem()
    {
        if (itemData == null) return;

        foreach (ItemData item in itemData)
        {
            item.itemCount = 0;
        }
    }
}
