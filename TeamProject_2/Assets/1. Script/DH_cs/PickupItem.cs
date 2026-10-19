using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public string itemName = "testitem";
    public GameObject PickupState;

    public StoryManager storyManager;

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
        // 간단한 감지 로직: 이름 패턴에 따라 다른 로그 출력
        if (itemName.Contains("Medicine") && storyManager.storyIndex == 0)
        {
            Debug.Log($"{itemName} 감지: 약 아이템 상호작용");
            storyManager.storyIndex ++;
        }

        Debug.Log(itemName + " 획득");
        Destroy(gameObject);

    }
}
