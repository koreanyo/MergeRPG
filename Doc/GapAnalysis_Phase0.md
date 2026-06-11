# Gap Analysis — Phase 0 설계 vs 현재 구현
> 기준일: 2026-03-15
> 비교 대상: 노션 Phase 0 Unity 구현 명세서 vs `Assets/Scripts/` 실제 코드

---

## 요약

Phase 0 핵심 루프(EventBus, MergeBoard, Generator, BattleManager, CharacterView, PopupManager, GameManager) 는 모두 구현됨.
**동작 테스트 가능 상태**이나 아래 Gap들이 남아 있음.

---

## ✅ 명세와 일치하는 것

| 항목 | 명세 | 구현 |
|------|------|------|
| ATB 공식 | `atbGauge += SPD * Time.deltaTime * 100f` | ✅ CharacterView.cs |
| 데미지 공식 | `damage = ATK - DEF (최소 1)` | ✅ BattleManager.ProcessAttackHit |
| EventBus 타입 | string 페이로드 통일 | ✅ EventNames.cs |
| BattleManager 자료구조 | effectiveTier Dict, activeCharacters List, _charViews (unitId키), _enemyViews (instanceId키) | ✅ BattleManager.cs |
| Generator | 클릭 기반 스폰, WeightModifier 확률 구조 | ✅ GeneratorView (IPointerClickHandler) |
| MergeBoard | 6×7 그리드, 드래그 합성 | ✅ MergeBoard.cs |
| 데이터 형식 | JSON (ScriptableObject 아님) | ✅ DataManager.cs |
| OnCharDied 페이로드 | string (unitId) | ✅ |
| Merge 시 OnCharDied 미발행 | MergeBoard 내부 조용히 처리 | ✅ MergeBoard.MergePieces |

---

## ❌ Gap (설계와 다른 것)

### 1. BootScene 없음 — **중요**
- **명세**: Boot → Game → Result → Game(재시작) 3씬 구조
- **현재**: GameScene 1개만 존재. BootScene 없음
- **영향**: 씬 전환 흐름 불완전. DataManager가 모든 씬에 있어야 하는 경우 문제 발생 가능
- **대응**: Phase 0 테스트용으로 BootScene은 빈 씬으로라도 생성 필요

### 2. ResultScene 없음 — **크래시 위험**
- **명세**: 스테이지 클리어/실패 시 ResultScene으로 전환
- **현재**: GameManager.cs에서 `SceneManager.LoadScene("ResultScene")` 호출하나 해당 씬 미존재 → 런타임 에러
- **대응**: 빈 ResultScene 씬 생성 + BuildSettings에 등록 필요

### 3. tier 최대값 불일치
- **명세**: tier 1~3
- **현재**: `game_balance.json`에 `maxTier: 5`, unit_data.json에 tier 1~5 데이터
- **영향**: 밸런스 검증 목적엔 큰 문제 없으나 명세와 다름
- **대응**: Phase 0 검증용이므로 현재 유지해도 무방. 명세 업데이트 또는 tier 3으로 줄이기 논의 필요

### 4. 임시 수치 불일치
- **명세**: DUMMY_A tier1 — hp:30, atk:5, spd:1.0, def:1
- **현재**: unit_data.json — hp:100, atk:10, spd:1.0, def:3
- **영향**: 전투 속도/난이도가 명세 기준보다 빠름 (DEF 대비 ATK 비율 동일하므로 체감 큰 차이 없을 수 있음)
- **대응**: Phase 0 목표가 느낌 검증이므로 수치 자체보다 흐름이 중요. 현재 유지 가능

### 5. 적 배율 체계 불완전
- **명세**: 일반 ×1.0, 엘리트 ×1.8, 보스 ×3.5
- **현재**: stage_config.json에 웨이브 2개 (multiplier 1.0, 2.0). 보스 없음
- **영향**: 보스 웨이브 없어 스테이지 엔딩 없음. 단, 2웨이브 클리어 시 ResultScene으로 가는 흐름은 동일
- **대응**: ResultScene 구현 후 함께 조정

### 6. CharacterView — Animator 미연결
- **명세**: "Animator 연동, 이동/공격 흐름" 명시
- **현재**: CharacterView.cs에 Animator 필드 있으나 프리팹에 Animator Controller 없음. MoveAndAttack 코루틴으로 위치 이동만 함
- **영향**: 공격 애니메이션 없음. Animation Event(OnAttackHit, OnActionComplete)가 발동 안 됨
- **대응**: Phase 0 목적인 "느낌 검증"에는 이동 코루틴만으로 충분. 실제 스프라이트/애니메이터 연결은 Phase 1에서

### 7. WorldCanvas (PopupManager) 연결 미확인
- **명세**: PopupManager는 World Space Canvas에 팝업 표시
- **현재**: PopupManager.cs에 `worldCanvas` 필드 있으나 씬에서 실제 연결됐는지 미검증
- **영향**: 머지 시 +ATK/+HP 팝업이 안 보일 수 있음
- **대응**: Play 후 머지 시 팝업 확인 필요. 없으면 PopupManager GO에 WorldCanvas 수동 연결

### 8. HP 바 / ATB 게이지 UI 없음
- **명세**: Phase 0 시각 요건에 명시적 HP바 언급 없으나 "느낌 검증" 목적상 필요
- **현재**: CharacterView에 Label(TMP) 텍스트만 있고 HP바/게이지 시각화 없음
- **영향**: 전투 진행 여부를 시각적으로 확인 불가 (로그로만 가능)
- **대응**: Phase 0 완성을 위해 간단한 텍스트 HP 표시 추가 권장

---

## ⚠️ 설계 미결 과제 (코드와 무관, 기획 수준)

| 항목 | 상태 |
|------|------|
| 우호 기믹 종류/수치 | 미결 |
| 메타 성장 시스템 | 미결 |
| 보상 구조 | 미결 |
| 캐릭터 3종 개성(패시브) | 미결 |
| 생성기 밸런스 검증 방법 | 미결 |
| 머지 테이블 전체 | 미결 |
| BootScene 용도 및 구조 | 미결 |

---

## 🎯 다음 할 일 (우선순위순)

### Phase 0 완성 필수
1. **ResultScene 생성** — 씬 파일 + BuildSettings 등록. 내용은 "Game Over / Clear" 텍스트만으로도 충분
2. **전투 동작 테스트** — Play → ATB 루프 → 공격 → 사망 → 웨이브 클리어 → ResultScene 전환 확인
3. **PopupManager WorldCanvas 연결 확인** — 머지 시 +ATK/+HP 팝업 표시 여부

### Phase 0 완성 권장
4. **HP/ATK 텍스트 표시** — CharacterView Label에 현재 HP 수치 표시 (전투 진행 확인용)
5. **BootScene 생성** — 빈 씬으로 생성 후 DataManager 초기화용으로 활용

### Phase 1 진입 조건 (Phase 0 검증 후)
6. **실제 스프라이트 연결** — 더미 색상 사각형 → 실제 캐릭터 이미지
7. **Animator Controller 연결** — 공격/피격/사망 애니메이션
8. **3종 캐릭터 데이터 추가** — DUMMY_A 외 실제 캐릭터 3종
9. **기믹 시스템 기초 구현** — 보스 직전 1회 발동 구조
