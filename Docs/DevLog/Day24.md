📅 Day 24 – Bench ↔ Board 드래그&드롭 + Roll 버튼 연결 + Bench 전투 버그 차단

## ✅ 오늘의 목표
1) **Roll 버튼(Modern UI) 클릭 → Shop Roll 자동 실행** (수동 연결 최소화)
2) **벤치 유닛이 전투에 끼어드는 버그 차단** (Bench = 대기, InBattle=false 유지)
3) **드래그로 Bench → Board 배치 구현** (드롭 성공/실패 처리 포함)
4) (선택) **Board → Bench 드롭**까지 확장 준비

---

## 🧱 오늘 작업 정리

### 1) Roll 버튼 자동 연결/구현
- 버튼을 눌렀을 때 반응이 없었던 원인:
  - 클릭 이벤트는 들어오는데(Grid 연결 OK), **실제 Roll 호출이 연결되지 않았거나**
  - Modern UI 버튼 구조 때문에 Unity Button onClick과 구조가 달라 **이벤트 바인딩이 누락**될 수 있음
- 해결 방향:
  - RollButton 쪽 스크립트에서 GridManager의 `UI_Roll()`을 직접 호출하는 구조로 정리
  - Debug로 `[RollButton] clicked` 로그 확인 완료

> 오늘 결론: 클릭 이벤트 수신 OK → 이제 “UI_Roll 호출이 확실히 실행되게” 바인딩을 고정하는 구조로 정리함.

---

### 2) Bench 유닛이 전투 참여하는 버그 차단
- 문제: Bench에 있는 유닛이 보드 밖에서 적을 공격하거나 전투 루프에 잡히는 현상
- 핵심 해결:
  - 전투 루프에서 **InBattle** 체크 우선 적용
  - Bench에 놓이는 유닛은 기본적으로 **InBattle=false**, 보드에 드롭 성공 시 **true**

> 결과: Bench 유닛은 “존재는 하지만 전투 시스템에서 완전히 제외”되도록 설계.

---

### 3) Bench → Board 드래그&드롭 구현 (핵심)
- `DragUnit` 스크립트로 Bench 위 유닛을 마우스로 드래그:
  - OnMouseDown: 시작 위치 저장 / BenchSlot 추적
  - OnMouseDrag: 바닥 plane 기준으로 위치 이동
  - OnMouseUp: 타일 판정 → 성공 시 타일에 배치 / 실패 시 원위치 복귀

#### 🔥 제일 큰 문제였던 부분: “드롭 위치에서 타일이 안 잡힘”
- 증상 로그:
  - `[DragUnit] ray -> tile=False`
  - `[DragUnit] FAIL: tile is null (raycast didn't hit Tile) -> revert`
- 원인:
  - `Physics.Raycast`에 layerMask를 썼는데, **tileLayerMask가 Nothing** 상태
  - Tile 오브젝트의 Layer가 마스크와 안 맞으면 Raycast가 무조건 실패함

✅ 해결:
- GridManager에 타일 레이어 마스크를 추가하고,
- Tile 오브젝트 Layer를 `Tile`로 맞춘 뒤,
- `GetTileUnderWorld()`에서 해당 마스크로 Raycast 처리

> 최종 결과:  
> 로그에 `ray -> tile=True` + `SUCCESS: dropped to tile` 확인 → 실제 배치도 정상 동작.

---

## ✅ 오늘 최종 결과(상태)
- ✅ Shop UI 정상 표시 + 카드 5개 표시 유지
- ✅ Shop 구매 → Bench 앞에서부터 빈 슬롯 채우기 정상
- ✅ Bench 유닛 전투 참여 버그 차단(루프 InBattle 체크 기반)
- ✅ Bench → Board 드래그 드롭 “완벽하게 성공”
- ✅ Tile LayerMask 세팅 문제 해결

---

## 🧪 오늘 테스트 체크리스트
- [x] Roll 버튼 클릭 로그 찍힘
- [x] Roll 호출 시 offers 텍스트 갱신됨
- [x] Bench에서 유닛 드래그 → 보드 타일 위에 드롭 성공
- [x] 보드 타일 아닌 곳에 드롭 → 원위치 복귀
- [x] 이미 유닛 있는 타일에 드롭 → 원위치 복귀
- [x] 보드에 올라간 유닛만 전투 참여

---

## 📌 내일 할 일 (Day 25 후보)
### A. (우선) Board → Bench 드롭 구현 ✅ (오늘의 연장선)
- 보드 위 유닛을 드래그해서 Bench 슬롯 위에 드롭하면
  - 빈 슬롯이면 배치 성공
  - 꽉 찼으면 원위치 복귀

### B. (필요하면) Board ↔ Bench 드롭 시 “스왑(교환)” 기능
- 같은 곳에 유닛이 이미 있으면 자리 바꾸기

### C. 드래그 UX 개선
- 드롭 가능한 타일/벤치 슬롯 하이라이트
- 드래그 중 유닛이 UI에 걸릴 때 입력 충돌 방지(EventSystem 체크)

---

