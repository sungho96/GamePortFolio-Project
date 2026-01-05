# 📅 Day 23– Shop/Bench UI 골격 구축 + 카드 클릭 구매(5오퍼) 연결

## ✅ 오늘 목표 (Why Day 23)
전투 로직은 이미 안정화되어 있으니(이동/공격/라운드 종료 흐름 정상),
오늘은 “게임처럼 보이게” 만드는 큰 기능 중 **Shop/Bench의 기본 틀**을 먼저 잡는다.

- 상점이 실제로 “카드 5장”으로 보이게 만들기
- 카드 클릭으로 구매 → 벤치에 유닛이 들어가는 흐름 만들기
- UI가 골드/오퍼 내용을 표시하도록 구성
- 기존 전투 흐름(이동/공격/라운드 전환)을 깨지 않는지 교차검증

---

## 🧱 오늘 완료한 기능 요약
### 1) Shop UI 패널 구조 생성 (Canvas 기반)
- `Canvas > ShopPanel` 생성
- `TopBar`(골드 텍스트, Roll 버튼 영역)
- `OffersRow`(오퍼 카드 5장 가로 배치)
- 각 OfferCard에 텍스트(TMP) 배치 완료
- Layout Group / ContentSizeFitter로 정렬 구성

### 2) 오퍼(Offer) 개념 정리
- Offer = “현재 상점에 노출되는 판매 후보 카드(슬롯)”
- 오늘은 오퍼 슬롯을 5개로 확장하여 “TFT 상점처럼” 보이도록 목표

### 3) ShopManager 오퍼를 5개로 확장
- 기존 3개 오퍼 → 5개 오퍼로 확장
- `Roll()` 시 5개 슬롯에 랜덤 UnitData 배치
- 콘솔에 `[SHOP] 1~5` 형태로 디버그 출력 확인

### 4) 카드 클릭 구매 연결 (키 입력 → UI 클릭)
- OfferCard 전체(이미지 자체)를 버튼처럼 사용
- 클릭 시 `GridManager.UI_Buy(index)` 호출
- 구매 성공 시:
  - 유닛 Instantiate
  - UnitData 적용
  - BenchManager의 빈 슬롯에 자동 배치
  - 골드 감소 로그 출력

### 5) GridManager 자동 참조 연결(수동 드래그 최소화)
- Grid를 매번 Inspector에서 꽂지 않도록
- `FindAnyObjectByType<GridManager>()` 방식으로 자동 연결 확인

---

## 📌 오늘 작업한 스크립트/역할
### ShopManager.cs
- pool(UnitData 목록)에서 랜덤으로 offers[5] 구성
- `GetOffer(index)`로 오퍼 접근

### BenchManager / BenchSlot
- BenchSlot: 단일 슬롯(placedUnit 보관)
- BenchManager: 빈 슬롯에 자동으로 유닛 배치(TryPlaceToEmptySlot)

### OfferCardView.cs
- 카드 클릭 이벤트를 받아 `GridManager.UI_Buy(index)` 호출
- grid 자동 연결로 Inspector 세팅 부담 감소

### ShopUI_TMP.cs (표시용)
- TMP 텍스트로 Gold / OfferText 표시
- 현재는 Update 기반으로 화면 갱신 (내일 Refresh 방식으로 정리 예정)

---

## 🧪 오늘 테스트 로그로 교차검증
### ✅ 교차검증 체크리스트(필수 3개)
1) 이동이 스르륵 자연스럽게 유지되는지 ✅  
2) 공격이 사거리에서 멈추지 않고 정상 발동하는지(쿨다운 포함) ✅  
3) 라운드 종료/보상/정리/다음 라운드 로그가 1회씩만 나오는지 ✅  

### 추가 확인
- 카드 클릭 → `UI_Buy → TryBuyFromShop` 정상 호출 ✅
- 골드 감소 로그 출력 ✅
- 벤치에 유닛 배치 ✅
- 라운드 종료 후 `PrepareNextRound` 정상 동작 ✅

---

## 🧩 오늘 관찰한 점 / 개선 포인트
### 1) 오퍼가 전부 Default로 나오는 현상
- 구조 문제는 아님. `ShopManager.pool`에 들어있는 UnitData가
  - 1개뿐이거나
  - unitId가 모두 Default로 설정된 상태일 가능성이 높음
- 내일: UnitData 에셋을 여러 개 만들어 pool에 넣어서 “상점 다양화” 예정

### 2) UI 갱신 방식 개선 필요
- 현재 `ShopUI_TMP`는 Update 갱신 방식
- 내일: 이벤트 기반 `Refresh()`로 전환해서
  - Roll/Buy 시점에만 갱신되게 정리 예정(성능/구조 안정화)

---

## 📝 오늘 결론
전투는 건드리지 않고,
**Shop/Bench “게임 기본 틀”을 실제로 보이게 만들고**
카드 클릭 구매 흐름까지 연결 완료.
내일부터는 “데이터 다양화 + UI 갱신 구조 정리”로 확장한다.

