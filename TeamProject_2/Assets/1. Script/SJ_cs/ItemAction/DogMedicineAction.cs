
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DogMedicineAction : MonoBehaviour
{
    public static DogMedicineAction Current;

    [Header("영롱이 상호작용")]
    public PickupItem pickupItem;

    [Header("영롱이 이동")]
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

    [Header("영롱이를 업은 스프라이트")]
    public Sprite carryingSprite;

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

    private Sprite originalSprite;
    private bool originalAnimatorEnabled;
    private bool originalMovementEnabled;

    private void Awake()
    {
        Current = this;

        triggers.SetActive(false);
    }

    private void Start()
    {
        textTyping = FindAnyObjectByType<TextTyping>(
            FindObjectsInactive.Include
        );

        if (darkUI == null)
            darkUI = FindAnyObjectByType<DarkUIEffect>();
    }

    private void Update()
    {
        if (!isWaitingForCarry || isCompleted)
            return;

        if (player == null)
            return;

        bool nearSofa = Vector2.Distance(
            player.position,
            (Vector2)sofaPosition
        ) <= 1.5f;

        if (nearSofa &&
            Keyboard.current != null &&
            Keyboard.current.fKey.wasPressedThisFrame)
        {
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
            textTyping.StartDialogue(medicineDialogue);

        StartCoroutine(MoveDogAfterDialogue());
    }

    private IEnumerator MoveDogAfterDialogue()
    {
        yield return null;

        if (darkUI != null)
            yield return new WaitUntil(() => !darkUI.isStory);

        if (dogPosMove == null || dog == null)
            yield break;

        dogPosMove.SpawnTarget(
            sofaPosition.x,
            sofaPosition.y
        );

        yield return new WaitUntil(() =>
            dog == null ||
            Vector2.Distance(
                dog.position,
                (Vector2)sofaPosition
            ) <= arrivalDistance
        );

        if (dog == null)
            yield break;

        isWaitingForCarry = true;

        // 기존 PickupItem의 UI 활성화
        if (pickupItem != null && pickupItem.PickupState != null)
        {
            pickupItem.PickupState.SetActive(true);
        }

        Debug.Log("영롱이가 소파에 도착했습니다. F키를 눌러주세요.");
    }

    // 영롱이를 업는 연출
    private void CarryDog()
    {
        if (!isWaitingForCarry || isCarrying)
            return;

        if (playerAnimator == null)
            return;

        isWaitingForCarry = false;
        isCarrying = true;

        // 기존 PickupItem의 UI 비활성화
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
        // 약 사용 후 2초 대기
        yield return new WaitForSeconds(medicineDuration);

        // 플레이어 이동 허용
        if (playerMovement != null)
        {
            playerMovement.enabled = originalMovementEnabled;
        }

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
