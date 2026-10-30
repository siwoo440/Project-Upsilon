using UnityEngine; // 유니티 기능 사용
namespace Moonlight // 게임 이름 공간
{ // 이름 공간 시작
    public static class CasinoMap // 임시 카지노 공간
    { // 맵 생성기 시작
        static Transform root; // 맵 오브젝트 묶음
        static Material burgundy, gold, black, green, cream; // 공용 임시 재질
        static Material MaterialFor(Color color) // 재질 생성
        { // 재질 생성 시작
            Shader shader = Shader.Find("Universal Render Pipeline/Lit"); // URP 셰이더 선택
            if (shader == null) // 대체 셰이더 확인
            { // 대체 시작
                shader = Shader.Find("Standard"); // 기본 셰이더 선택
            } // 대체 끝
            Material material = new Material(shader); // 재질 생성
            material.color = color; // 재질 색상 설정
            return material; // 재질 반환
        } // 재질 생성 끝
        static GameObject Shape(string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material, bool collision = true) // 기본 도형 배치
        { // 배치 시작
            GameObject item = GameObject.CreatePrimitive(type); // 도형 생성
            item.name = name; // 오브젝트 이름
            item.transform.SetParent(root); // 맵에 포함
            item.transform.position = position; // 위치 설정
            item.transform.localScale = scale; // 크기 설정
            item.GetComponent<Renderer>().sharedMaterial = material; // 공용 재질 적용
            if (!collision) // 충돌 사용 여부
            { // 충돌 해제 시작
                item.GetComponent<Collider>().enabled = false; // 장식 충돌 해제
            } // 충돌 해제 끝
            return item; // 생성 결과 반환
        } // 배치 끝
        static void Box(string name, float x, float y, float z, float w, float h, float d, Material material, bool collision = true) // 상자 배치
        { // 상자 생성 시작
            Shape(name, PrimitiveType.Cube, new Vector3(x, y, z), new Vector3(w, h, d), material, collision); // 상자 생성
        } // 상자 생성 끝
        static void Table(float x, float z, string name) // 테이블 모형
        { // 테이블 생성 시작
            if (name == "블랙잭" && PlaceBlackjack(x, z)) // 블랙잭 모델 배치 확인
            { // 모델 사용 시작
                return; // 임시 도형 생략
            } // 모델 사용 끝
            Shape(name + " 받침", PrimitiveType.Cylinder, new Vector3(x, 0.45f, z), new Vector3(1.25f, 0.45f, 0.8f), gold); // 금색 테이블 받침
            Shape(name + " 천", PrimitiveType.Cylinder, new Vector3(x, 0.93f, z), new Vector3(1.13f, 0.06f, 0.7f), green, false); // 녹색 테이블 천
        } // 테이블 생성 끝
        static bool PlaceBlackjack(float x, float z) // 블랙잭 모델 배치
        { // 모델 배치 시작
            GameObject asset = Resources.Load<GameObject>("Casino/BlackjackTable"); // 모델 에셋 읽기
            if (asset == null) // 에셋 누락 확인
            { // 대체 처리 시작
                return false; // 임시 도형 사용
            } // 대체 처리 끝
            GameObject table = Object.Instantiate(asset, root); // 테이블 모델 생성
            table.name = "블랙잭 모델"; // 테이블 이름
            table.transform.position = new Vector3(x, 0, z); // 기존 테이블 위치
            foreach (Renderer part in table.GetComponentsInChildren<Renderer>()) // 모델 부품 반복
            { // 부품 재질 시작
                string partName = part.gameObject.name; // 부품 이름 확인
                part.sharedMaterial = partName.Contains("Felt") ? green : partName.Contains("Burgundy") || partName.Contains("Chip 0") || partName.Contains("Chip 2") ? burgundy : partName.Contains("pedestal") && !partName.Contains("foot") ? black : gold; // 부품별 임시 재질
            } // 부품 재질 끝
            BoxCollider collider = table.AddComponent<BoxCollider>(); // 테이블 충돌체 추가
            collider.center = new Vector3(0, 0.54f, -0.18f); // 충돌체 중심
            collider.size = new Vector3(3.02f, 1.08f, 1.46f); // 충돌체 크기
            return true; // 모델 배치 완료
        } // 모델 배치 끝
        static void Column(float x, float z) // 기둥 모형
        { // 기둥 생성 시작
            Box("기둥", x, 2, z, 0.65f, 4, 0.65f, black); // 검은 기둥
            Box("기둥 장식", x, 3.8f, z, 0.85f, 0.22f, 0.85f, gold, false); // 금색 기둥 장식
            Box("기둥 하단", x, 0.2f, z, 0.85f, 0.4f, 0.85f, gold); // 기둥 받침
        } // 기둥 생성 끝
        static void Lamp(float x, float z) // 장식 조명 모형
        { // 조명 생성 시작
            Shape("조명 받침", PrimitiveType.Cylinder, new Vector3(x, 0.18f, z), new Vector3(0.38f, 0.18f, 0.38f), gold); // 금색 조명 받침
            Shape("조명 기둥", PrimitiveType.Cylinder, new Vector3(x, 0.8f, z), new Vector3(0.1f, 0.55f, 0.1f), gold, false); // 조명 기둥
            Shape("둥근 조명", PrimitiveType.Sphere, new Vector3(x, 1.55f, z), new Vector3(0.5f, 0.5f, 0.5f), cream, false); // 따뜻한 조명 구체
        } // 조명 생성 끝
        public static void Build() // 카지노 전체 구성
        { // 맵 구성 시작
            root = new GameObject("Casino Blockout").transform; // 교체 가능한 맵 묶음
            burgundy = MaterialFor(new Color(0.28f, 0.025f, 0.06f)); // 버건디 재질
            gold = MaterialFor(new Color(0.8f, 0.51f, 0.12f)); // 금색 재질
            black = MaterialFor(new Color(0.055f, 0.045f, 0.05f)); // 검은 재질
            green = MaterialFor(new Color(0.025f, 0.28f, 0.16f)); // 게임 천 재질
            cream = MaterialFor(new Color(0.9f, 0.76f, 0.51f)); // 따뜻한 장식 재질
            Box("바닥", 0, -0.25f, 0, 42, 0.5f, 42, black); // 전체 바닥
            Box("북쪽 벽", 0, 2.5f, 21, 42, 5, 0.5f, black); // 북쪽 경계
            Box("남쪽 벽", 0, 2.5f, -21, 42, 5, 0.5f, black); // 남쪽 경계
            Box("서쪽 벽", -21, 2.5f, 0, 0.5f, 5, 42, black); // 서쪽 경계
            Box("동쪽 벽", 21, 2.5f, 0, 0.5f, 5, 42, black); // 동쪽 경계
            Box("중앙 카펫", 0, 0.012f, 0, 5, 0.025f, 40, burgundy, false); // 중앙 이동 축
            Box("카펫 금색 테두리 왼쪽", -2.35f, 0.03f, 0, 0.08f, 0.015f, 40, gold, false); // 왼쪽 금색 선
            Box("카펫 금색 테두리 오른쪽", 2.35f, 0.03f, 0, 0.08f, 0.015f, 40, gold, false); // 오른쪽 금색 선
            Box("무대 상단 장식", 0, 4.85f, 20.1f, 13, 0.22f, 0.5f, gold, false); // 무대 금색 상단
            Box("서쪽 벽 장식", -20.7f, 4.6f, 0, 0.12f, 0.14f, 41, gold, false); // 왼쪽 벽 금색 선
            Box("동쪽 벽 장식", 20.7f, 4.6f, 0, 0.12f, 0.14f, 41, gold, false); // 오른쪽 벽 금색 선
            Box("왼쪽 홀 카펫", -12, 0.014f, 4, 15, 0.025f, 16, burgundy, false); // 블랙잭 구역
            Box("오른쪽 홀 카펫", 12, 0.014f, 4, 15, 0.025f, 16, burgundy, false); // 룰렛 구역
            Box("무대 커튼", 0, 2.7f, 20.5f, 12, 4.8f, 0.3f, burgundy); // 북쪽 무대 배경
            Box("무대", 0, 0.25f, 18.7f, 11, 0.5f, 3.5f, gold); // 공연 무대
            Box("안내 데스크", 0, 0.55f, -13.5f, 4, 1.1f, 1, gold); // 입구 안내대
            Box("업무 장소", -7, 0.5f, -3, 2, 1, 1, gold); // 업무 위치
            Box("정산 창구", 7, 0.5f, 2, 2, 1, 1, gold); // 정산 위치
            Shape("루나 임시 인물", PrimitiveType.Capsule, new Vector3(-5, 1, 4), new Vector3(0.8f, 1, 0.8f), cream); // 루나 표시
            Table(1, 5, "블랙잭"); // 상호작용 테이블
            Table(-12, 1, "카드 테이블 1"); // 왼쪽 게임 테이블
            Table(-12, 8, "카드 테이블 2"); // 왼쪽 게임 테이블
            Table(12, 1, "룰렛 모형 1"); // 오른쪽 게임 테이블
            Table(12, 8, "룰렛 모형 2"); // 오른쪽 게임 테이블
            for (int i = 0; i < 5; i++) // 슬롯 모형 반복
            { // 슬롯 배치 시작
                Box("슬롯 모형 " + i, 19, 1.1f, -6 + i * 2.7f, 1.1f, 2.2f, 1.2f, gold); // 슬롯 기계 외형
                Box("슬롯 화면 " + i, 18.42f, 1.4f, -6 + i * 2.7f, 0.04f, 0.9f, 0.8f, green, false); // 슬롯 화면 표시
            } // 슬롯 배치 끝
            for (int i = -1; i <= 1; i++) // 기둥 줄 반복
            { // 기둥 배치 시작
                Column(-3.8f, i * 9); // 왼쪽 중앙 기둥
                Column(3.8f, i * 9); // 오른쪽 중앙 기둥
                Lamp(-5, i * 9); // 왼쪽 중앙 조명
                Lamp(5, i * 9); // 오른쪽 중앙 조명
            } // 기둥 배치 끝
            Box("라운지 소파 왼쪽", -15, 0.45f, -14, 5, 0.9f, 1.2f, burgundy); // 왼쪽 휴식 공간
            Box("VIP 소파", 14, 0.45f, 16, 5, 0.9f, 1.2f, burgundy); // VIP 임시 공간
            Shape("중앙 장식 받침", PrimitiveType.Cylinder, new Vector3(0, 0.5f, 11), new Vector3(1.4f, 0.5f, 1.4f), gold); // 중앙 장식
            Shape("중앙 조각 모형", PrimitiveType.Sphere, new Vector3(0, 1.65f, 11), new Vector3(0.8f, 1.3f, 0.8f), cream, false); // 조각상 자리
            GameObject lightObject = new GameObject("Lobby Light"); // 전체 조명 생성
            lightObject.transform.SetParent(root); // 맵에 조명 포함
            Light light = lightObject.AddComponent<Light>(); // 조명 구성 요소
            light.type = LightType.Directional; // 방향 조명 설정
            light.intensity = 1.1f; // 조명 밝기
            light.color = new Color(1, 0.82f, 0.62f); // 따뜻한 조명색
            lightObject.transform.rotation = Quaternion.Euler(50, -30, 0); // 조명 각도
        } // 맵 구성 끝
    } // 맵 생성기 끝
} // 이름 공간 끝
