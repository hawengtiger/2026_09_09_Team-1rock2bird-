using UnityEngine;

public class InventoryUISetActive : MonoBehaviour
{
    [Header("인벤토리 슬롯")]
    public GameObject[] inventorySlot;

    private DarkUIEffect darkUI;
    private PetPuzzleManager petPuzzleManager;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        darkUI = FindAnyObjectByType<DarkUIEffect>();
        petPuzzleManager = FindAnyObjectByType<PetPuzzleManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (inventorySlot != null)
        {
            foreach (GameObject slot in inventorySlot)
            {
                if (slot != null)
                {
                    slot.SetActive(!darkUI.isStory);
                }

                if (!(petPuzzleManager.currentState == PuzzleState.WaitToEnter))
                {
                    slot.SetActive(false);
                }
            }
        }
    }
}
