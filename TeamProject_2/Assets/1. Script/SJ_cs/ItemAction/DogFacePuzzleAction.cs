
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

        // 이미 완료된 퍼즐은 건너뛰기
        CheckCompletedPuzzle();

        UpdateInteractionUI();
    }

    private void Update()
    {
        CheckCompletedPuzzle();
        UpdateInteractionUI();

        if (petPuzzleManager == null)
            return;

        // 결과 화면에서 F키 처리
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

                if (isSuccess)
                {
                    SuccessEvent();
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

    // 현재 퍼즐이 실행 가능한가?
    public bool CanStartPuzzle()
    {
        if (successCount < 0 || successCount >= successEvents.Count)
            return false;

        DogPuzzleData data = successEvents[successCount];

        return data.isStartPuzzle && !data.isEndPuzzle;
    }

    // 완료된 퍼즐은 다음 인덱스로 이동
    private void CheckCompletedPuzzle()
    {
        while (successCount < successEvents.Count &&
               successEvents[successCount].isEndPuzzle)
        {
            successCount++;

            Debug.Log($"현재 퍼즐 인덱스: {successCount}");
        }
    }

    // 상호작용 UI 갱신
    private void UpdateInteractionUI()
    {
        if (pickupItem == null || pickupItem.PickupState == null)
            return;

        bool canPuzzle = CanStartPuzzle();

        // 인벤토리 약 사용은 별도로 검사
        bool canMedicine =
            DogMedicineAction.Current != null &&
            DogMedicineAction.Current.CanTalkToDog();

        // 퍼즐 또는 약 먹이기 중 하나라도 가능하면 표시
        bool showUI = isPlayerNearby && (canPuzzle || canMedicine);

        if (pickupItem.PickupState.activeSelf != showUI)
        {
            pickupItem.PickupState.SetActive(showUI);
        }
    }

    // 퍼즐 성공 이벤트
    private void SuccessEvent()
    {
        if (!CanStartPuzzle())
            return;

        DogPuzzleData data = successEvents[successCount];

        // 현재 퍼즐 완료 처리
        data.isEndPuzzle = true;
        data.isStartPuzzle = false;

        // 성공 다이얼로그
        if (textTyping != null && data.successDialogue != null)
        {
            textTyping.StartDialogue(data.successDialogue);
        }

        // 스토리 종료 후 강아지 이동
        StartCoroutine(DelayTime(data));

        // 다음 퍼즐로 이동
        CheckCompletedPuzzle();

        UpdateInteractionUI();
    }

    private IEnumerator DelayTime(DogPuzzleData data)
    {
        // 스토리가 끝날 때까지 대기
        yield return new WaitUntil(() =>
            darkUI == null || !darkUI.isStory
        );

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
        // 약을 가지고 영롱이와 대화할 수 있다면
        if (DogMedicineAction.Current != null &&
            DogMedicineAction.Current.CanTalkToDog())
        {
            DogMedicineAction.Current.TalkToDog();
            return;
        }

        // 같은 프레임에 아이템을 사용했다면 퍼즐 실행 금지
        if (InventoryUI.LastItemUseFrame == Time.frameCount)
            return;

        if (!CanStartPuzzle() || petPuzzleManager == null)
            return;

        // 결과 화면은 Update()에서만 처리
        if (petPuzzleManager.currentState == PuzzleState.ResultWait)
            return;

        // 이미 퍼즐 진행 중이라면 다시 열지 않기
        if (petPuzzleManager.currentState == PuzzleState.PuzzleActive)
            return;

        // 퍼즐 시작
        petPuzzleManager.Interact();
    }

    // 원하는 번호의 퍼즐 활성화
    public void StartPuzzle(int index)
    {
        if (index < 0 || index >= successEvents.Count)
        {
            Debug.LogWarning($"존재하지 않는 퍼즐 인덱스: {index}");
            return;
        }

        if (successEvents[index].isEndPuzzle)
        {
            Debug.Log($"이미 완료된 퍼즐입니다: {index}");
            return;
        }

        if (petPuzzleManager == null)
        {
            Debug.LogWarning("PetPuzzleManager가 연결되지 않았습니다.");
            return;
        }

        if (petPuzzleManager.currentState != PuzzleState.WaitToEnter)
        {
            Debug.LogWarning("현재 퍼즐이 진행 중입니다.");
            return;
        }

        // 실제 질문, 선택지, 표정의 인덱스도 변경
        petPuzzleManager.SetPuzzle(index);

        // 현재 스토리 퍼즐 인덱스 변경
        successCount = index;

        // 모든 퍼즐 비활성화
        for (int i = 0 ; i < successEvents.Count ; i++)
        {
            successEvents[i].isStartPuzzle = false;
        }

        // 해당 퍼즐만 활성화
        successEvents[index].isStartPuzzle = true;

        UpdateInteractionUI();

        Debug.Log($"{index + 1}번 제스처 퍼즐 활성화!");
    }

    // 현재 퍼즐 활성화
    public void EnableInteraction()
    {
        CheckCompletedPuzzle();

        if (successCount >= successEvents.Count)
            return;

        StartPuzzle(successCount);
    }

    // 현재 퍼즐 비활성화
    public void DisableInteraction()
    {
        if (successCount >= successEvents.Count)
            return;

        successEvents[successCount].isStartPuzzle = false;

        UpdateInteractionUI();
    }

    // 현재 퍼즐 인덱스 확인
    public int GetCurrentPuzzleIndex()
    {
        return successCount;
    }
}
