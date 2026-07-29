# Academy Platformer Sample

Unity 6000.0.77f1용 2D 플랫포머 수업 샘플입니다.

긴 Tilemap 스테이지, 캐릭터 애니메이션, 적, 수집 아이템, 패럴랙스 배경과 효과음이 포함되어 있습니다.

## 실행

1. Unity Hub에서 이 프로젝트 폴더를 엽니다.
2. `Assets/Scenes/Main.unity`를 열고 Play를 누릅니다.
3. A/D 또는 방향키로 이동하고 Space로 점프하며 R로 다시 시작합니다.

## 책임 분리

- `PlayerInputReader`: 플레이어 입력 수집
- `PlayerMotor2D`: 수평 물리 이동
- `PlayerJump`: 코요테 타임, 입력 버퍼, 가변 점프 및 점프 이벤트
- `PlayerStompAttack`: 낙하 중 적 윗면을 밟는 공격과 반동 점프
- `GroundSensor`: 지면 감지
- `Health` / `DamageDealer`: 체력과 피해 판정
- `PlayerDamageFeedback`: 피격 넉백과 무적 시간 깜박임
- `PlayerAudioFeedback`: 점프, 수집, 피격, 밟기 공격 사운드
- `PatrolEnemy`: 지정된 구간을 왕복하는 기본 순찰형 적
- `ChasingEnemy`: 평상시 시간 기반 순찰과 거리·높이·벽을 고려한 플레이어 추적
- `EnemyAnimationController`: 적의 실제 이동 속도에 따른 대기 및 이동 애니메이션 전환
- `StompableEnemy`: 밟기 공격을 받은 적의 처리
- `Collectible` / `Goal`: 월드 상호작용
- `GameManager`: 점수와 게임 상태
- `GameHUD`: 게임 상태 표시
- `CameraFollow`: 카메라 추적
- `Grid/Ground Tilemap`: 타일 기반 지형과 통합 충돌
- `ParallaxLayer`: 카메라 이동량에 따른 다중 레이어 배경

## 외부 무료 애셋

- Magical Road Pixel Art Environment by Luis Zuno (@ansimuz), CC0
- Deva(@Shades)의 8-Bit Sound Effect Pack Vol. 001, CC0
- 배경 라이선스: `Assets/ThirdParty/MagicalRoad/LICENSE.txt`
- 음원 라이선스: `Assets/AcademyPlatformer/Audio/LICENSE-OpenGameArt.txt`

## 맵 재생성

상단 메뉴에서 `Tools > Academy Platformer > Rebuild Sample`을 실행하면 프리팹과 씬을 다시 생성합니다.
