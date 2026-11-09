using UnityEngine;

public class PlayerWalkChange : MonoBehaviour
{
    [Header("플레이어 애니메이터")]
    public Animator playerAnimator;

    [Header("변경할 애니메이션 파라미터")]
    public bool isHug = false;
    public bool isWalk = true;

    public void ChangeWalk()
    {
        if (playerAnimator == null)
            return;

        playerAnimator.SetBool("isHug", isHug);
        playerAnimator.SetBool("isWalk", isWalk);
    }

    // 콜라이더 접촉
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (CompareTag("Walk"))
        {
            ChangeWalk();
        }
    }
}
