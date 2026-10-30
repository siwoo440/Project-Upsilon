using UnityEngine; // 유니티 공간 기능
namespace Moonlight // 게임 이름 공간
{ // 이름 공간 시작
    public class SceneInteractionPoint : MonoBehaviour // 씬에 배치할 상호작용 지점
    { // 상호작용 지점 시작
        public string target; // 상호작용 대상 종류
        public float radius = 3; // 상호작용 가능 거리
        public static string Nearest(Vector3 position) // 가까운 지점 찾기
        { // 지점 탐색 시작
            string selected = ""; // 선택 대상 초기화
            float best = float.MaxValue; // 가장 가까운 거리
            foreach (SceneInteractionPoint point in Object.FindObjectsByType<SceneInteractionPoint>(FindObjectsSortMode.None)) // 활성 지점 확인
            { // 지점 검사 시작
                float dx = position.x - point.transform.position.x; // 가로 거리
                float dz = position.z - point.transform.position.z; // 세로 거리
                float distance = dx * dx + dz * dz; // 거리 제곱
                if (distance < point.radius * point.radius && distance < best) // 범위와 거리 확인
                { // 지점 선택 시작
                    best = distance; // 가까운 거리 갱신
                    selected = point.target; // 상호작용 대상 갱신
                } // 지점 선택 끝
            } // 지점 검사 끝
            return selected; // 가까운 대상 반환
        } // 지점 탐색 끝
    } // 상호작용 지점 끝
} // 이름 공간 끝

