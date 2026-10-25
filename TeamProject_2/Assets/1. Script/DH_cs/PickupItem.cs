using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public TextTyping textTyping;
    public DialogueDataSO dialogue;

    public string itemName = "testitem";
    public GameObject PickupState;

    private void Start()
    {
        textTyping = FindAnyObjectByType<TextTyping>();
        PickupState.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PickupState.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PickupState.SetActive(false);
        }
    }

    public void Collect()
    {
        Debug.Log(itemName + " 상호작용");

        // 이 오브젝트에 연결된 기능 실행
        ItemAction action = GetComponent<ItemAction>();

        if (action != null)
        {
            action.Execute(this);
        }
    }
}