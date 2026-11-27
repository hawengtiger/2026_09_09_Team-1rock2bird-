using UnityEngine;

// 73~86 연출 스크립트들이 같이 쓰는 공통 상태
// (팝업/퍼즐이 열려 있는 동안 F 상호작용, 이동 등을 막기 위해 사용)
public static class MemoryUI
{
    public static bool IsOpen = false;          // 사진/퍼즐/팝업/컷씬 중 하나라도 열려 있으면 true

    public static bool IsStory()
    {
        TextTyping t = Object.FindAnyObjectByType<TextTyping>();
        return t != null && t.darkUIEffect != null && t.darkUIEffect.isStory;
    }

    public static PlayerController FindPlayer()
    {
        return Object.FindAnyObjectByType<PlayerController>();
    }
}
