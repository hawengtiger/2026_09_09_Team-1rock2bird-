
using UnityEngine;

// Canvas UI Image를 드래그하는 퍼즐 조각
[RequireComponent(typeof(RectTransform))]
public class PuzzlePieceImage : MonoBehaviour
{
    [Header("정답 위치")]
    public RectTransform targetSlot;      // 조각의 정답 위치
    public float snapDistance = 50f;      // 정답 허용 거리 (픽셀)
    public bool returnIfWrong = true;     // 틀리면 원래 위치로 복귀

    [HideInInspector]
    public bool isPlaced = false;         // 정답 여부

    private RectTransform rectTransform;
    private Vector3 startPosition;
    private int originalSiblingIndex;

    private void Awake()
    {
        // UI 위치를 관리하는 RectTransform
        rectTransform = GetComponent<RectTransform>();

        // 원래 위치 저장
        startPosition = rectTransform.position;

        // 원래 UI 표시 순서 저장
        originalSiblingIndex = transform.GetSiblingIndex();
    }

    // UI 표시 순서
    public int SortingOrder => transform.GetSiblingIndex();

    // 드래그 시작
    public void BeginDrag()
    {
        // 선택한 조각을 맨 앞으로 표시
        transform.SetAsLastSibling();
    }

    // 드래그 종료 및 정답 판정
    public bool EndDrag(Camera cam)
    {
        // 원래 UI 표시 순서 복구
        transform.SetSiblingIndex(originalSiblingIndex);

        if (targetSlot == null)
            return false;

        // 조각의 화면 좌표
        Vector2 piecePos = RectTransformUtility.WorldToScreenPoint(cam, rectTransform.position);

        // 정답 위치의 화면 좌표
        Vector2 targetPos = RectTransformUtility.WorldToScreenPoint(cam, targetSlot.position);

        // 두 위치 사이의 거리
        float distance = Vector2.Distance(piecePos, targetPos);

        // 정답 허용 거리 이내라면
        if (distance <= snapDistance)
        {
            rectTransform.position = targetSlot.position;
            isPlaced = true;

            return true;
        }

        // 오답이면 원래 위치로 복귀
        if (returnIfWrong)
        {
            rectTransform.position = startPosition;
        }

        return false;
    }
}
