using System; // 예외 처리 기능
using System.Reflection; // 화면 배치 확인
using UnityEngine; // 유니티 기능
using Moonlight; // 게임 코드 사용
public static class RoyalLayoutValidation // 화면 크기 검증
{ // 검증 시작
    public static void Run() // 배치 검사
    { // 검사 시작
        Type layout = typeof(Prototype).Assembly.GetType("Moonlight.RoyalLayout"); // 화면 배치 도구 확인
        if (layout == null) throw new Exception("화면 크기 대응 배치가 없습니다."); // 배치 누락 확인
        MethodInfo fit = layout.GetMethod("Fit", BindingFlags.Public | BindingFlags.Static); // 크기 계산 함수 확인
        if (fit == null) throw new Exception("화면 크기 계산이 없습니다."); // 계산 누락 확인
        foreach (Vector2 size in new Vector2[] { new Vector2(1920, 1080), new Vector2(1280, 720), new Vector2(1024, 768) }) // 대표 해상도 확인
        { // 해상도 검사 시작
            Rect area = (Rect)fit.Invoke(null, new object[] { size.x, size.y }); // 화면 영역 계산
            if (area.x < 0 || area.y < 0 || area.xMax > size.x + 0.1f || area.yMax > size.y + 0.1f || Mathf.Abs(area.width / area.height - 16f / 9f) > 0.001f) throw new Exception("화면 크기 대응 오류: " + size); // 화면 비율과 경계 확인
        } // 해상도 검사 끝
        Debug.Log("ROYAL_UI_LAYOUT_OK"); // 검사 성공 표시
    } // 검사 끝
} // 검증 끝
