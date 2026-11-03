
using UnityEngine;

public class DogPuzzleTrigger : MonoBehaviour
{
    [Header("∆€¡Ò")]
    public PetPuzzleManager petPuzzleManager;

    private bool isActivated = false;

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
    }
}
