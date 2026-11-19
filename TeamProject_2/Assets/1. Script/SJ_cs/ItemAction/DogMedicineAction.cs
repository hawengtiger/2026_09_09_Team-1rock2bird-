
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DogMedicineAction : MonoBehaviour
{
    public static DogMedicineAction Current;

    [Header("영롱이 상호작용")]
    public PickupItem pickupItem;

    [Header("산책 트리거")]
    public GameObject triggers;

    [Header("영롱이 대화")]
    public DialogueDataSO medicineDialogue;
    public DarkUIEffect darkUI;

    [Header("영롱이 스프라이트")]
    public SpriteRenderer dogSpriteRenderer;

    [Header("영롱이 이동")]
    public DogPosMove dogPosMove;
    public Transform dog;
    public Vector2Int sofaPosition = new Vector2Int(1, 0);
    public float arrivalDistance = 0.2f;

    [Header("플레이어")]
    public Transform player;
    public MonoBehaviour playerMovement;
    public SpriteRenderer playerSpriteRenderer;
    public Animator playerAnimator;

    [Header("소파 상호작용 거리")]
    public float carryInteractionDistance = 1.5f;

    [Header("약 사용 후 연출 시간")]
    public float medicineDuration = 2f;

    [Header("산책하러가기 위한 다이얼로그")]
    public DialogueDataSO walkdialogue;

    private TextTyping textTyping;

    private bool isPlayerNearby = false;
    private bool isStoryStarted = false;
    private bool isWaitingForCarry = false;
    private bool isCarrying = false;
    private bool isCompleted = false;

    private bool originalMovementEnabled;

    private void Awake()
    {
        Current = this;

        if (triggers != null)
            triggers.SetActive(false);
    }

    private void Start()
    {
        textTyping = FindAnyObjectByType<TextTyping>(
            FindObjectsInactive.Include
        );

        if (darkUI == null)
            darkUI = FindAnyObjectByType<DarkUIEffect>();

        if (pickupItem == null)
            pickupItem = GetComponent<PickupItem>();
    }

    private void Update()
    {
        if (!CanCarryDog())
            return;

        if (Keyboard.current != null &&
            Keyboard.current.fKey.wasPressedThisFrame)
        {
            // 같은 프레임에 아이템을 사용했다면 업기 금지
            if (InventoryUI.LastItemUseFrame == Time.frameCount)
                return;

            CarryDog();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerNearby = false;
    }

    // 약을 가지고 영롱이와 대화할 수 있는가?
    public bool CanTalkToDog()
    {
        return isPlayerNearby &&
               !isStoryStarted &&
               Inventory.Instance != null &&
               Inventory.Instance.GetItemCount(1) > 0;
    }

    // 소파에서 영롱이를 업을 수 있는가?
    public bool CanCarryDog()
    {
        return isWaitingForCarry &&
               !isCarrying &&
               !isCompleted &&
               player != null &&
               Vector2.Distance(
                   player.position,
                   (Vector2)sofaPosition
               ) <= carryInteractionDistance;
    }

    // 약 사용 가능 여부: 업은 상태에서만
    public bool CanUseMedicine()
    {
        return isCarrying && !isCompleted;
    }

    // 강아지와 F키 상호작용 시 호출
    public void TalkToDog()
    {
        if (!CanTalkToDog())
            return;

        isStoryStarted = true;

        if (textTyping == null)
        {
            textTyping = FindAnyObjectByType<TextTyping>(
                FindObjectsInactive.Include
            );
        }

        if (textTyping != null && medicineDialogue != null)
        {
            textTyping.StartDialogue(medicineDialogue);
        }

        StartCoroutine(MoveDogAfterDialogue());
    }

    private IEnumerator MoveDogAfterDialogue()
    {
        yield return null;

        // 대화가 끝날 때까지 대기
        if (darkUI != null)
        {
            yield return new WaitUntil(() => !darkUI.isStory);
        }

        if (dogPosMove == null || dog == null)
        {
            Debug.LogWarning("DogPosMove 또는 Dog가 없습니다.");
            yield break;
        }

        // 영롱이 소파 이동
        dogPosMove.SpawnTarget(
            sofaPosition.x,
            sofaPosition.y
        );

        Debug.Log("영롱이가 소파로 이동합니다.");

        // DogMove의 이동 목표가 모두 처리될 때까지 대기
        if (dogPosMove.dogScript != null)
        {
            yield return new WaitUntil(() =>
                dogPosMove.dogScript.reachPointsList.Count == 0
            );
        }
        else
        {
            // DogMove 참조가 없을 경우 거리로 확인
            yield return new WaitUntil(() =>
                dog == null ||
                Vector2.Distance(
                    dog.position,
                    (Vector2)sofaPosition
                ) <= arrivalDistance
            );
        }

        if (dog == null)
            yield break;

        // 소파 도착! F키 상호작용 가능
        isWaitingForCarry = true;

        Debug.Log("영롱이가 소파에 도착했습니다. F키를 눌러주세요.");

        // PickupState는 DogFacePuzzleAction에서 갱신
    }

    // 영롱이를 업는 연출
    private void CarryDog()
    {
        if (!CanCarryDog())
            return;

        if (playerAnimator == null)
        {
            Debug.LogWarning("Player Animator가 없습니다.");
            return;
        }

        isWaitingForCarry = false;
        isCarrying = true;

        // UI 즉시 숨기기
        if (pickupItem != null && pickupItem.PickupState != null)
        {
            pickupItem.PickupState.SetActive(false);
        }

        // 플레이어 이동 비활성화
        if (playerMovement != null)
        {
            originalMovementEnabled = playerMovement.enabled;
            playerMovement.enabled = false;
        }

        // 영롱이를 업은 애니메이션으로 전환
        playerAnimator.SetBool("isHug", true);

        // 영롱이 모습 숨기기
        if (dogSpriteRenderer != null)
        {
            dogSpriteRenderer.enabled = false;
        }

        Debug.Log("영롱이를 업었습니다. 1번 선택 후 F키로 약을 사용하세요.");
    }

    // 인벤토리 onUse에서 호출
    public void UseMedicine()
    {
        if (!CanUseMedicine())
            return;

        isCompleted = true;

        Debug.Log("영롱이에게 약을 먹였습니다.");

        StartCoroutine(FinishMedicineSequence());
    }

    private IEnumerator FinishMedicineSequence()
    {
        // 약을 먹인 후 2초 대기
        yield return new WaitForSeconds(medicineDuration);

        // 플레이어 이동 다시 허용
        if (playerMovement != null)
        {
            playerMovement.enabled = originalMovementEnabled;
        }

        // isHug는 true 유지
        // 영롱이를 업은 애니메이션 그대로 이동 가능

        Debug.Log("약 먹이기 완료! 영롱이를 업은 상태로 이동 가능");

        if (textTyping != null && walkdialogue != null)
        {
            textTyping.StartDialogue(walkdialogue);
        }

        if (triggers != null)
        {
            triggers.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        if (Current == this)
            Current = null;
    }
}
