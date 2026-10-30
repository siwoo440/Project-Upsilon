using System; // 기본 기능 사용
using System.Collections.Generic; // 카드 목록 사용
namespace Moonlight // 게임 이름 공간
{ // 이름 공간 시작
    [Serializable] public class GameSettings // 조정 가능한 임시 설정
    { // 설정 시작
        public long startingChips = 5000000, startingDebt = 1000000000, minimumBill = 20000000; // 초기 경제 수치
        public long workReward = 4000000, minBet = 1000000, maxBet = 20000000; // 업무와 베팅 수치
        public float minutesPerSecond = 2, billingHours = 72, workMinutes = 120; // 시간 수치
        public int decks = 1; // 카드 덱 수
        public bool dealerHitsSoft17 = false; // 소프트 17 규칙
    } // 설정 끝
    [Serializable] public class Round // 블랙잭 진행 상태
    { // 라운드 시작
        public int phase; // 단계: 대기 0, 선택 1, 완료 2
        public long wager; // 예약된 베팅금
        public List<int> player = new List<int>(), dealer = new List<int>(), shoe = new List<int>(); // 카드 상태
        public string result = ""; // 결과 문구
    } // 라운드 끝
    public static class Blackjack // 독립적인 카드 판정
    { // 판정 시작
        public static int Score(IEnumerable<int> cards) // 에이스를 포함한 점수
        { // 점수 계산 시작
            int sum = 0, aces = 0; // 합계와 에이스 개수
            foreach (int card in cards) // 각 카드 확인
            { // 카드 처리 시작
                sum += Math.Min(card, 10); // 카드 값 추가
                if (card == 1) aces++; // 에이스 개수 추가
            } // 카드 처리 끝
            while (aces > 0 && sum + 10 <= 21) // 가능한 에이스 승격
            { // 승격 시작
                sum += 10; // 에이스를 11로 계산
                aces--; // 남은 에이스 감소
            } // 승격 끝
            return sum; // 최종 합계 반환
        } // 점수 계산 끝
        public static bool Soft(List<int> cards) // 소프트 핸드 확인
        { // 소프트 확인 시작
            int low = 0; // 에이스를 1로 계산한 합계
            bool ace = false; // 에이스 존재 여부
            foreach (int card in cards) { low += Math.Min(card, 10); if (card == 1) ace = true; } // 최소 합계 계산
            return ace && low + 10 <= 21; // 소프트 여부 반환
        } // 소프트 확인 끝
    } // 판정 끝
    [Serializable] public class GameState // 저장 가능한 전체 경제 상태
    { // 상태 시작
        public long Chips, Debt, Loan; // 확정 칩과 부채
        public int BillCount; // 완료한 청구 횟수
        public double Minutes, NextBill, WorkRemaining; // 게임 시각과 일정
        public uint RandomState = 2463534242; // 저장 가능한 난수 상태
        public bool GameOver, Escaped, WorkActive; // 종료와 업무 상태
        public Round Round = new Round(); // 현재 블랙잭
        public GameState() { } // 직렬화용 생성자
        public GameState(GameSettings settings) // 새 게임 생성
        { // 생성 시작
            Chips = settings.startingChips; // 시작 칩 설정
            Debt = settings.startingDebt; // 시작 빚 설정
            NextBill = settings.billingHours * 60; // 첫 청구 시각 설정
        } // 생성 끝
        public long PreviewBill(GameSettings settings) // 다음 청구액 계산
        { // 청구 계산 시작
            int tier = Math.Min(BillCount / 3, 4); // 회차별 요율 구간
            long withInterest = checked(Debt + Debt / 100 * (2 + tier) + Debt % 100 * (2 + tier) / 100); // 버림 이자 반영
            int rate = 12 + tier * 2; // 회차별 청구 비율
            long proportion = checked(withInterest / 100 * rate + (withInterest % 100 * rate + 99) / 100); // 올림 청구액
            return Math.Min(withInterest, Math.Max(settings.minimumBill, proportion)); // 최소 금액과 잔액 제한
        } // 청구 계산 끝
        public bool SettleBill(GameSettings settings) // 정기 청구 처리
        { // 정산 시작
            long due = PreviewBill(settings); // 청구액 확정
            int rate = 2 + Math.Min(BillCount / 3, 4); // 이자율 확인
            Debt = checked(Debt + Debt / 100 * rate + Debt % 100 * rate / 100); // 이자 부과
            BillCount++; // 청구 회차 증가
            NextBill += settings.billingHours * 60; // 다음 청구 시각 설정
            if (Chips < due) { GameOver = true; return false; } // 납부 불가 처리
            Chips -= due; // 확정 칩 납부
            Debt -= due; // 빚 감소
            if (Debt == 0 && Loan == 0) Escaped = true; // 전액 상환 탈출
            return true; // 납부 성공 반환
        } // 정산 끝
        public bool RepayAll() // 조기 전액 상환
        { // 조기 상환 시작
            if (Chips < Debt + Loan) return false; // 자금 부족 확인
            Chips -= Debt + Loan; // 전액 지불
            Debt = 0; Loan = 0; Escaped = true; // 부채 청산과 탈출
            return true; // 성공 반환
        } // 조기 상환 끝
        public void StartWork(GameSettings settings) // 업무 시작
        { // 업무 시작 범위
            if (!WorkActive) { WorkRemaining = settings.workMinutes; WorkActive = true; } // 미진행 업무 등록
        } // 업무 시작 끝
        public uint NextRandom() // 재현 가능한 난수
        { // 난수 생성 시작
            RandomState ^= RandomState << 13; RandomState ^= RandomState >> 17; RandomState ^= RandomState << 5; // 난수 상태 갱신
            return RandomState; // 난수 반환
        } // 난수 생성 끝
        int Draw() // 덱에서 카드 한 장 추출
        { // 추출 시작
            int index = (int)(NextRandom() % (uint)Round.shoe.Count); // 카드 위치 선택
            int card = Round.shoe[index]; // 카드 값 보관
            Round.shoe.RemoveAt(index); // 덱에서 카드 제거
            return card; // 카드 반환
        } // 추출 끝
        public bool StartBlackjack(GameSettings settings, long bet) // 베팅과 카드 배분
        { // 라운드 시작
            if (Round.phase == 1 || bet < settings.minBet || bet > settings.maxBet || bet > Chips) return false; // 베팅 가능 여부
            Chips -= bet; // 확정 칩에서 베팅 분리
            Round = new Round { phase = 1, wager = bet }; // 새 라운드 상태
            for (int d = 0; d < Math.Max(1, settings.decks); d++) // 덱 수 반복
            { // 덱 생성 시작
                for (int suit = 0; suit < 4; suit++) // 네 무늬 반복
                { // 무늬 생성 시작
                    for (int rank = 1; rank <= 13; rank++) Round.shoe.Add(rank); // 각 카드 추가
                } // 무늬 생성 끝
            } // 덱 생성 끝
            Round.player.Add(Draw()); Round.dealer.Add(Draw()); Round.player.Add(Draw()); Round.dealer.Add(Draw()); // 카드 배분
            if (Blackjack.Score(Round.player) == 21 || Blackjack.Score(Round.dealer) == 21) Resolve(settings); // 내추럴 즉시 판정
            return true; // 시작 성공 반환
        } // 라운드 시작 끝
        public void Hit(GameSettings settings) // 카드 추가
        { // 히트 시작
            if (Round.phase != 1) return; // 진행 단계 확인
            Round.player.Add(Draw()); // 플레이어 카드 추가
            if (Blackjack.Score(Round.player) >= 21) Resolve(settings); // 21 이상 판정
        } // 히트 끝
        public void Stand(GameSettings settings) // 플레이어 턴 종료
        { // 스탠드 시작
            if (Round.phase != 1) return; // 중복 정산 방지
            Resolve(settings); // 딜러 진행과 판정
        } // 스탠드 끝
        void Resolve(GameSettings settings) // 라운드 결과 확정
        { // 결과 처리 시작
            if (Round.phase != 1) return; // 중복 지급 방지
            int player = Blackjack.Score(Round.player); // 플레이어 점수
            bool playerNatural = Round.player.Count == 2 && player == 21; // 플레이어 내추럴
            bool dealerNatural = Round.dealer.Count == 2 && Blackjack.Score(Round.dealer) == 21; // 딜러 내추럴
            if (player <= 21 && !playerNatural && !dealerNatural) // 딜러 드로 조건
            { // 딜러 진행 시작
                while (Blackjack.Score(Round.dealer) < 17 || (settings.dealerHitsSoft17 && Blackjack.Score(Round.dealer) == 17 && Blackjack.Soft(Round.dealer))) Round.dealer.Add(Draw()); // 딜러 규칙 적용
            } // 딜러 진행 끝
            int dealer = Blackjack.Score(Round.dealer); // 딜러 점수
            long payout = 0; // 지급액 초기화
            if (player > 21 || (dealerNatural && !playerNatural)) Round.result = "패배"; // 플레이어 패배
            else if (playerNatural && !dealerNatural) { payout = Round.wager + Round.wager * 3 / 2; Round.result = "블랙잭 승리"; } // 내추럴 3대2 배당
            else if (playerNatural && dealerNatural) { payout = Round.wager; Round.result = "무승부"; } // 양쪽 내추럴
            else if (dealer > 21 || player > dealer) { payout = Round.wager * 2; Round.result = "승리"; } // 일반 승리
            else if (player == dealer) { payout = Round.wager; Round.result = "무승부"; } // 일반 무승부
            else Round.result = "패배"; // 딜러 승리
            Round.phase = 2; // 지급 전 완료 상태 확정
            Chips = checked(Chips + payout); // 보상 지급
            Round.wager = 0; // 예약 칩 해제
        } // 결과 처리 끝
    } // 상태 끝
} // 이름 공간 끝
