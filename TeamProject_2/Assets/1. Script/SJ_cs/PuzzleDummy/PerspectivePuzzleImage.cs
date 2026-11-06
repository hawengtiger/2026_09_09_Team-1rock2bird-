
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// 약병 퍼즐
// 상호작용으로 열기/닫기 → UI Image 드래그 → 정답 위치에 맞추면 완성
public class PerspectivePuzzleImage : ItemAction
{
    [Header("퍼즐 전용 레이어")]
    public LayerMask puzzleLayer;

    [Header("UI & Objects")]
    public GameObject puzzleUI;
    public Transform puzzleContainer;
    public Camera cam;
    public bool resetContainerRotation = true;

    [Header("UI 클릭 감지")]
    public GraphicRaycaster graphicRaycaster;

    [Header("퍼즐 조각")]
    public PuzzlePieceImage[] pieces;

    [Header("화면 메시지")]
    public TMP_Text messageText;
    public string solvedMessage = "퍼즐 완성! F를 눌러서 닫고 플레이어로 돌아가세요.";

    public string wrongMessage = "모양이 맞지 않는 것 같다.";
    public float wrongMessageDuration = 1.5f;

    private bool isPuzzleActive = false;
    private bool isSolved = false;
    private float messageTimer = 0f;

    private PuzzlePieceImage dragging;
    private Vector2 previousMousePosition;

    public bool IsSolved => isSolved;

    private void Awake()
    {
        if (cam == null)
            cam = Camera.main;

        // 퍼즐 조각이 속한 Canvas의 GraphicRaycaster 검색
        if (graphicRaycaster == null && puzzleContainer != null)
        {
            graphicRaycaster = puzzleContainer.GetComponentInParent<GraphicRaycaster>();
        }

        // 퍼즐 조각 자동 검색
        if (pieces == null || pieces.Length == 0)
        {
            if (puzzleContainer != null)
            {
                pieces = puzzleContainer.GetComponentsInChildren<PuzzlePieceImage>(true);
            }
            else if (puzzleUI != null)
            {
                pieces = puzzleUI.GetComponentsInChildren<PuzzlePieceImage>(true);
            }
        }

        // 기존 회전 초기화 기능 유지
        if (resetContainerRotation && puzzleContainer != null)
        {
            puzzleContainer.localEulerAngles = Vector3.zero;
        }

        if (messageText != null)
            messageText.text = "";

        // 시작 시 퍼즐 숨김
        if (puzzleUI != null)
            puzzleUI.SetActive(false);

        if (puzzleContainer != null)
            puzzleContainer.gameObject.SetActive(false);
    }

    private void Update()
    {
        // 퍼즐이 열려 있을 때만 드래그
        if (isPuzzleActive && !isSolved)
        {
            HandleDrag();
        }

        // 퍼즐이 완성된 상태에서 F키를 누르면 닫기
        if (isPuzzleActive && isSolved && Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            // 인벤토리에 아이템 추가
            Inventory.Instance.AddItem(1, 1);

            // 퍼즐 종료
            isPuzzleActive = false;
            TogglePuzzleMode(false);

            Destroy(this.gameObject);

            Debug.Log("퍼즐 완료! 아이템 지급 및 오브젝트 삭제");
            return;
        }

        // 오답 메시지 타이머
        if (messageTimer > 0f)
        {
            messageTimer -= Time.deltaTime;

            if (messageTimer <= 0f && !isSolved)
            {
                if (messageText != null)
                    messageText.text = "";
            }
        }
    }

    // 상호작용 시 호출
    public override void Execute(PickupItem item)
    {
        // 이미 퍼즐이 실행 중이거나 완성되었다면 무시
        if (isPuzzleActive || isSolved)
            return;

        // 퍼즐 시작
        isPuzzleActive = true;
        TogglePuzzleMode(true);

        Debug.Log("약병 퍼즐 시작!");
    }

