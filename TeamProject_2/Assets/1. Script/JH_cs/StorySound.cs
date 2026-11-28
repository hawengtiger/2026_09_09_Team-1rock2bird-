using System.Collections;
using UnityEngine;

// 스토리 배경음악(기 / 승 / 전 / 결 각각 다른 곡) + 아이템 획득 효과음
// - 팀원 SoundManager의 BGM/SFX 스피커를 그대로 써서 설정창 볼륨·음소거가 그대로 적용됨
//   (SoundManager가 없으면 이 오브젝트에 스피커를 자동으로 만들어 씀)
// - 기: 씬 시작 ~ 약 먹고 침대에 눕기 전까지                → 자동
// - 승: 침대에서 자고 일어났을 때 (팀원 침대 연출에서 영롱이2가 나타나는 순간) → 자동
// - 전: 영롱이 안고 병원 가는 장면부터 → HospitalRushTrigger → On Carry 에 PlayJeon() 연결
// - 결: 병원·장례식 후 꿈에서 깬 뒤 (처음 네컷 사진이 뜰 때)   → 자동 (PlayGyeol()로 더 일찍 틀 수도 있음)
// - 곡은 앞으로만 넘어감 (결 → 기처럼 거꾸로 돌아가지 않음)
// - 아이템 효과음: 인벤토리 아이템 개수가 늘어날 때마다 자동 재생 (팀원 아이템 / 내 아이템 전부)
public class StorySound : MonoBehaviour
{
    [Header("배경음악 (기 / 승 / 전 / 결)")]
    public AudioClip giBGM;
    public AudioClip seungBGM;
    public AudioClip jeonBGM;
    public AudioClip gyeolBGM;
    public bool playOnStart = true;             // 씬 시작하면 '기' 곡 재생
    public float fadeTime = 1.5f;               // 곡 바뀔 때 페이드 시간

    [Header("자동 전환 기준 (비우면 자동으로 찾음)")]
    public GameObject seungStartObject;         // 이게 켜지면 '승' (기본: 팀원 침대 연출의 영롱이2)
    public GameObject gyeolStartObject;         // 이게 켜지면 '결' (기본: 처음 뜨는 네컷 사진)

    [Header("아이템 획득 효과음")]
    public AudioClip pickupSFX;
    [Range(0f, 1f)] public float pickupVolume = 1f;

    private AudioSource music;
    private AudioSource sfx;
    private int[] lastCounts;
    private Coroutine fadeRoutine;
    private int stage = -1;                     // 0 기, 1 승, 2 전, 3 결

    private void Start()
    {
        if (SoundManager.Instance != null)
        {
            music = SoundManager.Instance.musicSource;
            sfx = SoundManager.Instance.sfxSource;
        }
        if (music == null) music = gameObject.AddComponent<AudioSource>();
        if (sfx == null) sfx = gameObject.AddComponent<AudioSource>();

        if (seungStartObject == null)
        {
            BedItemAction bed = FindAnyObjectByType<BedItemAction>(FindObjectsInactive.Include);
            if (bed != null) seungStartObject = bed.yeonglong2;
        }
        if (gyeolStartObject == null)
        {
            IntroPhotoEvent intro = FindAnyObjectByType<IntroPhotoEvent>(FindObjectsInactive.Include);
            if (intro != null && intro.photo != null) gyeolStartObject = intro.photo.gameObject;
        }

        RememberCounts();

        if (playOnStart) PlayGi();
    }

    public void PlayGi()    { GoTo(0, giBGM); }
    public void PlaySeung() { GoTo(1, seungBGM); }
    public void PlayJeon()  { GoTo(2, jeonBGM); }     // HospitalRushTrigger → On Carry
    public void PlayGyeol() { GoTo(3, gyeolBGM); }
    public void PlayJeongyeol() { PlayJeon(); }      // 예전 연결 호환용

    // 음악 천천히 끄기 (필요할 때 이벤트에 연결)
    public void FadeOutMusic() { ChangeMusic(null); }

    private void GoTo(int s, AudioClip clip)
    {
        if (s <= stage) return;                 // 이미 지난 구간이면 무시
        stage = s;
        ChangeMusic(clip);
    }

    private void ChangeMusic(AudioClip clip)
    {
        if (music == null) return;
        if (clip != null && music.clip == clip && music.isPlaying) return;

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeTo(clip));
    }

    private IEnumerator FadeTo(AudioClip clip)
    {
        float target = PlayerPrefs.GetFloat("MusicVolume", 1f);   // 설정창에서 정한 볼륨

        // 지금 곡 페이드 아웃
        if (music.isPlaying && music.clip != null)
        {
            float start = music.volume;
            for (float t = 0f; t < fadeTime; t += Time.deltaTime)
            {
                music.volume = Mathf.Lerp(start, 0f, t / fadeTime);
                yield return null;
            }
            music.Stop();
        }

        if (clip == null)
        {
            music.volume = target;
            fadeRoutine = null;
            yield break;
        }

        // 새 곡 페이드 인
        music.clip = clip;
        music.loop = true;
        music.volume = 0f;
        music.Play();
        for (float t = 0f; t < fadeTime; t += Time.deltaTime)
        {
            music.volume = Mathf.Lerp(0f, target, t / fadeTime);
            yield return null;
        }
        music.volume = target;
        fadeRoutine = null;
    }

    private void Update()
    {
        // 자동 구간 전환
        if (stage < 1 && seungStartObject != null && seungStartObject.activeInHierarchy) PlaySeung();
        if (stage < 3 && gyeolStartObject != null && gyeolStartObject.activeInHierarchy) PlayGyeol();

        CheckPickup();
    }

    // ───────── 아이템 획득 효과음 ─────────
    private void CheckPickup()
    {
        if (Inventory.Instance == null || Inventory.Instance.itemData == null) return;

        var list = Inventory.Instance.itemData;
        if (lastCounts == null || lastCounts.Length != list.Count) { RememberCounts(); return; }

        bool gained = false;
        for (int i = 0; i < list.Count; i++)
        {
            int c = (int)list[i].itemCount;
            if (c > lastCounts[i]) gained = true;
            lastCounts[i] = c;
        }

        // 퍼즐 밖에서 실수로 쓴 아이템이 돌아오는 경우(같은 프레임에 사용)는 제외
        if (gained && InventoryUI.LastItemUseFrame != Time.frameCount && pickupSFX != null && sfx != null)
            sfx.PlayOneShot(pickupSFX, pickupVolume);
    }

    private void RememberCounts()
    {
        if (Inventory.Instance == null || Inventory.Instance.itemData == null) { lastCounts = null; return; }
        var list = Inventory.Instance.itemData;
        lastCounts = new int[list.Count];
        for (int i = 0; i < list.Count; i++) lastCounts[i] = (int)list[i].itemCount;
    }
}
