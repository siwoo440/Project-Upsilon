using System; // 예외 처리 기능
using System.IO; // 저장 파일 읽기
using System.Reflection; // 게임 내부 상태 확인
using UnityEditor; // 편집기 기능
using UnityEditor.SceneManagement; // 씬 열기 기능
using UnityEngine; // 위치와 오브젝트 기능
using UnityEngine.SceneManagement; // 씬 상태 확인
using Moonlight; // 게임 기능
public static class HotelHallPlaySmoke // 복도 씬 왕복 검사
{ // 검사 시작
    static int step; // 검사 단계
    static bool oldEnabled; // 기존 옵션 상태
    static EnterPlayModeOptions oldOptions; // 기존 세부 옵션
    static string oldProduct; // 기존 저장 경로 이름
    public static void Run() // 검사 시작
    { // 검사 준비 시작
        oldEnabled = EditorSettings.enterPlayModeOptionsEnabled; // 기존 옵션 보관
        oldOptions = EditorSettings.enterPlayModeOptions; // 세부 옵션 보관
        oldProduct = PlayerSettings.productName; // 기존 제품 이름 보관
        PlayerSettings.productName = "MoonlightIsolatedHallSmoke"; // 전용 저장 경로
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
    static void Check() // 씬 왕복과 저장 확인
    { // 확인 시작
        try // 검사 예외 처리
        { // 검사 시도 시작
            Prototype player = UnityEngine.Object.FindFirstObjectByType<Prototype>(); // 플레이어 확인
            if (player == null) return; // 플레이어 대기
            GameState game = (GameState)typeof(Prototype).GetField("game", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player); // 게임 상태 확인
            if (game == null) return; // 게임 초기화 대기
            if ((bool)typeof(Prototype).GetField("isTransitioning", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)) return; // 씬 로딩 대기
            if (step == 0) // 카지노 출발 단계
            { // 출발 시작
                game.Chips = 200000000; game.StartWork(player.settings); // 진행 중 업무 설정
                Interact(player, new Vector3(19, 0.1f, -15)); // 복도 입구 상호작용
                step = 1; // 복도 도착 단계
                return; // 이동 완료 대기
            } // 출발 끝
            if (step == 1) // 복도 입구 도착 단계
            { // 복도 도착 확인
                if (SceneManager.GetActiveScene().path != "Assets/Moonlight/HotelHall.unity") throw new Exception("공용 복도 진입 실패"); // 복도 씬 확인
                SaveData saved = JsonUtility.FromJson<SaveData>(File.ReadAllText(Path.Combine(Application.persistentDataPath, "moonlight_event.json"))); // 도착 자동 저장
                if (saved.scenePath != "Assets/Moonlight/HotelHall.unity" || !saved.state.WorkActive) throw new Exception("복도 도착 저장 실패"); // 복도와 업무 저장 확인
                Interact(player, new Vector3(0, 0.1f, 8.5f)); // 객실 문 상호작용
                step = 2; // 객실 도착 단계
                return; // 이동 완료 대기
            } // 복도 도착 확인 끝
            if (step == 2) // 객실 도착 단계
            { // 객실 도착 확인
                if (SceneManager.GetActiveScene().path != "Assets/Moonlight/Room.unity") throw new Exception("복도에서 객실 이동 실패"); // 객실 씬 확인
                Interact(player, new Vector3(3, 0.1f, -4.3f)); // 객실 복도 문 상호작용
                step = 3; // 복도 복귀 단계
                return; // 이동 완료 대기
            } // 객실 도착 확인 끝
            if (step == 3) // 복도 복귀 단계
            { // 복도 복귀 확인
                if (SceneManager.GetActiveScene().path != "Assets/Moonlight/HotelHall.unity") throw new Exception("객실에서 복도 이동 실패"); // 복도 씬 확인
                Interact(player, new Vector3(0, 0.1f, -8.5f)); // 카지노 문 상호작용
                step = 4; // 카지노 복귀 단계
                return; // 이동 완료 대기
            } // 복도 복귀 확인 끝
            if (SceneManager.GetActiveScene().path != "Assets/Moonlight/Prototype.unity" || game.Chips != 200000000 || !game.WorkActive) throw new Exception("복도 왕복 중 상태가 바뀌었습니다."); // 카지노 복귀와 상태 확인
            Debug.Log("HOTEL_HALL_PLAY_OK"); // 검사 성공 표시
            Finish(0); // 정상 종료
        } // 검사 시도 끝
        catch (Exception error) // 검사 실패 처리
        { // 실패 처리 시작
            Debug.LogException(error); // 오류 기록
            Finish(1); // 실패 종료
        } // 실패 처리 끝
    } // 확인 끝
    static void Interact(Prototype player, Vector3 position) // 상호작용 위치 이동
    { // 상호작용 시작
        CharacterController controller = player.GetComponent<CharacterController>(); // 플레이어 충돌체
        controller.enabled = false; player.transform.position = position; controller.enabled = true; // 검사용 위치 이동
        typeof(Prototype).GetMethod("Interact", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(player, null); // 실제 상호작용 호출
    } // 상호작용 끝
    static void Finish(int code) // 검사 환경 복원
    { // 복원 시작
        EditorApplication.update -= Check; // 검사 등록 해제
        EditorApplication.playModeStateChanged -= OnMode; // 상태 감시 해제
        EditorSettings.enterPlayModeOptionsEnabled = oldEnabled; // 옵션 복원
        EditorSettings.enterPlayModeOptions = oldOptions; // 세부 옵션 복원
        PlayerSettings.productName = oldProduct; // 제품 이름 복원
        EditorApplication.Exit(code); // 편집기 종료
    } // 복원 끝
} // 검사 끝
