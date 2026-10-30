using System.IO; // 저장 파일 확인
using UnityEngine; // 화면 기능
using UnityEngine.SceneManagement; // 씬 전환 기능
namespace Moonlight // 게임 이름 공간
{ // 이름 공간 시작
    public class TitleMenu : MonoBehaviour // 타이틀 화면
    { // 타이틀 시작
        public static string PendingSlot; // 시작 후 불러올 저장 슬롯
        Texture2D background; // 타이틀 배경 이미지
        Texture2D normalButton, selectedButton; // 버튼 배경 이미지
        GUIStyle title, text, button; // 제목과 버튼 스타일
        public static bool HasManualSave() // 수동 저장 존재 확인
        { // 저장 확인 시작
            return File.Exists(Path.Combine(Application.persistentDataPath, "moonlight_manual.json")); // 수동 저장 여부
        } // 저장 확인 끝
        public static void BeginNew() // 새 게임 시작
        { // 새 게임 시작 범위
            PendingSlot = null; // 이전 불러오기 요청 제거
            SceneManager.LoadScene("Assets/Moonlight/Prototype.unity"); // 카지노 씬 열기
        } // 새 게임 시작 끝
        public static bool BeginContinue() // 수동 저장 이어하기
        { // 이어하기 시작
            if (!HasManualSave()) return false; // 저장 파일 부재 확인
            PendingSlot = "manual"; // 불러올 슬롯 기록
            SceneManager.LoadScene("Assets/Moonlight/Prototype.unity"); // 카지노 씬 열기
            return true; // 시작 성공 전달
        } // 이어하기 끝
        void OnGUI() // 타이틀 UI 출력
        { // 화면 시작
            if (title == null) // 스타일 초기화 확인
            { // 스타일 준비 시작
                background = Resources.Load<Texture2D>("CasinoUI/TitleBackground"); // 배경 이미지 읽기
                Font font = Font.CreateDynamicFontFromOSFont("Malgun Gothic", 24); // 한글 글꼴
                title = new GUIStyle(GUI.skin.label) { font = font, fontSize = 54, clipping = TextClipping.Clip }; // 제목 스타일
                title.normal.textColor = new Color(1f, 0.85f, 0.6f); // 금빛 제목
                text = new GUIStyle(GUI.skin.label) { font = font, fontSize = 23, clipping = TextClipping.Clip }; // 안내 스타일
                text.normal.textColor = Color.white; // 안내 글자 색상
                button = new GUIStyle(GUIStyle.none) { font = font, fontSize = 24, clipping = TextClipping.Clip, alignment = TextAnchor.MiddleCenter }; // 버튼 스타일
                normalButton = Resources.Load<Texture2D>("CasinoUI/Components/button_secondary_dark"); // 버튼 배경
                selectedButton = Resources.Load<Texture2D>("CasinoUI/Components/button_selected"); // 강조 배경
                button.normal.textColor = Color.white; // 버튼 글자 색상
                button.hover.textColor = Color.white; // 강조 글자 색상
            } // 스타일 준비 끝
            Matrix4x4 previous = GUI.matrix; // 기존 화면 변환
            Rect fit = RoyalLayout.Fit(Screen.width, Screen.height); // 화면 비율 계산
            GUI.matrix = Matrix4x4.TRS(new Vector3(fit.x, fit.y, 0), Quaternion.identity, new Vector3(fit.width / 1920f, fit.height / 1080f, 1)); // 기준 화면 적용
            if (background != null) GUI.DrawTexture(new Rect(0, 0, 1920, 1080), background, ScaleMode.ScaleAndCrop); // 배경 이미지 출력
            GUI.Label(new Rect(80, 180, 800, 90), "MOONLIGHT ROYAL", title); // 게임 제목
            GUI.Label(new Rect(85, 280, 600, 45), "카지노 호텔 생존 게임", text); // 게임 설명
            if (DrawButton(new Rect(90, 410, 460, 85), "새 게임")) BeginNew(); // 새 게임 선택
            GUI.enabled = HasManualSave(); // 저장 파일 존재 확인
            if (DrawButton(new Rect(90, 520, 460, 85), "수동 저장 이어하기")) BeginContinue(); // 저장 불러오기 선택
            GUI.enabled = true; // 버튼 활성 상태 복원
            if (DrawButton(new Rect(90, 630, 460, 85), "종료")) Application.Quit(); // 게임 종료 선택
            GUI.matrix = previous; // 기존 화면 변환 복원
        } // 화면 끝
        bool DrawButton(Rect area, string label) // 타이틀 버튼 출력
        { // 버튼 출력 시작
            Texture2D image = GUI.enabled && area.Contains(Event.current.mousePosition) ? selectedButton : normalButton; // 강조 이미지 선택
            if (image != null) GUI.DrawTextureWithTexCoords(area, image, new Rect(0.03f, 0.24f, 0.94f, 0.52f), true); // 이미지 여백 제거 표시
            return GUI.Button(area, label, button); // 버튼 입력 확인
        } // 버튼 출력 끝
    } // 타이틀 끝
} // 이름 공간 끝
