# Day 21 – Round Flow Stabilization (End → Reward → Cleanup → Next Round)

## 목표 (Why Day 21?)
전투 자체는 돌아가는데, 라운드가 끝났을 때 다음 문제가 자주 생김:

- 라운드 종료 처리가 여러 번 호출됨 (Result/Income 로그 중복)
- 죽은 유닛/타겟 참조가 남아서 다음 라운드에서 꼬임
- 다음 라운드 진입 시점이 불안정함 (Setup/Battle/End 상태 전이 불명확)

👉 Day 21의 목표는 **라운드 종료 흐름을 “단 1회”로 고정**하고  
**보상/정리/다음 라운드 준비를 안정적으로 연결**하는 것.

---

## 오늘 한 일 (What I did)

### 1) 라운드 종료를 “딱 1번만” 실행되게 만들기
- `CheckBattleEnd()`가 프레임마다 계속 호출되면서
  `Result`, `Income`, `Cleanup`, `PrepareNextRound`가 중복 실행되는 현상을 막음.
- **중복 방지용 변수를 도입**해서 같은 라운드에서는 보상이 다시 지급되지 않게 함.

예시 방식:
- `lastRewardedRound` (혹은 `hasRoundEnded` 같은 플래그)
- 라운드가 끝났을 때 한 번만 true가 되도록

---

### 2) RoundEnd에서 정리(Cleanup) 루틴 확실히 수행
라운드가 끝나면 다음을 정리:

- Enemy 오브젝트 제거
- 각 유닛의 `currentTarget` 제거/초기화
- 타일의 `placedUnit` 참조 정리 (죽은 유닛 남아있는 경우)

로그로 검증:
- `[RoundEnd] Cleanup done (enemy removed, targets removed)` 가 **1회만** 출력되는지 확인

---

### 3) Next Round 준비 흐름 확정
- End 상태에서 다음 라운드로 넘어가는 진입을 안정화
- `PrepareNextRound()`에서 다음 라운드 번호 증가, Setup 상태 복귀
- AutoRoll(상점 자동 리롤)을 켠 경우 다음 라운드 시작에 맞춰 동작 확인

검증 로그:
- `Prepare Round 2 | Gold=17 | AutoRoll=ON`

---

## 교차검증(버그 재발 방지 체크리스트)

### ✅ 반드시 “각 1회만” 떠야 정상
- `round 1 Result: Player win`
- `[Income] Round 1 ...`
- `[RoundEnd] Cleanup done ...`
- `Prepare Round 2 ...`

### ✅ 반복 실행하면 버그 의심
- Result가 여러 번 찍힘
- Income이 라운드당 여러 번 들어옴
- Cleanup 로그가 연속으로 반복됨

---

## 결과 (Outcome)
- 라운드 종료 → 보상 → 정리 → 다음 라운드 준비 흐름이
  **딱 1회씩** 순서대로 실행됨을 로그로 확인.
- 다음 라운드로 넘어가도 타겟/유닛 참조가 꼬이지 않음.

---

## 다음 할 일 (Next)
- (굵직한 기능) 라운드별 **Enemy 자동 스폰 + 난이도 증가**
- (UX) Shop/Bench 조작 개선 (키 입력 → 클릭/드래그 기반)
- (게임성) 패배 페널티(플레이어 HP) 및 라운드 진행 UI
