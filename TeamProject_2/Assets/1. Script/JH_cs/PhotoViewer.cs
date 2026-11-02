using UnityEngine;
using UnityEngine.UI;

// 왼쪽 위 작은 사진을 클릭하면 화면 전체로 확대,
// 사진 바깥(배경)을 클릭하면 다시 원래대로 돌아오는 스크립트
public class PhotoViewer : MonoBehaviour
{
    public static bool IsOpen { get; private set; }   // 사진을 크게 보고 있는지 (퍼즐 조작 막기용)

    [Header("작은 사진 (왼쪽 위)")]
    public Button thumbnailButton;      // 클릭하면 확대되는 작은 사진

    [Header("크게 보기")]
    public GameObject overlay;          // 화면 전체를 덮는 배경 (시작 시 꺼짐)
    public Button backgroundButton;     // overlay 배경 → 클릭하면 닫힘
    public RectTransform bigPhoto;      // 크게 보일 사진

    [Header("연출")]
    public float zoomSpeed = 10f;       // 확대/축소 속도
    public float startScale = 0.2f;     // 확대 시작 크기

    private Vector3 targetScale;
    private bool closing = false;

    void Start()
    {
        if (overlay != null) overlay.SetActive(false);
        IsOpen = false;

        if (thumbnailButton != null) thumbnailButton.onClick.AddListener(Open);
        if (backgroundButton != null) backgroundButton.onClick.AddListener(Close);
    }

    public void Open()
    {
        overlay.SetActive(true);
        IsOpen = true;
        closing = false;

        bigPhoto.localScale = Vector3.one * startScale;
        targetScale = Vector3.one;
    }

    public void Close()
    {
        closing = true;
        targetScale = Vector3.one * startScale;
    }

    void Update()
    {
        if (overlay == null || !overlay.activeSelf) return;

        bigPhoto.localScale = Vector3.Lerp(bigPhoto.localScale, targetScale, Time.unscaledDeltaTime * zoomSpeed);

        // 닫는 중이고 충분히 작아지면 완전히 끄기
        if (closing && bigPhoto.localScale.x <= startScale + 0.05f)
        {
            overlay.SetActive(false);
            IsOpen = false;
            closing = false;
        }
    }
}