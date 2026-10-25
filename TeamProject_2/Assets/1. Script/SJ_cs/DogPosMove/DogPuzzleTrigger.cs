
using UnityEngine;

public class DogPuzzleTrigger : MonoBehaviour
{
    [Header("퍼즐")]
    public PetPuzzleManager petPuzzleManager;

    [Header("성공 시 활성화할 오브젝트")]
    public GameObject targetObject;

    private bool isActivated = false;

    private void Start()
    {
        if (targetObject != null)
            targetObject.SetActive(false);
    }

    private void Update()
    {
        if (isActivated || petPuzzleManager == null) return;

        if (petPuzzleManager.currentState != PuzzleState.ResultWait)
            return;

        if (petPuzzleManager.CurrentSelectedIndex < 0 ||
            petPuzzleManager.CurrentSelectedIndex !=
            petPuzzleManager.correctOptionIndex)
            return;

        isActivated = true;

        if (targetObject != null)
            targetObject.SetActive(true);
    }
}
