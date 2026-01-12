# Day 30 – Wave Spawn(B안) + 진영 분리 + 배치 복원(Setup) 안정화

## 오늘 목표
- B안: 라운드별 Enemy 웨이브 테이블로 자동 스폰
- 보드 상/하 진영 분리 (위=적, 아래=플레이어)
- 전투 종료 후 Setup 복귀 시 “전투 시작 직전 배치 위치” 그대로 복원
- 드롭 프리뷰(링) 색상이 “실제 드롭 가능 조건”과 100% 일치하도록 통일

---

## 완료한 기능/작업

### 1) Wave Table(SO) 기반 Enemy 스폰(B안)
- `WaveTableSo(ScriptableObject)` 생성 후 라운드별 웨이브 데이터 입력
- `StartBattle()`에서 라운드에 맞는 웨이브를 찾아 Enemy를 상단 타일에 자동 배치

**체크**
- WaveTable 에셋의 `Waves` 리스트가 비어있으면 `"[Wave] no wave data..."` 경고 발생
- EnemyZone 타일이 부족할 때 `IndexOutOfRange` 방지(커서 범위 체크 후 중단)

---

### 2) 진영 분리(상/하)
- `height / 2` 기준으로 y축을 나눔  
  - 아래 절반: `PlayerZone`
  - 위 절반: `EnemyZone`

```csharp
public bool IsPlayerZone(Tile t)
{
    if (t == null) return false;
    int mid = height / 2;
    return t.gridPos.y < mid;
}

public bool IsEnemyZone(Tile t)
{
    if (t == null) return false;
    int mid = height / 2;
    return t.gridPos.y >= mid;
}
```

---

### 3) 배치 복원(Setup)
- 중앙 스냅 X
- 전투 시작 직전(Setup 마지막 배치) 위치를 저장해두고, 다음 Setup에서 그대로 복원

**캐시**
- `cachedSetupPos: Dictionary<Unit, Vector3>`
- `cachedSetupRot: Dictionary<Unit, Quaternion>`

**흐름**
- `StartBattle()` 직전에 `CachePlacementBeforeBattle()` 호출
- `PrepareNextRound()`에서 Setup 전환 후 `RestorePlacementForSetup()` 호출
- 복원 후 캐시 `Clear`

---

### 4) DropPreviewMarker(링 프리뷰) 판정 버그 수정
**문제**
- 상단(적 진영)에서도 프리뷰 링이 초록(Valid)으로 떠서 UX가 틀어짐
- 실제 드롭은 실패인데 프리뷰만 초록 → 판정 불일치

**해결**
- 판정을 `GridManager` 한 곳으로 단일화
- 프리뷰도 실제 드롭도 동일 함수 사용

```csharp
public bool CanPlaceUnitOnTile(Unit u, Tile tile)
{
    if (u == null || tile == null) return false;
    if (!tile.isPlaceable) return false;
    if (tile.placedUnit != null) return false;

    // 플레이어는 아래 진영만 허용
    if (u.team == TeamType.Player && !IsPlayerZone(tile)) return false;

    return true;
}
```

**UpatePreview() 수정**
- 기존: `tile.isPlaceable && tile.placedUnit == null`
- 변경: `grid.CanPlaceUnitOnTile(unit, tile)`

---

## 교차검증 테스트(통과)
- Setup에서 플레이어 유닛을 상단 타일에 드롭 시도  
  - 프리뷰: 빨강  
  - 드롭: 실패(원위치 복귀)
- 하단 타일 드롭  
  - 프리뷰: 초록  
  - 드롭: 성공
- `B`키 전투 시작  
  - 라운드 웨이브대로 적 스폰(상단)
- 전투 종료 → End 진입
- `N`키로 다음 라운드 Setup 복귀  
  - 플레이어 유닛이 전투 시작 직전 배치 위치로 복원
- 라운드2에서 다시 `B`키  
  - 라운드2 웨이브로 적 스폰 정상

---

## 오늘 잡은 이슈
- `"[Wave] no wave data for round N"`
  - 원인: WaveTable 에셋 `Waves` 리스트가 비어있음
  - 해결: 인스펙터에서 waves에 `round/spawns` 입력
- EnemyZone가 꽉 찼을 때 cursor가 계속 증가하며 `IndexOutOfRange` 가능
  - 해결: `cursor >= emptyTiles.Count`면 `return/break`로 중단
- 프리뷰 링이 상단에서도 초록으로 뜨는 문제
  - 해결: `CanPlaceUnitOnTile()` 단일화로 프리뷰/드롭 판정 통일

---

## 스크립트 위치 기록(신규/수정)
- `Assets/Scripts/TFT/Round/WaveTableSo.cs` (WaveTable ScriptableObject)
- `Assets/Scripts/TFT/Board/GridManager.cs` (Wave 스폰, 진영 분리, 캐시/복원, 단일 판정 함수)
- `Assets/Scripts/TFT/Input/DragUnit.cs` (프리뷰 판정 통일, 드롭 판정 통일)

---

## Day31 준비 메모(합의 완료, 오늘은 미구현)
- Q1: 플레이어 유닛은 라운드 끝나면 “부활/회복”(영구 사망 X)
- Q2: 패배 페널티는 “남은 적 유닛 기반”으로 Player HP 감소

