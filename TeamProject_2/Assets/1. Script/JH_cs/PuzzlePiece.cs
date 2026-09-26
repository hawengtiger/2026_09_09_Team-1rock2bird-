using UnityEngine;

public class PuzzlePiece : MonoBehaviour
{
    [Header("정답 조건 설정")]
    public Vector2 targetPosition;   // 이 조각이 가야 할 정답 위치
    public float targetRotation = 0f; // 이 조각의 정답 회전각
    
    [Header("판정 기준 (기획서 수치)")]
    public float snapDistance = 4f;       // 정답 위치와 일정 거리(4) 이내 접근 시[cite: 3]
    public float snapRotationAngle = 4f;  // 회전 각도 오차 ±4도[cite: 1]

    private bool isSnapped = false;
    public bool IsSnapped => isSnapped;

    // 조각이 정답 위치에 맞았는지 검증[cite: 1]
    public void CheckSnap()
    {
        if (isSnapped) return;

        // 위치 거리와 회전 오차 계산
        float distance = Vector2.Distance(transform.position, targetPosition);
        float angleDiff = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, targetRotation));

        // 두 조건이 모두 만족되면 정답 위치로 자석처럼 착 붙는 연출[cite: 1, 3]
        if (distance <= snapDistance && angleDiff <= snapRotationAngle)
        {
            SnapToTarget();
        }
    }

    private void SnapToTarget()
    {
        isSnapped = true;
      
        transform.position = targetPosition;
        transform.rotation = Quaternion.Euler(0, 0, targetRotation);
        
           }

    // 슬라이더 바를 통한 회전 적용
    public void SetRotation(float angle)
    {
        if (isSnapped) return; // 이미 정답에 맞췄다면 회전 불가
        transform.rotation = Quaternion.Euler(0, 0, angle);
        CheckSnap(); // 회전할 때마다 정답과 맞는지 체크
    }
}