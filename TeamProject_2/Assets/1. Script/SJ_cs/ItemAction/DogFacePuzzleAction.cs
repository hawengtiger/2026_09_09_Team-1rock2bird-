
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class DogPuzzleData
{
    [Header("성공 다이얼로그")]
    public DialogueDataSO successDialogue;

    [Header("강아지 이동 목표 위치")]
    public Vector2Int spawnPosition;

    [Header("퍼즐을 진행 할 수 있나?")]
    public bool isStartPuzzle;

    [Header("퍼즐이 끝났는가?")]
    public bool isEndPuzzle;
}

public class DogFacePuzzleAction : ItemAction
{
    [Header("상호작용 UI")]
    public PickupItem pickupItem;

    [Header("퍼즐")]
    public PetPuzzleManager petPuzzleManager;

    [Header("강아지 목표 생성")]
    public DogPosMove dogPosMove;

    [Header("2번 퍼즐 트리거")]
    public DogPuzzleTest dogPuzzleTest;

    [Header("성공 횟수별 이벤트")]
    public List<DogPuzzleData> successEvents = new();

    [Header("현재 퍼즐 인덱스")]
    [SerializeField] private int successCount = 0;

    private TextTyping textTyping;
    private DarkUIEffect darkUI;

    private bool resultHandled = false;
    private bool isSuccess = false;
    private bool isPlayerNearby = false;

    private void Start()
    {
        textTyping = FindAnyObjectByType<TextTyping>(
            FindObjectsInactive.Include
        );

        darkUI = FindAnyObjectByType<DarkUIEffect>();

        if (pickupItem == null)
            pickupItem = GetComponent<PickupItem>();

        CheckCompletedPuzzle();
        UpdateInteractionUI();
    }

