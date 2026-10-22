using UnityEngine;
using UnityEngine.AdaptivePerformance;

public class DogMove : MonoBehaviour
{
    public bool isMoving = false;
    public float moveSpeed = 5f;
    public Transform ReachPoint;
    public float stopDistance = 1f;
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    void Start()
    {
        isMoving = false;
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (ReachPoint == null) return;

        float dist = Vector3.Distance(transform.position, ReachPoint.position);

        if (dist <= stopDistance)
        {
            isMoving = false;
            if (animator != null) animator.SetBool("move", false);
            return;
        }

        isMoving = true;
        if (animator != null) animator.SetBool("move", true);

        // 좌우 방향에 따라 스프라이트 플립
        float dirX = ReachPoint.position.x - transform.position.x;
        if (spriteRenderer != null && Mathf.Abs(dirX) > 0.01f)
        {
            spriteRenderer.flipX = dirX < 0f;
        }

        transform.position = Vector3.MoveTowards(transform.position, ReachPoint.position, moveSpeed * Time.deltaTime);
    }

}
