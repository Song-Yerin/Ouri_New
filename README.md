# 🐿️ Nutpia

**Nutpia**는 다람쥐를 주인공으로 하는 3인칭 어드벤처 게임입니다. 역동적인 움직임, 벽 매달리기, 글라이딩 등 다채로운 기믹을 통해 플레이어에게 즐거운 이동 경험을 제공합니다.

## ✨ 주요 기능

### 1. **다양한 이동 시스템**
- **보행 및 달리기**: 카메라 기준 자유로운 방향 이동
- **점프 및 글라이딩**: 공중에서 활공하여 낙하 속도 제어
- **벽 매달리기 (Wall Hang)**: F 키로 가파른 벽에 매달려 안정적인 포지셀 유지
- **경사면 슬라이딩**: 일정 각도 이상의 경사면에서 자동으로 미끄러짐 (마찰 0 효과)

### 2. **고급 물리 및 애니메이션**
- **IK (Inverse Kinematics)**: 캐릭터가 시야 방향을 자연스럽게 따라감
- **루트 본 회전**: 상황별 캐릭터 자세 자동 조절 (등반, 슬라이딩 중)
- **부드러운 방향 전환**: Slerp를 통한 자연스러운 회전
- **상태별 애니메이션 동기화**: 이동, 점프, 글라이딩, 등반 상태에 맞는 애니메이션 자동 전환

### 3. **지능형 지형 감지**
- **Raycast 기반 경사각 계산**: 실시간으로 지면 기울기 감지
- **CharacterController 연동**: Unity의 표준 물리 시스템 활용
- **자동 슬라이딩**: `climbAngle` 이상의 경사면에서 입력과 무관하게 하강

## 🎮 조작법

| 입력 | 기능 |
|------|------|
| **WASD** | 이동 |
| **마우스** | 카메라 회전 |
| **스페이스 바** | 점프 |
| **Shift** | 달리기 |
| **E** | 글라이드 토글 |
| **F** | 벽 매달리기 |

## 🏗️ 코드 구조

### `CreatureMover.cs`
플레이어 움직임의 핵심 컨트롤러. 입력을 처리하고 상태 관리를 담당합니다.

**주요 기능:**
- 입력 수집 (`SetInput()`)
- 점프 요청 관리 (`RequestJump()`)
- 글라이드 토글 (`RequestGlideToggle()`)
- 벽 등반 상태 전환 (`SetClimbMode()`)
- 임시 중력 변경 (`ApplyTemporaryGravityMultiplier()`)

### `MovementHandler.cs`
실제 물리 기반 이동 로직 처리. `CharacterController`를 통해 이동을 수행합니다.

**주요 메서드:**
```csharp
// 경사면 슬라이딩 감지
private bool CheckSteepSlope(out Vector3 slideDir)

// 매 프레임 이동 업데이트
public void Move(float deltaTime, Vector2 axis, bool isRun, bool isJump, 
                 bool isMoving, bool isGlide, bool isClimbing, 
                 bool isSlidingMode, float gravityMultiplier, 
                 out Vector2 animAxis, out Vector3 moveDirection)
```

### `AnimationHandler.cs`
Animator 파라미터 관리 및 애니메이션 상태 동기화.

**주요 파라미터:**
- `Vert`: 수직 이동 속도
- `State`: 달리기 상태
- `IsSliding`: 슬라이드 모드 활성화
- `Jump`: 점프 트리거
- `IsGrounded`: 지면 접촉 여부
- `IsGliding`: 글라이드 활성화
- `IsClimbing`: 등반 상태

### `WallHang.cs`
가파른 벽면에 매달리는 기능 구현.

**동작:**
1. F 키로 벽 근처에서 매달리기 시작
2. 매달린 상태에서는 점프만 가능
3. 점프 후 땅에 닿기 전까지 재매달림 불가
4. 애니메이션 자동 재생 및 해제

## ⚙️ 설정 값

### `CreatureMover` Inspector 설정

| 설정 항목 | 기본값 | 설명 |
|-----------|--------|------|
| Walk Speed | 1 m/s | 보행 속도 |
| Run Speed | 4 m/s | 달리기 속도 |
| Jump Height | 5 m | 점프 높이 |
| Glide Gravity | -1 m/s² | 글라이드 중 중력 |
| Rotation Smoothing | 15 | 회전 부드러움 정도 |

### `MovementHandler` 설정

| 설정 항목 | 기본값 | 설명 |
|-----------|--------|------|
| Climb Angle | 45° | 자동 슬라이드 경사각 임계값 |
| Slope Check Distance | 1m | 경사면 감지 거리 |

## 🎨 애니메이션 구조

```
State Machine
├── Idle/Walk/Run (기본 이동)
├── Jump (점프 진입)
├── Glide (글라이딩)
├── WallHang (벽 매달림)
└── Slide (슬라이드 모드)
```

각 상태는 Animator의 파라미터에 의해 자동으로 전환됩니다.

## 🔧 설치 및 사용

### 요구사항
- Unity 2022.3 이상
- Character Controller 컴포넌트
- Animator 컴포넌트

### 설정 방법

1. **플레이어 오브젝트 구성:**
   ```
   Player (GameObject)
   ├── Character Controller (Component)
   ├── Animator (Component)
   ├── CreatureMover (Script)
   ├── WallHang (Script)
   └── Model (3D Model with SkinnedMeshRenderer)
       └── RootBone (Transform reference)
   ```

2. **레이어 설정:**
   - 벽/경사면에 "Climb" 태그 추가
   - 지면을 적절한 Layer로 구분

3. **Animator 설정:**
   - 애니메이션 파라미터 (`Vert`, `State`, `IsSliding` 등) 생성
   - 상태 머신 구성

4. **Inspector에서 연결:**
   - `PlayerCam` 참조
   - `RootBone` 할당
   - 각 속도 및 중력 값 조정

## 🎯 게임 플레이 팁

- **글라이드 활용**: 높은 곳에서 점프 후 글라이드로 거리 확보
- **벽 매달리기**: F 키로 가파른 벽에 매달린 후 점프로 방향 전환
- **경사면 슬라이딩**: 가파른 경사에서는 입력에 관계없이 아래로 미끄러짐 활용
- **카메라 시점**: 마우스로 자유로운 각도에서 관찰 가능

## 🐛 알려진 이슈 및 개선 사항

- [ ] 벽 근처에서의 더 정밀한 충돌 감지
- [ ] 슬라이딩 중 입력 반응성 개선
- [ ] 글라이드 중 공중 이동 속도 세밀 조절
- [ ] 다중 터치 입력 지원 (모바일 플랫폼)

## 📝 라이선스

Nutpia는 개인 프로젝트입니다. 상업적 사용에 대해서는 별도 문의 바랍니다.

## 👨‍💻 개발자

**Nutpia Development Team**

---

**즐거운 게임 플레이를 원합니다! 🎮**