    private void Update()
    {
        CheckCompletedPuzzle();
        UpdateInteractionUI();

        if (petPuzzleManager == null)
            return;

        // 퍼즐 결과 화면
        if (petPuzzleManager.currentState == PuzzleState.ResultWait)
        {
            if (!resultHandled)
            {
                resultHandled = true;

                isSuccess =
                    petPuzzleManager.CurrentSelectedIndex >= 0 &&
                    petPuzzleManager.CurrentSelectedIndex ==
                    petPuzzleManager.correctOptionIndex;
            }

            if (Keyboard.current != null &&
                Keyboard.current.fKey.wasPressedThisFrame)
            {
                // 결과 화면 닫기
                petPuzzleManager.Interact();

                // 동일 프레임 재시작 방지
                if (dogPuzzleTest != null)
                    dogPuzzleTest.NotifyResultClosed();

                if (isSuccess)
                {
                    // 2번 퍼즐 성공 시 Hug + 이동 잠금 해제
                    if (successCount == 1 && dogPuzzleTest != null)
                    {
                        dogPuzzleTest.CompletePuzzle();
                    }

                    // 성공 다이얼로그 및 기존 이벤트
                    SuccessEvent();
                }
                else
                {
                    Debug.Log("퍼즐 실패! F키로 다시 도전 가능");
                }

                isSuccess = false;
                resultHandled = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            UpdateInteractionUI();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            UpdateInteractionUI();
        }
    }

    public bool CanStartPuzzle()
    {
        if (successCount < 0 ||
            successCount >= successEvents.Count)
            return false;

        DogPuzzleData data = successEvents[successCount];

        return data.isStartPuzzle && !data.isEndPuzzle;
    }

    private void CheckCompletedPuzzle()
    {
        while (successCount < successEvents.Count &&
               successEvents[successCount].isEndPuzzle)
        {
            successCount++;

            Debug.Log($"현재 퍼즐 인덱스: {successCount}");
        }
    }

    private void UpdateInteractionUI()
    {
        if (pickupItem == null ||
            pickupItem.PickupState == null)
            return;

        bool canPuzzle = CanStartPuzzle();

        bool canMedicine =
            DogMedicineAction.Current != null &&
            DogMedicineAction.Current.CanTalkToDog();

        bool canCarry =
            DogMedicineAction.Current != null &&
            DogMedicineAction.Current.CanCarryDog();

        bool showUI =
            (isPlayerNearby && (canPuzzle || canMedicine)) ||
            canCarry;

        // 2번 퍼즐이 실행 중이거나 성공했다면
        // 기존 Dog 상호작용 UI는 표시하지 않음
        if (dogPuzzleTest != null &&
            petPuzzleManager != null &&
            petPuzzleManager.currentState != PuzzleState.WaitToEnter)
        {
            showUI = false;
        }

        if (pickupItem.PickupState.activeSelf != showUI)
        {
            pickupItem.PickupState.SetActive(showUI);
        }
    }

    private void SuccessEvent()
    {
        if (!CanStartPuzzle())
            return;

        int completedPuzzleIndex = successCount;

        DogPuzzleData data = successEvents[successCount];

        data.isEndPuzzle = true;
        data.isStartPuzzle = false;

        // 성공 다이얼로그 실행
        if (textTyping != null &&
            data.successDialogue != null)
        {
            textTyping.StartDialogue(data.successDialogue);
        }

        StartCoroutine(
            DelayTime(data, completedPuzzleIndex)
        );

        CheckCompletedPuzzle();
        UpdateInteractionUI();
    }

    private IEnumerator DelayTime(
        DogPuzzleData data,
        int completedPuzzleIndex)
    {
        yield return null;

        // 성공 다이얼로그 종료 대기
        if (darkUI != null &&
            data.successDialogue != null)
        {
            yield return new WaitUntil(
                () => !darkUI.isStory
            );
        }

        // 2번 퍼즐은 이미 성공 순간에 Hug + 이동 복구 완료
        if (completedPuzzleIndex == 1)
        {
            Debug.Log("2번 퍼즐 성공 다이얼로그 종료");
            yield break;
        }

        // 다른 퍼즐은 기존 강아지 이동 실행
        if (dogPosMove != null)
        {
            dogPosMove.SpawnTarget(
                data.spawnPosition.x,
                data.spawnPosition.y
            );

            Debug.Log("스토리 종료! 강아지 이동 시작");
        }
    }

    public override void Execute(PickupItem item)
    {
        // 약을 가지고 영롱이와 대화
        if (DogMedicineAction.Current != null &&
            DogMedicineAction.Current.CanTalkToDog())
        {
            DogMedicineAction.Current.TalkToDog();
            return;
        }

        if (InventoryUI.LastItemUseFrame == Time.frameCount)
            return;

        if (!CanStartPuzzle() ||
            petPuzzleManager == null)
            return;

        if (petPuzzleManager.currentState != PuzzleState.WaitToEnter)
            return;

        petPuzzleManager.Interact();
    }

    public void StartPuzzle(int index)
    {
        if (index < 0 || index >= successEvents.Count)
        {
            Debug.LogWarning(
                $"존재하지 않는 퍼즐 인덱스: {index}"
            );
            return;
        }

        if (successEvents[index].isEndPuzzle)
        {
            Debug.Log(
                $"이미 완료된 퍼즐입니다: {index}"
            );
            return;
        }

        if (petPuzzleManager == null)
        {
            Debug.LogWarning(
                "PetPuzzleManager가 연결되지 않았습니다."
            );
            return;
        }

        if (petPuzzleManager.currentState != PuzzleState.WaitToEnter)
        {
            Debug.LogWarning(
                "현재 퍼즐이 진행 중입니다."
            );
            return;
        }

        petPuzzleManager.SetPuzzle(index);

        successCount = index;

        for (int i = 0 ; i < successEvents.Count ; i++)
        {
            successEvents[i].isStartPuzzle = false;
        }

        successEvents[index].isStartPuzzle = true;

        UpdateInteractionUI();

        Debug.Log($"{index + 1}번 제스처 퍼즐 활성화!");
    }

    public void EnableInteraction()
    {
        CheckCompletedPuzzle();

        if (successCount >= successEvents.Count)
            return;

        StartPuzzle(successCount);
    }

    public void DisableInteraction()
    {
        if (successCount >= successEvents.Count)
            return;

        successEvents[successCount].isStartPuzzle = false;

        UpdateInteractionUI();
    }

    public int GetCurrentPuzzleIndex()
    {
        return successCount;
    }
}
