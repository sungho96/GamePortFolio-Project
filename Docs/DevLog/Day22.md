# Day 22 – Shop UI (Modern UI Pack) 기본 틀 구축 + 5 Offer 레이아웃 안정화

## 목표 (Why Day 22?)
Day21까지 전투/라운드 흐름은 안정화되었고,
이제 “TFT 느낌의 게임 틀”을 만들기 위해 가장 중요한 Shop/Bench의 UI 기반을 먼저 구축했습니다.

오늘의 목표는 “기능 자동화/이벤트 연결”이 아니라,
**Modern UI Pack(MUIP)을 사용한 Shop UI 레이아웃을 안정적으로 잡는 것**이었습니다.

---

## 오늘 완료한 것 (What I did)

### 1) ShopPanel (하단 고정) 구성
- Canvas 아래에 `ShopPanel`을 만들고 화면 하단에 고정되도록 RectTransform을 세팅했습니다.
- Anchor: Bottom Stretch 기반
- Pos Y = 0
- Pivot Y = 0 권장(높이 조절 시 아래 기준으로 유지되게)

✅ 결과: 해상도/화면 비율이 바뀌어도 ShopPanel이 하단에 붙는 기반 완성.

---

### 2) TopBar 구성 (GoldText / Spacer / RollButton)
- ShopPanel 내부에 `TopBar` 생성
- GoldText, Spacer, RollButton 배치
- “RollButton이 오른쪽 끝으로 안 붙는 문제”를 해결하는 과정에서
  Layout Group과 MUIP 오브젝트 구조(Disabled/Normal/Highlighted/Ripple)가 레이아웃에 영향을 준다는 걸 확인함.

#### RollButton 정렬 이슈 핵심 원인
- `Layout Element > Ignore Layout`이 켜져 있으면 LayoutGroup이 해당 오브젝트를 배치 대상으로 취급하지 않음
- `Content Size Fitter + Layout Group` 조합은 서로 크기 결정을 싸워서 위치가 “찝찝하게” 흔들릴 수 있음
- Horizontal Layout Group에서 `Use Child Scale`, `Child Force Expand` 옵션이 켜져 있으면 MUIP 버튼 구조와 충돌하며 배치가 어색해질 수 있음

✅ 대응:
- Ignore Layout 체크 상태를 확인/해제
- Content Size Fitter는 TopBar에서는 비활성화(또는 제거)하는 방향을 우선 권장
- Force Expand / Use Child Scale 옵션은 필요 시 끄는 방향으로 정리

---

### 3) OffersRow + OfferCard 5개 레이아웃 구성 (핵심)
- `OffersRow` 생성 후 `Horizontal Layout Group`으로 5개 카드 자동 정렬 기반 구축
- OfferCard_1을 만든 뒤 복제하여 5개 구성

#### OffersRow가 “이상하게 보이던 문제” 원인
- OffersRow가 Center Anchor + 100x100 크기였던 상태에서 카드 5개를 넣어 찌그러짐 발생
- 해결: OffersRow를 ShopPanel 내부에서 Stretch로 잡아서 폭을 확보해야 함

✅ 결과:
- OfferCard 5개가 하단에서 일정하게 배치되는 “TFT 상점 모양” 기반 완성

---

## 오늘은 일부러 안 한 것 (Not today)
### 버튼 자동 연결(코드로 UnityEvent 바인딩)
- RollButton / BuyButton들을 코드로 자동 연결하는 작업은 내일로 미룸
- 오늘은 “레이아웃 기반이 완성”되는 것이 우선이며,
  자동 연결은 UI 구조가 확정된 후에 진행하는 것이 버그를 줄일 수 있음

---

## 교차검증 체크리스트 (Regression)
오늘 UI 작업이 기존 전투/라운드 로직을 깨지 않는지 확인:

1) 이동이 스르륵 자연스럽게 유지되는지 ✅  
2) 공격이 사거리에서 멈추지 않고 정상 발동하는지(쿨다운 포함) ✅  
3) 라운드 종료/보상/정리/다음 라운드 로그가 1회씩만 나오는지 ✅  

추가 UI 확인:
- ShopPanel이 Game 뷰에서 하단 고정되는지 ✅
- OffersRow가 5개 카드 정렬로 “찌그러지지 않는지” ✅

