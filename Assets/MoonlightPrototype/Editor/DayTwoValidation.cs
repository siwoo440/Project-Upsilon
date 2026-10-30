using System; // 예외 처리 사용
using System.Collections.Generic; // 탐색 자료 사용
using UnityEditor; // 편집기 기능 사용
using UnityEditor.SceneManagement; // 임시 장면 사용
using UnityEngine; // 유니티 기능 사용
using Moonlight; // 카지노 맵 사용
public static class DayTwoValidation // 이동 경로 검증 도구
{ // 검증 도구 시작
    public static void Run() // 임시 맵의 경로 검사
    { // 검사 시작
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); // 임시 장면 생성
        CasinoMap.Build(); // 카지노 맵 생성
        Physics.SyncTransforms(); // 충돌 위치 반영
        Queue<Vector2Int> queue = new Queue<Vector2Int>(); // 탐색 대기열
        HashSet<Vector2Int> seen = new HashSet<Vector2Int>(); // 방문 위치 기록
        HashSet<string> reached = new HashSet<string>(); // 도달한 상호작용 대상
        Vector2Int start = new Vector2Int(0, -36); // 입구 시작 좌표
        queue.Enqueue(start); // 첫 위치 등록
        seen.Add(start); // 첫 위치 방문 기록
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right }; // 인접 방향
        while (queue.Count > 0) // 이동 가능한 위치 탐색
        { // 탐색 시작
            Vector2Int point = queue.Dequeue(); // 다음 위치 선택
            float x = point.x * 0.5f; // 가로 실제 좌표
            float z = point.y * 0.5f; // 세로 실제 좌표
            string target = MapLayout.Nearest(x, z); // 주변 상호작용 대상
            if (target != "") // 대상 존재 확인
            { // 대상 기록 시작
                reached.Add(target); // 도달 대상 기록
            } // 대상 기록 끝
            foreach (Vector2Int direction in directions) // 인접 위치 확인
            { // 인접 탐색 시작
                Vector2Int next = point + direction; // 다음 격자 위치
                if (Mathf.Abs(next.x) > 41 || Mathf.Abs(next.y) > 41 || seen.Contains(next)) // 경계와 중복 확인
                { // 제외 시작
                    continue; // 위치 건너뛰기
                } // 제외 끝
                seen.Add(next); // 위치 방문 기록
                Vector3 bottom = new Vector3(next.x * 0.5f, 0.38f, next.y * 0.5f); // 플레이어 하단
                Vector3 top = bottom + Vector3.up * 1.05f; // 플레이어 상단
                if (!Physics.CheckCapsule(bottom, top, 0.3f)) // 통로 충돌 확인
                { // 이동 가능 시작
                    queue.Enqueue(next); // 다음 탐색 위치 등록
                } // 이동 가능 끝
            } // 인접 탐색 끝
        } // 탐색 끝
        foreach (string name in new string[] { "luna", "work", "blackjack", "cashier" }) // 필수 대상 확인
        { // 대상 검사 시작
            if (!reached.Contains(name)) // 도달 실패 확인
            { // 실패 처리 시작
                throw new Exception("도달할 수 없는 상호작용: " + name); // 실패 알림
            } // 실패 처리 끝
        } // 대상 검사 끝
        Debug.Log("DAY2_VALIDATION_OK 위치 " + seen.Count + " 대상 " + reached.Count); // 성공 결과 기록
    } // 검사 끝
} // 검증 도구 끝
