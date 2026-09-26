using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SettingScreenUI : MonoBehaviour
{
    [Header("¼³Á¤ UI")]
    public GameObject panel;
    public Image settingScreen;

    private bool isOpen = false;
    private bool isTweening = false;

    void Start()
    {
        panel.SetActive(false);
        settingScreen.gameObject.SetActive(false);
        settingScreen.rectTransform.localScale = Vector3.one * 0.01f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isTweening)
                return;

            if (isOpen)
                CloseSettingUI();
            else
                OpenSettingUI();
        }
    }

    public void OpenSettingUI()
    {
        if (settingScreen == null)
            return;

        if (isOpen || isTweening)
            return;

        SoundManager.Instance.PlaySFX("MainClick");

        isTweening = true;
        isOpen = true;

        panel.SetActive(true);
        settingScreen.gameObject.SetActive(true);

        settingScreen.rectTransform.DOKill();
        settingScreen.rectTransform.localScale = Vector3.one * 0.01f;

        settingScreen.rectTransform
            .DOScale(1f, 0.2f)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                Time.timeScale = 0f;
                isTweening = false;
            });
    }

    public void CloseSettingUI()
    {
        if (settingScreen == null)
            return;

        if (!isOpen || isTweening)
            return;

        SoundManager.Instance.PlaySFX("MainClick");

        isTweening = true;
        isOpen = false;

        Time.timeScale = 1f;

        settingScreen.rectTransform.DOKill();

        settingScreen.rectTransform
            .DOScale(0.01f, 0.2f)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                settingScreen.gameObject.SetActive(false);
                panel.SetActive(false);
                isTweening = false;
            });
    }
}