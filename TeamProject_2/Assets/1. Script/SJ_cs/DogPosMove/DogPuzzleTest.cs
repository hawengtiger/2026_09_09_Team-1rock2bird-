using UnityEngine;
using UnityEngine.InputSystem;

public class DogPuzzleTest : MonoBehaviour
{
    [Header("강아지 제스처 퍼즐")]
    public DogFacePuzzleAction dogFacePuzzleAction;

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.f1Key.wasPressedThisFrame)
        {
            // 2번 제스처 퍼즐 활성화
            dogFacePuzzleAction.StartPuzzle(1);
        }
    }
}
