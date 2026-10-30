using System; // 예외 처리 기능
using UnityEditor; // 편집기 기능
using UnityEngine; // 유니티 기능
public static class UiAssetValidation // UI 이미지 검증 도구
{ // 검증 시작
    public static void Run() // 리소스 검사
    { // 검사 시작
        foreach (string name in new string[] { "RoyalPanel", "RoyalButton", "RoyalButtonActive" }) // 필수 이미지 확인
        { // 이미지 확인 시작
            Texture2D image = Resources.Load<Texture2D>("CasinoUI/" + name); // UI 이미지 불러오기
            if (image == null || image.width < 100 || image.height < 100) throw new Exception("UI 이미지 누락: " + name); // 이미지 크기 검사
        } // 이미지 확인 끝
        Debug.Log("CASINO_UI_ASSETS_OK"); // 이미지 검증 성공
    } // 검사 끝
} // 검증 끝

