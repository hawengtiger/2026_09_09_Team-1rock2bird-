using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

/// <summary>
/// === | 게임 음악 클래스 | ===
/// </summary>
[System.Serializable]                //Serializable(직렬화)란? :  커스텀 클래스나 구조체를 유니티 엔진이 이해할 수 있는 데이터 형태(바이트 스트림)로 변환해 인스펙터 창에 노출하고, 씬/프로젝트 저장 시 해당 데이터를 보존하게 하는 속성을 의미.
public class Sound
{
    public string name;             //해당 BGM또는 SFX 효과음을 사용하기위해 해당 이름으로 부르기 위해서.
    public AudioClip clip;          //AudioClip을 효과음을 할당해서 끌어다가 사용할 수 있게.
}

/// <summary>
/// === | 게임 음악 관련 | ===
/// </summary>
public class SoundManager : MonoBehaviour
{

    /// <summary>
    /// === | 인스턴스로 처리할 오디오 메니저 | ===    
    /// </summary>
    public static SoundManager Instance;

    /// <summary>
    /// | Public 변수 | ====================================
    /// </summary>
    [Header("추가할 BGM과 SFX")]
    public Sound[] musicSounds, sfxSounds;

    [Header("BGM과 SFX 할당")]
    public AudioSource musicSource, sfxSource;




    /// <summary>
    /// === | 게임 음악 관련 | ===
    /// </summary>
    private void Awake()
    {
        if(Instance == null)    //Instance가 없으면 생성함.
        {
            CreatBTSfx();           //CreatBTSfx 함수 활성화 (자세한 건 : CreatBTSfx함수 항목 참조.)

            InitialSound();         //InitialSound 함수 활성화 (자세한 건 :  InitialSound함수 항목 참조.)

            Instance = this;        //싱글톤 등록
            DontDestroyOnLoad(gameObject);                              // 씬 전환에도 제거 안되게 함.
            SceneManager.sceneLoaded += OnSceneLoaded;      //씬이 로드되면 실행할 함수.
        }
        else
        {
            Destroy(gameObject);    //(이미 있다면 추가 안함.) SoundManager가 2개 생기는 거 방지
        }
    }

    /// <summary>
    /// === | 음악 실행 (음악 이름) | ===
    /// </summary>
    public void PlayMusic(string bgmName)
    {
        Sound bgmSound = Array.Find(musicSounds, bgm => bgm.name == bgmName);   //bgmName으로 musicSounds에서 BGM을 찾는 코드.

        if (bgmSound  == null)  //bgmSound가 Null이면 반환하는 코드 (Null 버그 방지)
        {
            Debug.LogWarning($"[SoundManager]에 음악 '{bgmName}' <-- 인스펙터에 없거나 오타 발생.");    // bgmName인스펙터에 없을경우 디버그로 시각화 함.
            return;
        }

        musicSource.clip = bgmSound.clip;    //bgmName을 실행할 음악으로 설정
        musicSource.Play();                             //bgmName음악 실행
        PlayerPrefs.SetString("LastPlayedMusic", bgmName);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// === | 효과음 실행 (효과음 이름) | ===
    /// </summary>
    public void PlaySFX(string sfxName)
    {
        Sound sfxSound = Array.Find(sfxSounds, sfx=> sfx.name == sfxName);     //sfxName으로 sfxSounds에서 SFX를 찾는 코드.

        if (sfxSound == null) //sfxSound가 Null이면 반환하는 코드 (Null 버그 방지)
        {
            Debug.LogWarning($"[SoundManager]에 효과음 '{sfxName}' <-- 인스펙터에 없거나 오타 발생.");   // sfxName인스펙터에 없을경우 디버그로 시각화 함.
            return;
        }

        sfxSource.PlayOneShot(sfxSound.clip);   //효과음 소리를 한번만 재생함
    }

    /// <summary>
    /// === | 음악 정지 | ===
    /// </summary>
    public void StopMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }




    /// <summary>
    /// === | 음악 음소거 | ===
    /// </summary>
    public void MuteMusic()
    {
        musicSource.mute = !musicSource.mute;  // 음악 음소거 상태를 반전
        PlayerPrefs.SetInt("MusicMute", musicSource.mute ? 1 : 0);  // 음악 음소거 상태를 PlayerPrefs에 저장   (참이면 1 / 거짓이면 0)
    }

