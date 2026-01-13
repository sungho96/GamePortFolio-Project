# 📅 Day 31 – Battle Death Policy & Player Survival Loop

## 🎯 오늘의 목표
- 전투 중 **Player 유닛은 죽어도 파괴(Destroy)되지 않도록** 구조 수정
- Enemy / Player의 **사망 정책 분리**
- 라운드 기반 전투 루프를 **TFT 방식에 맞게 완성**

---

## 🧠 문제 인식 (Why)

기존 구조에서는:
- 유닛 HP가 0이 되면 **무조건 Destroy**
- 이로 인해 Player 유닛도 전투에서 완전히 소멸됨
- TFT의 핵심 개념인 **“유닛은 자산이고, 라운드 단위로 반복 사용된다”**는 철학이 깨짐

👉 따라서 Player와 Enemy의 **사망 처리 정책을 분리**할 필요가 있었습니다.

---

## 🔧 핵심 변경 사항 (What)

### 1) 전투 시작 전 Player 배치 위치 저장
전투 중 유닛이 이동하더라도, 라운드 종료 후 **전투 시작 직전의 마지막 배치 위치**로 정확히 복원되도록 설계했습니다.

    CachePlacementBeforeBattle()
    → Player 유닛의 position / rotation 캐싱

---

### 2) UnitDeathEffect 역할 재정의 (책임 분리)
기존에는 **사망 연출 + Destroy**까지 UnitDeathEffect가 담당했습니다.  
이를 **연출 전용 컴포넌트**로 분리했습니다.

    UnitDeathEffect (연출 전용)
    - 사망 시 Scale + Alpha 감소 연출
    - 연출 완료 시 콜백 호출
    - Destroy / SetActive(false) 여부는 외부(GridManager)에서 결정

👉 연출과 생명주기 관리를 분리하여 **재사용성과 확장성**을 확보했습니다.

---

### 3) 전투 중 사망 처리 정책 분리

#### Enemy 유닛
- 사망 시: **연출 → Destroy**
- 라운드 종료 전 완전히 제거됨

#### Player 유닛
- 사망 시: **연출 → SetActive(false)**
- Destroy ❌
- 전투 참여 중단을 위해 `InBattle = false` 처리

    결과:
    - Player 유닛은 “쓰러짐(비활성)” 상태로 유지
    - 자산으로 남아 다음 라운드에 재사용 가능

---

### 4) 라운드 종료 후 Player 유닛 복구
라운드가 끝나면 모든 Player 유닛에 대해 다음을 수행합니다:
- 다시 활성화 (`SetActive(true)`)
- HP 완전 회복
- 타겟 / 전투 상태 리셋
- 전투 시작 전 저장해둔 위치로 복원

    CleanupAfterRound()
    → Player 유닛 복구
    → RestorePlacementForSetup()

---

## ✅ 오늘 완료된 상태 (Result)
- Player 유닛은 전투 중 죽어도 Destroy되지 않음
- Enemy 유닛만 전투 종료 시 완전 제거됨
- 사망 연출은 공통 컴포넌트로 재사용 가능
- 전투 종료 후 Player 유닛 HP 및 상태 복구 완료
- 중앙 집결 없이, 마지막 배치 위치로 정확히 복원
- TFT 스타일 라운드 기반 전투 루프 완성

---

## 🔥 느낀 점 / 설계 메모
- “사망”과 “파괴”는 전혀 다른 개념이라는 걸 명확히 분리했습니다.
- UnitDeathEffect를 연출 전용으로 만든 것이 이후 확장에 큰 이점이 됩니다.
- 이 시점부터 프로젝트가 **프로토타입 → 실제 게임 구조**로 넘어왔다고 판단했습니다.

---

## ⏭ 다음 작업 예정 (Day 32)
- Player HP UI 시각화
- 전투 결과가 숫자뿐 아니라 눈에 보이도록 표현
- 기능 확장 없이 **표시(UI)**에만 집중

