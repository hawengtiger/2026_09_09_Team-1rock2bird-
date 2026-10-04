using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; 

public class PerspectivePuzzle : MonoBehaviour
{
    [Header("UI & Objects")]
    public GameObject puzzleUI;         // 드래그 바가 포함된 UI 캔버스
    public Transform puzzleContainer;   // 퍼즐 조각들이 들어있는 부모 객체
    public Slider dragBar;              // 화면 하단의 드래그 바

    [Header("Puzzle Settings")]
    public float correctRotation = 0f;  // 정답이 되는 Y축 각도
    public float tolerance = 3f;        // 정답으로 인정할 오차 범위 
    
    private bool isPuzzleActive = false;
    private bool isSolved = false;

    void Start()
    {
        // 시작할 때 퍼즐 UI와 퍼즐 오브젝트(컨테이너)를 모두 숨김
        if (puzzleUI != null) puzzleUI.SetActive(false);
        if (puzzleContainer != null) puzzleContainer.gameObject.SetActive(false); 
        
        dragBar.onValueChanged.AddListener(OnSliderMoved);
        dragBar.value = dragBar.minValue; 
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
                // [퍼즐을 푼 후] 퍼즐이 화면에 켜져있을 때 F를 누르면: 닫고 다신 안 열리게 처리
                isPuzzleActive = false;
                TogglePuzzleMode(false);
            }
        }
    }
   // 퍼즐과 플레이어의 상태를 반대로 켜고 끄는 함수
    private void TogglePuzzleMode(bool active)
    {
        if (puzzleUI != null) puzzleUI.SetActive(active);
        if (puzzleContainer != null) puzzleContainer.gameObject.SetActive(active);
        
        // 퍼즐이 켜지면(true) 플레이어는 꺼지고(!true = false), 퍼즐이 꺼지면 플레이어가 켜집니다.
       // if (player != null) player.SetActive(!active);
    }

    private void OnSliderMoved(float value)
    {
        if (isSolved || puzzleContainer == null) return;

        puzzleContainer.localEulerAngles = new Vector3(0, value, 0);
        CheckSolveCondition(value);
    }

    private void CheckSolveCondition(float currentRotation)
    {
        if (Mathf.Abs(currentRotation - correctRotation) <= tolerance)
        {
            isSolved = true;
            
            puzzleContainer.localEulerAngles = new Vector3(0, correctRotation, 0);
            dragBar.value = correctRotation;
            dragBar.interactable = false; 
            
            Debug.Log("퍼즐 완성! F를 눌러서 닫고 플레이어로 돌아가세요.");
        }
    }
}