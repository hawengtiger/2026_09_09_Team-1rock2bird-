using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public bool canMove = true;

    [Header("Pickup")]
    [Tooltip("아이템 검색 반경")]
    public float pickupRadius = 1.5f;
    [Tooltip("아이템 태그 (검색 시 이 태그를 가진 콜라이더만 검사)")]
    public string itemTag = "Item";
    [Tooltip("Gizmo로 반경 표시")]
    public bool showPickupGizmo = true;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    // 현재 범위 내에서 사용 가능한 가장 가까운 PickupItem(없으면 null)
    private PickupItem nearestPickup;
    private Interction_obj nearestInterction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // 이동 입력
        moveInput.x = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        {
            moveInput.x = -1f;
        }
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        {
            moveInput.x = 1f;
        }

        // 캐릭터 방향 전환
        if (moveInput.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (moveInput.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }

        // 이동 가능 여부에 따라 속도 조절
        if (!canMove)
        {
            moveInput.x = 0f;
        }

        // 주변 아이템 검색 (매 프레임 최신화)

        // F 키 입력으로 수집 시도
        if (Keyboard.current.fKey.wasPressedThisFrame)
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
    }

    private void FixedUpdate() // 외부에서 이동 입력을 적용할 때 사용, canMove이 false면 입력을 무시합니다.
    {
        // Rigidbody2D를 이용한 이동
        rb.linearVelocity = new Vector2(
            moveInput.x * moveSpeed,
            rb.linearVelocity.y
        );
    }
    
    public void ApplyMove(float x) // 반경 내의 itemTag태그, PickupItem 컴포넌트, canPickup == true인 가장 가까운 아이템을 반환
    {
        if (!canMove)
        {
            // 이동이 잠겨있으면 입력 초기화
            moveInput.x = 0f;
            return;
        }

        moveInput.x = x;
    }

    
    private PickupItem FindNearestPickup()// 반경 내에서 태그가 itemTag이고, PickupItem 컴포넌트가 있으며, canPickup == true 인 가장 가까운 아이템을 반환
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, pickupRadius);
        PickupItem best = null;
        float bestSqr = float.MaxValue;

        foreach (var c in cols)
        {
            if (c == null) continue;
            if (!string.IsNullOrEmpty(itemTag) && !c.CompareTag(itemTag)) continue;

            // PickupItem은 콜라이더에 붙어있을 수도, 부모 오브젝트에 있을 수도 있으므로 GetComponentInParent 사용
            var item = c.GetComponentInParent<PickupItem>();
            if (item == null) continue;

            float dsq = (item.transform.position - transform.position).sqrMagnitude;
            if (dsq < bestSqr)
            {
                bestSqr = dsq;
                best = item;
            }
        }

        return best;
    }
    
    private Interction_obj FindNearestInteraction() // 반경 내에서 태그가 Object이고, Interction_obj 컴포넌트가 있는 가장 가까운 오브젝트를 반환
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, pickupRadius);
        Interction_obj best = null;
        float bestSqr = float.MaxValue;

        foreach (var c in cols)
        {
            if (c == null) continue;
            // 태그가 "Object"인 것만 검사
            if (!c.CompareTag("Object")) continue;

            // Interction_obj은 콜라이더에 붙어있을 수도, 부모 오브젝트에 있을 수도 있으므로 GetComponentInParent 사용
            var interaction = c.GetComponentInParent<Interction_obj>();
            if (interaction == null) continue;

            float dsq = (interaction.transform.position - transform.position).sqrMagnitude;
            if (dsq < bestSqr)
            {
                bestSqr = dsq;
                best = interaction;
            }
        }

        return best;
    }

    private void OnDrawGizmosSelected() // 씬 뷰용 Gizmo (선택)
    {
        if (!showPickupGizmo) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
