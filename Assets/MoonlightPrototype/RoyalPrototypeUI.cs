using System; // 기본 계산 기능
using System.Collections.Generic; // 이미지 목록 기능
using UnityEngine; // 유니티 화면 기능
namespace Moonlight // 게임 이름 공간
{ // 이름 공간 시작
    public partial class Prototype // 시제품 UI 구성
    { // UI 구성 시작
        Dictionary<string, Texture2D> royalImages; // UI 이미지 보관
        GUIStyle royalText, royalSmall, royalTitle, royalButton; // UI 글자와 버튼
        void SetupRoyalUi() // UI 이미지와 스타일 준비
        { // 준비 시작
            if (royalImages != null) return; // 중복 초기화 방지
            royalImages = new Dictionary<string, Texture2D>(); // 이미지 목록 생성
            string[] names = { "hud_frame", "countdown_frame", "dialogue_frame", "work_row", "save_slot", "notice_frame", "prompt_keycap", "icon_chips", "icon_debt", "icon_rank", "icon_experience", "icon_bill_clock", "icon_work", "icon_locked", "icon_hit", "icon_stand", "work_bed", "rank_silver", "result_victory_badge", "result_defeat_badge", "result_draw_badge", "brand_moon", "button_confirm_gold", "button_secondary_dark", "button_selected", "portrait_luna", "portrait_player", "repayment_medallion", "background_gameover", "background_victory" }; // 사용할 이미지 이름
            foreach (string name in names) // 각 이미지 확인
            { // 이미지 읽기 시작
                royalImages[name] = Resources.Load<Texture2D>("CasinoUI/Components/" + name); // 리소스 이미지 읽기
            } // 이미지 읽기 끝
            Font font = Font.CreateDynamicFontFromOSFont("Malgun Gothic", 24); // 한글 시스템 글꼴
            royalText = new GUIStyle(GUI.skin.label) { font = font, fontSize = 24, wordWrap = true }; // 기본 글자 스타일
            royalText.clipping = TextClipping.Clip; // 글자 표시 영역 제한
            royalText.normal.textColor = new Color(1f, 0.95f, 0.88f); // 기본 글자 색상
            royalSmall = new GUIStyle(royalText) { fontSize = 19 }; // 작은 글자 스타일
            royalTitle = new GUIStyle(royalText) { fontSize = 34, alignment = TextAnchor.MiddleCenter }; // 제목 스타일
            royalButton = new GUIStyle(GUIStyle.none) { font = font, fontSize = 23, alignment = TextAnchor.MiddleCenter, wordWrap = true }; // 투명 버튼 글자 스타일
            royalButton.clipping = TextClipping.Clip; // 버튼 글자 표시 영역 제한
            royalButton.normal.textColor = Color.white; // 기본 버튼 글자 색상
            royalButton.hover.textColor = Color.white; // 강조 버튼 글자 색상
            royalButton.active.textColor = Color.white; // 클릭 버튼 글자 색상
            royalButton.focused.textColor = Color.white; // 선택 버튼 글자 색상
        } // 준비 끝
        void RoyalImage(string name, Rect area, bool preserve = false) // 이미지 표시
        { // 이미지 표시 시작
            Texture2D image; // 이미지 임시 보관
            if (royalImages.TryGetValue(name, out image) && image != null) GUI.DrawTexture(area, image, preserve ? ScaleMode.ScaleToFit : ScaleMode.StretchToFill, true); // 투명 이미지 출력
        } // 이미지 표시 끝
        bool RoyalButton(Rect area, string text) // 공통 버튼 표시
        { // 버튼 표시 시작
            string imageName = area.Contains(Event.current.mousePosition) ? "button_selected" : "button_secondary_dark"; // 마우스 강조 이미지 선택
            Texture2D background = royalImages[imageName]; // 버튼 배경 이미지
            if (background != null) GUI.DrawTextureWithTexCoords(area, background, new Rect(0.03f, 0.24f, 0.94f, 0.52f), true); // 이미지 여백을 제외한 버튼 출력
            int originalSize = royalButton.fontSize; // 기존 버튼 글자 크기
            GUIContent content = new GUIContent(text); // 버튼 글자 내용
            while (royalButton.fontSize > 12 && royalButton.CalcHeight(content, area.width - 8) > area.height - 4) royalButton.fontSize--; // 버튼 안쪽에 맞춘 크기
            bool selected = GUI.Button(area, content, royalButton); // 버튼 선택 결과
            royalButton.fontSize = originalSize; // 기존 버튼 글자 크기 복원
            return selected; // 버튼 선택 전달
        } // 버튼 표시 끝
        void RoyalLabel(Rect area, string text, GUIStyle style) // 영역에 맞춘 글자 출력
        { // 글자 출력 시작
            int originalSize = style.fontSize; // 기존 글자 크기
            GUIContent content = new GUIContent(text); // 출력 글자 내용
            while (style.fontSize > 12 && style.CalcHeight(content, area.width - 4) > area.height - 2) style.fontSize--; // 표시 영역에 맞춘 크기
            GUI.Label(area, content, style); // 잘림 영역 안쪽에 글자 출력
            style.fontSize = originalSize; // 기존 글자 크기 복원
        } // 글자 출력 끝
        void OnGUI() // 시제품 UI 출력
        { // UI 출력 시작
            if (game == null) return; // 게임 초기화 확인
            SetupRoyalUi(); // 화면 자원 준비
            Matrix4x4 previousMatrix = GUI.matrix; // 기존 화면 변환 보관
            Rect fitted = RoyalLayout.Fit(Screen.width, Screen.height); // 화면 크기 계산
            GUI.matrix = Matrix4x4.TRS(new Vector3(fitted.x, fitted.y, 0), Quaternion.identity, new Vector3(fitted.width / 1920f, fitted.height / 1080f, 1)); // 기준 해상도 적용
            if (game.GameOver || game.Escaped) panel = "end"; // 종료 화면 전환
            if (panel == "end") // 종료 상태 확인
            { // 종료 화면 시작
                DrawRoyalEnd(); // 종료 결과 출력
            } // 종료 화면 끝
            else // 진행 상태 확인
            { // 진행 화면 시작
                DrawRoyalHud(); // 상태창과 청구 시계 출력
                if (panel == "") DrawRoyalPrompt(); // 주변 상호작용 안내
                if (panel == "luna") DrawRoyalLuna(); // 루나 대화 출력
                if (panel == "work") DrawRoyalWork(); // 업무 선택 출력
                if (panel == "blackjack") DrawRoyalBlackjack(); // 블랙잭 조작 출력
                if (panel == "cashier") DrawRoyalCashier(); // 상환 창구 출력
                if (panel == "pause") DrawRoyalPause(); // 일시정지 출력
            } // 진행 화면 끝
            GUI.matrix = previousMatrix; // 기존 화면 변환 복원
        } // UI 출력 끝
        void DrawRoyalHud() // 상단 상태 정보 출력
        { // 상태 정보 시작
            RoyalImage("hud_frame", new Rect(15, 5, 520, 470)); // 네 줄 상태창 배경
            string[] icons = { "icon_chips", "icon_debt", "icon_rank", "icon_experience" }; // 상태별 아이콘
            string[] titles = { "보유 칩", "남은 빚", "회원 등급", "누적 경험치" }; // 상태별 이름
            string[] values = { game.Chips.ToString("N0"), game.Debt.ToString("N0"), game.Member.Rank == 0 ? "브론즈" : "실버", game.Member.Experience + " EXP" }; // 현재 상태 값
            for (int i = 0; i < 4; i++) // 네 가지 상태 표시
            { // 상태 행 시작
                float y = 92 + i * 79; // 상태 행 높이
                RoyalImage(icons[i], new Rect(78, y + 4, 65, 65), true); // 상태 아이콘
                RoyalLabel(new Rect(180, y + 5, 255, 27), titles[i], royalSmall); // 상태 이름
                RoyalLabel(new Rect(180, y + 30, 270, 35), values[i], royalText); // 상태 값
            } // 상태 행 끝
            RoyalImage("countdown_frame", new Rect(1480, 15, 420, 160)); // 청구 시계 배경
            RoyalImage("icon_bill_clock", new Rect(1530, 47, 75, 75), true); // 청구 시계 아이콘
            double remaining = Math.Max(0, game.NextBill - game.Minutes); // 남은 게임 시간
            RoyalLabel(new Rect(1610, 67, 260, 28), "다음 청구까지  " + (remaining / 60).ToString("F1") + "시간", royalSmall); // 청구 남은 시간
            RoyalLabel(new Rect(1610, 96, 260, 33), game.PreviewBill(settings).ToString("N0") + " 칩", royalText); // 예상 청구액
            RoyalImage("dialogue_frame", new Rect(15, 425, 680, 190)); // 장소와 알림 배경
            RoyalLabel(new Rect(75, 470, 550, 25), MapLayout.Zone(transform.position.x, transform.position.z), royalSmall); // 현재 장소
            RoyalLabel(new Rect(75, 500, 550, 28), message, royalSmall); // 상태 안내
            RoyalLabel(new Rect(75, 530, 550, 25), "주기별 게임 경험치  " + game.Member.CycleGameExperience + " / " + settings.cycleGameCap + " EXP", royalSmall); // 주기 경험치 한도
        } // 상태 정보 끝
        void DrawRoyalPrompt() // 주변 상호작용 출력
        { // 주변 안내 시작
            string nearby = NearbyLabel(); // 가까운 대상 이름
            if (nearby == "") return; // 대상 없음 확인
            RoyalImage("work_row", new Rect(700, 910, 520, 110)); // 안내 배경
            RoyalImage("prompt_keycap", new Rect(755, 935, 55, 55), true); // 상호작용 키 그림
            RoyalLabel(new Rect(773, 946, 30, 32), "E", royalText); // 상호작용 키 글자
            RoyalLabel(new Rect(825, 945, 330, 35), nearby.Replace("E: ", ""), royalText); // 상호작용 이름
        } // 주변 안내 끝
        void DrawRoyalLuna() // 루나 대화 출력
        { // 대화 시작
            RoyalImage("dialogue_frame", new Rect(300, 700, 1320, 290)); // 대화 상자 배경
            RoyalImage("portrait_luna", new Rect(330, 720, 210, 210), true); // 루나 임시 초상화
            RoyalLabel(new Rect(560, 775, 400, 40), "루나  |  블랙잭 안내", royalText); // 대화 인물 이름
            RoyalLabel(new Rect(560, 815, 900, 80), "21에 가까운 쪽이 이겨요. A는 1 또는 11입니다. 히트로 카드를 받고 스탠드로 멈추세요.", royalText); // 규칙 대사
            if (RoyalButton(new Rect(1260, 900, 270, 70), "닫기")) panel = ""; // 대화 종료
        } // 대화 끝
        void DrawRoyalWork() // 업무 선택 출력
        { // 업무 선택 시작
            RoyalImage("notice_frame", new Rect(570, 85, 780, 900)); // 업무 창 배경
            RoyalLabel(new Rect(700, 220, 520, 55), "오늘의 호텔 업무", royalTitle); // 업무 화면 제목
            RoyalImage("work_row", new Rect(670, 305, 580, 130)); // 기본 업무 행
            RoyalImage("work_bed", new Rect(710, 330, 75, 75), true); // 업무 아이콘
            RoyalLabel(new Rect(805, 330, 390, 65), "기본 업무  ·  " + settings.workMinutes + "분\n보수  " + settings.workReward.ToString("N0") + " 칩", royalText); // 기본 업무 조건
            if (RoyalButton(new Rect(760, 420, 400, 70), game.WorkActive ? "업무 진행 중" : "기본 업무 시작") && !game.WorkActive) // 기본 업무 선택
            { // 기본 업무 시작
                game.StartWork(settings); // 기본 업무 등록
                panel = ""; // 업무 화면 종료
                Save("event"); // 업무 시작 저장
            } // 기본 업무 끝
            RoyalImage("work_row", new Rect(670, 510, 580, 130)); // 실버 업무 행
            RoyalImage(game.Member.Rank >= 1 ? "rank_silver" : "icon_locked", new Rect(710, 535, 75, 75), true); // 해금 상태 아이콘
            RoyalLabel(new Rect(805, 535, 390, 65), "실버 업무\n보수  " + settings.silverWorkReward.ToString("N0") + " 칩", royalText); // 실버 업무 조건
            if (RoyalButton(new Rect(760, 630, 400, 70), game.Member.Rank >= 1 ? "실버 업무 시작" : "실버 등급 필요") && game.Member.Rank >= 1 && game.StartSilverWork(settings)) // 실버 업무 선택
            { // 실버 업무 시작
                panel = ""; // 업무 화면 종료
                Save("event"); // 업무 시작 저장
            } // 실버 업무 끝
            if (RoyalButton(new Rect(810, 755, 300, 70), "닫기")) panel = ""; // 업무 화면 닫기
        } // 업무 선택 끝
        void DrawRoyalBlackjack() // 블랙잭 화면 출력
        { // 블랙잭 화면 시작
            RoyalImage("save_slot", new Rect(125, 820, 1670, 245)); // 하단 조작판 배경
            RoyalImage("portrait_player", new Rect(180, 835, 120, 120), true); // 플레이어 임시 초상화
            RoyalLabel(new Rect(300, 893, 325, 32), "플레이어  ·  " + game.Chips.ToString("N0") + " 칩", royalText); // 플레이어 칩
            RoyalLabel(new Rect(300, 930, 325, 48), settings.decks + "덱  ·  소프트 17 " + (settings.dealerHitsSoft17 ? "히트" : "스탠드") + "  ·  블랙잭 3:2", royalSmall); // 임시 게임 규칙
            if (game.Round.phase != 1) // 베팅 단계 확인
            { // 베팅 조작 시작
                RoyalImage("work_row", new Rect(730, 800, 500, 90)); // 베팅 정보 배경
                RoyalImage("icon_chips", new Rect(770, 817, 55, 55), true); // 베팅 칩 아이콘
                RoyalLabel(new Rect(860, 828, 320, 32), "베팅 금액  " + bet.ToString("N0") + " 칩", royalText); // 현재 베팅 금액
                if (RoyalButton(new Rect(720, 905, 180, 80), "－")) bet = Math.Max(settings.minBet, bet - settings.minBet); // 베팅 감소
                if (RoyalButton(new Rect(900, 905, 180, 80), "＋")) bet = Math.Min(settings.maxBet, bet + settings.minBet); // 베팅 증가
                if (RoyalButton(new Rect(1080, 905, 250, 80), "카드 배분")) // 게임 시작 선택
                { // 게임 시작 처리
                    if (!game.StartBlackjack(settings, bet)) message = "베팅 가능한 칩이 부족합니다."; // 베팅 실패 안내
                    else Save("event"); // 라운드 시작 저장
                } // 게임 시작 끝
            } // 베팅 조작 끝
            if (game.Round.phase == 1) // 카드 선택 단계 확인
            { // 카드 조작 시작
                RoyalImage("work_row", new Rect(730, 800, 600, 90)); // 카드 정보 배경
                RoyalImage("icon_hit", new Rect(770, 817, 55, 55), true); // 카드 정보 아이콘
                RoyalLabel(new Rect(885, 828, 400, 32), "내 카드 " + Blackjack.Score(game.Round.player) + "  ·  딜러 " + CardNames(game.Round.dealer.GetRange(0, 1)) + ", ?", royalText); // 현재 카드 정보
                RoyalImage("icon_hit", new Rect(745, 912, 65, 65), true); // 히트 아이콘
                RoyalImage("icon_stand", new Rect(1035, 912, 65, 65), true); // 스탠드 아이콘
                if (RoyalButton(new Rect(800, 905, 210, 80), "히트")) // 히트 선택
                { // 히트 처리 시작
                    game.Hit(settings); // 카드 한 장 받기
                    Save("event"); // 게임 상태 저장
                } // 히트 처리 끝
                if (RoyalButton(new Rect(1090, 905, 210, 80), "스탠드")) // 스탠드 선택
                { // 스탠드 처리 시작
                    game.Stand(settings); // 딜러 차례 진행
                    Save("event"); // 게임 상태 저장
                } // 스탠드 처리 끝
            } // 카드 조작 끝
            if (game.Round.phase == 2) // 결과 단계 확인
            { // 결과 표시 시작
                string resultImage = game.Round.result.Contains("승리") ? "result_victory_badge" : game.Round.result == "무승부" ? "result_draw_badge" : "result_defeat_badge"; // 결과 배지 선택
                RoyalImage(resultImage, new Rect(760, 710, 400, 100)); // 결과 배지 표시
                RoyalLabel(new Rect(800, 735, 320, 48), game.Round.result, royalTitle); // 결과 문구 표시
            } // 결과 표시 끝
            if (RoyalButton(new Rect(1380, 905, 270, 80), "일어나기")) // 테이블 이탈 선택
            { // 테이블 이탈 시작
                if (game.LeaveBlackjack()) // 이탈 가능 확인
                { // 이탈 성공 시작
                    panel = ""; // 게임 화면 종료
                    UpdateCameraPose(); // 이동 시점 복원
                    Save("event"); // 테이블 퇴장 저장
                } // 이탈 성공 끝
                else message = "라운드가 끝난 뒤 일어날 수 있습니다."; // 이탈 제한 안내
            } // 테이블 이탈 끝
        } // 블랙잭 화면 끝
        void DrawRoyalCashier() // 정산 창구 출력
        { // 정산 화면 시작
            RoyalImage("notice_frame", new Rect(570, 75, 780, 900)); // 상환 창 배경
            RoyalLabel(new Rect(700, 225, 520, 60), "전액 상환", royalTitle); // 상환 화면 제목
            RoyalImage("repayment_medallion", new Rect(790, 300, 340, 260), true); // 상환 휘장
            RoyalLabel(new Rect(720, 580, 480, 75), "남은 빚  " + game.Debt.ToString("N0") + " 칩\n별도 대출  " + game.Loan.ToString("N0") + " 칩", royalText); // 상환 대상 금액
            RoyalLabel(new Rect(720, 665, 480, 60), "전액 상환하면 즉시 탈출합니다.", royalSmall); // 상환 안내
            if (RoyalButton(new Rect(760, 735, 400, 75), "전액 상환하고 탈출")) // 상환 선택
            { // 상환 처리 시작
                if (!game.RepayAll()) message = "전액 상환할 칩이 부족합니다."; // 상환 실패 안내
                else Save("event"); // 상환 성공 저장
            } // 상환 처리 끝
            if (RoyalButton(new Rect(800, 815, 320, 65), "닫기")) panel = ""; // 정산 화면 종료
        } // 정산 화면 끝
        void DrawRoyalPause() // 일시정지 메뉴 출력
        { // 정지 화면 시작
            RoyalImage("notice_frame", new Rect(610, 55, 700, 965)); // 정지 창 배경
            RoyalImage("brand_moon", new Rect(875, 125, 170, 140), true); // 달 장식
            RoyalLabel(new Rect(690, 245, 540, 50), "일시정지", royalTitle); // 정지 제목
            if (RoyalButton(new Rect(760, 320, 400, 70), "게임 계속하기")) panel = previousPanel; // 이전 화면 복원
            if (RoyalButton(new Rect(760, 395, 400, 70), "수동 저장")) Save("manual"); // 수동 저장
            if (RoyalButton(new Rect(760, 470, 400, 70), "수동 저장 불러오기")) Load("manual"); // 수동 저장 복구
            if (RoyalButton(new Rect(760, 545, 400, 70), "청구 직전 불러오기")) Load("before_bill"); // 청구 직전 복구
            if (RoyalButton(new Rect(760, 620, 400, 70), "정산 직후 불러오기")) Load("after_bill"); // 정산 직후 복구
            if (RoyalButton(new Rect(760, 695, 400, 70), "시작 지점 불러오기")) Load("start"); // 시작 지점 복구
            if (RoyalButton(new Rect(760, 770, 400, 70), "최근 이벤트 불러오기")) Load("event"); // 최근 이벤트 복구
        } // 정지 화면 끝
        void DrawRoyalEnd() // 실패와 탈출 결과 출력
        { // 결과 화면 시작
            RoyalImage(game.GameOver ? "background_gameover" : "background_victory", new Rect(0, 0, 1920, 1080)); // 결과 배경 이미지
            RoyalImage("notice_frame", new Rect(80, 75, 850, 940)); // 결과 창 배경
            RoyalImage(game.GameOver ? "icon_debt" : "repayment_medallion", new Rect(340, 180, 300, 240), true); // 결과 장식
            RoyalLabel(new Rect(200, 425, 600, 70), game.GameOver ? "청구 실패" : "빚을 모두 상환했습니다", royalTitle); // 결과 제목
            RoyalLabel(new Rect(250, 505, 530, 80), game.GameOver ? "청구액을 납부하지 못해 끌려갔습니다." : "모든 빚을 갚고 혼자 탈출했습니다.", royalText); // 결과 설명
            if (RoyalButton(new Rect(280, 590, 450, 65), "시작 지점 불러오기")) Load("start"); // 시작 지점 복구
            if (RoyalButton(new Rect(280, 660, 450, 65), "청구 직전 불러오기")) Load("before_bill"); // 청구 직전 복구
            if (RoyalButton(new Rect(280, 730, 450, 65), "수동 저장 불러오기")) Load("manual"); // 수동 저장 복구
            if (RoyalButton(new Rect(280, 800, 450, 65), "정산 직후 불러오기")) Load("after_bill"); // 정산 직후 복구
            if (RoyalButton(new Rect(280, 870, 450, 65), "최근 이벤트 불러오기")) Load("event"); // 최근 이벤트 복구
        } // 결과 화면 끝
    } // UI 구성 끝
} // 이름 공간 끝
