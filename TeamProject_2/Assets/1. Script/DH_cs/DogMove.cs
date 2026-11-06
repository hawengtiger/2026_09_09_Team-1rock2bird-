
using UnityEngine;
using System.Collections.Generic;

public class DogMove : MonoBehaviour
{
    public bool isMoving = false;
    public float moveSpeed = 5f;
    public List<Transform> reachPointsList = new List<Transform>();
    public float stopDistance = 1f;
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    [Header("자식 오브젝트 방향 전환")]
    public Transform pivo;

    private float pivoScaleX;

    void Start()
    {
        isMoving = false;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (pivo != null)
            pivoScaleX = Mathf.Abs(pivo.localScale.x);

        DisableCanvas();
    }

    void Update()
    {
        if (reachPointsList == null || reachPointsList.Count == 0)
        {
            isMoving = false;

            if (animator != null)
                animator.SetBool("move", false);

            return;
        }

        Transform targetPoint = reachPointsList[0];

        if (targetPoint == null)
        {
            reachPointsList.RemoveAt(0);
            return;
        }

        float dist = Vector3.Distance(transform.position, targetPoint.position);

        if (dist <= stopDistance)
        {
            reachPointsList.RemoveAt(0);
            Destroy(targetPoint.gameObject);
            return;
        }

        isMoving = true;

        if (animator != null)
            animator.SetBool("move", true);

        // 좌우 방향에 따라 스프라이트 플립
        float dirX = targetPoint.position.x - transform.position.x;

        if (spriteRenderer != null && Mathf.Abs(dirX) > 0.01f)
        {
            spriteRenderer.flipX = dirX < 0f;
        }

        // SpriteRenderer의 flipX 상태에 따라 Pivo 반전
        if (pivo != null && spriteRenderer != null)
        {
            Vector3 scale = pivo.localScale;

            scale.x = spriteRenderer.flipX ? -pivoScaleX : pivoScaleX;

            pivo.localScale = scale;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, moveSpeed * Time.deltaTime);
    }

    public void AddTargetPoint(Transform newtarget)
    {
        if (newtarget != null)
        {
            reachPointsList.Add(newtarget);
        }
    }

    private void DisableCanvas()
    {
        if (pivo == null) return;

        Canvas canvas = pivo.GetComponentInChildren<Canvas>(true);

        if (canvas != null)
        {
            canvas.gameObject.SetActive(false);
        }
    }
}
