using System; // 예외 처리 기능
using System.IO; // 저장 파일 기능
using System.Reflection; // 게임 상태 확인
using UnityEditor; // 편집기 기능
using UnityEditor.SceneManagement; // 장면 열기 기능
using UnityEngine; // 유니티 기능
using Moonlight; // 게임 상태 사용
public static class TitlePlaySmoke // 타이틀 이어하기 검사
{ // 검사 시작
    static bool requested; // 이어하기 요청 상태
    static int frames; // 화면 출력 대기 횟수
    static bool oldEnabled; // 기존 옵션 상태
    static EnterPlayModeOptions oldOptions; // 기존 옵션 내용
    static string oldProduct; // 기존 제품 이름
    public static void Run() // 검사 실행
    { // 검사 준비 시작
        oldEnabled = EditorSettings.enterPlayModeOptionsEnabled; // 기존 옵션 보관
        oldOptions = EditorSettings.enterPlayModeOptions; // 기존 세부 옵션 보관
        oldProduct = PlayerSettings.productName; // 기존 제품 이름 보관
        PlayerSettings.productName = "MoonlightIsolatedTitleSmoke"; // 전용 저장 경로
        EditorSettings.enterPlayModeOptionsEnabled = true; // 도메인 유지 설정
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload; // 검사 콜백 유지
        EditorSceneManager.OpenScene("Assets/Moonlight/Title.unity", OpenSceneMode.Single); // 타이틀 씬 열기
        EditorApplication.playModeStateChanged += OnMode; // 상태 감시 등록
        EditorApplication.isPlaying = true; // 플레이 시작
    } // 검사 준비 끝
    static void OnMode(PlayModeStateChange state) // 플레이 상태 변경
    { // 상태 변경 시작
        if (state == PlayModeStateChange.EnteredPlayMode) EditorApplication.update += Check; // 검사 등록
    } // 상태 변경 끝
    static void Check() // 실제 이어하기 확인
    { // 확인 시작
        try // 검사 예외 처리
        { // 검사 시도 시작
            string start = Path.Combine(Application.persistentDataPath, "moonlight_start.json"); // 시작 저장 경로
            frames++; // 화면 대기 횟수 증가
            if (frames == 100) ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath, "..", "title-preview.png")); // 타이틀 화면 저장
            if (frames < 120) return; // 타이틀 표시 대기
            if (!requested) // 첫 검사 확인
            { // 이어하기 준비 시작
                string manual = Path.Combine(Application.persistentDataPath, "moonlight_manual.json"); // 수동 저장 경로
                Directory.CreateDirectory(Application.persistentDataPath); // 검사용 폴더 생성
                if (File.Exists(start)) File.Delete(start); // 이전 검사 시작 저장 정리
                GameState state = new GameState(new GameSettings()); // 저장용 게임 상태
                state.Chips = 7654321; // 검사할 칩 수
                SaveData data = new SaveData { state = state, scenePath = "Assets/Moonlight/Prototype.unity" }; // 저장 자료 구성
                File.WriteAllText(manual, JsonUtility.ToJson(data)); // 검사 저장 생성
                requested = true; // 중복 요청 방지
                if (!TitleMenu.BeginContinue()) throw new Exception("이어하기 요청 실패"); // 이어하기 실행
                return; // 씬 초기화 대기
            } // 이어하기 준비 끝
            Prototype player = UnityEngine.Object.FindFirstObjectByType<Prototype>(); // 플레이어 확인
            if (player == null) return; // 플레이어 대기
            GameState loaded = (GameState)typeof(Prototype).GetField("game", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player); // 복구된 게임 상태
            if (loaded == null) return; // 초기화 대기
            if (loaded.Chips != 7654321 || File.Exists(start)) throw new Exception("이어하기 저장 복구 실패"); // 상태와 시작 저장 확인
            Debug.Log("TITLE_PLAY_SMOKE_OK"); // 검사 성공 표시
            Finish(0); // 정상 종료
        } // 검사 시도 끝
        catch (Exception error) // 검사 실패 처리
        { // 실패 처리 시작
            Debug.LogException(error); // 오류 기록
            Finish(1); // 실패 종료
        } // 실패 처리 끝
    } // 확인 끝
    static void Finish(int code) // 검사 환경 복구
    { // 복구 시작
        EditorApplication.update -= Check; // 검사 등록 해제
        EditorApplication.playModeStateChanged -= OnMode; // 상태 감시 해제
        EditorSettings.enterPlayModeOptionsEnabled = oldEnabled; // 옵션 상태 복원
        EditorSettings.enterPlayModeOptions = oldOptions; // 세부 옵션 복원
        PlayerSettings.productName = oldProduct; // 제품 이름 복원
        EditorApplication.Exit(code); // 편집기 종료
    } // 복구 끝
} // 검사 끝
