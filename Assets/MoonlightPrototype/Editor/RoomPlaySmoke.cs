using System; // 예외 처리 기능
using System.Reflection; // 게임 내부 기능 확인
using UnityEditor; // 편집기 기능
using UnityEditor.SceneManagement; // 씬 열기 기능
using UnityEngine; // 유니티 기능
using UnityEngine.SceneManagement; // 씬 상태 확인
using Moonlight; // 게임 상태 사용
public static class RoomPlaySmoke // 객실 왕복과 휴식 검사
{ // 검사 시작
    static bool oldEnabled; // 기존 플레이 옵션
    static EnterPlayModeOptions oldOptions; // 기존 세부 옵션
    static string oldProduct; // 기존 제품 이름
    static int step; // 씬 왕복 검사 단계
    public static void Run() // 검사 시작
    { // 검사 준비 시작
        oldEnabled = EditorSettings.enterPlayModeOptionsEnabled; // 기존 옵션 보관
        oldOptions = EditorSettings.enterPlayModeOptions; // 세부 옵션 보관
        oldProduct = PlayerSettings.productName; // 저장 경로 이름 보관
        PlayerSettings.productName = "MoonlightIsolatedRoomSmoke"; // 검사용 저장 경로
        EditorSettings.enterPlayModeOptionsEnabled = true; // 검사 콜백 유지
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload; // 도메인 재시작 방지
        EditorSceneManager.OpenScene("Assets/Moonlight/Prototype.unity", OpenSceneMode.Single); // 카지노 씬 열기
        EditorApplication.playModeStateChanged += OnMode; // 플레이 상태 감시
        EditorApplication.isPlaying = true; // 플레이 시작
    } // 검사 준비 끝
    static void OnMode(PlayModeStateChange state) // 플레이 상태 처리
    { // 상태 처리 시작
        if (state == PlayModeStateChange.EnteredPlayMode) EditorApplication.update += Check; // 검사 등록
    } // 상태 처리 끝
    static void Check() // 객실과 휴식 확인
    { // 확인 시작
        try // 검사 예외 처리
        { // 검사 시도 시작
            Prototype player = UnityEngine.Object.FindFirstObjectByType<Prototype>(); // 플레이어 확인
            if (player == null) return; // 플레이어 초기화 대기
            FieldInfo field = typeof(Prototype).GetField("game", BindingFlags.Instance | BindingFlags.NonPublic); // 게임 상태 필드
            GameState game = (GameState)field.GetValue(player); // 현재 게임 상태
            if (game == null) return; // 게임 초기화 대기
            CharacterController controller = player.GetComponent<CharacterController>(); // 이동 충돌체
            if (step == 0) // 카지노 입구 단계
            { // 입구 이동 시작
                game.Chips = 200000000; game.Minutes = 0; game.NextBill = 30; game.Energy = 40; // 청구와 정력 조건
                controller.enabled = false; player.transform.position = new Vector3(12, 0.1f, -15); controller.enabled = true; // 객실 입구 이동
                typeof(Prototype).GetMethod("Interact", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(player, null); // 실제 입구 상호작용
                step = 1; // 객실 로딩 단계
                return; // 로딩 완료 대기
            } // 입구 이동 끝
            bool transitioning = (bool)typeof(Prototype).GetField("isTransitioning", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player); // 씬 전환 상태
            if (transitioning) return; // 씬 로딩 완료 대기
            if (step == 2) // 카지노 복귀 단계
            { // 복귀 검증 시작
                if (SceneManager.GetActiveScene().path != "Assets/Moonlight/Prototype.unity") throw new Exception("카지노 복귀 실패"); // 카지노 복귀 확인
                typeof(Prototype).GetMethod("Load", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(player, new object[] { "manual" }); // 객실 저장 불러오기
                step = 3; // 객실 복구 대기
                return; // 저장 씬 로딩 대기
            } // 복귀 검증 끝
            if (step == 3) // 객실 저장 복구 단계
            { // 객실 복구 검증 시작
                if (SceneManager.GetActiveScene().path != "Assets/Moonlight/Room.unity" || !game.RestActive || game.Energy != 40) throw new Exception("다른 씬에서 객실 휴식 복구 실패"); // 객실 저장 상태 확인
                Debug.Log("ROOM_PLAY_SMOKE_OK"); // 검사 성공 표시
                Finish(0); // 정상 종료
                return; // 검사 종료
            } // 객실 복구 검증 끝
            if (SceneManager.GetActiveScene().path != "Assets/Moonlight/Room.unity") throw new Exception("객실 이동 실패"); // 객실 씬 확인
            string eventPath = System.IO.Path.Combine(Application.persistentDataPath, "moonlight_event.json"); // 자동 저장 경로
            SaveData arrived = JsonUtility.FromJson<SaveData>(System.IO.File.ReadAllText(eventPath)); // 도착 자동 저장 확인
            if (arrived.scenePath != "Assets/Moonlight/Room.unity") throw new Exception("객실 도착 저장 씬 오류"); // 도착 씬 기록 확인
            game.Minutes = 0; game.NextBill = 30; // 휴식 경계 시각 재설정
            controller.enabled = false; player.transform.position = new Vector3(0, 0.1f, -1); controller.enabled = true; // 침대 근처 이동
            typeof(Prototype).GetMethod("Interact", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(player, null); // 실제 침대 상호작용
            if ((string)typeof(Prototype).GetField("panel", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player) != "bed") throw new Exception("침대 상호작용 실패"); // 휴식 화면 확인
            if (!game.StartRest(player.settings)) throw new Exception("객실 휴식 시작 실패"); // 휴식 시작
            MethodInfo advance = typeof(Prototype).GetMethod("Advance", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 시계 처리
            MethodInfo save = typeof(Prototype).GetMethod("Save", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 저장 처리
            MethodInfo load = typeof(Prototype).GetMethod("Load", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 복구 처리
            advance.Invoke(player, new object[] { 20d }); // 휴식 중 시간 진행
            save.Invoke(player, new object[] { "manual" }); // 진행 중 휴식 저장
            advance.Invoke(player, new object[] { 15d }); // 휴식 중 청구 진행
            if (game.BillCount != 1 || game.Energy != 40 || !game.RestActive) throw new Exception("휴식 중 청구 처리 실패"); // 청구와 휴식 유지 확인
            load.Invoke(player, new object[] { "manual" }); // 휴식 중 저장 복구
            game = (GameState)field.GetValue(player); // 복구된 상태 확인
            advance.Invoke(player, new object[] { 40d }); // 청구와 휴식 완료 진행
            if (game.BillCount != 1 || game.Energy != 60 || game.RestActive) throw new Exception("휴식 저장 복구 실패"); // 청구와 회복 확인
            advance.Invoke(player, new object[] { 1d }); // 완료 후 추가 시간
            if (game.Energy != 60 || game.BillCount != 1) throw new Exception("휴식 또는 청구 중복 처리"); // 중복 처리 방지 확인
            load.Invoke(player, new object[] { "after_bill" }); // 청구 직후 저장 복구
            game = (GameState)field.GetValue(player); // 청구 직후 상태 확인
            advance.Invoke(player, new object[] { 30d }); // 남은 휴식 시간 진행
            if (game.Energy != 60 || game.BillCount != 1 || game.RestActive) throw new Exception("청구 직후 휴식 복구 실패"); // 단일 회복 확인
            controller.enabled = false; player.transform.position = new Vector3(0, 0.1f, -3); controller.enabled = true; // 객실 문 이동
            typeof(Prototype).GetMethod("Interact", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(player, null); // 실제 복귀 상호작용
            step = 2; // 카지노 로딩 단계
        } // 검사 시도 끝
        catch (Exception error) // 검사 실패 처리
        { // 실패 처리 시작
            Debug.LogException(error); // 오류 기록
            Finish(1); // 실패 종료
        } // 실패 처리 끝
    } // 확인 끝
    static void Finish(int code) // 검사 환경 복원
    { // 복원 시작
        EditorApplication.update -= Check; // 검사 해제
        EditorApplication.playModeStateChanged -= OnMode; // 상태 감시 해제
        EditorSettings.enterPlayModeOptionsEnabled = oldEnabled; // 옵션 복원
        EditorSettings.enterPlayModeOptions = oldOptions; // 세부 옵션 복원
        PlayerSettings.productName = oldProduct; // 제품 이름 복원
        EditorApplication.Exit(code); // 편집기 종료
    } // 복원 끝
} // 검사 끝

