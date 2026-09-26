using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public bool canMove = true;

    private Rigidbody2D rb;
    private Vector2 moveInput;

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
    }

    private void FixedUpdate()
    {
        // Rigidbody2D를 이용한 이동
        rb.linearVelocity = new Vector2(
            moveInput.x * moveSpeed,
            rb.linearVelocity.y
        );
    }
    // 외부에서 이동 입력을 적용할 때 사용
    // canMove이 false면 입력을 무시합니다.
    public void ApplyMove(float x)
    {
        if (!canMove)
        {
            // 이동이 잠겨있으면 입력 초기화
            moveInput.x = 0f;
            return;
        }

        moveInput.x = x;
    }
}
