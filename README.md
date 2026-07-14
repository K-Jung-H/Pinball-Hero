# 통통 디펜스: 핀볼 히어로

PurpleCow 클라이언트 프로그래머 채용 과제인 `통통 디펜스: 핀볼 마스터`의 핵심 플레이를 기반으로 제작한 Unity 2D 핀볼 디펜스 프로젝트입니다.

## Repository

[K-Jung-H/Pinball-Hero](https://github.com/K-Jung-H/Pinball-Hero)

## 개발 환경

| 항목 | 내용 |
| --- | --- |
| Unity | 6000.3.10f1 |
| Target Platform | Android |
| Render Pipeline | Universal Render Pipeline 2D |
| Input | Unity Input System |
| UI | UGUI, TextMeshPro |

## 실행 및 조작 방법

### Scene Lobby

- 게임은 `Scene Lobby`에서 시작합니다.
- Lobby 화면에서 Ball 타입별 강화 레벨을 결정할 수 있습니다.
- 강화 효과는 임의로 레벨 1당 충돌 데미지 `+1`로 설정했습니다.
- `Start` 버튼을 누르면 `Scene Stage`로 진입합니다.

### Scene Stage

- Wave 1부터 시작하며 총 Wave 12까지 진행됩니다.
- 화면을 터치한 방향으로 Ball이 발사됩니다.
- 몬스터 처치 시 경험치를 획득하고, 일정 경험치를 획득하면 3택지 스킬 선택 화면이 활성화됩니다.
- 플레이어의 체력이 모두 소진되면 실패, Wave 12까지 생존하면 성공으로 게임이 종료됩니다.

### 디버그 및 보조 입력

#### 모바일 환경

- 모바일 뒤로가기 버튼 또는 화면 상단의 Pause UI를 누르면 게임이 일시 정지됩니다.
- Exp Bar를 누르면 3택지 스킬 선택 화면이 활성화됩니다.

#### PC Editor 환경

- Exp Bar를 누르거나 `Space`를 입력하면 3택지 스킬 선택 화면이 활성화됩니다.
- 화면 상단의 Pause UI를 누르거나 `Esc`를 입력하면 게임이 일시 정지됩니다.

## 주요 구현 내용

### 1. 핀볼 발사와 반사

- `Rigidbody2D`, `Collider2D`, `PhysicsMaterial2D` 기반 반사 구현
- 조준 방향을 기준으로 대기 Ball을 일정 간격마다 자동 발사
- Ball별 속도를 일정하게 유지해 연속 충돌 중 감속 방지
- Endline 재진입 후 복귀 상태로 전환
- 복귀 상태에서 Wall 충돌 시 Shooter 방향으로 이동 방향 보정
- Shooter 영역 도착 시 Ball을 비활성화하고 발사 Queue에 다시 등록
- 일반 Ball과 관통 Ball의 물리 충돌 Layer 분리

### 2. 전투 처리

- Ball은 충돌 대상과 충돌 지점만 감지하고 `EnemyHit` 이벤트를 전달
- `CombatPipeline`이 데미지 처리 순서를 통제
- 직접 타격, 지속 피해, 범위 피해를 공통 `DamageRequest`로 처리
- 패시브 보정, 치명타, 상태이상 추가 피해 배율을 적용한 뒤 최종 데미지 확정
- 확정된 `DamageResult`를 Enemy와 데미지 텍스트 시스템에 전달

### 3. Ball과 스킬 효과

| Ball | 구현 효과 |
| --- | --- |
| Normal | 기본 물리 반사 및 직접 타격 |
| Fire | 화상 중첩, 지속시간, 초당 중첩 피해 |
| Ice | 확률형 빙결, 이동속도 감소, 받는 피해 증가 |
| Laser | 충돌한 Enemy의 행을 기준으로 범위 Collider를 활성화해 1회 피해 |
| Ghost | Enemy를 관통하면서 Trigger 기반으로 1회 타격 |
| Cluster | 확률적으로 1회성 Cluster Fragment 생성 |
| Random | Wall/Enemy 충돌마다 데미지, 속도, 반사각을 설정 범위에서 변경 |

Ball 효과는 `Ball_Base`의 타입 분기 대신 `BallEffectDefinitionSO`, `BallEffectRuntimeStat`, `HitEffectSystem`의 Handler 구조로 처리합니다. 특수한 물리 동작이 필요한 Random Ball만 `Ball_Base`를 상속합니다.

### 4. 패시브 스킬

| 패시브 | 구현 효과 |
| --- | --- |
| 따뜻한 양철 심장 | Normal Ball 최종 피해 증가 |
| 마법 거울 | 다음 Enemy 타격 시 누적 Wall 충돌 횟수에 비례한 피해 증가 |
| 자수정 단검 | Enemy 전면 타격 시 치명타 확률 증가 |
| 에메랄드 단검 | Enemy 후면 타격 시 치명타 확률 증가 |
| 마지막 성냥 | Enemy 사망 시 주변 범위 폭발 피해 |

일반 데미지/치명타 패시브는 `PassiveModifierSystem`, 사망 발동 효과는 `DeathEffectSystem`에서 처리합니다. Enemy는 어떤 공격으로 사망했는지와 무관하게 사망 이벤트만 전달합니다.

### 5. 웨이브와 Enemy

- 총 12개 Wave 구성
- `10 x 8` Wave 데이터와 `1x1`, `2x1`, `1x2`, `2x2` Enemy 배치 지원
- Stage 시작 시 초기 N개 Line을 한 번에 생성
- Spawn 영역이 확보되면 남은 Line을 위에서 한 줄씩 생성
- 이전 Wave의 활성 Enemy가 모두 제거된 뒤 다음 Wave 시작
- Enemy 크기에 따라 중심 좌표와 Spawn 위치를 자동 보정
- Enemy별 연속 이동, 개별 빙결 감속, Endline 도달 처리
- Endline 도달 시 Player를 향한 공격 연출 후 Player HP 감소
- 사망 애니메이션 종료 후 Enemy Pool로 반환

### 6. 로그라이크 3택지

- 전체 `SkillCatalogSO`에서 중복 없이 최대 3개 후보 추출
- 미보유 스킬은 Lv.1 카드로만 등장
- 보유 스킬은 다음 레벨 업그레이드 카드로 등장
- 최대 레벨 스킬은 후보에서 제외
- 최대 보유 수: Active Ball 4개, Passive 2개
- 보유 수가 최대일 때 해당 분류의 신규 스킬을 제외하고 기존 스킬 업그레이드만 제시
- 선택 화면이 활성화된 동안 `Time.timeScale = 0`으로 게임 정지
- 카드에 아이콘, 분류, 이름, 현재/선택/미획득 레벨 색상, 레벨별 수치 변화 표시

### 7. 경험치와 Stage 결과

- Enemy의 Cell 면적을 경험치로 변환
  - `1x1 = 1`, `1x2 = 2`, `2x2 = 4`
- Stage Level 구간별 요구 경험치를 `StageDefinitionSO`에서 관리
- 경험치 Bar를 보간해 표시하고 레벨업 횟수만큼 선택지 예약
- 12개 Wave 완료 및 Player 생존 시 `Success`
- Player HP가 0이 되면 즉시 `Fail`
- 결과 발생 시 게임 정지 및 결과 팝업 표시
- Retry 시 Player HP, Enemy, Ball Queue, 스킬, 경험치, VFX Pool을 초기화
- 로비에서 선택한 영구 성장 레벨은 Retry 후에도 유지

## 주요 클래스와 관계

### Stage

| 클래스 | 역할 및 관계 |
| --- | --- |
| `StageBuilder` | Stage 진입 시 Player, Shooter, Combat, Skill, Wave, UI 시스템의 참조와 이벤트를 연결하는 조립 지점 |
| `StageRunController` | 최초 실행과 Retry에서 Stage Runtime 상태를 정해진 순서로 초기화하고 실행 |
| `StageDefinitionSO` | Wave 목록, 초기 생성 Line 수, Stage Level/경험치 구간을 보관하는 정적 데이터 |
| `WaveDefinitionSO` | Board 크기와 Enemy Definition별 Cell 좌표를 보관하는 Wave 정적 데이터 |
| `WaveSpawner` | Wave 순서, Line Spawn, 활성 Enemy 추적, 경험치/Player 데미지/Stage 완료 이벤트 관리 |
| `StageExperienceSystem` | 경험치, Stage Level, 선택지 예약 횟수를 관리하고 `SkillSelectionController` 호출 |
| `StageResultController` | Player 사망과 Stage 완료 이벤트를 받아 Pause/Success/Fail 상태 및 Time Scale 관리 |

### Ball과 Player

| 클래스 | 역할 및 관계 |
| --- | --- |
| `PlayerController` | Touch/Mouse 입력을 월드 방향으로 변환하고 PlayerRenderer와 BallShooter에 Aim 전달, Player HP 관리 |
| `PlayerRenderer` | Aim 방향에 따라 Head 회전/Offset과 Head/Body Flip 적용 |
| `BallShooter` | 보유 Ball, 발사 대기 Queue, 자동 발사 간격, Ball 회수, Cluster Fragment Pool 관리 |
| `BallFactory` | `BallCatalogSO`에서 타입별 Definition/Prefab을 찾고 생성 또는 Runtime Stat 갱신 |
| `Ball_Base` | Ball 상태, Rigidbody 이동, 충돌 감지, 복귀와 회수 요청, EnemyHit 이벤트 관리 |
| `BallRuntimeStat` | 발사 시점에 확정된 데미지, 속도, 치명타, 타입별 효과 수치를 보관하는 런타임 데이터 |
| `BallCollisionAudio` | Ball별 2D 충돌음을 재생하며 같은 Ball의 이전 소리는 중단 후 재시작 |

### Combat과 Enemy

| 클래스 | 역할 및 관계 |
| --- | --- |
| `CombatPipeline` | Ball 타격 이벤트를 받아 데미지 요청 생성, 최종 데미지 확정, Enemy 적용 및 결과 이벤트 발행 |
| `PassiveModifierSystem` | 현재 Run의 Passive Skill을 순회해 피해 배율과 치명타 확률 보정 |
| `HitEffectSystem` | BallRuntimeStat의 Effect 목록에 따라 Burn, Freeze, Laser, Cluster Handler 실행 |
| `DeathEffectSystem` | Enemy 사망 이벤트를 받아 마지막 성냥과 같은 사망 기반 Passive 실행 |
| `AreaEffectSystem` | Laser/Explosion용 DamageArea를 Pool에서 대여하고 크기/위치/데미지를 주입 |
| `Enemy_Base` | HP, 이동, 피격, Endline 공격, 사망 상태와 상위 시스템용 이벤트 관리 |
| `StatusController` | Burn Tick/중첩과 Freeze 지속시간/감속/추가 피해 배율 관리 |
| `EnemyAnimator` | Hit/Death 상태 재생, Burn/Frozen 시각 상태, Endline 공격 이동과 Fade 처리 |
| `EnemyPool` | EnemyDefinition별 Pool을 구성하고 Spawn/Despawn 시 Enemy 상태 재초기화 |

### Skill과 Presentation

| 클래스 | 역할 및 관계 |
| --- | --- |
| `SkillCardResolver` | 보유 상태, 최대 보유 수, 최대 레벨을 검사하고 중복 없는 무작위 선택지 생성 |
| `SkillSelectionController` | 선택 UI 표시, 게임 정지, 선택 결과를 RunSkillInventory와 BallShooter에 반영 |
| `RunSkillInventory` | 현재 Stage에서 획득한 Active/Passive 스킬과 Card Level 관리 |
| `SkillDefinitionSO` | 스킬 이름, 아이콘, 설명 형식, 최대 레벨을 보관하는 공통 정적 데이터 |
| `SkillCardPanel` | 공용 카드 Prefab에 스킬 정보와 레벨 상태를 표시하고 선택 Callback 전달 |
| `DamageTextSystem` | DamageResult를 Queue에 저장하고 프레임당 생성 수를 제한해 Sprite Damage Text Pool 실행 |
| `SpriteHpBar` | SpriteRenderer의 Sliced 크기와 Gradient를 이용해 Player/Enemy 공용 HP Bar 표시 |

## 데이터 적용 순서

Ball의 정적 원본과 런타임 가변값을 분리했습니다.

- `BallDefinitionSO`: 타입, Prefab, 기본 데미지/속도/치명타, 효과 원본
- `BallGrowthDefinitionSO`: Stage 진입 전에 선택한 타입별 영구 성장 수치
- `BallCardDefinitionSO`: Stage 내부에서 변경되는 Active Ball Card 레벨 수치
- `RunSkillInventory`: Stage 내부 Active/Passive 보유 상태
- `BallRuntimeStat`: 발사 직전에 최신 성장/카드 값을 덮어쓴 실행 데이터

ScriptableObject는 정적 원본 데이터로만 사용하며, 실행 중 직접 수정하지 않습니다.

## Pooling 및 성능 고려

- EnemyDefinition별 Enemy Pool 및 사전 생성
- Damage Area Pool과 Damage Text Pool 사용
- Cluster Fragment 전용 Pool 사용
- 회수된 Ball은 비활성화 후 Queue에서 반복 사용
- 충돌 Layer를 캐싱해 문자열 검색 반복 방지
- 충돌 시 `GetContact(0)` 사용, LINQ 미사용
- Damage Text 요청을 Queue에 저장하고 프레임당 표시 수 제한
- Pool 반환 시 Rigidbody, Collider, Status, Animator, HP Bar, Audio 상태 초기화

## 가산점 구현

- 로비에서 Ball 타입별 영구 성장 레벨 선택
- Scene 재진입 방식이 아닌 데이터 기반 Stage Retry
- Cell 크기가 다른 Enemy와 데이터 기반 Wave 배치
- 화상/빙결 상태이상 및 상태별 시각 표현
- Sprite 기반 Damage Text와 Player/Enemy HP Bar
- Laser/사망 폭발 등 반복 생성되는 연출 효과에 Damage Area Pooling 적용
- Random Ball: 충돌마다 데미지, 속도, 반사각 무작위 변경
- Player Aim에 따른 Head 회전, 좌우 Flip, 위치 Offset
- Pause/Continue와 현재 Wave/Stage Level UI
- Ball별 2D 충돌 사운드 재생 구조

## AI 활용 여부

주요 게임 파이프라인과 클래스 구조 및 관계에 대한 설계와 디자인은 개발자가 직접 진행했습니다.

AI가 구조와 코드의 초안을 작성하면 개발자가 이를 검토하고 세부 구현을 완성하는 방식으로 활용했습니다. 반복 작업인 Data SO 생성은 AI에 맡겼으며, README 초안 작성에도 AI를 활용했습니다.
