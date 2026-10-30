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
        public static string Nearest(float x, float z) // 가장 가까운 대상 찾기
        { // 탐색 시작
            string result = ""; // 선택 결과 초기화
            float best = float.MaxValue; // 최근접 거리 초기화
            Check(x, z, -5, 4, 3, "luna", ref result, ref best); // 루나 거리 확인
            Check(x, z, -7, -3, 3, "work", ref result, ref best); // 업무 거리 확인
            Check(x, z, 1, 5, 3.5f, "blackjack", ref result, ref best); // 블랙잭 거리 확인
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
