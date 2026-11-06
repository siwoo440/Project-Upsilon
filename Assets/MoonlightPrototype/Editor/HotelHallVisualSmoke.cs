using System; // 예외 처리 기능
using System.IO; // 화면 파일 경로
using System.Reflection; // 전환 상태 확인
using UnityEditor; // 편집기 기능
using UnityEditor.SceneManagement; // 씬 열기 기능
using UnityEngine; // 화면 캡처 기능
using Moonlight; // 게임 기능
public static class HotelHallVisualSmoke // 실제 복도 화면 확인
{ // 확인 시작
    static bool requested, oldEnabled; // 이동 요청과 기존 옵션
    static int frames; // 화면 대기 횟수
    static EnterPlayModeOptions oldOptions; // 기존 세부 옵션
    static string oldProduct; // 기존 저장 경로 이름
    public static void Run() // 화면 검사 시작
    { // 검사 준비 시작
        oldEnabled = EditorSettings.enterPlayModeOptionsEnabled; // 기존 옵션 보관
        oldOptions = EditorSettings.enterPlayModeOptions; // 세부 옵션 보관
        oldProduct = PlayerSettings.productName; // 기존 제품 이름 보관
        PlayerSettings.productName = "MoonlightIsolatedHallVisual"; // 검사용 저장 경로
        EditorSettings.enterPlayModeOptionsEnabled = true; // 도메인 유지 설정
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload; // 검사 콜백 유지
        EditorSceneManager.OpenScene("Assets/Moonlight/Prototype.unity", OpenSceneMode.Single); // 카지노 씬 열기
        EditorApplication.playModeStateChanged += OnMode; // 플레이 상태 감시
        EditorApplication.isPlaying = true; // 플레이 시작
    } // 검사 준비 끝
    static void OnMode(PlayModeStateChange state) // 플레이 상태 처리
    { // 상태 처리 시작
        if (state == PlayModeStateChange.EnteredPlayMode) EditorApplication.update += Check; // 검사 등록
    } // 상태 처리 끝
    static void Check() // 복도 화면 확인
    { // 확인 시작
        try // 검사 예외 처리
        { // 검사 시도 시작
            Prototype player = UnityEngine.Object.FindFirstObjectByType<Prototype>(); // 플레이어 확인
            if (player == null) return; // 플레이어 대기
            if (typeof(Prototype).GetField("game", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player) == null) return; // 게임 초기화 대기
            if (!requested) // 복도 이동 요청 확인
            { // 복도 이동 시작
                requested = player.TravelToScene("Assets/Moonlight/HotelHall.unity", new Vector3(0, 0.1f, -6)); // 복도 이동 요청
                if (!requested) throw new Exception("복도 이동 요청 실패"); // 이동 실패 확인
                return; // 로딩 대기
            } // 복도 이동 끝
            if ((bool)typeof(Prototype).GetField("isTransitioning", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)) return; // 로딩 완료 대기
            frames++; // 화면 대기 횟수 증가
            if (frames == 80) ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath, "..", "hotel-hall-preview.png")); // 복도 화면 캡처
            if (frames < 120) return; // 캡처 저장 대기
            Debug.Log("HOTEL_HALL_VISUAL_OK"); // 화면 확인 성공
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
        EditorApplication.update -= Check; // 검사 등록 해제
        EditorApplication.playModeStateChanged -= OnMode; // 상태 감시 해제
        EditorSettings.enterPlayModeOptionsEnabled = oldEnabled; // 옵션 복원
        EditorSettings.enterPlayModeOptions = oldOptions; // 세부 옵션 복원
        PlayerSettings.productName = oldProduct; // 제품 이름 복원
        EditorApplication.Exit(code); // 편집기 종료
    } // 복원 끝
} // 확인 끝
