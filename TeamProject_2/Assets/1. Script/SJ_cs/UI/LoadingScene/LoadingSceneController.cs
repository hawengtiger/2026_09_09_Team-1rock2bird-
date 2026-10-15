using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// === | 로딩 씬 화면 전환 | ===
/// </summary>
public class LoadingSceneController : MonoBehaviour
{
    static string nextScene;

    [Header("프로그레스 바")]
    [SerializeField]
    Image progressBar;

    /// <summary>
    /// === | 비동기씬 로딩 | ===
    /// </summary>
    public static void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        nextScene = sceneName;
        SceneManager.LoadScene("LoadingScenes");
    }

    void Start()
    {
        StartCoroutine(LoadSceneProcess());     //로딩 시작
    }

    /// <summary>
    /// === | 로딩씬 프로그래스 관련 설정. | ===
    /// </summary>
    IEnumerator LoadSceneProcess()
    {
        // 씬 비동기 로딩
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false; // 씬 전환을 잠시 막음

        float timer = 0f;
        while (!op.isDone)      //씬로딩이 끝나지 않았다면 계속 반복
        {
            yield return null;      //반복 될 때마다 유니티엔진에 제어권을 넘기지 않으면 반복문이 끝나기 전에 화면이 갱신 되지 않아 진행바가 차오르는걸 볼 수 없다

            if (op.progress < 0.9f)     // 90퍼는 실제 로딩에 맞춰서 채워짐.
            {
                progressBar.fillAmount = op.progress; // 로딩 진행 상태 표시
            }
            else        // 나머지 10퍼는 1초만에 바로 처리. (페이크 로딩을 넣는이유는 씬이 생각보다 빨리 로드 되면 내가 전달하고자 하는 앱/게임의 Tip을 사용자가 못보고 지나칠 수 있기 때문에.)
            {
                timer += Time.unscaledDeltaTime;
                progressBar.fillAmount = Mathf.Lerp(0.9f, 1f, timer); // 0.9 이상에서 부드럽게 증가

                if (progressBar.fillAmount >= 1f)   // 100%가 채워지면.
                {
                    op.allowSceneActivation = true; // 씬 활성화
                    yield break; // 코루틴 종료
                }
            }
        }
    }
}
