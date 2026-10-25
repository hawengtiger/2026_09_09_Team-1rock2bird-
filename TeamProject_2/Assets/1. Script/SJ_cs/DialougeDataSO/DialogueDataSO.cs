using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DialogueDataSO", menuName = "Scriptable Objects/DialogueDataSO")]
public class DialogueDataSO : ScriptableObject
{
    [Header("대사 목록")]
    [TextArea(3, 10)]
    public List<string> dialogues = new List<string>();
}
