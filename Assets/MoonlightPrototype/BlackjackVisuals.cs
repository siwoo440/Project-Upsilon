using UnityEngine; // 유니티 기능 사용
using System.Collections.Generic; // 카드 목록 사용
namespace Moonlight // 게임 이름 공간
{ // 이름 공간 시작
    public class BlackjackVisuals : MonoBehaviour // 테이블 카드 시각화
    { // 시각화 시작
        GameObject group; // 표시 오브젝트 묶음
        string previous = ""; // 이전 화면 상태
        Round displayedRound; // 표시 중 라운드
        readonly List<GameObject> playerCards = new List<GameObject>(); // 플레이어 표시 카드
        readonly List<GameObject> dealerCards = new List<GameObject>(); // 딜러 표시 카드
        GameObject token; // 베팅 칩 물체
        bool dealerCardHidden; // 딜러 카드 비공개 여부
        Material face, back, chip; // 카드와 칩 재질
        Font font; // 카드 글꼴
        public bool Paused; // 연출 정지 여부
        Material MakeMaterial(Color color) // 단색 재질 생성
        { // 재질 생성 시작
            Shader shader = Shader.Find("Universal Render Pipeline/Lit"); // URP 셰이더 선택
            if (shader == null) shader = Shader.Find("Standard"); // 기본 셰이더 대체
            Material material = new Material(shader); // 재질 생성
            material.color = color; // 색상 적용
            return material; // 재질 반환
        } // 재질 생성 끝
        void Awake() // 재질 초기화
        { // 초기화 시작
            face = MakeMaterial(Color.white); // 카드 앞면 재질
            back = MakeMaterial(new Color(0.35f, 0.02f, 0.06f)); // 카드 뒷면 재질
            chip = MakeMaterial(new Color(0.9f, 0.6f, 0.1f)); // 베팅 칩 재질
            font = Font.CreateDynamicFontFromOSFont("Arial", 32); // 숫자 글꼴 생성
        } // 초기화 끝
        public void Sync(GameState game) // 게임 상태 표시
        { // 동기화 시작
            string signature = game.Seated + ":" + game.Round.phase + ":" + game.Round.wager + ":" + string.Join(",", game.Round.player) + ":" + string.Join(",", game.Round.dealer); // 표시 상태 식별
            if (signature == previous && (!game.Seated || game.Round.phase == 0 || ReferenceEquals(displayedRound, game.Round))) return; // 변경 없는 화면 유지
            previous = signature; // 현재 상태 기록
            if (!game.Seated || game.Round.phase == 0) // 착석과 라운드 확인
            { // 표시 종료 시작
                Clear(); // 테이블 표시 정리
                return; // 표시 종료
            } // 표시 종료 끝
            if (group == null || !ReferenceEquals(displayedRound, game.Round)) // 새 라운드 확인
            { // 새 라운드 시작
                Clear(); // 이전 라운드 정리
                group = new GameObject("Blackjack Cards and Wager"); // 카드 묶음 생성
                displayedRound = game.Round; // 표시할 라운드 기록
            } // 새 라운드 끝
            while (playerCards.Count < game.Round.player.Count) // 새 플레이어 카드 확인
            { // 플레이어 카드 시작
                int i = playerCards.Count; // 새 카드 위치
                playerCards.Add(Card(game.Round.player[i], false, i, 4.68f)); // 플레이어 카드 추가
            } // 플레이어 카드 끝
            while (dealerCards.Count < game.Round.dealer.Count) // 새 딜러 카드 확인
            { // 딜러 카드 시작
                int i = dealerCards.Count; // 새 카드 위치
                bool hidden = game.Round.phase == 1 && i == 1; // 비공개 카드 확인
                dealerCards.Add(Card(game.Round.dealer[i], hidden, i, 5.25f)); // 딜러 카드 추가
                if (hidden) dealerCardHidden = true; // 비공개 카드 기록
            } // 딜러 카드 끝
            if (dealerCardHidden && game.Round.phase != 1) // 딜러 카드 공개 확인
            { // 카드 공개 시작
                Destroy(dealerCards[1]); // 비공개 카드 제거
                dealerCards[1] = Card(game.Round.dealer[1], false, 1, 5.25f); // 공개 카드 표시
                dealerCardHidden = false; // 비공개 상태 해제
            } // 카드 공개 끝
            if (game.Round.wager > 0 && token == null) // 진행 중 베팅 확인
            { // 베팅 칩 시작
                token = GameObject.CreatePrimitive(PrimitiveType.Cylinder); // 칩 모양 생성
                token.name = "베팅 칩"; token.transform.SetParent(group.transform); // 칩 묶음 연결
                token.transform.position = new Vector3(1.75f, 1.1f, 4.7f); token.transform.localScale = new Vector3(0.14f, 0.025f, 0.14f); // 칩 위치와 크기
                token.GetComponent<Renderer>().sharedMaterial = chip; token.GetComponent<Collider>().enabled = false; // 칩 재질과 충돌 설정
            } // 베팅 칩 끝
            if (game.Round.wager == 0 && token != null) // 베팅 정산 확인
            { // 베팅 제거 시작
                token.transform.SetParent(null, true); // 라운드 묶음에서 칩 분리
                TokenSettlementMotion motion = token.AddComponent<TokenSettlementMotion>(); // 정산 칩 연출 추가
                motion.Owner = this; // 일시정지 상태 연결
                motion.Target = token.transform.position + new Vector3(0, 0, game.Round.result == "패배" ? 0.65f : -0.65f); // 결과별 칩 이동 방향
                token = null; // 칩 상태 초기화
            } // 베팅 제거 끝
        } // 동기화 끝
        void Clear() // 이전 라운드 표시 정리
        { // 정리 시작
            if (group != null) Destroy(group); // 이전 표시 제거
            group = null; token = null; displayedRound = null; // 라운드 표시 초기화
            playerCards.Clear(); dealerCards.Clear(); dealerCardHidden = false; // 카드 목록 초기화
        } // 정리 끝
        GameObject Card(int rank, bool hidden, int index, float z) // 카드 모형 생성
        { // 카드 생성 시작
            GameObject card = GameObject.CreatePrimitive(PrimitiveType.Cube); // 카드 판 생성
            card.name = hidden ? "비공개 카드" : "공개 카드"; card.transform.SetParent(group.transform); // 카드 묶음 연결
            Vector3 destination = new Vector3(0.55f + index * 0.25f, 1.1f + index * 0.003f, z); // 카드 최종 위치
            card.transform.position = destination + new Vector3(0, 0.3f, 0.3f); // 카드 배분 시작 위치
            CardDealMotion motion = card.AddComponent<CardDealMotion>(); // 카드 이동 연출
            motion.Owner = this; motion.Target = destination; // 연출 대상과 목적지
            card.transform.localScale = new Vector3(0.21f, 0.012f, 0.3f); // 카드 크기
            card.GetComponent<Renderer>().sharedMaterial = hidden ? back : face; card.GetComponent<Collider>().enabled = false; // 앞뒷면과 충돌 설정
            if (hidden) return card; // 비공개 카드 숫자 숨김
            GameObject textObject = new GameObject("카드 숫자"); // 숫자 물체 생성
            textObject.transform.SetParent(card.transform, false); // 카드에 숫자 연결
            textObject.transform.localPosition = new Vector3(0, 0.6f, 0); // 숫자 위치
            textObject.transform.localRotation = Quaternion.Euler(-90, 0, 0); // 숫자 위쪽 방향
            textObject.transform.localScale = new Vector3(1 / 0.21f, 1 / 0.012f, 1 / 0.3f); // 부모 크기 보정
            TextMesh text = textObject.AddComponent<TextMesh>(); // 숫자 텍스트 생성
            text.text = rank == 1 ? "A" : rank == 11 ? "J" : rank == 12 ? "Q" : rank == 13 ? "K" : rank.ToString(); // 카드 숫자 표시
            text.anchor = TextAnchor.MiddleCenter; text.alignment = TextAlignment.Center; text.characterSize = 0.1f; text.fontSize = 32; text.color = Color.black; // 글자 표시 설정
            if (font != null) { text.font = font; text.GetComponent<MeshRenderer>().sharedMaterial = font.material; } // 숫자 글꼴 적용
            return card; // 생성된 카드 반환
        } // 카드 생성 끝
        void OnDestroy() // 재질 정리
        { // 정리 시작
            if (group != null) Destroy(group); // 표시 묶음 제거
            if (face != null) Destroy(face); // 앞면 재질 제거
            if (back != null) Destroy(back); // 뒷면 재질 제거
            if (chip != null) Destroy(chip); // 칩 재질 제거
            if (font != null) Destroy(font); // 동적 글꼴 제거
        } // 정리 끝
    } // 시각화 끝
    public class CardDealMotion : MonoBehaviour // 카드 배분 연출
    { // 연출 시작
        public BlackjackVisuals Owner; // 연출 정지 상태 소유자
        public Vector3 Target; // 카드 목적지
        void Update() // 매 프레임 연출
        { // 연출 갱신 시작
            if (Owner == null || Owner.Paused) return; // 정지 상태 확인
            transform.position = Vector3.MoveTowards(transform.position, Target, Time.deltaTime * 2); // 목적지로 카드 이동
        } // 연출 갱신 끝
    } // 연출 끝
    public class TokenSettlementMotion : MonoBehaviour // 정산 칩 이동 연출
    { // 칩 연출 시작
        public BlackjackVisuals Owner; // 연출 정지 상태 소유자
        public Vector3 Target; // 칩 목적지
        void Update() // 매 프레임 연출
        { // 이동 시작
            if (Owner == null) // 테이블 종료 확인
            { // 종료 처리 시작
                Destroy(gameObject); // 남은 칩 정리
                return; // 이동 중단
            } // 종료 처리 끝
            if (Owner.Paused) return; // 일시정지 확인
            transform.position = Vector3.MoveTowards(transform.position, Target, Time.deltaTime * 1.5f); // 칩 이동
            if (Vector3.Distance(transform.position, Target) < 0.001f) Destroy(gameObject); // 이동 완료 칩 제거
        } // 이동 끝
    } // 칩 연출 끝
} // 이름 공간 끝
