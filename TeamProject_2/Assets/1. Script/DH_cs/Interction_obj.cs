using UnityEngine;

public class Interction_obj : MonoBehaviour
{
    public string ObjName = "testitem";
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

    public void Interact()
    {
        Debug.Log(ObjName + " 상호작용");
    }
}
