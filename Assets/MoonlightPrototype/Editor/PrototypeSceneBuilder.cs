using UnityEditor; // 편집기 기능 사용
using UnityEditor.SceneManagement; // 장면 편집 기능 사용
using UnityEngine; // 유니티 기능 사용
using Moonlight; // 시제품 기능 사용
public static class PrototypeSceneBuilder // 시제품 장면 생성 도구
{ // 도구 시작
    [MenuItem("Moonlight/시제품 장면 만들기")] // 편집기 메뉴 등록
    public static void CreateScene() // 독립 장면 생성
    { // 장면 생성 시작
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); // 빈 장면 생성
        var root = new GameObject("Moonlight Prototype"); // 시제품 시작 오브젝트
        root.AddComponent<Prototype>(); // 시제품 구성 요소 추가
        if (!AssetDatabase.IsValidFolder("Assets/Moonlight")) AssetDatabase.CreateFolder("Assets", "Moonlight"); // 장면 폴더 생성
        EditorSceneManager.SaveScene(scene, "Assets/Moonlight/Prototype.unity"); // 독립 장면 저장
        AssetDatabase.SaveAssets(); // 에셋 변경 저장
        Debug.Log("시제품 장면 생성 완료: Assets/Moonlight/Prototype.unity"); // 완료 알림
    } // 장면 생성 끝
} // 도구 끝
