# Project Memory

> 이 파일은 짧게 유지한다. 상세 내용은 Doc/Design/ 참조.

---

## 프로젝트 개요 (불변)
- **장르**: 머지 + RPG + 로그라이트, 모바일
- **개발 방식**: Data-Driven (엑셀 → JSON 테이블)
- **리소스**: 에셋 활용 + 프리팹 기반
- **목표**: 상업화

---

## 📂 Archive (최근 3개)
- [2026-05-01: 머지보드 구조 확정](History/2026-05-01_머지보드구조확정.md)
- [2026-05-09: 머지보드 뷰 구조 정비](History/2026-05-09_머지보드뷰구조정비.md)

---

## ⚡ 오늘의 작업 (가변 영역 - 날짜 변경 시 초기화)
> Last Updated: 2026-06-06

### 현재 단계
신규 세션 시작. 이전 작업 내용은 Archive 참조.

### 다음 할 일
(이전 세션 인계 항목)
- [ ] 셀/피스 크기 가시성 조정 (cellSize, 피스 Border 넘침 구조 정리)
- [ ] 🔴 ATB 공식 수정: CharacterView.cs `spd * deltaTime * 100f` → `spd * deltaTime`
- [ ] 🔴 unit_data.json SPD 수치 수정: Tier1=25, Tier2=30, Tier3=40
- [ ] 🟠 SessionData static class 구현 + BattleManager/GameManager 연동
- [ ] 🟠 PopupManager.ShowResult() → ShowResultPopup() 리네임
- [ ] Phase 0 완료 기준 달성 테스트 (생성→머지→전투→게임오버 루프)

### 주의사항
- MergeBoard: 7×4=28칸, cellSize=120, cellSpacing=10, paddingLeft=40, paddingTop=40, Upper Left 기준
- MergeBoard.cs는 Panel_Bottom 오브젝트에 직접 붙어있음
- UnitPieceView_1: 루트 100×100, Bg 116×116, Border 120×120 (stretch+sizeDelta 구조)
- PlacePiece()에서 피스 anchor를 (0,1)로 강제 설정 (CellView와 동일 기준)
- UnitPieceView가 IDropHandler 구현 → 피스 위 드롭 시 data.cell로 포워딩
- Script Execution Order: BattleManager(-100) > MergeBoard(-90) > GameManager(-80)
- Enter Play Mode: Domain Reload 비활성화 → ResetStatic() 패턴 모든 싱글턴에 필수
- 노션 이슈 트래커: 331c996b-376d-8180-9319-e53768bc92ef
