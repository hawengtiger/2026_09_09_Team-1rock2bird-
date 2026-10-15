using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// === | 사운드 음소거 설정. | ===
/// </summary>
public class MuteUIChange : MonoBehaviour
{
    /// <summary>
    /// | public 변수 | =====================
    /// </summary>
    public enum SoundType { BGM, SFX }
    
    [Header("사운드 타입")]
    public SoundType type;

    [Header("버튼 이미지")]
    public Image buttonImage;

    [Header("음소거 유무 이미지")]
    public Sprite onSprite;
    public Sprite offSprite;




    /// <summary>
    /// | private 변수 | =====================
    /// </summary>
    private bool isMute;

    void Start()
    {
        UpdateUI(); //UpdateUI 함수 항목 참고 (시작 할때 )
    }

    /// <summary>
    /// === | 버튼 클릭 시 | ===
    /// </summary>
    public void OnClick()
    {
        if (buttonImage == null)
        {
            Debug.LogWarning("버튼 이미지가 할당 안됐는데 개발자! 어케된거야!");
            return;
        }

        if (type == SoundType.BGM)
        {
            SoundManager.Instance.MuteMusic();      //BGM 음소거
        }
        else
        {
            SoundManager.Instance.MuteSFX();         //SFX 음소거
        }

        UpdateUI();     //UpdateUI 함수 참고
    }

    /// <summary>
    /// === | 소리 버튼 UI 갱신 | ===
    /// </summary>
    void UpdateUI()
    {
        if ((type == SoundType.BGM)) 
        {
            isMute = SoundManager.Instance.musicSource.mute;        //bgm이 음소거일 경우 isMute가 참이됨.
        }
        else
        {
            isMute = SoundManager.Instance.sfxSource.mute;          //sfx가 음소거일 경우 isMute가 참이됨.
        }


        buttonImage.sprite = isMute ? offSprite : onSprite;              //isMute가 참이면 버튼 이미지를 offSprite로 변환 거짓이면 onSprite.
    }
}