# Ember — Animator Controller Setup Guide
# File: Assets/Animator/Ember_AnimatorController.controller
# Tạo thủ công trong Unity theo hướng dẫn dưới đây

## Parameters cần tạo

| Name         | Type    | Default |
|--------------|---------|---------|
| Speed        | Float   | 0       |
| VSpeed       | Float   | 0       |
| IsGrounded   | Bool    | true    |
| IsDashing    | Bool    | false   |
| Shoot        | Trigger | —       |
| DashAttack   | Trigger | —       |
| Evade        | Trigger | —       |
| Parry        | Trigger | —       |
| Hurt         | Trigger | —       |
| Death        | Trigger | —       |

## States (Base Layer)

### Locomotion Sub-State Machine
```
Entry ──► Idle
Idle ──[Speed > 0.1]──► Walk
Walk ──[Speed > 4.0]──► Run
Walk ──[Speed < 0.1]──► Idle
Run  ──[Speed < 4.0]──► Walk
```

### Jump / Air Sub-State Machine
```
Idle/Walk/Run ──[!IsGrounded]──► JumpRise
JumpRise ──[VSpeed < 0]──────► JumpFall
JumpFall ──[IsGrounded]──────► Land (0.1s, no exit time)
Land ──[0.15s]───────────────► Idle
```

### Combat (Any State transitions — high priority)
```
Any ──[DashAttack trigger]──► DashAttack  (Can transition from self: false)
Any ──[Shoot trigger]───────► Shoot       (Can transition from self: false)
Any ──[Evade trigger]───────► Evade       (Can transition from self: false)
Any ──[Parry trigger]───────► Parry       (Can transition from self: false)
Any ──[Hurt trigger]────────► Hurt        (Can transition from self: false)
Any ──[Death trigger]───────► Death       (Can transition from self: false, no exit)
```

### State Durations (approximate at 24fps)

| State       | Frames | Loop | Exit Condition      |
|-------------|--------|------|---------------------|
| Idle        | 60     | Yes  | Speed > 0.1         |
| Walk        | 24     | Yes  | Speed threshold     |
| Run         | 18     | Yes  | Speed threshold     |
| JumpRise    | 12     | No   | VSpeed < 0          |
| JumpFall    | 10     | Yes  | IsGrounded          |
| Land        | 8      | No   | Has Exit Time=true  |
| Shoot       | 16     | No   | Has Exit Time=true  |
| DashAttack  | 28     | No   | Has Exit Time=true  |
| Evade       | 18     | No   | Has Exit Time=true  |
| Parry       | 20     | No   | Has Exit Time=true  |
| Hurt        | 14     | No   | Has Exit Time=true  |
| Death       | 36     | No   | NEVER exits         |

## Keyframe Guide cho từng Animation

### Idle (60 frames, loop)
| Bone       | f0  | f15 | f30 | f45 | f60 |
|------------|-----|-----|-----|-----|-----|
| torso      | 0°  | +2° | 0°  | -2° | 0°  |  ← breathing sway
| head       | 0°  | +1° | 0°  | -1° | 0°  |
| coat_tail  | 0°  | +3° | 0°  | -3° | 0°  |  ← subtle flutter
| hair       | 0°  | +4° | 0°  | -4° | 0°  |
| mana_saber | 0°  | +5° | 0°  | -5° | 0°  |  ← saber glow pulse (via emission)

### Walk (24 frames, loop)
| Bone          | f0   | f6   | f12  | f18  | f24 |
|---------------|------|------|------|------|-----|
| upper_arm_R   | +30° | 0°   | -30° | 0°   | +30° |
| upper_arm_L   | -30° | 0°   | +30° | 0°   | -30° |
| upper_leg_R   | +25° | 0°   | -25° | 0°   | +25° |
| upper_leg_L   | -25° | 0°   | +25° | 0°   | -25° |
| torso         | +3°  | 0°   | -3°  | 0°   | +3°  |
| coat_tail     | -5°  | 0°   | +5°  | 0°   | -5°  |

### Run (18 frames, loop)  
| Bone          | f0   | f9   | f18 |
|---------------|------|------|-----|
| upper_arm_R   | +55° | -55° | +55° |
| upper_arm_L   | -55° | +55° | -55° |
| upper_leg_R   | +45° | -45° | +45° |
| upper_leg_L   | -45° | +45° | -45° |
| torso         | +8°  | -8°  | +8°  |  ← lean forward
| head          | -5°  | -3°  | -5°  |  ← chin down
| coat_tail     | +15° | +15° | +15° |  ← flowing back

### DashAttack (28 frames, no loop) ← COMBAT: Dash Attack từ model sheet
| Bone          | f0   | f5         | f12        | f20   | f28  |
|---------------|------|------------|------------|-------|------|
| hips          | 0    | +Y leap    | peak       | land  | land |
| torso         | 0°   | -20° lean  | -15°       | +10°  | 0°   |
| upper_arm_R   | 0°   | +80° fwd   | +120° ext  | +30°  | 0°   |
| lower_arm_R   | 0°   | +40°       | +90°       | +20°  | 0°   |
| upper_arm_L   | 0°   | +60° fwd   | +90° gun   | +20°  | 0°   |
| upper_leg_R   | 0°   | +50° kick  | +70°       | -60°  | 0°   |
| upper_leg_L   | 0°   | -30°       | -50°       | +30°  | 0°   |
| coat_tail     | 0°   | +20°       | +30°       | +10°  | 0°   |
| hair          | 0°   | +10°       | +15°       | +5°   | 0°   |

### Evade & Parry (18–20 frames) ← COMBAT: Evade & Parry từ model sheet
#### Evade (dodge roll):
- f0–f4: anticipation (torso dips -15°)
- f5–f12: leap/roll (hips translate -1.5 units X, full body rotate)
- f13–f18: recovery (return to stance)

#### Parry (block riposte):
- f0–f5: guard up (both arms cross +60°, torso leans back +10°)
- f6–f10: HOLD — wait for strike (loop-able via state or scripted timing)
- f11–f16: riposte (arm_L thrusts Mana-Saber forward +100°, parry spark VFX)
- f17–f20: recovery

## Layer Setup

### Base Layer (weight 1.0)
Full body locomotion + combat as above.

### Face Layer (weight 1.0, Additive)
- Controls head/hair/face bones only
- 4 states: `Face_Neutral`, `Face_Smiling`, `Face_Determined`, `Face_Surprised`
- Driven by a separate `FaceState` integer parameter (0–3)
- Allows face expression to play over any locomotion state

## Transition Settings (important)
- All `Any State → Combat` transitions: **Has Exit Time = OFF**, **Fixed Duration = 0.05s**
- Locomotion transitions: **Has Exit Time = OFF**, **Fixed Duration = 0.1s**  
- DashAttack → exit: **Has Exit Time = ON** (play full clip)
- Death → (no exit)
