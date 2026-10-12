using UnityEngine;

public class RoomPortal : MonoBehaviour
{
    public Transform PortalEnd;
    public string playerTag = "Player";
    public GameObject playerObject;
    public Vector3 arrivalOffset = new Vector3(2f, 0f, 0f);
    private float moveCooldown = 0.65f;

    private System.Collections.Generic.Dictionary<GameObject, float> lastTeleported = new System.Collections.Generic.Dictionary<GameObject, float>();

    void Start()
    {
        if (PortalEnd == null)
            Debug.LogWarning("RoomPortal: PortalEnd가 할당되지 않았습니다.");
    }

    void OnTriggerEnter2D(Collider2D other) => TryTeleport(other.gameObject);

    private void TryTeleport(GameObject collidedObj)
    {
        if (collidedObj == null || PortalEnd == null) return;

        GameObject targetObj = null;

        if (playerObject != null)
        {
            if (collidedObj == playerObject || collidedObj.transform.IsChildOf(playerObject.transform))
                targetObj = playerObject;
            else
                return;
        }
        else
        {
            if (!string.IsNullOrEmpty(playerTag) && collidedObj.CompareTag(playerTag))
            {
                targetObj = collidedObj;
            }
            else
            {
                // 태그가 없거나 일치하지 않으면 컴포넌트 기반으로 판단
                if (collidedObj.GetComponentInChildren<PlayerController>() != null || collidedObj.GetComponent<Rigidbody2D>() != null || collidedObj.GetComponent<Rigidbody>() != null)
                    targetObj = collidedObj;
                else
                    return;
            }
        }

        if (targetObj == null) return;

        // 목적지 설정 (PortalEnd 위치에 오프셋을 더함)
        Vector3 dest = PortalEnd.position + arrivalOffset;

        // Rigidbody2D/3D가 있으면 물리 위치와 속도를 초기화, 없으면 Transform으로 설정
        var rb2d = targetObj.GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            rb2d.position = new Vector2(dest.x, dest.y);
            rb2d.linearVelocity = Vector2.zero;
        }
        else
        {
            var rb3 = targetObj.GetComponent<Rigidbody>();
            if (rb3 != null)
            {
                rb3.position = dest;
                rb3.linearVelocity = Vector3.zero;
            }
            else
            {
                targetObj.transform.position = dest;
            }
        }

        lastTeleported[targetObj] = Time.time;

        // PlayerController가 있으면 이동 잠금 후 일정 시간 후 재허용
        var pc = targetObj.GetComponentInChildren<PlayerController>();
        if (pc != null)
        {
            pc.canMove = false;
            StartCoroutine(ReenableMoveAfter(pc, moveCooldown));
        }
    }

    private System.Collections.IEnumerator ReenableMoveAfter(PlayerController pc, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (pc != null) pc.canMove = true;
    }
}
