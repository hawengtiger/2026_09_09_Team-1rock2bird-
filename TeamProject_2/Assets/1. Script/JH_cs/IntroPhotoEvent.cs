using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// [73~] 대사가 끝난 뒤 플레이어가 움직이려고 하면 화면 중앙에 사진을 띄움
// → 빈 곳 클릭 시 사진이 좌측 상단으로 작아지며 인게임 복귀 (자유 조작)
// → 기믹 해결 후 DropPhoto()를 부르면 좌측 상단 사진이 '팔랑' 떨어짐
//
// 사용법: 사진 Image(photo)와, 위치 표시용 빈 RectTransform 2개(centerSlot, cornerSlot)를
//        같은 Canvas 안에 만들고 연결. 셋 다 Anchor/Pivot을 가운데(0.5)로 맞추기.
public class IntroPhotoEvent : MonoBehaviour
{
    [Header("사용 가능 여부")]
    public bool startReady = false;             // 테스트용 (보통은 앞 단계에서 SetReady)

    [Header("사진 UI")]
    public RectTransform photo;                 // 띄울 사진 (네컷 사진)
    public RectTransform centerSlot;            // 화면 중앙 크게 보일 때 위치/크기
    public RectTransform cornerSlot;            // 좌측 상단 작게 있을 때 위치/크기
    public GameObject guideText;                // "빈 곳을 클릭해 원래 화면으로 돌아가세요"
    public float moveTime = 0.6f;               // 중앙 → 좌측 상단 이동 시간

    [Header("떨어지는 연출")]
    public float fallDistance = 900f;           // 아래로 떨어지는 거리 (UI 픽셀)
    public float fallTime = 1.4f;
    public float swayAngle = 25f;               // 좌우로 흔들리는 각도
    public float swayWidth = 60f;               // 좌우로 흔들리는 폭

    [Header("끝났을 때 실행할 것")]
    public UnityEvent onReturnedToGame;         // 사진이 좌측 상단으로 간 뒤 (아이템 등장 등)
    public UnityEvent onPhotoDropped;           // 사진이 떨어진 뒤 (바닥 사진 아이템 켜기 등)

    private bool isReady;
    private bool isDone = false;
    private PlayerController player;

    private void Start()
    {
        isReady = startReady;
        player = MemoryUI.FindPlayer();

        if (photo != null) photo.gameObject.SetActive(false);
        if (guideText != null) guideText.SetActive(false);
    }

    // 앞 단계 On Finished에 연결
    public void SetReady()
    {
        isReady = true;
    }

    private void Update()
    {
        if (!isReady || isDone || MemoryUI.IsOpen) return;
        if (MemoryUI.IsStory()) return;
        if (Keyboard.current == null) return;

        // 플레이어가 움직이려고 하면
        bool tryMove = Keyboard.current.aKey.wasPressedThisFrame ||
                       Keyboard.current.dKey.wasPressedThisFrame ||
                       Keyboard.current.leftArrowKey.wasPressedThisFrame ||
                       Keyboard.current.rightArrowKey.wasPressedThisFrame;

        if (tryMove) StartCoroutine(ShowPhoto());
    }

    private IEnumerator ShowPhoto()
    {
        isDone = true;
        MemoryUI.IsOpen = true;
        if (player != null) player.canMove = false;

        // 1. 화면 중앙에 사진
        photo.gameObject.SetActive(true);
        CopySlot(centerSlot);
        photo.localRotation = Quaternion.identity;
        if (guideText != null) guideText.SetActive(true);

        // 바로 클릭이 먹지 않게 한 프레임 쉬기
        yield return null;

        // 2. 사진 바깥(빈 곳) 클릭 기다리기
        while (true)
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2 mouse = Mouse.current.position.ReadValue();
                if (!RectTransformUtility.RectangleContainsScreenPoint(photo, mouse, null))
                    break;
            }
            yield return null;
        }

        if (guideText != null) guideText.SetActive(false);

        // 3. 좌측 상단으로 작아지기
        yield return MoveTo(cornerSlot, moveTime);

        // 4. 인게임 복귀
        MemoryUI.IsOpen = false;
        if (player != null) player.canMove = true;

        onReturnedToGame?.Invoke();
    }

    // 기믹 해결 후 호출 → 좌측 상단 사진이 팔랑 떨어짐
    public void DropPhoto()
    {
        if (photo != null && photo.gameObject.activeInHierarchy)
            StartCoroutine(FallSequence());
        else
            onPhotoDropped?.Invoke();
    }

    private IEnumerator FallSequence()
    {
        Vector3 start = photo.position;
        float scale = photo.lossyScale.y;   // Canvas 크기에 맞춰 거리 보정
        float t = 0f;

        CanvasGroup cg = photo.GetComponent<CanvasGroup>();
        if (cg == null) cg = photo.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 1f;

        while (t < fallTime)
        {
            t += Time.deltaTime;
            float p = t / fallTime;

            float sway = Mathf.Sin(p * Mathf.PI * 3f);
            photo.position = start + new Vector3(sway * swayWidth * scale, -fallDistance * p * scale, 0f);
            photo.localRotation = Quaternion.Euler(0f, 0f, sway * swayAngle);
            cg.alpha = 1f - Mathf.Clamp01((p - 0.7f) / 0.3f);   // 끝부분에서 사라짐

            yield return null;
        }

        photo.gameObject.SetActive(false);
        cg.alpha = 1f;
        photo.localRotation = Quaternion.identity;

        onPhotoDropped?.Invoke();
    }

    private IEnumerator MoveTo(RectTransform slot, float time)
    {
        Vector3 startPos = photo.position;
        Vector2 startSize = photo.sizeDelta;
        float t = 0f;

        while (t < time)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / time);
            photo.position = Vector3.Lerp(startPos, slot.position, p);
            photo.sizeDelta = Vector2.Lerp(startSize, slot.sizeDelta, p);
            yield return null;
        }

        CopySlot(slot);
    }

    private void CopySlot(RectTransform slot)
    {
        if (slot == null) return;
        photo.position = slot.position;
        photo.sizeDelta = slot.sizeDelta;
    }
}
