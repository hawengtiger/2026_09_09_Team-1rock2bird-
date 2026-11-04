using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// 약병 퍼즐
// F키로 퍼즐 열기/닫기 → 조각을 마우스로 드래그해서 약병의 절취선에 맞추면 완성
public class PerspectivePuzzle : MonoBehaviour
{
    [Header("UI & Objects")]
    public GameObject puzzleUI;         // 퍼즐 UI (안내 글, 메시지)
    public Transform puzzleContainer;   // 약병과 조각들이 들어있는 부모 객체
    public Camera cam;                  // 비워두면 Main Camera
    public bool resetContainerRotation = true; // 예전 회전 퍼즐 각도 초기화

    [Header("퍼즐 조각")]
    public PuzzlePiece[] pieces;        // 움직이는 조각들 (비워두면 자동으로 찾음)

    [Header("화면 메시지")]
    public TMP_Text messageText;        // puzzleUI 안에 두기
    public string solvedMessage = "퍼즐 완성! F를 눌러서 닫고 플레이어로 돌아가세요.";
    public string wrongMessage = "모양이 맞지 않는 것 같다.";
    public float wrongMessageDuration = 1.5f;

    private bool isPuzzleActive = false;
    private bool isSolved = false;
    private float messageTimer = 0f;

    private PuzzlePiece dragging;       // 지금 드래그 중인 조각
    private Vector3 grabOffset;

    public bool IsSolved => isSolved;

    void Start()
    {
        if (cam == null) cam = Camera.main;

        // 조각 목록이 비어 있으면 자동으로 찾기
        if (pieces == null || pieces.Length == 0)
        {
            if (puzzleContainer != null)
                pieces = puzzleContainer.GetComponentsInChildren<PuzzlePiece>(true);
            else if (puzzleUI != null)
                pieces = puzzleUI.GetComponentsInChildren<PuzzlePiece>(true);
        }

        if (resetContainerRotation && puzzleContainer != null)
            puzzleContainer.localEulerAngles = Vector3.zero;

        if (messageText != null) messageText.text = "";

        // 시작할 때 퍼즐 숨김
        if (puzzleUI != null) puzzleUI.SetActive(false);
        if (puzzleContainer != null) puzzleContainer.gameObject.SetActive(false);
    }

    void Update()
    {
        // F키 입력 감지
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (!isSolved)
            {
                // [퍼즐을 풀기 전] F키를 누를 때마다 켜고 끄기 반복
                isPuzzleActive = !isPuzzleActive;
                TogglePuzzleMode(isPuzzleActive);
            }
            else if (isPuzzleActive)
            {
                // [퍼즐을 푼 후] 퍼즐이 켜져있을 때 F를 누르면: 닫고 다신 안 열리게 처리
                isPuzzleActive = false;
                TogglePuzzleMode(false);
            }
        }

        if (isPuzzleActive && !isSolved) HandleDrag();

        // 틀렸을 때 메시지는 잠깐만 표시
        if (messageTimer > 0f)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0f && messageText != null && !isSolved)
                messageText.text = "";
        }
    }

    // ───────── 드래그 처리 ─────────
    private void HandleDrag()
    {
        if (Mouse.current == null || cam == null) return;
        Vector2 mouse = Mouse.current.position.ReadValue();

        // 클릭: 마우스 아래 조각 잡기 (겹쳐 있으면 앞에 있는 것)
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = cam.ScreenPointToRay(mouse);
            RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity);

            PuzzlePiece best = null;
            foreach (RaycastHit2D hit in hits)
            {
                PuzzlePiece p = hit.collider.GetComponent<PuzzlePiece>();
                if (p == null || p.isPlaced) continue;
                if (best == null || p.SortingOrder > best.SortingOrder) best = p;
            }

            if (best != null)
            {
                dragging = best;
                dragging.BeginDrag();
                grabOffset = dragging.transform.position - MouseOnPlane(mouse, dragging.transform.position.z);
            }
        }

        if (dragging == null) return;

        // 놓기: 절취선 판정
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            bool correct = dragging.EndDrag(cam);
            dragging = null;

            if (correct) OnPiecePlaced();
            else OnPieceMissed();
            return;
        }

        // 드래그 중: 마우스 따라 이동
        float z = dragging.transform.position.z;
        Vector3 pos = MouseOnPlane(mouse, z) + grabOffset;
        pos.z = z;
        dragging.transform.position = pos;
    }

    // 마우스 위치를 조각과 같은 깊이(z)의 월드 좌표로 변환 (원근/직교 카메라 모두 동작)
    private Vector3 MouseOnPlane(Vector2 screenPos, float z)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        Plane plane = new Plane(Vector3.forward, new Vector3(0, 0, z));
        if (plane.Raycast(ray, out float dist)) return ray.GetPoint(dist);
        return ray.origin;
    }

    // ───────── 퍼즐 상태 ─────────
    private void TogglePuzzleMode(bool active)
    {
        if (puzzleUI != null) puzzleUI.SetActive(active);
        if (puzzleContainer != null) puzzleContainer.gameObject.SetActive(active);

        if (!active && dragging != null)
        {
            dragging.EndDrag(cam);
            dragging = null;
        }

        // 퍼즐이 켜지면 플레이어는 꺼지고, 퍼즐이 꺼지면 플레이어가 켜집니다.
        // if (player != null) player.SetActive(!active);
    }

    private void OnPiecePlaced()
    {
        foreach (PuzzlePiece piece in pieces)
        {
            if (piece == null) continue;
            if (!piece.isPlaced) return;   // 아직 안 맞춘 조각이 있음
        }

        isSolved = true;
        ShowMessage(solvedMessage, 0f);    // 0 = 계속 표시
    }

    private void OnPieceMissed()
    {
        if (isSolved) return;
        ShowMessage(wrongMessage, wrongMessageDuration);
    }

    private void ShowMessage(string text, float duration)
    {
        if (messageText == null) return;
        messageText.text = text;
        messageTimer = duration;
    }
}