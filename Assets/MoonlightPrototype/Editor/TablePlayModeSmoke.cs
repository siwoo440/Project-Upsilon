using System; // 기본 기능 사용
using System.Reflection; // 내부 상태 확인
using UnityEditor; // 편집기 기능 사용
using UnityEditor.SceneManagement; // 장면 열기 기능
using UnityEngine; // 유니티 기능 사용
using Moonlight; // 게임 상태 사용
public static class TablePlayModeSmoke // 플레이 모드 검증
{ // 검증 도구 시작
    static bool oldEnabled; // 기존 플레이 설정 사용 여부
    static EnterPlayModeOptions oldOptions; // 기존 플레이 옵션
    static string oldProduct; // 기존 제품 이름
    static double deadline; // 검증 제한 시각
    public static void Run() // 격리된 플레이 검증 시작
    { // 검증 시작 범위
        oldEnabled = EditorSettings.enterPlayModeOptionsEnabled; // 기존 옵션 보관
        oldOptions = EditorSettings.enterPlayModeOptions; // 기존 세부 옵션 보관
        oldProduct = PlayerSettings.productName; // 기존 저장 경로 식별자 보관
        PlayerSettings.productName = "MoonlightIsolatedTableSmoke"; // 검증 전용 저장 경로
        EditorSettings.enterPlayModeOptionsEnabled = true; // 도메인 유지 설정 사용
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload; // 검증 콜백 유지
        EditorSceneManager.OpenScene("Assets/Moonlight/Prototype.unity", OpenSceneMode.Single); // 시제품 장면 열기
        deadline = EditorApplication.timeSinceStartup + 45; // 검증 제한 시간 설정
        EditorApplication.playModeStateChanged += OnModeChanged; // 플레이 상태 알림 등록
        EditorApplication.isPlaying = true; // 플레이 모드 시작
    } // 검증 시작 끝
    static void OnModeChanged(PlayModeStateChange state) // 플레이 상태 변경 처리
    { // 상태 처리 시작
        if (state == PlayModeStateChange.EnteredPlayMode) // 플레이 진입 확인
        { // 실행 대기 시작
            EditorApplication.update += Check; // 초기화 후 검증 등록
        } // 실행 대기 끝
    } // 상태 처리 끝
    static void Check() // 실제 게임 구성 확인
    { // 확인 시작
        try // 검증 예외 처리
        { // 검증 시도 시작
            Prototype player = UnityEngine.Object.FindFirstObjectByType<Prototype>(); // 플레이어 찾기
            if (player == null) return; // 초기화 전 대기
            FieldInfo field = typeof(Prototype).GetField("game", BindingFlags.Instance | BindingFlags.NonPublic); // 게임 상태 필드
            GameState game = (GameState)field.GetValue(player); // 현재 게임 상태
            if (!Application.persistentDataPath.EndsWith("MoonlightIsolatedTableSmoke")) throw new Exception("검증 저장 경로 격리 실패"); // 저장 경로 격리 확인
            if (game == null) // 시작 처리 대기
            { // 대기 처리 시작
                if (EditorApplication.timeSinceStartup > deadline) throw new Exception("플레이어 초기화 시간 초과"); // 시간 초과 확인
                return; // 다음 프레임 대기
            } // 대기 처리 끝
            CharacterController controller = player.GetComponent<CharacterController>(); // 이동 충돌체 확인
            controller.enabled = false; player.transform.position = new Vector3(1, 0.1f, 1.4f); controller.enabled = true; // 의자 근처 배치
            typeof(Prototype).GetMethod("Interact", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(player, null); // 실제 상호작용 실행
            if (!game.Seated) throw new Exception("의자 상호작용 실패"); // 착석 결과 확인
            Camera camera = player.GetComponentInChildren<Camera>(); // 플레이어 카메라 확인
            if (Vector3.Distance(camera.transform.position, new Vector3(1, 1.55f, 2.9f)) > 0.01f) throw new Exception("착석 카메라 위치 오류"); // 착석 시점 확인
            game.Round = new Round { phase = 1, wager = 100, player = new System.Collections.Generic.List<int> { 10, 8 }, dealer = new System.Collections.Generic.List<int> { 1, 6 } }; // 테스트 카드 설정
            player.GetComponent<BlackjackVisuals>().Sync(game); // 실제 카드 표시 갱신
            if (GameObject.Find("비공개 카드") == null || GameObject.Find("공개 카드") == null || GameObject.Find("베팅 칩") == null) throw new Exception("카드 또는 칩 표시 실패"); // 게임 물체 확인
            if (game.LeaveBlackjack()) throw new Exception("진행 중 이탈 허용 오류"); // 라운드 중 이탈 확인
            string state = JsonUtility.ToJson(game); // 착석 상태 저장 형식
            GameState restored = JsonUtility.FromJson<GameState>(state); // 착석 상태 복구
            if (!restored.Seated || restored.Round.wager != 100) throw new Exception("착석 복구 오류"); // 저장 복구 확인
            typeof(Prototype).GetMethod("Save", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(player, new object[] { "manual" }); // 실제 수동 저장 실행
            game.Seated = false; game.Round.wager = 0; // 검증용 상태 변경
            typeof(Prototype).GetMethod("Load", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(player, new object[] { "manual" }); // 실제 수동 불러오기 실행
            game = (GameState)field.GetValue(player); // 불러온 상태 확인
            if (!game.Seated || game.Round.wager != 100 || Vector3.Distance(camera.transform.position, new Vector3(1, 1.55f, 2.9f)) > 0.01f) throw new Exception("수동 저장의 착석 복구 실패"); // 파일 복구 확인
            game.Stand(new GameSettings()); // 진행 중 라운드 정산
            if (!game.LeaveBlackjack()) throw new Exception("정산 후 퇴장 실패"); // 일어나기 확인
            typeof(Prototype).GetMethod("UpdateCameraPose", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(player, null); // 이동 시점 복원
            if (Vector3.Distance(camera.transform.localPosition, new Vector3(0, 1.6f, 0)) > 0.01f) throw new Exception("이동 카메라 복원 실패"); // 복귀 시점 확인
            Debug.Log("TABLE_PLAYMODE_SMOKE_OK"); // 실제 플레이 상태 검증 성공
            Finish(0); // 정상 종료
        } // 검증 시도 끝
        catch (Exception error) // 검증 실패 처리
        { // 실패 처리 시작
            Debug.LogException(error); // 오류 기록
            Finish(1); // 실패 종료
        } // 실패 처리 끝
    } // 확인 끝
    static void Finish(int code) // 검증 환경 복구
    { // 환경 복구 시작
        EditorApplication.update -= Check; // 검증 콜백 해제
        EditorApplication.playModeStateChanged -= OnModeChanged; // 상태 콜백 해제
        EditorSettings.enterPlayModeOptionsEnabled = oldEnabled; // 기존 옵션 복원
        EditorSettings.enterPlayModeOptions = oldOptions; // 기존 세부 옵션 복원
        PlayerSettings.productName = oldProduct; // 기존 제품 이름 복원
        EditorApplication.Exit(code); // 편집기 종료
    } // 환경 복구 끝
} // 검증 도구 끝
