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
        HashSet<string> zones = new HashSet<string>(); // 도달한 장소 기록
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
            zones.Add(MapLayout.Zone(x, z)); // 현재 장소 기록
            if (target != "") // 대상 존재 확인
            { // 대상 기록 시작
                reached.Add(target); // 도달 대상 기록
            } // 대상 기록 끝
            foreach (Vector2Int direction in directions) // 인접 위치 확인
            { // 인접 탐색 시작
                Vector2Int next = point + direction; // 다음 격자 위치
                if (Mathf.Abs(next.x) > 107 || Mathf.Abs(next.y) > 95 || seen.Contains(next)) // 경계와 중복 확인
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
        foreach (string zone in new string[] { "입구·안내 데스크", "중앙 홀", "블랙잭·카드 홀", "룰렛 구역", "슬롯머신 구역", "바·휴식 공간", "카드 게임룸", "공연 무대", "VIP 라운지" }) // 필수 장소 확인
        { // 장소 검사 시작
            if (!zones.Contains(zone)) // 도달 실패 확인
            { // 장소 실패 시작
                throw new Exception("도달할 수 없는 장소: " + zone); // 실패 알림
            } // 장소 실패 끝
        } // 장소 검사 끝
        Debug.Log("CASINO_ZONES_OK " + zones.Count); // 장소 검증 성공
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
        GameState memberGame = new GameState(settings); // 회원 승급 검증 상태
        memberGame.Chips = 122400000; memberGame.Member.Experience = 70; // 청구와 경험치 조건
        if (!memberGame.SettleBill(settings) || memberGame.Member.Rank != 1 || memberGame.Chips != settings.silverBonus) // 승급 결과 확인
        { // 승급 실패 시작
            throw new Exception("회원 승급 검증 실패"); // 실패 알림
        } // 승급 실패 끝
        GameState memberRestored = JsonUtility.FromJson<GameState>(JsonUtility.ToJson(memberGame)); // 회원 저장 복구
        long savedChips = memberRestored.Chips; // 복구 칩 보관
        memberRestored.CheckPromotion(settings); // 복구 후 승급 재검사
        if (memberRestored.Member.Rank != 1 || !memberRestored.Member.SilverBonusPaid || memberRestored.Chips != savedChips) // 보너스 중복 확인
        { // 회원 복구 실패 시작
            throw new Exception("회원 저장 복구 검증 실패"); // 실패 알림
        } // 회원 복구 실패 끝
        Debug.Log("MEMBERSHIP_JSON_OK"); // 회원 저장 검증 성공
        GameState legacy = JsonUtility.FromJson<GameState>("{\"Chips\":1234,\"Debt\":9000}"); // 이전 형식 저장 자료 복구
        legacy.EnsureMembership(); // 누락된 회원 정보 생성
        if (legacy.Chips != 1234 || legacy.Member.Rank != 0 || legacy.Member.Experience != 0) // 기존 저장 보존 확인
        { // 이전 저장 실패 시작
            throw new Exception("기존 저장 회원 정보 변환 실패"); // 실패 알림
        } // 이전 저장 실패 끝
        Debug.Log("MEMBERSHIP_LEGACY_OK"); // 기존 저장 검증 성공
        EditorSceneManager.OpenScene("Assets/Moonlight/Prototype.unity", OpenSceneMode.Single); // 실제 시제품 장면 열기
        Prototype prototype = UnityEngine.Object.FindFirstObjectByType<Prototype>(); // 장면 설정 확인
        if (prototype == null || prototype.settings.startingChips != 10000000 || prototype.settings.silverBonus != 20000000 || prototype.settings.workExperience != 10) // 장면 설정값 확인
        { // 장면 설정 실패 시작
            throw new Exception("회원 기본 설정이 장면에 반영되지 않았습니다."); // 실패 알림
        } // 장면 설정 실패 끝
        Debug.Log("MEMBERSHIP_SCENE_OK"); // 실제 장면 설정 검증 성공
        if (prototype.settings.walkSpeed != 4 || prototype.settings.runSpeed != 7) // 이동 기본 속도 확인
        { // 속도 설정 실패 시작
            throw new Exception("이동 속도 설정 오류"); // 실패 알림
        } // 속도 설정 실패 끝
        Debug.Log("SPRINT_SCENE_OK"); // 달리기 설정 검증 성공
    } // 검사 끝
} // 검증 도구 끝
