using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

// 2D 공간 맞추기 퍼즐
// 마우스 클릭 = 확인하기(확대) / 1~2 = 아이템 선택 / E = 올려두기 / 우클릭·ESC = 기본뷰로 복귀
public class PlacementPuzzle : MonoBehaviour
{
    [Header("카메라 (Orthographic)")]
    public Camera cam;                  // 비워두면 Main Camera 사용
    public float moveSpeed = 5f;        // 확대/복귀 속도

    [Header("퍼즐 칸")]
    public PlacementSpot[] spots;

    [Header("아이템 (1~2번)")]
    public string[] itemNames = { "아이템1", "아이템2" };

    [Header("퍼즐 UI (오브젝트 클릭 시 켜짐)")]
    public GameObject puzzleUI;         // 슬롯들이 들어있는 UI 부모 (시작 시 꺼짐)

    [Header("인벤토리 UI (슬롯 이미지 2개)")]
    public Image[] slots;                                        // 1번, 2번 슬롯 이미지
    public Color selectedColor = Color.yellow;                   // 선택 중인 슬롯 색
    public Color normalColor = Color.white;                      // 선택 안 된 슬롯 색
    public Color usedColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);  // 사용한 아이템 슬롯 색

    [Header("안내 문구 (선택)")]
    public TMP_Text promptText;

    private bool[] hasItem;
    private int selectedIndex = 0;
    private PlacementSpot hoveredSpot;
    private PlacementSpot currentSpot;  // 현재 확대 중인 칸
    private bool isSolved = false;

    private Vector3 homePos;
    private float homeSize;
    private Vector3 targetPos;
    private float targetSize;

    void Start()
    {
        if (cam == null) cam = Camera.main;

        homePos = targetPos = cam.transform.position;
        homeSize = targetSize = cam.orthographicSize;

        hasItem = new bool[itemNames.Length];
        for (int i = 0; i < hasItem.Length; i++) hasItem[i] = true;

        if (puzzleUI != null) puzzleUI.SetActive(false); // 기본 화면에서는 UI 숨김

        UpdateSlotUI();
        SetPrompt("");
    }

    void Update()
    {
        HandleItemSelect();

        if (currentSpot == null) HandleDefaultView();
        else HandleZoomView();

        // 카메라 부드럽게 이동/확대
        cam.transform.position = Vector3.Lerp(cam.transform.position, targetPos, Time.deltaTime * moveSpeed);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.deltaTime * moveSpeed);
    }

    // 1~2 키로 아이템 전환
    private void HandleItemSelect()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame) SelectItem(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) SelectItem(1);
    }

    private void SelectItem(int index)
    {
        if (index >= hasItem.Length) return;
        selectedIndex = index;
        UpdateSlotUI();
    }

    // 기본뷰: 마우스 올리면 하이라이트, 클릭하면 확대(확인하기)
    private void HandleDefaultView()
    {
        PlacementSpot spot = GetSpotUnderMouse();

        if (spot != hoveredSpot)
        {
            if (hoveredSpot != null) hoveredSpot.SetHighlight(false);
            hoveredSpot = spot;
            if (hoveredSpot != null) hoveredSpot.SetHighlight(true);
        }

        SetPrompt(hoveredSpot != null ? "클릭: 확인하기" : "");

        if (hoveredSpot != null && Mouse.current.leftButton.wasPressedThisFrame)
            ZoomIn(hoveredSpot);
    }

    // 확대뷰: E로 올려두기, 우클릭/ESC로 복귀
    private void HandleZoomView()
    {
        SetPrompt($"E: {itemNames[selectedIndex]} 올려두기   |   우클릭: 돌아가기");

        if (Keyboard.current.eKey.wasPressedThisFrame)
            TryPlace();

        if (Mouse.current.rightButton.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
            ZoomOut();
    }

    // 2D: 마우스 위치에 있는 Collider2D 찾기
    private PlacementSpot GetSpotUnderMouse()
    {
        if (Mouse.current == null) return null;

        Vector2 worldPos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Collider2D hit = Physics2D.OverlapPoint(worldPos);

        if (hit != null)
        {
            PlacementSpot spot = hit.GetComponentInParent<PlacementSpot>();
            if (spot != null && !spot.isFilled) return spot;
        }
        return null;
    }

    private void ZoomIn(PlacementSpot spot)
    {
        currentSpot = spot;
        spot.SetHighlight(false);
        hoveredSpot = null;

        Vector3 p = spot.GetZoomPosition();
        targetPos = new Vector3(p.x, p.y, homePos.z); // 2D라서 Z는 유지
        targetSize = spot.zoomSize;

        if (puzzleUI != null) puzzleUI.SetActive(true);  // 오브젝트 클릭 → UI 켜기
        UpdateSlotUI();
    }

    private void ZoomOut()
    {
        currentSpot = null;
        targetPos = homePos;
        targetSize = homeSize;

        if (puzzleUI != null) puzzleUI.SetActive(false); // 기본 화면으로 → UI 끄기
    }

    private void TryPlace()
    {
        if (!hasItem[selectedIndex])
        {
            SetPromptTemp("이미 사용한 아이템입니다.");
            return;
        }

        if (selectedIndex != currentSpot.requiredItemIndex)
        {
            SetPromptTemp($"{itemNames[selectedIndex]}은(는) 여기에 맞지 않는 것 같다.");
            return;
        }

        // 배치 성공 → 인벤토리에서 제거
        currentSpot.Place();
        hasItem[selectedIndex] = false;
        UpdateSlotUI();

        ZoomOut();
        CheckSolved();
    }

    private void CheckSolved()
    {
        // Spots 칸이 비어 있으면 씬에서 자동으로 찾기
        if (spots == null || spots.Length == 0)
            spots = FindObjectsByType<PlacementSpot>(FindObjectsSortMode.None);

        foreach (PlacementSpot spot in spots)
        {
            if (spot == null) continue;   // 빈 칸은 건너뜀
            if (!spot.isFilled) return;
        }

        isSolved = true;
        Debug.Log("공간 맞추기 퍼즐 완성!");
    }

    private void UpdateSlotUI()
    {
        if (slots == null) return;

        for (int i = 0; i < slots.Length && i < hasItem.Length; i++)
        {
            if (slots[i] == null) continue;

            if (!hasItem[i]) slots[i].color = usedColor;            // 사용한 아이템
            else if (i == selectedIndex) slots[i].color = selectedColor; // 선택 중
            else slots[i].color = normalColor;                      // 선택 안 됨
        }
    }

    private void SetPrompt(string text)
    {
        if (promptText != null) promptText.text = text;
    }

    private void SetPromptTemp(string text)
    {
        Debug.Log(text);
        if (promptText != null) promptText.text = text;
    }

    public bool IsSolved => isSolved;
}