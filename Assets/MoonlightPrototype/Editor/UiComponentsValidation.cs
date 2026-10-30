using System; // 예외 처리 기능
using UnityEditor; // 편집기 기능
using UnityEngine; // 유니티 기능
public static class UiComponentsValidation // UI 구성 이미지 검증
{ // 검증 시작
    public static void Run() // 구성 이미지 검사
    { // 검사 시작
        Texture2D[] images = Resources.LoadAll<Texture2D>("CasinoUI/Components"); // 구성 이미지 불러오기
        if (images.Length < 48) throw new Exception("UI 구성 이미지 수 오류: " + images.Length); // 이미지 수 확인
        Debug.Log("CASINO_UI_COMPONENTS_OK: " + images.Length); // 검사 성공 표시
    } // 검사 끝
} // 검증 끝
