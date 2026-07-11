# Ember — 2D Character Animation Pipeline (Unity)

## Input
- Character model sheet: `Ember` (steampunk style, cartoon 2D)
- Engine: Unity 2D
- Tool: Unity 2D Animation Package (free, built-in)

## Folder Structure
```
Assets/
├── Characters/
│   └── Ember/
│       ├── Parts/              ← 18 PNG parts đã tách
│       ├── Ember_Atlas.png     ← Sprite Atlas gộp lại
│       ├── Ember_Atlas.tpsheet ← TexturePacker project
│       ├── Ember.prefab        ← Prefab có rig sẵn
│       └── Animations/
│           ├── Ember_Idle.anim
│           ├── Ember_Walk.anim
│           ├── Ember_Run.anim
│           ├── Ember_Jump.anim
│           ├── Ember_Fall.anim
│           ├── Ember_Land.anim
│           ├── Ember_DashAttack.anim
│           ├── Ember_EvadeParry.anim
│           ├── Ember_Shoot.anim
│           ├── Ember_Hurt.anim
│           └── Ember_Death.anim
├── Scripts/
│   ├── EmberAnimationController.cs
│   └── EmberPlayerController.cs
└── Animator/
    └── Ember_AnimatorController.controller
```

## Bước 1: Tách Part ảnh Ember

### Parts cần tách từ ảnh "Neutral Pose" (front view)
| Part Name | Vùng cắt | Ghi chú |
|---|---|---|
| `hair` | Mái tóc đỏ phồng | Cắt sát da đầu |
| `head` | Khuôn mặt + tai | Không tóc |
| `neck` | Cổ | |
| `torso` | Áo choàng thân trên | Từ vai đến hông |
| `hips` | Dây lưng + hông | Phần thắt lưng tiện ích |
| `upper_arm_R` | Cánh tay trên phải | |
| `lower_arm_R` | Cẳng tay phải | |
| `hand_R` | Bàn tay phải | Chứa Steam-Blaster |
| `upper_arm_L` | Cánh tay trên trái | |
| `lower_arm_L` | Cẳng tay trái | |
| `hand_L` | Bàn tay trái | Chứa Mana-Saber |
| `upper_leg_R` | Đùi phải | |
| `lower_leg_R` | Bắp chân phải | |
| `foot_R` | Boot phải | |
| `upper_leg_L` | Đùi trái | |
| `lower_leg_L` | Bắp chân trái | |
| `foot_L` | Boot trái | |
| `steam_blaster` | Súng Steampunk | Tách riêng |
| `mana_saber` | Kiếm ánh sáng xanh | Tách riêng, thêm glow |
| `coat_tail` | Đuôi áo choàng | Part riêng để animate flutter |

### Cách tách nhanh nhất (không có Photoshop):
1. Upload ảnh lên **remove.bg** → tách nền
2. Dùng **GIMP** (miễn phí): File > Open > dùng Free Select Tool cắt từng phần
3. Export từng part PNG với nền trong suốt (Alpha channel)
4. Đặt pivot point ở vị trí khớp xương (khuỷu tay, đầu gối, vai...)

## Bước 2: Cài Unity 2D Animation Package

```
Window > Package Manager > Unity Registry
> Search "2D Animation" > Install
> Search "2D IK" > Install  
> Search "2D Sprite" > Install
```

Version khuyên dùng: Unity 2022.3 LTS trở lên

## Bước 3: Import và Setup Sprite

1. Kéo tất cả PNG parts vào `Assets/Characters/Ember/Parts/`
2. Chọn tất cả > Inspector:
   - Texture Type: `Sprite (2D and UI)`
   - Pixels Per Unit: `100`
   - Filter Mode: `Bilinear`
   - Compression: `None` (để giữ chất lượng)
3. Tạo Sprite Atlas: `Assets > Create > 2D > Sprite Atlas`
   - Kéo thư mục Parts vào Objects for Packing

## Bước 4: Rigging trong Sprite Editor

1. Chọn sprite của body chính > `Sprite Editor > Skinning Editor`
2. Tạo bone hierarchy:

```
Root
└── hips
    ├── torso
    │   ├── neck
    │   │   └── head
    │   │       └── hair
    │   ├── upper_arm_R
    │   │   └── lower_arm_R
    │   │       └── hand_R
    │   │           └── steam_blaster
    │   ├── upper_arm_L
    │   │   └── lower_arm_L
    │   │       └── hand_L
    │   │           └── mana_saber
    │   └── coat_tail
    ├── upper_leg_R
    │   └── lower_leg_R
    │       └── foot_R
    └── upper_leg_L
        └── lower_leg_L
            └── foot_L
```

3. Thiết lập **IK (Inverse Kinematics)**:
   - Limb Solver cho 2 tay và 2 chân
   - Target: bàn tay và bàn chân

## Bước 5: Tạo Animator Controller

Tạo file `Ember_AnimatorController.controller` với các layer và transitions theo code trong `EmberAnimationController.cs`
