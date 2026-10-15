using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public TextTyping textTyping;
    public DialogueDataSO dialogue;

    public string itemName = "testitem";
    public GameObject PickupState;

    void Start()
    {
        textTyping = FindAnyObjectByType<TextTyping>();
        PickupState.SetActive(false);
    }

    void Update()
    {
        
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
        Debug.Log(itemName + " 획득");

        if (itemName == "testitem")
        {
            textTyping.StartDialogue(dialogue);
        }

        Destroy(gameObject);
    }
}
