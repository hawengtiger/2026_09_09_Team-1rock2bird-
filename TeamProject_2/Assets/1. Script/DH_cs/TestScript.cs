using UnityEngine;

public class TestScript : MonoBehaviour
{
    void Start()
    {
        Inventory.Instance.AddItem(2, 3); // 아이템 ID 1을 3개 추가
    }

    void Update()
    {
        
    }
    public void TestAddItem()
    {
        Inventory.Instance.AddItem(1, 3);
    }
    public void TestRemoveItem()
    {
        Inventory.Instance.RemoveItem(1, 2); // 아이템 ID 1을 2개 제거
    }
    public void TestAddItemINT(int itemID, int itemCount)
    {
        Inventory.Instance.AddItem(itemID, itemCount);
    }
}
