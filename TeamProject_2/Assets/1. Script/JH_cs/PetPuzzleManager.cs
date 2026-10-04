using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class PetPuzzleManager : MonoBehaviour
{
    public enum PuzzleState
    {
        WaitToEnter,
        PuzzleActive,
        ResultWait
    }

    [Header("퍼즐 설정")]
    public int correctOptionIndex = 0;

    [Header("UI 요소")]
    public GameObject puzzleUI;              
    public Image[] optionHighlights;
    public TextMeshProUGUI statusText;

    [Header("사운드")]
    public AudioSource audioSource;
    public AudioClip kkiingSound;

    private PuzzleState currentState = PuzzleState.WaitToEnter;
    private int currentSelectedIndex = -1; 

    private void Start()
    {
        ResetToEmptyScreen();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        switch (currentState)
        {
            case PuzzleState.WaitToEnter:
                if (Keyboard.current.fKey.wasPressedThisFrame)
                {
                    EnterPuzzle();
                }
                break;

            case PuzzleState.PuzzleActive:
                HandleKeyboardSelection();
                break;

            case PuzzleState.ResultWait:
                if (Keyboard.current.fKey.wasPressedThisFrame)
                {
                    ResetToEmptyScreen();
                }
                break;
        }
    }

    private void ResetToEmptyScreen()
    {
        currentState = PuzzleState.WaitToEnter;
        if (puzzleUI != null) puzzleUI.SetActive(false); 
        
        if (statusText != null) statusText.text = ""; 
    }

    private void EnterPuzzle()
    {
        currentState = PuzzleState.PuzzleActive;
        if (puzzleUI != null) puzzleUI.SetActive(true);
        
        currentSelectedIndex = -1;
        UpdateOptionHighlight();

        if (audioSource != null && kkiingSound != null)
        {
            audioSource.PlayOneShot(kkiingSound);
        }

        if (statusText != null)
            statusText.text = "영룡이가 무언가 원합니다!\n1, 2, 3 키를 누르거나 클릭하세요.";
    }

    // 키보드 입력 처리
    private void HandleKeyboardSelection()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
            SelectOption(0);
        else if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame)
            SelectOption(1);
        else if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame)
            SelectOption(2);
    }

    // 마우스 클릭(버튼) 시 호출될 퍼블릭 함수
    public void OnOptionButtonClicked(int index)
    {
        // 퍼즐 진행 중일 때만 버튼 클릭이 작동하도록 제한
        if (currentState == PuzzleState.PuzzleActive)
        {
            SelectOption(index);
        }
    }

    // 선택 처리 (키보드, 버튼 공통 사용)
    private void SelectOption(int index)
    {
        currentSelectedIndex = index;
        UpdateOptionHighlight();
        EvaluateChoice();
    }

    private void UpdateOptionHighlight()
    {
        for (int i = 0; i < optionHighlights.Length; i++)
        {
            if (optionHighlights[i] != null)
            {
                optionHighlights[i].color = (i == currentSelectedIndex) ? Color.yellow : Color.white;
            }
        }
    }

    private void EvaluateChoice()
    {
        currentState = PuzzleState.ResultWait;
        
        if (currentSelectedIndex == correctOptionIndex)
        {
            if (statusText != null)
                statusText.text = "성공! 영룡이가 만족했습니다.\n[F] 키를 눌러 나가기";
        }
        else
        {
            if (statusText != null)
                statusText.text = "실패! 영룡이가 경멸하는 표정을 짓습니다.\n[F] 키를 눌러 나가기";
        }
    }
}