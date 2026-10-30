using System.Collections.Generic; // 씬 목록 기능
using UnityEditor; // 편집기 기능
using UnityEditor.SceneManagement; // 씬 저장 기능
using UnityEngine; // 게임 오브젝트 기능
using UnityEngine.SceneManagement; // 씬 기능
using Moonlight; // 타이틀 화면 사용
public static class TitleSceneBuilder // 타이틀 씬 제작 도구
{ // 도구 시작
    public static void Build() // 타이틀 씬 제작
    { // 제작 시작
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); // 빈 타이틀 씬
        GameObject cameraObject = new GameObject("Title Camera"); // 타이틀 카메라 객체
        cameraObject.AddComponent<Camera>(); // 카메라 연결
        cameraObject.AddComponent<TitleMenu>(); // 타이틀 화면 연결
        EditorSceneManager.SaveScene(scene, "Assets/Moonlight/Title.unity"); // 씬 파일 저장
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(); // 빌드 씬 목록
        scenes.Add(new EditorBuildSettingsScene("Assets/Moonlight/Title.unity", true)); // 첫 타이틀 씬
        foreach (EditorBuildSettingsScene existing in EditorBuildSettings.scenes) // 기존 빌드 씬 확인
        { // 기존 씬 처리 시작
            if (existing.path != "Assets/Moonlight/Title.unity") scenes.Add(existing); // 기존 씬 유지
        } // 기존 씬 처리 끝
        EditorBuildSettings.scenes = scenes.ToArray(); // 빌드 씬 순서 저장
        AssetDatabase.SaveAssets(); // 변경 사항 저장
        Debug.Log("TITLE_SCENE_CREATED"); // 제작 완료 표시
    } // 제작 끝
} // 도구 끝
