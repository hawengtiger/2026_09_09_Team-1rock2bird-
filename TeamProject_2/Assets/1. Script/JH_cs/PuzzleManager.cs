using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PuzzleManager : MonoBehaviour
{
    [Header("참조 설정")]
    public PlayerController playerController; // 플레이어 이동 잠금을 위한 참조
    public GameObject puzzleUI;               // 하단 스크롤 바가 포함된 퍼즐 전용 UI 캔버스
    public Slider rotationSlider;             // 회전 조절용 하단 스크롤 바[cite: 1]
    
    public GameObject puzzleGroup;

    [Header("퍼즐 조각들")]
    public PuzzlePiece[] allPieces;           // 씬에 배치된 퍼즐 조각들 배열

    private bool isPuzzleActive = false;
    private PuzzlePiece selectedPiece = null;
    private bool isDragging = false;
    private Camera mainCamera;
    private Vector2 dragOffset;

    void Start()
    {
        mainCamera = Camera.main;
        puzzleUI.SetActive(false); // 처음에는 퍼즐 UI 비활성화
        
        if (rotationSlider != null)
        {
            // 하단 스크롤 바 값이 변경될 때마다 OnSliderValueChanged 함수 호출[cite: 1]
            rotationSlider.onValueChanged.AddListener(OnSliderValueChanged);
            rotationSlider.minValue = -180f; // 슬라이더 최소/최대값 설정 (원하는 회전 범위로 수정 가능)
            rotationSlider.maxValue = 180f;
        }
    }

    void Update()
    {
        // F키를 누르면 퍼즐 모드 진입/종료 전환
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            TogglePuzzleMode();
        }

        if (!isPuzzleActive) return;

        HandleMouseInput();
    }

    void TogglePuzzleMode()
    {
        isPuzzleActive = !isPuzzleActive;
        
        // UI 활성화 및 플레이어 이동 제어 연동
        puzzleUI.SetActive(isPuzzleActive);
        if (playerController != null)
        {
            playerController.canMove = !isPuzzleActive; // 퍼즐 중이면 플레이어 이동 불가
        }

        if (puzzleGroup != null)
        {
            puzzleGroup.SetActive(isPuzzleActive);
        }   



        // 퍼즐을 종료할 때 선택된 조각 초기화
        if (!isPuzzleActive)
        {
            selectedPiece = null;
        }
    }

    void HandleMouseInput()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouse.position.ReadValue());

        // 1. 마우스/터치 클릭
        if (mouse.leftButton.wasPressedThisFrame)
        {
            // 마우스가 UI(슬라이더 바) 위에 있다면 퍼즐 조각 선택 무시
            if (EventSystem.current.IsPointerOverGameObject()) 
                return;

            SelectPiece(mouseWorldPos);
            
            if (selectedPiece != null) 
            {
                isDragging = true; // 조각이 선택되었을 때만 드래그 시작
            }
        }
        
        // 2. 마우스/터치 드래그 중 (이동)
        if (mouse.leftButton.isPressed && isDragging && selectedPiece != null && !selectedPiece.IsSnapped)
        {
            selectedPiece.transform.position = mouseWorldPos + dragOffset;
        }

        // 3. 마우스/터치 뗌 (스냅 검증)
        if (mouse.leftButton.wasReleasedThisFrame)
        {
            if (isDragging && selectedPiece != null)
            {
                selectedPiece.CheckSnap();
                CheckPuzzleCleared();
            }
            
          
                      isDragging = false; 
        }
    }

    void SelectPiece(Vector2 mousePos)
    {
        // 클릭한 위치에 있는 2D 콜라이더 감지
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
        if (hit.collider != null)
        {
            PuzzlePiece piece = hit.collider.GetComponent<PuzzlePiece>();
            
            // 정답을 맞추지 않은 조각만 선택 가능
            if (piece != null && !piece.IsSnapped)
            {
                selectedPiece = piece;
                dragOffset = (Vector2)selectedPiece.transform.position - mousePos; // 자연스러운 드래그를 위한 오프셋

                // 선택한 조각의 현재 회전값을 하단 스크롤 바에 동기화
                if (rotationSlider != null)
                {
                    float currentZ = selectedPiece.transform.eulerAngles.z;
                    if (currentZ > 180f) currentZ -= 360f; // -180 ~ 180 범위로 정규화
                    rotationSlider.SetValueWithoutNotify(currentZ);
                }
            }
        }
    }

    // 하단 스크롤 바 조절 시 호출되는 함수[cite: 1]
    public void OnSliderValueChanged(float value)
    {
        if (selectedPiece != null)
        {
            selectedPiece.SetRotation(value);
            CheckPuzzleCleared();
        }
    }

    void CheckPuzzleCleared()
    {
        foreach (var piece in allPieces)
        {
            if (!piece.IsSnapped) return; // 하나라도 안 맞춰졌으면 리턴
        }
        
        Debug.Log("퍼즐 완성!");
           }
}