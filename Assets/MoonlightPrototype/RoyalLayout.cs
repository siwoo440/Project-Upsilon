using UnityEngine; // 유니티 화면 기능
namespace Moonlight // 게임 이름 공간
{ // 이름 공간 시작
    public static class RoyalLayout // 화면 비율 계산
    { // 배치 계산 시작
        public static Rect Fit(float width, float height) // 화면 안쪽 기준 영역
        { // 비율 계산 시작
            float scale = Mathf.Min(width / 1920f, height / 1080f); // 기준 화면 축척
            float fittedWidth = 1920f * scale; // 축척 적용 너비
            float fittedHeight = 1080f * scale; // 축척 적용 높이
            return new Rect((width - fittedWidth) / 2f, (height - fittedHeight) / 2f, fittedWidth, fittedHeight); // 중앙 배치 영역
        } // 비율 계산 끝
    } // 배치 계산 끝
} // 이름 공간 끝
