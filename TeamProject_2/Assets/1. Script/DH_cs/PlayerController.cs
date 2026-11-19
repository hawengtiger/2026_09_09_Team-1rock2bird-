
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("애니메이션")]
    public Animator animator;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public bool canMove = true;

    [Header("Pickup")]
    [Tooltip("아이템 검색 반경")]
    public float pickupRadius = 1.5f;

    [Tooltip("아이템 태그")]
    public string itemTag = "Item";

    [Tooltip("Gizmo로 반경 표시")]
    public bool showPickupGizmo = true;

    [Header("달리기")]
    public float sprintMultiplier = 2f;

    private bool isRunning = false;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private PickupItem nearestPickup;
    private Interction_obj nearestInterction;
    private PetPuzzleManager pet;

    private TextTyping typing;

    // 자동 이동
    private bool isAutoMoving = false;
    private Vector2 autoMoveInput = Vector2.zero;

    public bool IsAutoMoving => isAutoMoving;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        typing = FindAnyObjectByType<TextTyping>();
        pet = FindAnyObjectByType<PetPuzzleManager>();
    }

    private void Update()
    {
        // 이동 입력 초기화
        moveInput = Vector2.zero;

        isRunning =
            Keyboard.current != null &&
            Keyboard.current.leftShiftKey.isPressed;

        // 자동 이동 중에는 입력을 강제로 적용
        if (isAutoMoving)
        {
            moveInput = autoMoveInput;

            // 자동 이동 중 달리기 금지
            isRunning = false;
        }
        else
        {
            // 기존 플레이어 수동 이동
            bool canInput =
                canMove &&
                Keyboard.current != null &&
                (pet == null ||
                 pet.currentState == PuzzleState.WaitToEnter) &&
                (typing == null ||
                 typing.darkUIEffect == null ||
                 !typing.darkUIEffect.isStory) &&
                (FadeManager.Instance == null ||
                 FadeManager.Instance.isMove);

            if (canInput)
            {
                if (Keyboard.current.aKey.isPressed ||
                    Keyboard.current.leftArrowKey.isPressed)
                {
                    moveInput.x = -1f;
                }

                if (Keyboard.current.dKey.isPressed ||
                    Keyboard.current.rightArrowKey.isPressed)
                {
                    moveInput.x = 1f;
                }
            }
        }

        // 캐릭터 방향 변경
        if (moveInput.x > 0f)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (moveInput.x < 0f)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }

        // F키 상호작용
        // 자동 이동 중에는 상호작용 금지
        if (!isAutoMoving &&
            Keyboard.current != null &&
            Keyboard.current.fKey.wasPressedThisFrame)
        {
            Debug.Log("아이템 탐색 / 상호작용 키 입력");

            nearestPickup = FindNearestPickup();

            if (nearestPickup != null)
            {
                nearestPickup.Collect();
            }

            nearestInterction = FindNearestInteraction();

            if (nearestInterction != null)
            {
                nearestInterction.Interact();
            }
        }

        PlayerAnimation();
    }

    private void FixedUpdate()
    {
        // Shift를 누르면 이동 속도 2배
        float currentSpeed = isRunning
            ? moveSpeed * sprintMultiplier
            : moveSpeed;

        rb.linearVelocity = new Vector2(
            moveInput.x * currentSpeed,
            rb.linearVelocity.y
        );

        PlayerAnimation();
    }

    // 자동 이동 입력 시작 및 변경
    public void SetAutoMove(Vector2 direction)
    {
        isAutoMoving = true;

        autoMoveInput = direction;

        // 자동 이동은 걷기 속도
        isRunning = false;

        // 입력을 즉시 반영
        moveInput = autoMoveInput;
    }

    // 자동 이동 종료
    public void StopAutoMove()
    {
        isAutoMoving = false;
        autoMoveInput = Vector2.zero;
        moveInput = Vector2.zero;
        isRunning = false;

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(
                0f,
                rb.linearVelocity.y
            );
        }

        PlayerAnimation();
    }

    // 애니메이션
    private void PlayerAnimation()
    {
        if (animator == null || rb == null)
            return;

        float speed = 0f;

        if (Mathf.Abs(rb.linearVelocity.x) > 0.01f)
        {
            speed = isRunning ? 1f : 0.5f;
        }

        animator.SetFloat("Speed", speed);
    }

    // 외부 이동 입력
    public void ApplyMove(float x)
    {
        if (!canMove || isAutoMoving)
        {
            return;
        }

        moveInput.x = x;
    }

    // 가장 가까운 아이템 검색
    private PickupItem FindNearestPickup()
    {
        Collider2D[] cols =
            Physics2D.OverlapCircleAll(
                transform.position,
                pickupRadius
            );

        PickupItem best = null;
        float bestSqr = float.MaxValue;

        foreach (var c in cols)
        {
            if (c == null)
                continue;

            if (!string.IsNullOrEmpty(itemTag) &&
                !c.CompareTag(itemTag))
                continue;

            var item = c.GetComponentInParent<PickupItem>();

            if (item == null)
                continue;

            float dsq =
                (item.transform.position -
                 transform.position).sqrMagnitude;

            if (dsq < bestSqr)
            {
                bestSqr = dsq;
                best = item;
            }
        }

        return best;
    }

    // 가장 가까운 상호작용 오브젝트 검색
    private Interction_obj FindNearestInteraction()
    {
        Collider2D[] cols =
            Physics2D.OverlapCircleAll(
                transform.position,
                pickupRadius
            );

        Interction_obj best = null;
        float bestSqr = float.MaxValue;

        foreach (var c in cols)
        {
            if (c == null)
                continue;

            var interaction =
                c.GetComponentInParent<Interction_obj>();

            if (interaction == null)
                continue;

            float dsq =
                (interaction.transform.position -
                 transform.position).sqrMagnitude;

            if (dsq < bestSqr)
            {
                bestSqr = dsq;
                best = interaction;
            }
        }

        return best;
    }

    // Gizmo 표시
    private void OnDrawGizmosSelected()
    {
        if (!showPickupGizmo)
            return;

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            pickupRadius
        );
    }

    private void OnDisable()
    {
        if (rb != null && rb.simulated)
            StopAutoMove();
    }
}
