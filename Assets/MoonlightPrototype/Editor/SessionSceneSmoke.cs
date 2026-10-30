using System; // 예외 처리 기능
using System.Reflection; // 게임 상태 확인
using UnityEditor; // 편집기 기능
using UnityEditor.SceneManagement; // 장면 열기 기능
using UnityEngine; // 유니티 기능
using UnityEngine.SceneManagement; // 씬 전환 기능
using Moonlight; // 게임 상태 사용
public static class SessionSceneSmoke // 씬 간 상태 유지 검사
{ // 검사 시작
    static AsyncOperation unloading; // 씬 종료 작업
    static GameState original; // 기존 게임 상태
    static int duplicateFrames; // 중복 객체 검사 대기
    static bool loadRequested; // 저장 복구 요청 상태
    static bool oldEnabled; // 기존 플레이 설정
    static EnterPlayModeOptions oldOptions; // 기존 세부 설정
    static string oldProduct; // 기존 제품 이름
    public static void Run() // 검사 실행
    { // 검사 준비 시작
        oldEnabled = EditorSettings.enterPlayModeOptionsEnabled; // 기존 설정 보관
        oldOptions = EditorSettings.enterPlayModeOptions; // 기존 세부 설정 보관
        oldProduct = PlayerSettings.productName; // 저장 경로 보관
        PlayerSettings.productName = "MoonlightIsolatedSessionSmoke"; // 검사용 저장 경로
        EditorSettings.enterPlayModeOptionsEnabled = true; // 콜백 유지 설정
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload; // 도메인 유지 설정
        EditorSceneManager.OpenScene("Assets/Moonlight/Prototype.unity", OpenSceneMode.Single); // 원본 씬 열기
        EditorApplication.playModeStateChanged += OnMode; // 플레이 상태 감시
        EditorApplication.isPlaying = true; // 플레이 시작
    } // 검사 준비 끝
    static void OnMode(PlayModeStateChange state) // 플레이 상태 처리
    { // 상태 처리 시작
        if (state == PlayModeStateChange.EnteredPlayMode) EditorApplication.update += Check; // 검사 등록
    } // 상태 처리 끝
    static void Check() // 상태 유지 확인
    { // 확인 시작
        try // 검사 예외 처리
        { // 검사 시도 시작
            if (unloading == null) // 첫 씬 확인
            { // 전환 준비 시작
                Prototype player = UnityEngine.Object.FindFirstObjectByType<Prototype>(); // 플레이어 확인
                if (player == null) return; // 초기화 대기
                original = (GameState)typeof(Prototype).GetField("game", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player); // 게임 상태 확인
                if (original == null) return; // 게임 초기화 대기
                original.Chips = 12345678; // 검사용 칩
                original.WorkActive = true; // 진행 중 업무
                original.WorkRemaining = 50; // 업무 남은 시간
                original.Round.phase = 1; // 진행 중 라운드
                original.Round.wager = 100; // 예약된 베팅금
                typeof(Prototype).GetMethod("Save", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(player, new object[] { "manual" }); // 현재 씬 저장
                string saved = System.IO.File.ReadAllText(System.IO.Path.Combine(Application.persistentDataPath, "moonlight_manual.json")); // 검사용 저장 파일
                if (!saved.Contains("Assets/Moonlight/Prototype.unity")) throw new Exception("저장 파일에 씬 경로가 없습니다."); // 저장 위치 확인
                Scene oldScene = SceneManager.GetActiveScene(); // 기존 씬 보관
                Scene nextScene = SceneManager.CreateScene("SessionSmokeDestination"); // 임시 도착 씬
                SceneManager.SetActiveScene(nextScene); // 도착 씬 활성화
                unloading = SceneManager.UnloadSceneAsync(oldScene); // 기존 씬 종료
                return; // 종료 대기
            } // 전환 준비 끝
            if (!unloading.isDone) return; // 씬 종료 대기
            if (duplicateFrames == 0) // 도착 씬 중복 생성 확인
            { // 중복 검사 준비
                new GameObject("Session Duplicate").AddComponent<Prototype>(); // 새 씬의 중복 플레이어 생성
                duplicateFrames = 1; // 중복 초기화 대기 시작
                return; // 다음 프레임 대기
            } // 중복 검사 준비 끝
            if (duplicateFrames++ < 5) return; // 중복 삭제 대기
            Prototype survivor = UnityEngine.Object.FindFirstObjectByType<Prototype>(); // 유지된 플레이어 확인
            if (survivor == null) throw new Exception("씬 전환 중 플레이어가 사라졌습니다."); // 플레이어 유지 확인
            if (UnityEngine.Object.FindObjectsByType<Prototype>(FindObjectsSortMode.None).Length != 1) throw new Exception("플레이어가 중복 생성됐습니다."); // 중복 세션 확인
            GameState current = (GameState)typeof(Prototype).GetField("game", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(survivor); // 유지된 게임 상태
            if (!loadRequested) // 저장 복구 전 확인
            { // 저장 복구 시작
                if (!ReferenceEquals(current, original) || current.Chips != 12345678 || !current.WorkActive || current.Round.wager != 100) throw new Exception("진행 중 게임 상태가 유지되지 않았습니다."); // 상태 보존 확인
                if (!survivor.TravelToScene("Assets/Moonlight/Prototype.unity", new Vector3(2, 0.1f, -10))) throw new Exception("등록된 씬 이동에 실패했습니다."); // 실제 씬 이동 확인
                if (Vector3.Distance(survivor.transform.position, new Vector3(2, 0.1f, -10)) > 0.01f) throw new Exception("씬 도착 위치가 다릅니다."); // 도착 위치 확인
                loadRequested = true; // 저장 복구 요청 기록
                typeof(Prototype).GetMethod("Load", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(survivor, new object[] { "manual" }); // 저장 씬 복구
                return; // 복구 완료 대기
            } // 저장 복구 끝
            if (SceneManager.GetActiveScene().path != "Assets/Moonlight/Prototype.unity" || current.Chips != 12345678 || !current.WorkActive || current.Round.wager != 100) throw new Exception("저장된 씬과 게임 상태가 복구되지 않았습니다."); // 씬과 상태 복구 확인
            Debug.Log("SESSION_SCENE_SMOKE_OK"); // 검사 성공 기록
            Finish(0); // 정상 종료
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
        EditorSettings.enterPlayModeOptionsEnabled = oldEnabled; // 설정 복원
        EditorSettings.enterPlayModeOptions = oldOptions; // 세부 설정 복원
        PlayerSettings.productName = oldProduct; // 제품 이름 복원
        EditorApplication.Exit(code); // 편집기 종료
    } // 복원 끝
} // 검사 끝

