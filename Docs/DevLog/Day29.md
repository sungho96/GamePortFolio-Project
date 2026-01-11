📅 Day 29 – Drag Drop Preview Ring (Placement UX) 완성

📌 오늘 목표 (Why Day 29?)
드래그 이동(Setup 상태)에서 “놓을 수 있는 위치/불가능한 위치/판매존”을
사용자가 즉시 판단할 수 있도록 **프리뷰 링(발밑 링)** UI를 추가했다.
기존 구조(DragUnit ↔ GridManager 레이캐스트)를 갈아엎지 않고,
**최소 수정 + 규칙 통일**에 집중했다.

---

## ✅ 오늘 완료한 기능
### 1) 드래그 프리뷰 링(Preview Marker) 구현
- 드래그 중 마우스 위치 기준으로 “현재 드롭 후보”를 실시간 표시
- 링 1개 오브젝트만 움직이는 방식(타일/슬롯 자체 색 변경 X)
- 상태별 색상 표현
  - ✅ 배치 가능: Green
  - ❌ 배치 불가: Red
  - 💰 판매존: Orange (Sell Label은 최종 삭제하고 색상만으로 표현)

### 2) 우선순위 규칙 확정(교차검증 기준)
드래그 중 후보 판정 우선순위:
1) SellZone
2) Board Tile
3) Bench Slot
4) None → 링 숨김

### 3) 실패 시 처리 규칙 유지
- 드롭 실패(유효 후보 없음) 시 **원위치 복귀**
- 성공/실패 관계 없이 마지막에 프리뷰 링은 무조건 Hide 처리

---

## 🧩 구현 구조 (기존 구조 존중)
### 신규 스크립트
- `Assets/Scripts/TFT/UI/DropPreviewMarker.cs`
  - 링 오브젝트 1개를 제어
  - `Show(pos, mode, valid)` / `Hide()` 제공
  - Sell Label은 최종 제거(링 색으로만 SellZone 표현)

### 기존 스크립트 수정 포인트(최소)
- `DragUnit.cs`
  - `OnMouseDrag()`에서 위치 갱신 후 `UpdatePreview()` 호출 추가
  - `OnMouseUp()`에서 종료 시 `Hide()` 호출 추가
  - 판정은 GridManager의 레이캐스트 함수들을 재사용해 일관성 유지

- `GridManager.cs`
  - 프리뷰 마커 참조(씬 오브젝트) 연결
  - SellZone 레이캐스트는 Trigger 대응 포함하도록 일관성 유지

---

## 🔍 디버그/검증 체크리스트 (통과)
- Setup 상태
  - 빈 타일 위: 링 Green + 타일 중앙
  - 찬 타일 위: 링 Red
  - 빈 벤치 슬롯 위: 링 Green + 슬롯 중앙
  - 찬 벤치 슬롯 위: 링 Red
  - SellZone 위: 링 Orange
  - 아무 곳도 아니면: 링 Hide

- Battle 상태
  - 드래그/프리뷰가 뜨지 않도록 제한(Setup 전용 UX 유지)

---

## ⚠️ 오늘 발견/정리한 주의점
- SellZone이 Trigger인 경우 Raycast 옵션이 Collide가 아니면 감지 누락 가능
- 프리뷰는 매 프레임 호출되므로 불필요한 Debug.Log는 콘솔 스팸 위험
- 링 오브젝트는 시작 시 거슬리지 않게 기본 비활성/초기 Hide 처리

---

## ➡️ 다음 작업 (Day 30 후보)
1) (UX) 드롭 가능 타일/슬롯 “스냅” 강화 또는 링 애니메이션(부드러운 lerp)
2) (게임성) 라운드별 Enemy 자동 스폰 + 난이도 스케일링 시작
3) (UX) Shop/Bench 조작 개선(더블클릭 배치 / 자동 배치 버튼 등)
4) (게임성) 패배 페널티(플레이어 HP) 및 라운드 UI 표시