    // UI Image 드래그 처리
    private void HandleDrag()
    {
        if (Mouse.current == null || graphicRaycaster == null || EventSystem.current == null)
        {
            return;
        }

        Vector2 mouse = Mouse.current.position.ReadValue();

        // 마우스를 클릭했을 때
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            PointerEventData pointer = new PointerEventData(EventSystem.current);

            pointer.position = mouse;

            List<RaycastResult> results = new List<RaycastResult>();

            // 마우스 아래 UI 검색
            graphicRaycaster.Raycast(pointer, results);

            Debug.Log($"감지된 UI 개수 : {results.Count}");

            foreach (RaycastResult result in results)
            {
                // Puzzle 레이어가 아니면 무시
                if ((puzzleLayer.value &
                    (1 << result.gameObject.layer)) == 0)
                {
                    continue;
                }

                // PuzzlePiece 컴포넌트 검색
                PuzzlePieceImage piece = result.gameObject.GetComponentInParent<PuzzlePieceImage>();

                // 퍼즐 조각이 아니거나 이미 맞춘 조각이면 무시
                if (piece == null || piece.isPlaced)
                {
                    continue;
                }

                // 드래그할 조각 선택
                dragging = piece;

                Debug.Log($"드래그 시작 : {dragging.name}");

                dragging.BeginDrag();

                // 기존 grabOffset 계산 코드 유지
                // 마우스 이동량을 계산하기 위해 현재 위치 저장
                previousMousePosition = mouse;

                // 조각 하나만 선택
                break;
            }
        }

        if (dragging == null)
            return;

        // 마우스를 놓았을 때
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            RectTransform rect = dragging.GetComponent<RectTransform>();

            bool correct = dragging.EndDrag(GetUICamera(rect));

            dragging = null;

            if (correct)
                OnPiecePlaced();
            else
                OnPieceMissed();

            return;
        }

        // 마우스를 따라 조각 이동
        RectTransform dragRect = dragging.GetComponent<RectTransform>();

        // 이전 마우스 위치와 현재 마우스 위치의 차이
        Vector2 mouseDelta = mouse - previousMousePosition;

        // Canvas의 화면 배율 가져오기
        Canvas canvas = dragRect.GetComponentInParent<Canvas>();

        float scaleFactor = 1f;

        if (canvas != null)
        {
            scaleFactor = canvas.rootCanvas.scaleFactor;
        }

        // 마우스 이동량을 UI 좌표에 맞게 변환
        Vector2 move = mouseDelta / Mathf.Max(scaleFactor, 0.0001f);

        // RectTransform의 Pos X, Pos Y 직접 변경
        dragRect.anchoredPosition = new Vector2(dragRect.anchoredPosition.x + move.x, dragRect.anchoredPosition.y + move.y);

        // 다음 프레임에서 사용할 마우스 위치 저장
        previousMousePosition = mouse;
    }

    // Canvas 모드에 맞는 카메라 반환
    private Camera GetUICamera(RectTransform rect)
    {
        Canvas canvas = rect.GetComponentInParent<Canvas>();

        // Overlay Canvas는 카메라가 필요 없음
        if (canvas != null &&
            canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return null;
        }

        if (canvas != null && canvas.worldCamera != null)
            return canvas.worldCamera;

        return cam;
    }

    // 퍼즐 UI 열기 / 닫기
    private void TogglePuzzleMode(bool active)
    {
        // 닫기 전에 드래그 중인 조각 처리
        if (!active && dragging != null)
        {
            RectTransform rect = dragging.GetComponent<RectTransform>();

            dragging.EndDrag(GetUICamera(rect));
            dragging = null;
        }

        if (puzzleUI != null)
            puzzleUI.SetActive(active);

        if (puzzleContainer != null)
            puzzleContainer.gameObject.SetActive(active);
    }

    // 모든 조각을 맞췄는지 확인
    private void OnPiecePlaced()
    {
        foreach (PuzzlePieceImage piece in pieces)
        {
            if (piece == null)
                continue;

            if (!piece.isPlaced)
                return;
        }

        isSolved = true;
        ShowMessage(solvedMessage, 0f);

    }

    // 오답 처리
    private void OnPieceMissed()
    {
        if (isSolved)
            return;

        ShowMessage(wrongMessage, wrongMessageDuration);
    }

    // 메시지 출력
    private void ShowMessage(string text, float duration)
    {
        if (messageText == null)
            return;

        messageText.text = text;
        messageTimer = duration;
    }
}
