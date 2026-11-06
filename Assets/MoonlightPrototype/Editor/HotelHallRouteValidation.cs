using System; // 예외 처리 기능
using System.Collections.Generic; // 경로 탐색 자료
using UnityEditor.SceneManagement; // 씬 열기 기능
using UnityEngine; // 충돌과 위치 기능
using Moonlight; // 상호작용 지점 사용
public static class HotelHallRouteValidation // 복도 이동 경로 검사
{ // 검사 시작
    public static void Run() // 복도 경로 검사
    { // 검사 준비 시작
        EditorSceneManager.OpenScene("Assets/Moonlight/HotelHall.unity", OpenSceneMode.Single); // 복도 씬 열기
        SceneInteractionPoint[] points = UnityEngine.Object.FindObjectsByType<SceneInteractionPoint>(FindObjectsSortMode.None); // 복도 상호작용 지점
        Physics.SyncTransforms(); // 충돌 위치 갱신
        Queue<Vector2Int> queue = new Queue<Vector2Int>(); // 이동 대기 위치
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>(); // 확인한 위치
        HashSet<string> reached = new HashSet<string>(); // 도달한 대상
        Vector2Int start = new Vector2Int(0, -12); // 카지노 쪽 도착 위치
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right }; // 이동 방향
        queue.Enqueue(start); visited.Add(start); // 시작 위치 등록
        while (queue.Count > 0) // 이동 가능한 위치 탐색
        { // 탐색 시작
            Vector2Int current = queue.Dequeue(); // 다음 위치
            string target = SceneInteractionPoint.NearestFrom(new Vector3(current.x * 0.5f, 0, current.y * 0.5f), points); // 주변 상호작용 대상
            if (target != "") reached.Add(target); // 접근 대상 기록
            foreach (Vector2Int direction in directions) // 인접 위치 확인
            { // 인접 확인 시작
                Vector2Int next = current + direction; // 다음 격자
                if (Mathf.Abs(next.x) > 8 || Mathf.Abs(next.y) > 20 || !visited.Add(next)) continue; // 범위와 중복 확인
                Vector3 bottom = new Vector3(next.x * 0.5f, 0.38f, next.y * 0.5f); // 플레이어 하단
                if (!Physics.CheckCapsule(bottom, bottom + Vector3.up * 1.05f, 0.3f)) queue.Enqueue(next); // 충돌 없는 위치 추가
            } // 인접 확인 끝
        } // 탐색 끝
        if (!reached.Contains("casino") || !reached.Contains("guestroom") || !reached.Contains("elevator") || !reached.Contains("staff")) throw new Exception("복도의 주요 출입 지점에 접근할 수 없습니다."); // 필수 경로 확인
        Debug.Log("HOTEL_HALL_ROUTE_OK " + visited.Count); // 검사 성공 표시
    } // 검사 끝
} // 검사 끝
