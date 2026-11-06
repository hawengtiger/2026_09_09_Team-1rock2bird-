
using System.Collections.Generic;
using System.Collections;
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
    [Header("상호작용 UI")]
    public PickupItem pickupItem;

    [Header("퍼즐")]
    public PetPuzzleManager petPuzzleManager;

    [Header("강아지 목표 생성")]
    public DogPosMove dogPosMove;

    [Header("성공 횟수별 이벤트")]
    public List<DogPuzzleSuccessData> successEvents = new();

    [Header("상호작용 가능 여부")]
    public bool canInteract = true;

    private TextTyping textTyping;
    private DarkUIEffect darkUI;

    private int successCount = 0;
    private bool resultHandled = false;
    private bool isSuccess = false;

    // 플레이어가 강아지 근처에 있는지
    private bool isPlayerNearby = false;

    private void Start()
    {
        textTyping = FindAnyObjectByType<TextTyping>();
        darkUI = FindAnyObjectByType<DarkUIEffect>();

        if (pickupItem == null)
            pickupItem = GetComponent<PickupItem>();

        canInteract = false;

        UpdateInteractionUI();
    }

    private void Update()
    {
        UpdateInteractionUI();

        if (petPuzzleManager == null)
            return;

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

    private void UpdateInteractionUI()
    {
        if (pickupItem == null || pickupItem.PickupState == null)
            return;

        // 퍼즐 제스처를 실행할 수 있는 상태
        bool canPuzzle = canInteract;

        // 약을 먹일 수 있는 상태
        bool canMedicine =
            DogMedicineAction.Current != null &&
            DogMedicineAction.Current.CanUseMedicine() &&
            Inventory.Instance != null &&
            Inventory.Instance.GetItemCount(1) > 0;

        // 퍼즐 또는 약 먹이기 중 하나라도 가능하면 활성화
        bool canPickup = canPuzzle || canMedicine;

        // 강아지 근처에 있을 때만 UI 표시
        bool showUI = isPlayerNearby && canPickup;

        if (pickupItem.PickupState.activeSelf != showUI)
        {
            pickupItem.PickupState.SetActive(showUI);
        }
    }

    private void SuccessEvent()
    {
        if (successCount >= successEvents.Count)
            return;

        DogPuzzleSuccessData data = successEvents[successCount];

        if (textTyping != null && data.successDialogue != null)
        {
            textTyping.StartDialogue(data.successDialogue);
        }

        // 다음 퍼즐 상호작용 비활성화
        canInteract = false;

        // 스토리가 끝난 후 강아지 이동
        StartCoroutine(DelayTime(data));

        successCount++;
    }

    IEnumerator DelayTime(DogPuzzleSuccessData data)
    {
        // DarkUIEffect가 존재하고 스토리 진행 중이라면 대기
        yield return new WaitUntil(() =>
            darkUI == null || !darkUI.isStory
        );

        // 스토리가 끝나면 강아지 목표 생성
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
        // 이번 프레임에 아이템을 사용했다면 퍼즐 실행 금지
        if (InventoryUI.LastItemUseFrame == Time.frameCount)
            return;

        // 약을 사용할 수 있고 약을 선택한 상태라면
        // 퍼즐보다 약 사용을 우선하도록 처리해야 함

        if (!canInteract || petPuzzleManager == null)
            return;

        if (petPuzzleManager.currentState == PuzzleState.ResultWait)
        {
            bool success =
                petPuzzleManager.CurrentSelectedIndex >= 0 &&
                petPuzzleManager.CurrentSelectedIndex ==
                petPuzzleManager.correctOptionIndex;

            Debug.Log($"퍼즐 성공 여부: {success}");

            petPuzzleManager.Interact();

            if (success)
            {
                SuccessEvent();
            }

            resultHandled = false;
            isSuccess = false;

            return;
        }

        // 퍼즐 시작
        petPuzzleManager.Interact();
    }

    public void EnableInteraction()
    {
        canInteract = true;
        UpdateInteractionUI();
    }
}
