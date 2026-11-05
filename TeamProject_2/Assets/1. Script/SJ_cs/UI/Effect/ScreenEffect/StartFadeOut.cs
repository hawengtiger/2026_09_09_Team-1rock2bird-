using UnityEngine;

public class StartFadeOut : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.FadeIn(1.5f, DG.Tweening.Ease.OutQuad);
        }
    }
}
