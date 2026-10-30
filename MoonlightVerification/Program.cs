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
    } // 검증 범위 끝
} // 실행기 끝
