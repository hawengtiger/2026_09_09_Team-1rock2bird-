using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public string itemName = "testitem";
    public GameObject PickupState;

    void Start()
    {
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
        Destroy(gameObject);
    }
}
