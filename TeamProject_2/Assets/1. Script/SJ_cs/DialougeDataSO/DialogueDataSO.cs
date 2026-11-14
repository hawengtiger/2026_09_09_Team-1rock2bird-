
using System.Collections.Generic;
using UnityEngine;

public enum DialogueType
{
    Monologue,  // 독백
    Dialogue    // NPC 대화
}

public enum BubbleType
{
    A,  // 플레이어
    B   // 현재 대화 상대 NPC
}

[System.Serializable]
public class DialogueSetting
{
    [Header("출력 방식")]
    public DialogueType dialogueType = DialogueType.Monologue;

    [Header("말풍선 선택 (Dialogue일 때만 사용)")]
    public BubbleType bubbleType = BubbleType.A;
}

[CreateAssetMenu(
    fileName = "DialogueDataSO",
    menuName = "Scriptable Objects/DialogueDataSO")]
public class DialogueDataSO : ScriptableObject
{
    [Header("대사 목록")]
    [TextArea(2, 5)]
    public List<string> dialogues = new List<string>();

    [Header("대사별 출력 설정")]
    public List<DialogueSetting> dialogueSettings =
        new List<DialogueSetting>();

    // 특정 대사의 설정 반환
    public DialogueSetting GetSetting(int index)
    {
        if (index < 0 ||
            dialogueSettings == null ||
            index >= dialogueSettings.Count ||
            dialogueSettings[index] == null)
        {
            // 설정이 없으면 기존 모놀로그로 출력
            return new DialogueSetting();
        }

        return dialogueSettings[index];
    }
}
