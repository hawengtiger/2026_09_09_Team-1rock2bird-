using UnityEngine;

// 아이템을 올려둘 수 있는 "공간" 하나에 붙이는 스크립트 (2D)
// 반드시 Collider2D(Box Collider 2D 등)가 있어야 마우스로 선택할 수 있음
public class PlacementSpot : MonoBehaviour
{
    [Header("정답 설정")]
    public int requiredItemIndex = 0;   // 이 칸의 정답 아이템 (0 = 1번 키, 1 = 2번 키)

    [Header("확대 설정")]
    public Transform zoomPoint;         // 확대 시 카메라 중심이 될 위치 (비워두면 이 오브젝트 위치)
    public float zoomSize = 2f;         // 확대 시 카메라 Size (작을수록 크게 보임)

    [Header("배치 연출")]
    public GameObject placedObject;     // 올려두기 성공 시 나타날 오브젝트 (시작 시 자동으로 꺼짐)

    [Header("마우스 올렸을 때 하이라이트")]
    public SpriteRenderer highlightRenderer; // 비워두면 자기 자신의 SpriteRenderer 사용
    public Color highlightColor = Color.yellow;

    [HideInInspector] public bool isFilled = false;

    private Color originalColor;

    void Awake()
    {
        if (highlightRenderer == null) highlightRenderer = GetComponent<SpriteRenderer>();
        if (highlightRenderer != null) originalColor = highlightRenderer.color;
        if (placedObject != null) placedObject.SetActive(false);
    }

    public Vector3 GetZoomPosition()
    {
        return zoomPoint != null ? zoomPoint.position : transform.position;
    }

    public void SetHighlight(bool on)
    {
        if (highlightRenderer == null) return;
        highlightRenderer.color = on ? highlightColor : originalColor;
    }

    public void Place()
    {
        isFilled = true;
        if (placedObject != null) placedObject.SetActive(true);
        SetHighlight(false);
    }
}