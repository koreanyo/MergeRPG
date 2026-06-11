# Project Memory & Context Handover

<!-- ================================================================================== -->
<!-- 🔒 IMMUTABLE ZONE (불변 영역): 사용자 요청 없이 AI 임의 수정 금지                            -->
<!-- ================================================================================== -->

## 0. ⚡ Session Workflow (작업 루틴 SOP)
### 🏁 Start Session (시작)
*   **명령어**: `"프로젝트 메모리를 읽어줘"`
*   **AI 행동 수칙**:
    1.  이 파일(`Project_Memory.md`)을 정독한다.
    2. Doc폴더와 Scripts 폴더의 모든 내용을 꼼꼼하고 세세하게 정독한다.
    3.  **날짜 확인 (Date Check)**: 가변 영역(Mutable Zone)의 `Last Updated` 날짜가 오늘과 같은지 비교.
    4.  **날짜가 변경되었다면 (New Day)**:
        *   현재 가변 영역(Mutable Zone)의 내용을 요약하여 `Doc/YYYY-MM-DD_작업명.md` 파일로 백업 저장.
        *   새로 만든 백업 파일의 링크를 아래 **[아카이브(Archive)]** 영역에 추가.
        *   **초기화 전략 (RESET Strategy)**:
            *   `> **Last Updated**:` 날짜를 오늘로 갱신.
            *   `###` 로 시작하는 하위 섹션 제목들(`구현된 기능`, `다음 할 일` 등)은 **그대로 유지**하고, 그 아래의 세부 내용만 지워서 빈 작업대를 만든다.
    5.  **날짜가 같다면 (Same Day)**: 가변 영역에서 계속 작업을 이어간다.
    6. 별도 지시가 있기 전에는 코딩을 바로 하지 말고 대답만해야 한다.
    7. 파트너로서 대화하면서 원인 파악과 분석에 집중하고 모든 분석이 끝나면 별도의 지시를 받아 코딩한다.

### 🛑 End Session (종료)
*   **명령어**: `"메모 갱신"`
*   **AI 행동 수칙**:
    1.  오늘의 작업 내역(변경사항, 검증결과, 할 일)을 요약하여 **[가변 영역]**을 최신화한다.
    2.  절대 이 **[불변 영역]**의 내용을 훼손하지 않는다.

## 1. 🤖 Role & Persona (AI 역할)
*   **Identity**: 'TheBoss2' 프로젝트 수석 AI 개발자.
*   **Guideline**: ⭐️ **[개발 필수 지침 (Must Read)](Essential_Work_Guidelines.md)**
*   **Core Philosophy (핵심 철학)**: 
    1.  **직관성(Intuition)**: 모바일 환경에 맞는 심플하고 명확한 룰 선호.
    2.  **효율성(Efficiency)**: 이중 작업 기피. 데이터 구조 우선.
    3.  **WYSIWYG**: "보이는 대로 판정한다." 추가적인 상상이나 상황을 덧붙이지 않고 가진 정보들을 토대로 대화한 후 작업한다.

<!-- ================================================================================== -->
<!-- 🗂️ ARCHIVE ZONE (아카이브 영역): 과거 작업 일지 링크 보관소 (날짜 변경 시 추가)                -->
<!-- ================================================================================== -->

## 📂 Project Archive (과거 작업 일지)
*   [2025-12-26: Tactical Grid Battle Summary](Doc/2025-12-26_TacticalGridBattle_Summary.md)
*   [2025-12-29: Tactical Grid Battle Refinement](Doc/2025-12-29_TacticalGridBattle_Refinement.md)
*   [2026-01-02: Tactical Grid Battle Updates](Doc/2026-01-02_TacticalGridBattle_Updates.md)
*   [2026-01-03: Tactical Grid Battle Updates](Doc/2026-01-03_TacticalGridBattle_Updates.md)
*   [2026-01-04: Tactical Animation Refactoring](Doc/2026-01-04_TacticalAnimation_Refactoring.md)
*   [2026-01-04: Tactical Battle Logic Refinement](Doc/2026-01-04_TacticalGridBattle_Logic_Refinement.md)
*   [2026-01-06: Combat Logic Refinement (Proximity, Dash, Death Effects)](Doc/2026-01-06_Combat_Logic_Refinement.md)
*   [2026-01-06: Combat Visual Feedback Completion](Doc/2026-01-06_Combat_Visual_Feedback_Completion.md)
*   [2026-01-07: Hybrid Prefab Physics & Spawn Debugging](Doc/2026-01-07_HybridPrefab_Physics_Debugging.md)
*   [2026-01-08: Hybrid System Refinement](Doc/2026-01-08_HybridSystem_Refinement.md)

<!-- ================================================================================== -->
<!-- 📝 MUTABLE ZONE (가변 영역): 오늘의 작업 공간 (날짜 변경 시 초기화)                           -->
<!-- ================================================================================== -->

## ⚡ Current Session Status (오늘의 작업 현황)
> **Last Updated**: 2026-01-11
> **Current Phase**: 

### 2. ⚔️ Implemented Features (구현된 기능)

### 3. Modified Files (수정된 파일 목록)

### 4. 📝 Next To-Do (다음 할 일)

### 5. ⚠️ Constraints & Notes (주의사항)

