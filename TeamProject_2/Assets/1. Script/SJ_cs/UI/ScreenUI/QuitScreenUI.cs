using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class QuitScreenUI : MonoBehaviour
{
    public void QuitGame()
    {
        SoundManager.Instance.StopMusic();

        FadeManager.Instance.FadeOut(0.8f, DG.Tweening.Ease.InQuad, () =>
        {
            #if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
            #else
                                            Application.Quit();
            #endif
        });
    }
}