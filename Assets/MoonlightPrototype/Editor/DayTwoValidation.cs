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
        GameObject table = GameObject.Find("블랙잭 모델"); // 가져온 테이블 확인
        if (table == null || table.GetComponentsInChildren<MeshRenderer>().Length < 3 || table.GetComponent<BoxCollider>() == null) // 모델과 충돌체 확인
        { // 모델 실패 시작
            throw new Exception("블랙잭 모델 가져오기 실패"); // 실패 알림
        } // 모델 실패 끝
        Debug.Log("BLACKJACK_MODEL_OK"); // 모델 검증 성공
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
        GameSettings settings = new GameSettings(); // 기본 게임 설정
        GameState original = new GameState(settings); // 저장할 게임 상태
        original.Chips = 10000000; // 검증용 보유 칩
        original.StartWork(settings); // 진행 중 업무 시작
        original.StartBlackjack(settings, 1000000); // 블랙잭 라운드 시작
        string json = JsonUtility.ToJson(original); // 유니티 저장 형식 변환
        GameState restored = JsonUtility.FromJson<GameState>(json); // 저장 상태 복구
        if (restored.Chips != original.Chips || restored.WorkActive != original.WorkActive || restored.WorkRemaining != original.WorkRemaining || restored.RandomState != original.RandomState || restored.Round.wager != original.Round.wager || restored.Round.phase != original.Round.phase || restored.Round.shoe.Count != original.Round.shoe.Count) // 복구 상태 비교
        { // 복구 실패 시작
            throw new Exception("Unity 저장 상태 복구 실패"); // 실패 알림
        } // 복구 실패 끝
        Debug.Log("DAY8_JSON_OK"); // 직렬화 검증 성공
        SaveData save = new SaveData { state = original, x = 1, y = 2, z = 3, yaw = 45, pitch = -10 }; // 위치 포함 저장 자료
        string savePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "moonlight_day8_" + Guid.NewGuid().ToString("N") + ".json"); // 독립 임시 저장 경로
        try // 저장 파일 검증
        { // 파일 검증 시작
            System.IO.File.WriteAllText(savePath, JsonUtility.ToJson(save)); // 저장 파일 작성
            SaveData loaded = JsonUtility.FromJson<SaveData>(System.IO.File.ReadAllText(savePath)); // 저장 파일 복구
            if (loaded.state.RandomState != original.RandomState || loaded.state.Round.wager != original.Round.wager || loaded.x != 1 || loaded.yaw != 45) // 저장 내용 확인
            { // 복구 실패 시작
                throw new Exception("수동 저장 파일 복구 실패"); // 실패 알림
            } // 복구 실패 끝
        } // 파일 검증 끝
        finally // 임시 파일 정리
        { // 정리 시작
            System.IO.File.Delete(savePath); // 검증 파일 삭제
        } // 정리 끝
        Debug.Log("DAY8_FILE_OK"); // 저장 파일 검증 성공
    } // 검사 끝
} // 검증 도구 끝
