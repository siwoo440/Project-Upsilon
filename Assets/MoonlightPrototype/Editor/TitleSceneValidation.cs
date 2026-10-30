using System; // 예외 처리 기능
using UnityEditor; // 빌드 설정 기능
using UnityEngine; // 리소스 확인 기능
public static class TitleSceneValidation // 타이틀 씬 검증
{ // 검증 시작
    public static void Run() // 타이틀 구성 검사
    { // 검사 시작
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Moonlight/Title.unity") == null) throw new Exception("타이틀 씬이 없습니다."); // 타이틀 씬 확인
        if (EditorBuildSettings.scenes.Length == 0 || EditorBuildSettings.scenes[0].path != "Assets/Moonlight/Title.unity" || !EditorBuildSettings.scenes[0].enabled) throw new Exception("타이틀 시작 씬이 등록되지 않았습니다."); // 시작 씬 확인
        if (Resources.Load<Texture2D>("CasinoUI/TitleBackground") == null) throw new Exception("타이틀 이미지가 없습니다."); // 타이틀 이미지 확인
        Debug.Log("TITLE_SCENE_OK"); // 검사 성공 표시
    } // 검사 끝
} // 검증 끝
