using System; // 예외 처리 기능
using UnityEditor.SceneManagement; // 씬 편집 기능
using UnityEngine; // 오브젝트 기능
using UnityEngine.SceneManagement; // 씬 기능
using Moonlight; // 상호작용 지점 사용
public static class InteractionPointSetup // 상호작용 지점 배치
{ // 배치 도구 시작
    public static void Build() // 기존 씬에 지점 저장
    { // 배치 시작
        Scene scene = EditorSceneManager.OpenScene("Assets/Moonlight/Prototype.unity", OpenSceneMode.Single); // 기존 카지노 씬 열기
        if (GameObject.Find("Interaction Targets") != null) throw new Exception("기존 상호작용 지점이 있습니다."); // 기존 작업 보존
        GameObject root = new GameObject("Interaction Targets"); // 지점 묶음 생성
        Add(root, "Luna", "luna", -5, 4, 3); // 루나 지점 배치
        Add(root, "Work", "work", -7, -3, 3); // 업무 지점 배치
        Add(root, "Blackjack", "blackjack", 1, 2.9f, 1.8f); // 블랙잭 지점 배치
        Add(root, "Cashier", "cashier", 7, 2, 3); // 정산 지점 배치
        EditorSceneManager.MarkSceneDirty(scene); // 씬 변경 표시
        if (!EditorSceneManager.SaveScene(scene)) throw new Exception("상호작용 지점 저장 실패"); // 씬 저장
        Debug.Log("INTERACTION_POINTS_SAVED"); // 저장 성공 표시
    } // 배치 끝
    static void Add(GameObject root, string name, string target, float x, float z, float radius) // 지점 생성
    { // 생성 시작
        GameObject point = new GameObject(name); // 지점 오브젝트
        point.transform.SetParent(root.transform); // 지점 묶음 연결
        point.transform.position = new Vector3(x, 0, z); // 지점 위치
        SceneInteractionPoint interaction = point.AddComponent<SceneInteractionPoint>(); // 지점 기능 연결
        interaction.target = target; // 대상 종류 설정
        interaction.radius = radius; // 상호작용 거리 설정
    } // 생성 끝
} // 배치 도구 끝

