using UnityEngine;

public class TestScript : MonoBehaviour
{
    public void TestAddItem1()
    {
        Inventory.Instance.AddItem(1, 3);
    }
    public void TestRemoveItem1()
    {
        Inventory.Instance.RemoveItem(1, 3); // 아이템 ID 1을 2개 제거
    }
    public void TestAddItem2()
    {
        Inventory.Instance.AddItem(2, 3);
    }
    public void TestRemoveItem2()
    {
        Inventory.Instance.RemoveItem(2, 3); // 아이템 ID 2을 3개 제거
    }
    public void TestAddItem3()
    {
        Inventory.Instance.AddItem(3, 3);
    }
    public void TestRemoveItem3()
    {
        Inventory.Instance.RemoveItem(3, 3); // 아이템 ID 3을 3개 제거
    }
}
