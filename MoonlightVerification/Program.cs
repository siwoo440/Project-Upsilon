using System; // 기본 기능 사용
using Moonlight; // 게임 규칙 사용
using System.Text.Json; // 저장 상태 검증
class Program // 검증 실행기
{ // 실행기 시작
    static void Check(bool ok, string name) // 결과 확인
    { // 확인 시작
        if (!ok) throw new Exception(name); // 실패 알림
        Console.WriteLine("PASS " + name); // 성공 표시
    } // 확인 끝
    static void Main() // 검증 시작
    { // 검증 범위 시작
        var settings = new GameSettings(); // 기본 설정 생성
        var game = new GameState(settings); // 초기 상태 생성
        Check(game.PreviewBill(settings) == 122400000, "첫 청구액"); // 청구액 확인
        game.Chips = 122400000; // 납부 자금 설정
        Check(game.SettleBill(settings), "청구 납부"); // 납부 확인
        Check(game.Debt == 897600000 && game.Chips == 0, "첫 청구 잔액"); // 잔액 확인
        Check(Blackjack.Score(new int[] { 1, 1, 9 }) == 21, "복수 에이스"); // 에이스 점수 확인
        Check(Blackjack.Score(new int[] { 10, 8, 5 }) == 23, "버스트 점수"); // 초과 점수 확인
        var wager = new GameState(settings); // 베팅 상태 생성
        wager.Chips = 122400000; // 납부 자금 설정
        wager.StartBlackjack(settings, 1000000); // 베팅 시작
        Check(!wager.SettleBill(settings), "베팅금 청구 제외"); // 베팅 제외 확인
        var repeat = new GameState(settings); // 중복 지급 검증 상태
        repeat.Chips = 10000000; // 칩 설정
        repeat.StartBlackjack(settings, 1000000); // 게임 시작
        if (repeat.Round.phase == 1) repeat.Stand(settings); // 진행 중 라운드 완료
        long balance = repeat.Chips; // 지급 후 잔액 보관
        repeat.Stand(settings); // 중복 완료 시도
        Check(repeat.Chips == balance, "블랙잭 중복 정산 방지"); // 중복 지급 확인
        var saved = new GameState(settings); // 복구 검증 상태
        saved.Chips = 10000000; saved.StartWork(settings); saved.StartBlackjack(settings, 1000000); // 업무와 베팅 시작
        var options = new JsonSerializerOptions { IncludeFields = true }; // 공개 필드 저장 설정
        var restored = JsonSerializer.Deserialize<GameState>(JsonSerializer.Serialize(saved, options), options); // 상태 직렬화와 복구
        Check(restored.WorkActive && restored.WorkRemaining == saved.WorkRemaining && restored.Round.wager == saved.Round.wager && restored.RandomState == saved.RandomState, "업무와 난수 상태 복구"); // 진행 상태 확인
        if (saved.Round.phase == 1) { saved.Stand(settings); restored.Stand(settings); } // 복구 전후 동일한 선택
        Check(saved.Chips == restored.Chips && saved.Round.result == restored.Round.result, "복구 후 동일한 결과"); // 결과 재현 확인
        var small = new GameState(settings); small.Debt = 101; // 작은 빚 설정
        Check(small.PreviewBill(settings) == 103, "최소 청구와 이자 버림"); // 잔액 상한 확인
        var repay = new GameState(settings); repay.Debt = 10; repay.Loan = 5; repay.Chips = 15; // 대출 포함 상환 설정
        Check(repay.RepayAll() && repay.Escaped && repay.Chips == 0, "대출 포함 전액 상환"); // 조기 탈출 확인
        Check(MapLayout.Nearest(-5, 4) == "luna", "루나 위치 선택"); // 루나 대상 확인
        Check(MapLayout.Nearest(-7, -3) == "work", "업무 위치 선택"); // 업무 대상 확인
        Check(MapLayout.Nearest(1, 5) == "blackjack", "블랙잭 위치 선택"); // 게임 대상 확인
        Check(MapLayout.Nearest(7, 2) == "cashier", "정산 위치 선택"); // 정산 대상 확인
        Check(MapLayout.Nearest(0, -17) == "", "먼 거리 상호작용 차단"); // 거리 제한 확인
        Check(MenuState.TogglePause("blackjack", "") == "pause", "진행 화면에서 일시정지"); // 일시정지 확인
        Check(MenuState.TogglePause("pause", "blackjack") == "blackjack", "일시정지 전 화면 복원"); // 게임 화면 복원 확인
        var work = new GameState(settings); // 업무 검증 상태
        work.Chips = 0; work.StartWork(settings); // 칩 없이 업무 시작
        GameClock.Advance(work, settings, settings.workMinutes, null); // 업무 완료까지 진행
        Check(work.Chips == settings.workReward && !work.WorkActive, "업무 완료 보상"); // 업무 보상 확인
        GameClock.Advance(work, settings, 1, null); // 추가 시간 진행
        Check(work.Chips == settings.workReward, "업무 보상 중복 방지"); // 중복 보상 확인
        var simultaneous = new GameState(settings); // 동시 발생 검증 상태
        simultaneous.Chips = 0; simultaneous.NextBill = settings.workMinutes; simultaneous.StartWork(settings); // 업무와 청구 시각 일치
        GameClock.Advance(simultaneous, settings, settings.workMinutes, null); // 동시 시각까지 진행
        Check(simultaneous.GameOver && simultaneous.Chips == 0, "청구 시 미확정 업무 보상 제외"); // 청구 우선 확인
        for (int tier = 0; tier < 5; tier++) // 이자 구간 반복
        { // 구간 검증 시작
            var tierGame = new GameState(settings); // 구간별 청구 상태
            tierGame.BillCount = tier * 3; // 청구 회차 설정
            long expectedDebt = 1000000000L + 1000000000L * (2 + tier) / 100; // 이자 포함 빚
            long expectedBill = expectedDebt * (12 + tier * 2) / 100; // 구간별 청구액
            Check(tierGame.PreviewBill(settings) == expectedBill, "이자 구간 " + (tier + 1)); // 구간 청구액 확인
        } // 구간 검증 끝
        var pendingBet = new GameState(settings); // 청구 중 게임 상태
        pendingBet.Chips = 200000000; // 청구와 베팅 자금
        pendingBet.Chips -= 1000000; // 베팅 칩 분리
        pendingBet.Round = new Round { phase = 1, wager = 1000000 }; // 진행 중 베팅 설정
        pendingBet.NextBill = 1; // 빠른 청구 시각
        GameClock.Advance(pendingBet, settings, 1, null); // 청구 시각 진행
        Check(!pendingBet.GameOver && pendingBet.Round.wager == 1000000 && pendingBet.Round.phase == 1, "납부 후 베팅 유지"); // 베팅 지속 확인
        Check(Blackjack.Score(new int[] { 1, 6 }) == 17, "소프트 17 점수"); // 소프트 합계 확인
        Check(Blackjack.Soft(new System.Collections.Generic.List<int> { 1, 6 }), "소프트 17 판정"); // 소프트 여부 확인
        Check(Blackjack.Score(new int[] { 1, 6, 10 }) == 17, "에이스 1 전환"); // 에이스 감소 확인
        var tieRound = new GameState(settings); // 무승부 검증 상태
        tieRound.Chips = 0; // 남은 칩 설정
        tieRound.Round = new Round { phase = 1, wager = 100, player = new System.Collections.Generic.List<int> { 10, 8 }, dealer = new System.Collections.Generic.List<int> { 10, 8 } }; // 동점 카드 설정
        tieRound.Stand(settings); // 라운드 정산
        Check(tieRound.Chips == 100 && tieRound.Round.result == "무승부", "무승부 원금 반환"); // 무승부 지급 확인
        var naturalRound = new GameState(settings); // 내추럴 검증 상태
        naturalRound.Chips = 0; // 남은 칩 설정
        naturalRound.Round = new Round { phase = 1, wager = 100, player = new System.Collections.Generic.List<int> { 1, 10 }, dealer = new System.Collections.Generic.List<int> { 10, 9 } }; // 블랙잭 카드 설정
        naturalRound.Stand(settings); // 라운드 정산
        Check(naturalRound.Chips == 250 && naturalRound.Round.result == "블랙잭 승리", "블랙잭 3대2 배당"); // 블랙잭 지급 확인
        var softStand = new GameState(settings); // 소프트 스탠드 검증 상태
        softStand.Chips = 0; // 보유 칩 초기화
        softStand.Round = new Round { phase = 1, wager = 100, player = new System.Collections.Generic.List<int> { 10, 8 }, dealer = new System.Collections.Generic.List<int> { 1, 6 }, shoe = new System.Collections.Generic.List<int> { 4 } }; // 소프트 17 카드 설정
        settings.dealerHitsSoft17 = false; // 소프트 17 스탠드 규칙
        softStand.Stand(settings); // 스탠드 결과 계산
        Check(softStand.Chips == 200 && softStand.Round.dealer.Count == 2, "딜러 소프트 17 스탠드"); // 스탠드 동작 확인
        var softHit = new GameState(settings); // 소프트 히트 검증 상태
        softHit.Chips = 0; // 보유 칩 초기화
        softHit.Round = new Round { phase = 1, wager = 100, player = new System.Collections.Generic.List<int> { 10, 8 }, dealer = new System.Collections.Generic.List<int> { 1, 6 }, shoe = new System.Collections.Generic.List<int> { 4 } }; // 동일 카드 설정
        settings.dealerHitsSoft17 = true; // 소프트 17 히트 규칙
        softHit.Stand(settings); // 히트 결과 계산
        Check(softHit.Chips == 0 && softHit.Round.dealer.Count == 3, "딜러 소프트 17 히트"); // 히트 동작 확인
        settings.dealerHitsSoft17 = false; // 기본 규칙 복원
        var checkpoint = new GameState(settings); // 자동 저장 검증 상태
        checkpoint.Chips = 200000000; checkpoint.NextBill = 1; // 납부 가능한 청구 설정
        string beforeSnapshot = ""; string afterSnapshot = ""; // 자동 저장 자료 보관
        GameClock.Advance(checkpoint, settings, 1, name => // 청구 시각 진행
        { // 자동 저장 알림 시작
            if (name == "before_bill") beforeSnapshot = JsonSerializer.Serialize(checkpoint, options); // 청구 직전 보관
            if (name == "after_bill") afterSnapshot = JsonSerializer.Serialize(checkpoint, options); // 정산 직후 보관
        }); // 자동 저장 알림 끝
        var beforeRestore = JsonSerializer.Deserialize<GameState>(beforeSnapshot, options); // 청구 직전 복구
        var afterRestore = JsonSerializer.Deserialize<GameState>(afterSnapshot, options); // 정산 직후 복구
        GameClock.Advance(beforeRestore, settings, 0.1, null); // 청구 재실행
        GameClock.Advance(afterRestore, settings, 0.1, null); // 정산 이후 진행
        Check(beforeRestore.Debt == checkpoint.Debt && beforeRestore.Chips == checkpoint.Chips && beforeRestore.BillCount == 1, "청구 직전 복구 후 단일 정산"); // 청구 직전 복구 검증
        Check(afterRestore.Debt == checkpoint.Debt && afterRestore.Chips == checkpoint.Chips && afterRestore.BillCount == 1, "정산 직후 복구 중복 방지"); // 정산 직후 복구 검증
        var flowSettings = new GameSettings { startingChips = 100, startingDebt = 1000, minimumBill = 1, workReward = 3000, workMinutes = 2, minBet = 10, maxBet = 100, billingHours = 1 }; // 빠른 통합 검증 설정
        var flow = new GameState(flowSettings); // 새 게임 시작
        flow.StartWork(flowSettings); // 업무 시작
        GameClock.Advance(flow, flowSettings, 2, null); // 업무 완료
        Check(flow.StartBlackjack(flowSettings, 10), "통합 흐름 베팅 시작"); // 베팅 가능 확인
        if (flow.Round.phase == 1) // 진행 중 라운드 확인
        { // 라운드 완료 시작
            flow.Stand(flowSettings); // 딜러 정산
        } // 라운드 완료 끝
        GameClock.Advance(flow, flowSettings, 58, null); // 첫 청구까지 진행
        Check(!flow.GameOver && flow.BillCount == 1, "통합 흐름 청구 납부"); // 정기 청구 확인
        Check(flow.RepayAll() && flow.Escaped, "통합 흐름 전액 상환 탈출"); // 최종 탈출 확인
    } // 검증 범위 끝
} // 실행기 끝
