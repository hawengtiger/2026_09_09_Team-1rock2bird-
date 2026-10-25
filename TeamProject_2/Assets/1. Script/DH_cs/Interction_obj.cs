using UnityEngine;

public class Interction_obj : MonoBehaviour
{
    public string ObjName = "testitem";
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

    public void Interact()
    {
        // 간단한 감지 로직: 이름 패턴에 따라 다른 로그 출력
        if (ObjName.Contains("Bed") && storyManager.storyIndex == 1)
        {
            Debug.Log($"{ObjName} 감지: 침대 아이템 상호작용");
            storyManager.storyIndex ++;
            storyManager.StoryGoDream();
        }
        else if (ObjName.Contains("Bed") && storyManager.storyIndex != 1)
        {
            Debug.Log($"{ObjName} 감지: 비정상_침대 아이템 상호작용");
        }
    }
}
