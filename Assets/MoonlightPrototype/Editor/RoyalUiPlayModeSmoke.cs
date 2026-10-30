using System; // 예외 처리 기능
using System.Reflection; // 게임 내부 상태 확인
using UnityEditor; // 편집기 기능
using UnityEditor.SceneManagement; // 장면 열기 기능
using UnityEngine; // 유니티 기능
using Moonlight; // 게임 코드 사용
public static class RoyalUiPlayModeSmoke // UI 플레이 검사
{ // 검사 시작
    static int frame; // 검사 프레임 번호
    static string failure; // 화면 오류 보관
    static string oldProduct; // 기존 저장 경로 이름
    static bool oldEnabled; // 기존 플레이 옵션 상태
    static EnterPlayModeOptions oldOptions; // 기존 플레이 옵션 내용
    public static void Run() // 실제 화면 검사 시작
    { // 검사 준비 시작
        oldProduct = PlayerSettings.productName; // 기존 제품 이름 보관
        oldEnabled = EditorSettings.enterPlayModeOptionsEnabled; // 기존 옵션 상태 보관
        oldOptions = EditorSettings.enterPlayModeOptions; // 기존 옵션 내용 보관
        PlayerSettings.productName = "MoonlightIsolatedUiSmoke"; // 검사 전용 저장 경로
        EditorSettings.enterPlayModeOptionsEnabled = true; // 검사 콜백 유지 설정
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload; // 도메인 재시작 방지
        EditorSceneManager.OpenScene("Assets/Moonlight/Prototype.unity", OpenSceneMode.Single); // 시제품 장면 열기
        Application.logMessageReceived += Capture; // 오류 감시 등록
        EditorApplication.playModeStateChanged += OnMode; // 플레이 상태 감시
        EditorApplication.isPlaying = true; // 플레이 모드 시작
    } // 검사 준비 끝
    static void Capture(string condition, string stack, LogType type) // 화면 오류 감시
    { // 오류 감시 시작
        if ((type == LogType.Exception || type == LogType.Error) && stack.Contains("Moonlight.Prototype")) failure = condition; // 게임 화면 오류 기록
    } // 오류 감시 끝
    static void OnMode(PlayModeStateChange state) // 플레이 상태 처리
    { // 상태 처리 시작
        if (state == PlayModeStateChange.EnteredPlayMode) EditorApplication.update += Check; // 플레이 화면 검사 등록
    } // 상태 처리 끝
    static void Check() // 실제 UI 상태 전환
    { // 상태 전환 시작
        try // 검사 예외 처리
        { // 검사 시도 시작
            if (failure != null) throw new Exception("화면 오류: " + failure); // 화면 오류 확인
            Prototype player = UnityEngine.Object.FindFirstObjectByType<Prototype>(); // 플레이어 찾기
            if (player == null) return; // 플레이어 대기
            GameState game = (GameState)typeof(Prototype).GetField("game", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player); // 실제 게임 상태
            if (game == null) return; // 게임 초기화 대기
            frame++; // 검사 프레임 증가
            if (frame == 1) game.Chips = game.Debt = 999999999999999999; // 긴 금액 표시 검사
            if (frame < 90) return; // 첫 화면 렌더링 대기
            if (typeof(Prototype).GetField("royalImages", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player) == null) throw new Exception("UI 이미지가 초기화되지 않았습니다."); // 화면 자원 확인
            string[] panels = { "", "luna", "work", "blackjack", "blackjack_hit", "blackjack_result", "cashier", "pause", "end", "victory" }; // 검사할 화면 목록
            int index = (frame - 90) / 15; // 현재 화면 번호
            if (index >= panels.Length) // 모든 화면 확인 여부
            { // 검사 완료 시작
                Debug.Log("ROYAL_UI_PLAYMODE_OK"); // 화면 검사 성공 표시
                Finish(0); // 정상 종료
                return; // 검사 종료
            } // 검사 완료 끝
            string current = panels[index]; // 현재 검사 화면
            bool table = current.StartsWith("blackjack"); // 블랙잭 화면 확인
            typeof(Prototype).GetField("panel", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(player, table ? "blackjack" : current == "victory" ? "end" : current); // 검사 화면 전환
            game.GameOver = current == "end"; // 실패 화면 상태 설정
            game.Escaped = current == "victory"; // 탈출 화면 상태 설정
            if ((frame - 90) % 15 == 0) // 화면 변경 시점 확인
            { // 화면별 상태 준비
                game.Seated = table; // 착석 상태 반영
                typeof(Prototype).GetMethod("UpdateCameraPose", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(player, null); // 착석 카메라 반영
                game.Round = new Round(); // 게임 단계 초기화
                if (current == "blackjack_hit" || current == "blackjack_result") // 카드 단계 확인
                { // 카드 상태 준비
                    game.Round.phase = current == "blackjack_hit" ? 1 : 2; // 게임 진행 단계
                    game.Round.player.AddRange(new int[] { 10, 8 }); // 플레이어 카드 구성
                    game.Round.dealer.AddRange(new int[] { 10, 7 }); // 딜러 카드 구성
                    game.Round.result = "승리"; // 결과 배지 확인
                } // 카드 상태 준비 끝
            } // 화면별 상태 준비 끝
            if ((frame - 90) % 15 == 10) ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(Application.dataPath, "..", "ui-preview-" + (panels[index] == "" ? "hud" : panels[index]) + ".png")); // 화면 검토 이미지 저장
        } // 검사 시도 끝
        catch (Exception error) // 검사 실패 처리
        { // 실패 처리 시작
            Debug.LogException(error); // 실패 내용 기록
            Finish(1); // 실패 종료
        } // 실패 처리 끝
    } // 상태 전환 끝
    static void Finish(int code) // 검사 환경 복원
    { // 환경 복원 시작
        Application.logMessageReceived -= Capture; // 오류 감시 해제
        EditorApplication.update -= Check; // 검사 콜백 해제
        EditorApplication.playModeStateChanged -= OnMode; // 상태 감시 해제
        PlayerSettings.productName = oldProduct; // 제품 이름 복원
        EditorSettings.enterPlayModeOptionsEnabled = oldEnabled; // 옵션 상태 복원
        EditorSettings.enterPlayModeOptions = oldOptions; // 옵션 내용 복원
        EditorApplication.Exit(code); // 편집기 종료
    } // 환경 복원 끝
} // 검사 끝
