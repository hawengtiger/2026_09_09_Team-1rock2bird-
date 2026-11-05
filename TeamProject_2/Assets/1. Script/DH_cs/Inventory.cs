using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    }
    public List<ItemData> itemData;


    public int GetItemCount(int itemID)
    {
        foreach (ItemData item in itemData)
        {
            if ((int)item.itemID == itemID)
            {
                Debug.Log($"GetItemCount : {item.itemName} = {item.itemCount}");
                return (int)item.itemCount;
            }
        }

        Debug.LogWarning($"GetItemCount 실패 : ItemID {itemID}");
        return 0;
    }

    public void AddItem(int itemID, int count)
    {
        Debug.Log($"AddItem 호출됨");
        Debug.Log($"추가할 ItemID : {itemID}");
        Debug.Log($"추가할 개수 : {count}");

        foreach (ItemData item in itemData)
        {
            Debug.Log($"검사중 : {item.itemName} ({item.itemID})");

            if ((int)item.itemID == itemID)
            {
                Debug.Log($"매칭 성공 : {item.itemName}");

                item.itemCount += count;

                Debug.Log($"증가 후 수량 : {item.itemCount}");

                return;
            }
        }

        Debug.LogError($"ItemID {itemID} 를 가진 아이템을 찾을 수 없음");
    }

    public bool RemoveItem(int itemID, int count)
    {
        foreach (ItemData item in itemData)
        {
            if ((int)item.itemID == itemID)
            {
                if (item.itemCount < count)
                {
                    Debug.Log($"소모 실패 : {item.itemName}");
                    return false;
                }

                item.itemCount -= count;

                Debug.Log($"소모 성공 : {item.itemName}");
                Debug.Log($"남은 수량 : {item.itemCount}");

                return true;
            }
        }

        Debug.LogError($"ConsumeItem 실패 : ItemID {itemID}");
        return false;
    }

    // 모든 아이템의 개수를 0으로 초기화합니다.
    public void ClearItem()
    {
        if (itemData == null)
        {
            Debug.LogWarning("ClearItem: itemData가 null입니다.");
            return;
        }

        foreach (var item in itemData)
        {
            item.itemCount = 0;
        }

        Debug.Log("ClearItem: 모든 아이템 수량을 0으로 초기화했습니다.");
    }


}