    /// <summary>
    /// === | 효과음 음소거  | ===
    /// </summary>
    public void MuteSFX()
    {
        sfxSource.mute = !sfxSource.mute;  // 효과음 음소거 상태를 반전
        PlayerPrefs.SetInt("SFXMute", sfxSource.mute ? 1 : 0);  // 효과음 음소거 상태를 PlayerPrefs에 저장 (참이면 1 / 거짓이면 0)
    }

    /// <summary>
    /// === | 음악 볼륨 크기 | ===
    /// </summary>
    public void MusicVolume(float volume)
    {
        musicSource.volume = volume;  // 음악 볼륨 설정
        PlayerPrefs.SetFloat("MusicVolume", volume);  // 음악 볼륨 값을 PlayerPrefs에 저장
    }

    /// <summary>
    /// === | 효과음 볼륨 크기 | ===
    /// </summary>
    public void SFXVolume(float volume)
    {
        sfxSource.volume = volume;  // 효과음 볼륨 설정
        PlayerPrefs.SetFloat("SFXVolume", volume);  // 효과음 볼륨 값을 PlayerPrefs에 저장
    }

    /// <summary>
    /// === | 로딩씬에서 음악 사용 관련 | ===
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "LoadingScenes") // 로딩 씬일 경우
        {
            // 로딩 씬에서는 음악을 멈춘다
            StopMusic();
        }
        else        // 로딩 씬이 아닐 경우
        {
            // 로딩 씬이 끝나면, 저장된 'NextBGM'을 재생
            string nextBGM = PlayerPrefs.GetString("NextBGM", "");

            if (!string.IsNullOrEmpty(nextBGM))
            {
                PlayMusic(nextBGM);
                PlayerPrefs.DeleteKey("NextBGM"); // 사용 후 삭제
            }
        }
    }

    /// <summary>
    /// === | 버튼 하이라이트 효과음 | ===
    /// </summary>
    private void CreatBTSfx()
    {
        //모든 씬안에 Button을 찾고 안에 BT_SFX를 집어넣기.

        Button []
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);     // 정렬 안하고 씬에있는 버튼 오브젝트를 변수안에 가져옴.

            foreach (Button btn in buttons)      // 씬에서 찾은 버튼 오브젝트들을 하나씩 검사하는 코드
            {
                if (btn.GetComponent<BT_SFX>() == null)     //만약 버튼 오브젝트에 (BT_SFX)가 없다면?
                {
                    btn.gameObject.AddComponent<BT_SFX>();      //버튼 오브젝트에 (BT_SFX)가자동 추가되는 코드
                }
            }
    }

    /// <summary>
    /// === | 사운드 초기 설정 | ===
    /// </summary>
    private void InitialSound()
    {
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);        // 저장된 음악 BGM 소리값 불러오기 --> 없으면 초기 기본값 1(최대)
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);                // 저장된 음악 SFX 소리값 불러오기 --> 없으면 초기 기본값 1(최대)
        bool isMusicMuted = PlayerPrefs.GetInt("MusicMute", 0) == 1;      // 저장된 BGM이 Mute인지 아닌지 불러오기 --> 없으면 초기 기본값 0 (참고 : PlayerPrefs는 bool 저장 안됨 --> int로 우회해야 함.) int --> bool 변환 1이면 true | 0이면 false
        bool isSFXMuted = PlayerPrefs.GetInt("SFXMute", 0) == 1;             // 저장된 SFX가 Mute인지 아닌지 불러오기 --> 없으면 초기 기본값 0

        musicSource.volume = musicVolume;       // Audio Source에 BGM 오브젝트 Volume값 할당
        musicSource.mute = isMusicMuted;         // Audio Source에 SFX 오브젝트 Volume값 할당

        sfxSource.volume = sfxVolume;               // BGM 오브젝트 Mute 유무 할당
        sfxSource.mute = isSFXMuted;                // SFX 오브젝트 Mute 유무 할당
    }
}

