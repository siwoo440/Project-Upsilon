using System; // 예외 처리 기능
using UnityEngine; // 저장 변환 기능
using Moonlight; // 게임 상태 기능
public static class RestLegacyValidation // 휴식 저장 호환 검사
{ // 검사 시작
    public static void Run() // 유니티 저장 검사
    { // 검사 실행 시작
        GameState legacy = JsonUtility.FromJson<GameState>("{\"Chips\":1234,\"Debt\":9000}"); // 정력 추가 전 저장 복구
        if (legacy.Energy != 100 || legacy.MaxEnergy != 100 || legacy.RestActive) throw new Exception("구형 저장의 정력 기본값 오류"); // 구형 저장 확인
        GameState original = new GameState(new GameSettings()); // 새 휴식 상태
        original.Energy = 10; // 검사용 정력
        if (!original.StartRest(new GameSettings())) throw new Exception("휴식 시작 실패"); // 휴식 시작 확인
        GameClock.Advance(original, new GameSettings(), 25, null); // 휴식 일부 진행
        GameState restored = JsonUtility.FromJson<GameState>(JsonUtility.ToJson(original)); // 휴식 저장 복구
        if (!restored.RestActive || restored.RestRemaining != 35 || restored.Energy != 10) throw new Exception("휴식 진행 저장 복구 오류"); // 복구 상태 확인
        GameClock.Advance(restored, new GameSettings(), 35, null); // 남은 휴식 진행
        if (restored.RestActive || restored.Energy != 30) throw new Exception("휴식 완료 복구 오류"); // 단일 회복 확인
        Debug.Log("REST_LEGACY_JSON_OK"); // 검사 성공 표시
    } // 검사 실행 끝
} // 검사 끝

