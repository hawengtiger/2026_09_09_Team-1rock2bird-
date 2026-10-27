using UnityEngine;

public class TestScript : MonoBehaviour
{
    public void TestAddItem1()
    {
        Inventory.Instance.AddItem(1, 1);
    }
    public void TestRemoveItem1()
    {
        Inventory.Instance.RemoveItem(1, 1); // 아이템 ID 1을 2개 제거
    }
    public void TestAddItem2()
    {
        Inventory.Instance.AddItem(2, 1);
    }
    public void TestRemoveItem2()
    {
        Inventory.Instance.RemoveItem(2, 1); // 아이템 ID 2을 3개 제거
    }
    public void TestAddItem3()
    {
        Inventory.Instance.AddItem(3, 1);
    }
    public void TestRemoveItem3()
    {
        Inventory.Instance.RemoveItem(3, 1); // 아이템 ID 3을 3개 제거
    }
    public void TestAddItem4()
    {
        Inventory.Instance.AddItem(4, 1);
    }
    public void TestRemoveItem4()
    {
        Inventory.Instance.RemoveItem(4, 1); // 아이템 ID 3을 3개 제거
    }

    public void SlotTest1()
    {
        Debug.Log("slottest1");
    }
    public void SlotTest2()
    {
        Debug.Log("slottest2");
    }
    public void SlotTest3()
    {
        Debug.Log("slottest3");
    }
}
