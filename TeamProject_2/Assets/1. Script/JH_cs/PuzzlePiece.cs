using UnityEngine;

// 드래그해서 절취선(정답 위치)에 맞추는 퍼즐 조각 (스프라이트용)
// SpriteRenderer + Collider2D가 있는 조각 오브젝트에 붙여서 사용
// 실제 드래그 처리는 PerspectivePuzzle이 함
[RequireComponent(typeof(Collider2D))]
public class PuzzlePiece : MonoBehaviour
{
    [Header("정답 위치 (절취선)")]
    public Transform targetSlot;        // 이 조각이 들어가야 할 위치 (빈 오브젝트)
    public float snapDistance = 50f;    // 화면 기준 이 픽셀 거리 안에 놓으면 정답 처리
    public bool returnIfWrong = true;   // 틀린 곳에 놓으면 원래 자리로 돌아갈지

    [HideInInspector] public bool isPlaced = false;

    private Vector3 startPos;
    private SpriteRenderer sr;
    private int originalOrder;

    void Awake()
    {
        startPos = transform.position;
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalOrder = sr.sortingOrder;
    }

    public int SortingOrder => sr != null ? sr.sortingOrder : 0;

    // 드래그 시작: 맨 앞에 보이게
    public void BeginDrag()
    {
        if (sr != null) sr.sortingOrder = originalOrder + 100;
    }

    // 드래그 끝: 정답이면 true
    public bool EndDrag(Camera cam)
    {
        if (sr != null) sr.sortingOrder = originalOrder;
        if (targetSlot == null || cam == null) return false;

        // 화면(픽셀) 기준으로 거리 비교 → 크기/원근에 상관없이 일정하게 판정
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