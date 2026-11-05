using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// === | 사운드 슬라이더 컨트롤 | ===
/// </summary>
public class UIController : MonoBehaviour
{
    /// <summary>
    /// | public 변수 | =====================
    /// </summary>
    [Header("볼륨 슬라이더")]
    public Slider _musicSlider, _sfxSlider;         // 음악 볼륨과 효과음 볼륨을 조절할 슬라이더

    void Start()
    {
        Reset();        //Reset함수 항목 참고
    }

    /// <summary>
    /// === | 브금 볼륨값 | ===
    /// </summary>
    public void MusicVolume()    // 음악 볼륨 슬라이더 값 변경 시 호출되는 함수
    {
        SoundManager.Instance.MusicVolume(_musicSlider.value);  // 슬라이더 값에 맞게 음악 볼륨 변경
    }

    /// <summary>
    /// === | 효과음 볼륨값 | ===
    /// </summary>
    public void SFXVolume()    // 효과음 볼륨 슬라이더 값 변경 시 호출되는 함수
    {
        SoundManager.Instance.SFXVolume(_sfxSlider.value);  // 슬라이더 값에 맞게 효과음 볼륨 변경
    }



    /// <summary>
    /// === | 초기 세팅 | ===
    /// </summary>
    private void Reset()
    {
        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            if (_musicSlider == null)
            {
                Debug.LogWarning("BGM슬라이더가 할당 안됐는데 개발자! 어케된거야!");
                return;      // 할당 안돼도 일단 빨간 버그 내지마!
            }

            _musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");       // 저장된 음악 볼륨 값을 슬라이더에 반영
        }


        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            if (_sfxSlider == null)
            {
                Debug.LogWarning("SFX슬라이더가 할당 안됐는데 개발자! 어케된거야!");
                return;      // 할당 안돼도 일단 빨간 버그 내지마!
            }

            _sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume");               // 저장된 효과음 볼륨 값을 슬라이더에 반영
        }
    }
}