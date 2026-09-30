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
        
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame && !isSolved)
        {
            isPuzzleActive = !isPuzzleActive;
            
           
            if (puzzleUI != null) puzzleUI.SetActive(isPuzzleActive);
            if (puzzleContainer != null) puzzleContainer.gameObject.SetActive(isPuzzleActive);
        }
    }

    private void OnSliderMoved(float value)
    {
        if (isSolved || puzzleContainer == null) return;

        // 슬라이더 값에 따라 퍼즐 부모 객체의 Y축 회전값 변경
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
            
            Debug.Log("퍼즐 완성!");
        }
    }
}