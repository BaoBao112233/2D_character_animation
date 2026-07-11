## Unity Input Actions — Ember
# Tạo file Input Actions trong Unity:
# Assets > Create > Input Actions > đặt tên "EmberInputActions"

## Action Map: "Ember"

| Action      | Type           | Binding (Keyboard) | Binding (Gamepad)    |
|-------------|----------------|--------------------|----------------------|
| Move        | Value/Vector2  | WASD / Arrow Keys  | Left Stick           |
| Run         | Button (Hold)  | Left Shift         | Left Stick Press     |
| Jump        | Button         | Space              | Button South (A/X)   |
| Shoot       | Button         | J / Left Click     | Right Trigger (RT)   |
| DashAttack  | Button         | K / Right Click    | Button West (X/□)    |
| Evade       | Button         | L / Middle Click   | Button East (B/○)    |
| Parry       | Button         | I                  | Left Trigger (LT)    |

## Setup trong Unity:
1. Assets > Create > Input Actions > tên "EmberInputActions"
2. Add Action Map "Ember"
3. Thêm từng action theo bảng trên
4. Save Asset
5. Trên Ember GameObject: Add Component "Player Input"
6. Chọn Actions = EmberInputActions
7. Behavior = "Send Messages" (kết nối tự động với EmberPlayerController)
