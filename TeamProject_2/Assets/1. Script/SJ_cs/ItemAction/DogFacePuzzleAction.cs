
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class DogPuzzleSuccessData
{
    [Header("성공 다이얼로그")]
    public DialogueDataSO successDialogue;

    [Header("강아지 이동 목표 위치")]
    public Vector2Int spawnPosition;
}

public class DogFacePuzzleAction : ItemAction
{
    [Header("퍼즐")]
    public PetPuzzleManager petPuzzleManager;

    [Header("강아지 목표 생성")]
    public DogPosMove dogPosMove;

    [Header("성공 횟수별 이벤트")]
    public List<DogPuzzleSuccessData> successEvents = new();

    [Header("상호작용 가능 여부")]
    public bool canInteract = true;

    private TextTyping textTyping;

    private int successCount = 0;
    private bool resultHandled = false;
    private bool isSuccess = false;

    private void Start()
    {
        textTyping = FindAnyObjectByType<TextTyping>();

        canInteract = false;
    }

    private void Update()
    {
        if (petPuzzleManager == null) return;

        if (petPuzzleManager.currentState == PuzzleState.ResultWait)
        {
            if (!resultHandled)
            {
                resultHandled = true;

                isSuccess = petPuzzleManager.CurrentSelectedIndex == petPuzzleManager.correctOptionIndex;
            }

            // 결과 화면에서 F키를 누르면 종료
            if (Keyboard.current != null &&
                Keyboard.current.fKey.wasPressedThisFrame)
            {
                petPuzzleManager.Interact();

                if (isSuccess)
                {
                    SuccessEvent();
                }

                isSuccess = false;
                resultHandled = false;
            }
        }
    }

    private void SuccessEvent()
    {
        if (successCount >= successEvents.Count) return;

        DogPuzzleSuccessData data = successEvents[successCount];

        if (textTyping != null && data.successDialogue != null)
        {
            textTyping.StartDialogue(data.successDialogue);
        }

        if (dogPosMove != null)
        {
            dogPosMove.SpawnTarget(
                data.spawnPosition.x,
                data.spawnPosition.y
            );

            canInteract = false;
        }

        successCount++;
    }
    public override void Execute(PickupItem item)
    {
        if (!canInteract || petPuzzleManager == null) return;

        if (petPuzzleManager.currentState == PuzzleState.ResultWait)
        {
            bool isSuccess =
                petPuzzleManager.CurrentSelectedIndex >= 0 &&
                petPuzzleManager.CurrentSelectedIndex ==
                petPuzzleManager.correctOptionIndex;

            Debug.Log($"퍼즐 성공 여부: {isSuccess}");

            // 퍼즐 종료
            petPuzzleManager.Interact();

            // 정답이었다면 스토리 진행
            if (isSuccess)
            {
                SuccessEvent();
            }

            return;
        }

        // 퍼즐 시작
        petPuzzleManager.Interact();
    }
    public void EnableInteraction()
    {
        canInteract = true;
    }
}
