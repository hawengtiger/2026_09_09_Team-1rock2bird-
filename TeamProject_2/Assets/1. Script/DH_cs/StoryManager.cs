using UnityEngine;

public class StoryManager : MonoBehaviour
{
    public int storyIndex = 0;
    public int storychapter = 0;

    public Transform Player;
    public Transform GoDreamTF;

    void Start()
    {
        storyIndex = 0;
    }


    void Update()
    {
        
    }

    public void StoryGoDream()
    {
        // 위치 동기화
        Player.position = GoDreamTF.position;
        Debug.Log($"이동 완료: {GoDreamTF.name} 위치로 이동했습니다.");
    }

}
