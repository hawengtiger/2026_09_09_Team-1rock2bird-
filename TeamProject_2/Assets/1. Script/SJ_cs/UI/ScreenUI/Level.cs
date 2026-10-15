using UnityEngine;
using UnityEngine.UI;

public class Level : MonoBehaviour
{
    [Header("넘어갈 씬")]
    public string nextLevel;

    [Header("시작 버튼")]
    public Button startBTN;

    private void Start()
    {
        startBTN.onClick.AddListener(MovetoLevel);
    }

    public void MovetoLevel()
    {
        SoundManager.Instance.StopMusic();
        SoundManager.Instance.PlaySFX("Start");

        // [수정] FadeManager를 통해 페이드 아웃을 실행하고, 완료되면 씬 전환
        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.FadeOut(0.8f, DG.Tweening.Ease.InQuad, () =>
            {
                LoadingSceneController.LoadScene(nextLevel);
            });
        }
        else
        {
            // 혹시 FadeManager가 없을 경우를 대비한 예외 처리
            LoadingSceneController.LoadScene(nextLevel);
        }
    }
}
