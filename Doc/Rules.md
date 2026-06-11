# MergeRPG - 프로젝트 규칙 및 컨벤션

> 이 문서는 프로젝트 규모가 커지면서 점진적으로 적용한다.
> 계획 단계이므로 실제 구조와 다를 수 있으며, 필요에 따라 수정한다.

---

## 폴더 구조

```
Assets/
├── Editor/                  ← 에디터 전용 스크립트 (빌드 미포함)
├── Prefabs/
│   ├── Units/               ← 캐릭터, 적 유닛
│   ├── Items/               ← 머지 아이템
│   ├── UI/                  ← UI 프리팹
│   └── Effects/             ← 이펙트, 파티클
├── Resources/               ← Resources.Load로 런타임 로드
│   └── Table/               ← 엑셀 → JSON 변환 테이블
├── Scenes/                  ← Unity 씬 파일
├── Scripts/
│   ├── Table/               ← 테이블 임포트 시스템
│   ├── Managers/            ← 전역 매니저 (GameManager 등)
│   ├── Merge/               ← 머지 보드, 아이템 로직
│   ├── Battle/              ← 전투 시스템
│   ├── Roguelite/           ← 런 관리, 보상, 이벤트
│   ├── UI/                  ← UI 컨트롤러
│   └── Utils/               ← 공통 유틸리티
├── Settings/                ← Unity 렌더링, 인풋 설정
└── Art/                     ← 외부 구매 에셋 정리
    └── PixelFantasy/
```

---

## 네이밍 컨벤션

### 스크립트
| 종류 | 규칙 | 예시 |
|------|------|------|
| 클래스 | PascalCase | `GameManager`, `MergeItem` |
| 메서드 | PascalCase | `SpawnItem()`, `OnMerge()` |
| 변수 (private) | camelCase + `_` prefix | `_currentHp`, `_grid` |
| 변수 (public) | PascalCase | `MaxHp`, `Level` |
| 상수 | ALL_CAPS | `MAX_LEVEL`, `GRID_SIZE` |

### 파일 및 폴더
| 종류 | 규칙 | 예시 |
|------|------|------|
| 스크립트 | PascalCase | `GridManager.cs` |
| 프리팹 | PascalCase | `HeroUnit.prefab` |
| 씬 | PascalCase | `BattleScene.unity` |
| 테이블 JSON | PascalCase (시트명 그대로) | `ItemTable.txt` |

### 엑셀 테이블
| 행 | 내용 |
|----|------|
| Row 1 | 컬럼 타입 (`int`, `float`, `string`, `bool`, `notused`) |
| Row 2 | 컬럼 이름 (영문, camelCase) |
| Row 3~ | 데이터 |

---

## 씬 구성 계획

| 씬 | 용도 |
|----|------|
| Boot | 초기 로딩, 데이터 초기화 |
| Lobby | 메인 화면, 덱 관리 |
| Merge | 머지 보드 메인 게임플레이 |
| Battle | 전투 화면 |
| Result | 결과, 보상 |

---

## 기타 규칙

- 프리팹은 항상 `Prefabs/` 하위에 위치 (씬에 직접 저장 금지)
- `Resources/` 는 동적 로드가 필요한 것만 (테이블, 런타임 생성 프리팹)
- 외부 에셋은 `Art/` 하위에만 위치, 직접 수정 금지
- 스크립트 1파일 = 1클래스 원칙
