using System; // 기본 기능 사용
using System.IO; // 저장 파일 사용
using UnityEngine; // 유니티 기능 사용
using UnityEngine.InputSystem; // 새 입력 시스템 사용
namespace Moonlight // 게임 이름 공간
{ // 이름 공간 시작
    [Serializable] public class SaveData // 저장 자료 묶음
    { // 저장 자료 시작
        public int version = 1; // 저장 형식 버전
        public GameState state; // 경제와 게임 상태
        public float x, y, z, yaw, pitch; // 위치와 시점
    } // 저장 자료 끝
    public class Prototype : MonoBehaviour // 시제품 화면과 입력
    { // 시제품 시작
        public GameSettings settings = new GameSettings(); // 인스펙터에서 조정할 설정
        GameState game; // 현재 게임 상태
        CharacterController controller; // 이동 충돌 처리
        Camera view; // 플레이어 카메라
        float yaw, pitch; // 시점 회전
        string panel = "", message = "루나에게 안내를 받은 뒤 업무와 블랙잭을 이용하세요."; // 현재 화면과 안내
        long bet = 1000000; // 베팅 금액
        GUIStyle label, button; // 화면 글꼴 설정
        string SavePath(string slot) { return Path.Combine(Application.persistentDataPath, "moonlight_" + slot + ".json"); } // 저장 경로
        void Start() // 시작 처리
        { // 시작 범위
            controller = gameObject.AddComponent<CharacterController>(); // 플레이어 충돌체
            controller.height = 1.8f; controller.center = new Vector3(0, 0.9f, 0); // 충돌체 크기
            var cameraObject = new GameObject("Player Camera"); // 카메라 생성
            cameraObject.transform.SetParent(transform); cameraObject.transform.localPosition = new Vector3(0, 1.6f, 0); // 시점 높이
            view = cameraObject.AddComponent<Camera>(); cameraObject.AddComponent<AudioListener>(); // 카메라 활성화
            CasinoMap.Build(); // 카지노 공간 생성
            game = new GameState(settings); // 새 게임 생성
            transform.position = new Vector3(0, 0.1f, -18); // 입구 위치
            Save("start"); // 시작 자동 저장
            Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; // 마우스 고정
        } // 시작 끝
        void Save(string slot) // 상태 파일 저장
        { // 저장 시작
            try // 파일 오류 처리
            { // 시도 시작
                var data = new SaveData { state = game, x = transform.position.x, y = transform.position.y, z = transform.position.z, yaw = yaw, pitch = pitch }; // 저장 자료 구성
                File.WriteAllText(SavePath(slot), JsonUtility.ToJson(data, true)); // 슬롯에 기록
                message = "저장 완료: " + slot; // 저장 안내
            } // 시도 끝
            catch (Exception ex) { message = "저장 실패: " + ex.Message; } // 오류 표시
        } // 저장 끝
        void Load(string slot) // 상태 파일 불러오기
        { // 불러오기 시작
            try // 파일 오류 처리
            { // 시도 시작
                var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath(slot))); // 저장 자료 읽기
                if (data == null || data.version != 1 || data.state == null) throw new Exception("지원하지 않는 저장 데이터"); // 저장 형식 확인
                game = data.state; controller.enabled = false; transform.position = new Vector3(data.x, data.y, data.z); controller.enabled = true; // 상태와 위치 복원
                yaw = data.yaw; pitch = data.pitch; panel = ""; message = "불러오기 완료: " + slot; // 시점과 화면 복원
            } // 시도 끝
            catch (Exception ex) { message = "불러오기 실패: " + ex.Message; } // 오류 표시
        } // 불러오기 끝
        void Update() // 매 프레임 진행
        { // 갱신 시작
            var keyboard = Keyboard.current; var mouse = Mouse.current; // 입력 장치 확인
            if (keyboard == null) return; // 키보드 부재 처리
            if (keyboard.escapeKey.wasPressedThisFrame && !game.GameOver && !game.Escaped) panel = panel == "pause" ? "" : "pause"; // 일시정지 전환
            if (panel == "pause") { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; return; } // 정지 중 시계 중단
            if (!game.GameOver && !game.Escaped) Advance(Time.deltaTime * settings.minutesPerSecond); // 연속 시계 진행
            if (keyboard.f5Key.wasPressedThisFrame) Save("manual"); // 수동 저장 단축키
            if (keyboard.f9Key.wasPressedThisFrame) Load("manual"); // 수동 불러오기 단축키
            bool free = panel == "" && !game.GameOver && !game.Escaped; // 이동 가능 여부
            Cursor.lockState = free ? CursorLockMode.Locked : CursorLockMode.None; Cursor.visible = !free; // 마우스 모드 설정
            if (!free) return; // 화면 표시 중 이동 제한
            if (mouse != null) { Vector2 delta = mouse.delta.ReadValue(); yaw += delta.x * 0.12f; pitch = Mathf.Clamp(pitch - delta.y * 0.12f, -80, 80); } // 마우스 시점
            transform.rotation = Quaternion.Euler(0, yaw, 0); view.transform.localRotation = Quaternion.Euler(pitch, 0, 0); // 시점 반영
            Vector3 movement = new Vector3((keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0), -0.4f, (keyboard.wKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed ? 1 : 0)); // 이동 입력
            controller.Move(transform.TransformDirection(movement.normalized) * 4 * Time.deltaTime); // 이동 적용
            if (keyboard.eKey.wasPressedThisFrame) Interact(); // 상호작용 시도
        } // 갱신 끝
        void Advance(double elapsed) // 시계와 업무 처리
        { // 시간 처리 시작
            while (elapsed > 0 && !game.GameOver && !game.Escaped) // 남은 시간 처리
            { // 시간 구간 시작
                double untilBill = Math.Max(0, game.NextBill - game.Minutes); // 청구까지 시간
                double step = Math.Min(elapsed, untilBill); // 청구 경계까지 진행
                if (game.WorkRemaining > 0) step = Math.Min(step, game.WorkRemaining); // 업무 완료 경계
                game.Minutes += step; elapsed -= step; // 시간 증가
                if (game.WorkRemaining > 0) game.WorkRemaining = Math.Max(0, game.WorkRemaining - step); // 업무 시간 감소
                if (game.Minutes >= game.NextBill) // 청구 시각 우선 처리
                { // 청구 시작
                    Save("before_bill"); // 청구 직전 별도 저장
                    bool paid = game.SettleBill(settings); // 청구 정산
                    Save("after_bill"); // 정산 직후 별도 저장
                    message = paid ? "정기 청구 납부 완료" : "청구액을 납부하지 못했습니다."; // 결과 안내
                    if (!paid) break; // 실패 시 활동 중단
                } // 청구 끝
                if (game.WorkRemaining == 0 && game.WorkActive) // 업무 완료 확인
                { // 업무 보상 시작
                    game.WorkActive = false; game.Chips += settings.workReward; message = "업무 완료: " + settings.workReward.ToString("N0") + "칩"; Save("event"); // 확정 보상과 저장
                } // 업무 보상 끝
                if (step == 0 && game.Minutes < game.NextBill) break; // 무한 반복 방지
            } // 시간 구간 끝
        } // 시간 처리 끝
        void Interact() // 가까운 장소 이용
        { // 상호작용 시작
            string target = MapLayout.Nearest(transform.position.x, transform.position.z); // 최근접 대상 확인
            if (target == "") // 대상이 없는 경우
            { // 거리 안내 시작
                message = "가까운 장소에서 E를 누르세요."; // 거리 안내
                return; // 상호작용 중단
            } // 거리 안내 끝
            panel = target; // 대상 화면 열기
        } // 상호작용 끝
        string NearbyLabel() // 주변 대상 안내
        { // 안내 시작
            string target = MapLayout.Nearest(transform.position.x, transform.position.z); // 최근접 대상 확인
            if (target == "luna") return "E: 루나와 대화"; // 루나 안내
            if (target == "work") return "E: 호텔 업무"; // 업무 안내
            if (target == "blackjack") return "E: 블랙잭"; // 게임 안내
            if (target == "cashier") return "E: 정산 창구"; // 정산 안내
            return ""; // 주변 대상 없음
        } // 안내 끝
        void OnGUI() // 임시 화면 그리기
        { // 화면 시작
            if (game == null) return; // 초기화 확인
            if (label == null) // 글꼴 초기화 확인
            { // 글꼴 생성 시작
                Font font = Font.CreateDynamicFontFromOSFont("Malgun Gothic", 18); // 한글 시스템 글꼴
                label = new GUIStyle(GUI.skin.label) { font = font, fontSize = 18, wordWrap = true }; label.normal.textColor = Color.white; // 본문 스타일
                button = new GUIStyle(GUI.skin.button) { font = font, fontSize = 18 }; // 버튼 스타일
            } // 글꼴 생성 끝
            GUI.Box(new Rect(10, 10, 600, 150), ""); // HUD 배경
            GUI.Label(new Rect(20, 18, 580, 140), "칩: " + game.Chips.ToString("N0") + "   빚: " + game.Debt.ToString("N0") + "\n다음 청구 예상: " + game.PreviewBill(settings).ToString("N0") + "   남은 시간: " + Math.Max(0, (game.NextBill - game.Minutes) / 60).ToString("F1") + "시간\n" + message + "\nWASD 이동 · 마우스 시점 · E 상호작용 · Esc 메뉴", label); // 핵심 정보 표시
            if (game.GameOver || game.Escaped) panel = "end"; // 종료 화면 전환
            if (panel == "") // 주변 안내 표시 여부
            { // 주변 안내 시작
                GUI.Label(new Rect(Screen.width / 2 - 110, Screen.height - 65, 250, 35), NearbyLabel(), label); // 상호작용 안내
                return; // 열린 화면 없음
            } // 주변 안내 끝
            GUILayout.BeginArea(new Rect(Screen.width / 2 - 270, 180, 540, 470), GUI.skin.box); // 중앙 대화 상자
            if (panel == "luna") GUILayout.Label("루나: 21에 가까운 쪽이 이겨요. A는 1 또는 11입니다. 히트로 카드를 받고 스탠드로 멈추세요.", label); // 루나 안내
            if (panel == "work") // 업무 화면
            { // 업무 화면 시작
                GUILayout.Label("호텔 업무: " + settings.workMinutes + "분 / 보수 " + settings.workReward.ToString("N0") + "칩", label); // 업무 조건
                if (GUILayout.Button("업무 시작", button) && !game.WorkActive) { game.StartWork(settings); panel = ""; Save("event"); } // 업무 시작
            } // 업무 화면 끝
            if (panel == "blackjack") DrawBlackjack(); // 블랙잭 화면
            if (panel == "cashier") // 정산 화면
            { // 정산 화면 시작
                GUILayout.Label("정산 창구: 청구 전에는 빚과 대출을 전액 상환할 수 있습니다.", label); // 상환 설명
                if (GUILayout.Button("전액 상환하고 탈출", button)) { if (!game.RepayAll()) message = "전액 상환할 칩이 부족합니다."; else Save("event"); } // 조기 상환
            } // 정산 화면 끝
            if (panel == "pause" || panel == "end") // 저장 메뉴
            { // 저장 메뉴 시작
                GUILayout.Label(game.GameOver ? "청구 실패 — 끌려가 죽었습니다." : game.Escaped ? "빚을 모두 갚고 혼자 탈출했습니다." : "일시정지", label); // 종료 또는 정지 안내
                if (GUILayout.Button("수동 저장", button) && !game.GameOver && !game.Escaped) Save("manual"); // 수동 저장
                if (GUILayout.Button("수동 저장 불러오기", button)) Load("manual"); // 수동 불러오기
                if (GUILayout.Button("청구 직전 불러오기", button)) Load("before_bill"); // 청구 전 복구
                if (GUILayout.Button("정산 직후 불러오기", button)) Load("after_bill"); // 정산 후 복구
                if (GUILayout.Button("시작 지점 불러오기", button)) Load("start"); // 시작 복구
                if (GUILayout.Button("최근 이벤트 불러오기", button)) Load("event"); // 이벤트 복구
            } // 저장 메뉴 끝
            if (panel != "end" && GUILayout.Button("닫기", button)) panel = ""; // 화면 닫기
            GUILayout.EndArea(); // 대화 상자 끝
        } // 화면 끝
        void DrawBlackjack() // 블랙잭 조작 화면
        { // 블랙잭 화면 시작
            GUILayout.Label("블랙잭 | 1덱 · 딜러 소프트 17 스탠드 · 내추럴 3:2", label); // 임시 규칙 표시
            if (game.Round.phase != 1) // 베팅 가능 상태
            { // 베팅 화면 시작
                GUILayout.Label("베팅: " + bet.ToString("N0") + "칩", label); // 베팅 금액 표시
                if (GUILayout.Button("베팅 감소", button)) bet = Math.Max(settings.minBet, bet - settings.minBet); // 베팅 감소
                if (GUILayout.Button("베팅 증가", button)) bet = Math.Min(settings.maxBet, bet + settings.minBet); // 베팅 증가
                if (GUILayout.Button("카드 배분", button)) { if (!game.StartBlackjack(settings, bet)) message = "베팅 가능한 칩이 부족합니다."; else Save("event"); } // 라운드 시작
            } // 베팅 화면 끝
            if (game.Round.phase > 0) // 카드 표시 가능 상태
            { // 카드 화면 시작
                GUILayout.Label("내 카드: " + string.Join(", ", game.Round.player) + " = " + Blackjack.Score(game.Round.player), label); // 플레이어 카드
                GUILayout.Label("딜러: " + (game.Round.phase == 1 ? game.Round.dealer[0].ToString() + ", ?" : string.Join(", ", game.Round.dealer) + " = " + Blackjack.Score(game.Round.dealer)), label); // 딜러 카드
                if (game.Round.phase == 1 && GUILayout.Button("히트", button)) { game.Hit(settings); Save("event"); } // 히트 선택
                if (game.Round.phase == 1 && GUILayout.Button("스탠드", button)) { game.Stand(settings); Save("event"); } // 스탠드 선택
                if (game.Round.phase == 2) GUILayout.Label("결과: " + game.Round.result, label); // 결과 표시
            } // 카드 화면 끝
        } // 블랙잭 화면 끝
    } // 시제품 끝
} // 이름 공간 끝
