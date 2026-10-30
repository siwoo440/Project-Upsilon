using UnityEditor; // 편집기 기능 사용
using UnityEditor.SceneManagement; // 장면 편집 기능 사용
using UnityEngine; // 유니티 기능 사용
using Moonlight; // 시제품 기능 사용
using System; // 예외 처리 사용
using System.Collections.Generic; // 재질 목록 사용
public static class PrototypeSceneBuilder // 시제품 장면 생성 도구
{ // 도구 시작
    public static void SaveEnvironmentInScene() // 기존 맵을 장면 오브젝트로 저장
    { // 장면 배치 시작
        var scene = EditorSceneManager.OpenScene("Assets/Moonlight/Prototype.unity", OpenSceneMode.Single); // 시제품 장면 열기
        if (GameObject.Find("Casino Blockout") != null) throw new Exception("장면에 기존 맵이 있어 생성을 중단합니다."); // 기존 맵 보호
        if (!AssetDatabase.IsValidFolder("Assets/Moonlight/EnvironmentMaterials")) AssetDatabase.CreateFolder("Assets/Moonlight", "EnvironmentMaterials"); // 재질 폴더 생성
        CasinoMap.Build(); // 기존 맵 구조 생성
        GameObject environment = GameObject.Find("Casino Blockout"); // 생성된 맵 확인
        if (environment == null) throw new Exception("카지노 맵 생성 실패"); // 생성 실패 확인
        Dictionary<Material, Material> saved = new Dictionary<Material, Material>(); // 재질 에셋 대응표
        foreach (Renderer renderer in environment.GetComponentsInChildren<Renderer>(true)) // 맵의 렌더러 반복
        { // 렌더러 처리 시작
            Material[] materials = renderer.sharedMaterials; // 현재 재질 목록
            for (int i = 0; i < materials.Length; i++) // 각 재질 확인
            { // 재질 처리 시작
                Material source = materials[i]; // 원본 재질
                if (source == null || AssetDatabase.Contains(source)) continue; // 저장된 재질 유지
                if (!saved.ContainsKey(source)) // 임시 재질 확인
                { // 재질 저장 시작
                    Material asset = new Material(source); // 재질 에셋 복제
                    string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Moonlight/EnvironmentMaterials/Environment.mat"); // 충돌 없는 저장 경로
                    AssetDatabase.CreateAsset(asset, path); // 재질 에셋 저장
                    saved.Add(source, asset); // 재질 대응 기록
                } // 재질 저장 끝
                materials[i] = saved[source]; // 저장된 재질 연결
            } // 재질 처리 끝
            renderer.sharedMaterials = materials; // 렌더러 재질 반영
        } // 렌더러 처리 끝
        EditorSceneManager.MarkSceneDirty(scene); // 장면 변경 표시
        if (!EditorSceneManager.SaveScene(scene)) throw new Exception("카지노 장면 저장 실패"); // 장면 저장
        AssetDatabase.SaveAssets(); // 재질 에셋 저장
        Debug.Log("SCENE_ENVIRONMENT_SAVED " + environment.transform.childCount + " 재질 " + saved.Count); // 저장 결과 기록
    } // 장면 배치 끝
    [MenuItem("Moonlight/시제품 장면 만들기")] // 편집기 메뉴 등록
    public static void CreateScene() // 독립 장면 생성
    { // 장면 생성 시작
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); // 빈 장면 생성
        var root = new GameObject("Moonlight Prototype"); // 시제품 시작 오브젝트
        root.AddComponent<Prototype>(); // 시제품 구성 요소 추가
        if (!AssetDatabase.IsValidFolder("Assets/Moonlight")) AssetDatabase.CreateFolder("Assets", "Moonlight"); // 장면 폴더 생성
        EditorSceneManager.SaveScene(scene, "Assets/Moonlight/Prototype.unity"); // 독립 장면 저장
        AssetDatabase.SaveAssets(); // 에셋 변경 저장
        SaveEnvironmentInScene(); // 장면 안에 맵 오브젝트 저장
        Debug.Log("시제품 장면 생성 완료: Assets/Moonlight/Prototype.unity"); // 완료 알림
    } // 장면 생성 끝
} // 도구 끝
