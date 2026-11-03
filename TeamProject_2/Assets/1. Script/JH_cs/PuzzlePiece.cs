using UnityEngine;

// 드래그해서 절취선(정답 위치)에 맞추는 퍼즐 조각 (스프라이트용)
// SpriteRenderer + Collider2D가 있는 조각 오브젝트에 붙여서 사용
// 자식 오브젝트(글씨, 그림 등)도 같이 움직이고 같이 앞으로 보임
[RequireComponent(typeof(Collider2D))]
public class PuzzlePiece : MonoBehaviour
{
    [Header("정답 위치 (절취선)")]
    public Transform targetSlot;        // 이 조각이 들어가야 할 위치
    public float snapDistance = 50f;    // 화면 기준 이 픽셀 거리 안에 놓으면 정답 처리
    public bool returnIfWrong = true;   // 틀린 곳에 놓으면 원래 자리로 돌아갈지

    [HideInInspector] public bool isPlaced = false;

    private Vector3 startPos;
    private Renderer[] renderers;       // 자기 자신 + 자식 렌더러 (스프라이트, TMP 글씨 포함)
    private int[] originalOrders;

    void Awake()
    {
        startPos = transform.position;

        renderers = GetComponentsInChildren<Renderer>(true);
        originalOrders = new int[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            originalOrders[i] = renderers[i].sortingOrder;
    }

    // 겹친 조각 중 앞에 있는 것을 고르기 위한 값
    public int SortingOrder
    {
        get
        {
            int max = int.MinValue;
            foreach (Renderer r in renderers)
                if (r != null && r.sortingOrder > max) max = r.sortingOrder;
            return max == int.MinValue ? 0 : max;
        }
    }

    // 드래그 시작: 자식까지 같이 맨 앞으로 (서로의 앞뒤 순서는 유지)
    public void BeginDrag()
    {
        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i] != null) renderers[i].sortingOrder = originalOrders[i] + 100;
    }

    // 드래그 끝: 정답이면 true
    public bool EndDrag(Camera cam)
    {
        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i] != null) renderers[i].sortingOrder = originalOrders[i];

        if (targetSlot == null || cam == null) return false;

        // 화면(픽셀) 기준으로 거리 비교
        Vector2 a = cam.WorldToScreenPoint(transform.position);
        Vector2 b = cam.WorldToScreenPoint(targetSlot.position);

        if (Vector2.Distance(a, b) <= snapDistance)
        {
            transform.position = targetSlot.position;   // 절취선에 딱 맞춤
            isPlaced = true;
            return true;
        }

        if (returnIfWrong) transform.position = startPos;
        return false;
    }
}