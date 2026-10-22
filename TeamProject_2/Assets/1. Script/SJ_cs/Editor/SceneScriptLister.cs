using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SceneScriptLister : EditorWindow
{
    private Vector2 scrollPosition;
    private string searchFilter = ""; // ◀ 검색어를 저장할 변수 추가

    [MenuItem("Tools/씬 스크립트 목록 보기")]
    public static void ShowWindow()
    {
        GetWindow<SceneScriptLister>("씬 스크립트 목록");
    }

    private void OnGUI()
    {
        GUILayout.Label("현재 씬에 사용된 스크립트 목록", EditorStyles.boldLabel);

        // ◀ 검색창 UI 디자인 추가
        EditorGUILayout.BeginHorizontal(GUI.skin.box);
        GUILayout.Label("스크립트 검색:", GUILayout.Width(80));
        searchFilter = EditorGUILayout.TextField(searchFilter); // 텍스트 입력창
        if (GUILayout.Button("X", GUILayout.Width(20))) // 검색어 초기화 버튼
        {
            searchFilter = "";
            GUI.FocusControl(null); // 포커스 해제
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("새로고침"))
        {
            Repaint();
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // 씬 내의 모든 MonoBehaviour(스크립트)를 찾음
        MonoBehaviour[] scripts = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        HashSet<string> uniqueScripts = new HashSet<string>();

        foreach (var script in scripts)
        {
            if (script == null) continue;

            string scriptName = script.GetType().Name;

            // 유니티 자체 내장 컴포넌트(UI 등)는 제외하고 커스텀 스크립트만 필터링
            if (script.GetType().Namespace != null && script.GetType().Namespace.StartsWith("UnityEngine"))
                continue;

            // ◀ [핵심 검색 필터링] 검색어가 입력되었을 때, 스크립트 이름에 검색어가 포함되지 않으면 패스
            if (!string.IsNullOrEmpty(searchFilter) &&
                scriptName.IndexOf(searchFilter, System.StringComparison.OrdinalIgnoreCase) == -1)
            {
                continue;
            }

            if (!uniqueScripts.Contains(scriptName))
            {
                uniqueScripts.Add(scriptName);

                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                EditorGUILayout.LabelField(scriptName, EditorStyles.label);
                if (GUILayout.Button("오브젝트 찾기", GUILayout.Width(100)))
                {
                    Selection.activeGameObject = script.gameObject;
                    EditorGUIUtility.PingObject(script.gameObject);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.EndScrollView();
    }
}