using System; // 기본 계산 사용
namespace Moonlight // 게임 이름 공간
{ // 이름 공간 시작
    public static class MenuState // 메뉴 상태 전환
    { // 메뉴 상태 시작
        public static string TogglePause(string current, string previous) // 일시정지 전환
        { // 전환 시작
            return current == "pause" ? previous : "pause"; // 이전 화면 또는 정지 화면
        } // 전환 끝
    } // 메뉴 상태 끝
    public static class MapLayout // 상호작용 위치 정의
    { // 위치 정의 시작
        public static string Zone(float x, float z) // 현재 공간 이름
        { // 공간 판별 시작
            if (z >= 24 && x < -20) return "카드 게임룸"; // 왼쪽 위 카드룸
            if (z >= 24 && x > 20) return "VIP 라운지"; // 오른쪽 위 VIP 공간
            if (z >= 28 && Math.Abs(x) <= 20) return "공연 무대"; // 중앙 공연 공간
            if (x <= -22 && z < -5) return "바·휴식 공간"; // 왼쪽 아래 바
            if (x >= 42 && z < 24) return "슬롯머신 구역"; // 오른쪽 슬롯 구역
            if (z < -13 && Math.Abs(x) < 20) return "입구·안내 데스크"; // 남쪽 입구
            if (x <= -7 && z >= -5 && z < 24) return "블랙잭·카드 홀"; // 왼쪽 카드 홀
            if (x >= 10 && z >= -5 && z < 24) return "룰렛 구역"; // 오른쪽 룰렛 홀
            return "중앙 홀"; // 중앙 이동 공간
        } // 공간 판별 끝
        public static string Nearest(float x, float z) // 가장 가까운 대상 찾기
        { // 탐색 시작
            string result = ""; // 선택 결과 초기화
            float best = float.MaxValue; // 최근접 거리 초기화
            Check(x, z, -5, 4, 3, "luna", ref result, ref best); // 루나 거리 확인
            Check(x, z, -7, -3, 3, "work", ref result, ref best); // 업무 거리 확인
            Check(x, z, 1, 2.9f, 1.8f, "blackjack", ref result, ref best); // 블랙잭 의자 거리 확인
            Check(x, z, 7, 2, 3, "cashier", ref result, ref best); // 정산 거리 확인
            return result; // 선택 결과 반환
        } // 탐색 끝
        static void Check(float x, float z, float targetX, float targetZ, float radius, string name, ref string result, ref float best) // 대상별 거리 비교
        { // 비교 시작
            float dx = x - targetX; // 가로 거리
            float dz = z - targetZ; // 세로 거리
            float distance = dx * dx + dz * dz; // 거리 제곱
            if (distance < radius * radius && distance < best) // 유효한 최근접 대상 확인
            { // 대상 선택 시작
                best = distance; // 최근접 거리 갱신
                result = name; // 대상 이름 갱신
            } // 대상 선택 끝
        } // 비교 끝
    } // 위치 정의 끝
} // 이름 공간 끝
