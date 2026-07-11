using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Procedural animation system for Ember placeholder rig.
/// Drives all bones programmatically — no Animator Controller needed for prototype.
/// Works with EmberVisualPlaceholder bones.
/// </summary>
[RequireComponent(typeof(EmberVisualPlaceholder))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class EmberProtoController : MonoBehaviour
{
    // ──────────────────────────────────────────────
    //  Inspector
    // ──────────────────────────────────────────────
    [Header("Movement")]
    [SerializeField] float _walkSpeed    = 4.0f;
    [SerializeField] float _runSpeed     = 7.5f;
    [SerializeField] float _jumpForce    = 13f;
    [SerializeField] float _dashForce    = 20f;
    [SerializeField] float _dashDuration = 0.18f;

    [Header("Ground Check")]
    [SerializeField] LayerMask _groundLayer;
    [SerializeField] float _groundCheckDist = 0.08f;

    [Header("Camera")]
    [SerializeField] Transform _cam;

    // ──────────────────────────────────────────────
    //  State Machine
    // ──────────────────────────────────────────────
    enum State { Idle, Walk, Run, JumpRise, JumpFall, Land,
                 Shoot, DashAttack, Evade, Parry, Hurt, Dead }

    State _state = State.Idle;
    float _stateTimer;
    float _animTime;

    // ──────────────────────────────────────────────
    //  Runtime
    // ──────────────────────────────────────────────
    Rigidbody2D          _rb;
    CapsuleCollider2D    _col;
    EmberVisualPlaceholder _vis;

    Vector2 _moveInput;
    bool    _isGrounded;
    bool    _runHeld;
    bool    _jumpPressed;
    bool    _shootPressed;
    bool    _dashPressed;
    bool    _evadePressed;
    bool    _parryPressed;

    float _dashTimer;
    float _shootCd, _dashCd, _evadeCd, _parryCd;
    bool  _isDashing;

    // Bone refs (shorthand)
    Transform _hips, _torso, _head, _hair, _coatTail;
    Transform _uArmR, _lArmR, _uArmL, _lArmL;
    Transform _uLegR, _lLegR, _uLegL, _lLegL;

    // UI labels
    GUIStyle _labelStyle;
    string   _stateLabel = "Idle";

    // ══════════════════════════════════════════════
    //  UNITY LIFECYCLE
    // ══════════════════════════════════════════════
    void Awake()
    {
        _rb  = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();
        _vis = GetComponent<EmberVisualPlaceholder>();

        _rb.gravityScale  = 4f;
        _rb.freezeRotation = true;
        _col.size          = new Vector2(0.40f, 1.10f);
        _col.offset        = new Vector2(0f, 0.55f);
    }

    void Start()
    {
        // Cache bone references (set after EmberVisualPlaceholder.Build())
        _hips     = _vis.BoneHips;
        _torso    = _vis.BoneTorso;
        _head     = _vis.BoneHead;
        _hair     = _vis.BoneHair;
        _coatTail = _vis.BoneCoatTail;
        _uArmR    = _vis.BoneUpperArmR;
        _lArmR    = _vis.BoneLowerArmR;
        _uArmL    = _vis.BoneUpperArmL;
        _lArmL    = _vis.BoneLowerArmL;
        _uLegR    = _vis.BoneUpperLegR;
        _lLegR    = _vis.BoneLowerLegR;
        _uLegL    = _vis.BoneUpperLegL;
        _lLegL    = _vis.BoneLowerLegL;

        if (_cam == null && Camera.main != null)
            _cam = Camera.main.transform;
    }

    void Update()
    {
        ReadInput();
        TickCooldowns();
        CheckGround();
        UpdateStateMachine();
        AnimateBones();
        FlipCharacter();
        CameraFollow();
        _stateTimer += Time.deltaTime;
        _animTime   += Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (_isDashing) return;
        float speed = _runHeld ? _runSpeed : _walkSpeed;
        _rb.linearVelocity = new Vector2(_moveInput.x * speed, _rb.linearVelocity.y);
    }

    // ══════════════════════════════════════════════
    //  INPUT (Keyboard direct — no Input System asset needed for prototype)
    // ══════════════════════════════════════════════
    void ReadInput()
    {
        _moveInput.x   = Input.GetAxisRaw("Horizontal");
        _runHeld       = Input.GetKey(KeyCode.LeftShift);
        _jumpPressed   = Input.GetKeyDown(KeyCode.Space);
        _shootPressed  = Input.GetKeyDown(KeyCode.J);
        _dashPressed   = Input.GetKeyDown(KeyCode.K);
        _evadePressed  = Input.GetKeyDown(KeyCode.L);
        _parryPressed  = Input.GetKeyDown(KeyCode.I);
    }

    // ══════════════════════════════════════════════
    //  STATE MACHINE
    // ══════════════════════════════════════════════
    void UpdateStateMachine()
    {
        if (_state == State.Dead) return;

        // ── Hurt (test: H key)
        if (Input.GetKeyDown(KeyCode.H))        { SetState(State.Hurt);      return; }

        // ── Death (test: X key)
        if (Input.GetKeyDown(KeyCode.X))        { SetState(State.Dead);      return; }

        // ── One-shot combat actions
        if (_shootPressed  && _shootCd  <= 0f)  { SetState(State.Shoot);       _shootCd  = 0.40f; return; }
        if (_dashPressed   && _dashCd   <= 0f)  { SetState(State.DashAttack);  _dashCd   = 1.00f; StartDash(); return; }
        if (_evadePressed  && _evadeCd  <= 0f)  { SetState(State.Evade);       _evadeCd  = 0.70f; DoEvade(); return; }
        if (_parryPressed  && _parryCd  <= 0f)  { SetState(State.Parry);       _parryCd  = 0.60f; return; }

        // ── Return to locomotion after one-shot states
        bool oneShot = _state == State.Shoot || _state == State.DashAttack ||
                       _state == State.Evade || _state == State.Parry  ||
                       _state == State.Hurt  || _state == State.Land;
        if (oneShot && _stateTimer > OneShotDuration(_state))
        {
            SetState(State.Idle);
            return;
        }
        if (oneShot) return;

        // ── Jump
        if (_jumpPressed && _isGrounded)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
            SetState(State.JumpRise);
            return;
        }

        // ── Air states
        if (!_isGrounded)
        {
            SetState(_rb.linearVelocity.y > 0f ? State.JumpRise : State.JumpFall);
            return;
        }

        // ── Land from air
        if (_isGrounded && (_state == State.JumpFall || _state == State.JumpRise))
        {
            SetState(State.Land);
            return;
        }

        // ── Locomotion
        float speed = Mathf.Abs(_rb.linearVelocity.x);
        if (speed > 0.1f)
            SetState(_runHeld && speed > 4f ? State.Run : State.Walk);
        else
            SetState(State.Idle);
    }

    void SetState(State s)
    {
        if (_state == s) return;
        _state      = s;
        _stateTimer = 0f;
        _animTime   = 0f;
        _stateLabel = s.ToString();
    }

    float OneShotDuration(State s) => s switch
    {
        State.Shoot      => 0.40f,
        State.DashAttack => 0.55f,
        State.Evade      => 0.35f,
        State.Parry      => 0.50f,
        State.Hurt       => 0.30f,
        State.Land       => 0.18f,
        _                => 0.30f
    };

    // ══════════════════════════════════════════════
    //  BONE ANIMATION (procedural)
    // ══════════════════════════════════════════════
    void AnimateBones()
    {
        if (_hips == null) return;
        float t  = _animTime;
        float dt = Time.deltaTime;

        switch (_state)
        {
            case State.Idle:       AnimIdle(t);       break;
            case State.Walk:       AnimWalk(t);       break;
            case State.Run:        AnimRun(t);        break;
            case State.JumpRise:   AnimJumpRise(t);   break;
            case State.JumpFall:   AnimJumpFall(t);   break;
            case State.Land:       AnimLand(t);       break;
            case State.Shoot:      AnimShoot(t);      break;
            case State.DashAttack: AnimDashAttack(t); break;
            case State.Evade:      AnimEvade(t);      break;
            case State.Parry:      AnimParry(t);      break;
            case State.Hurt:       AnimHurt(t);       break;
            case State.Dead:       AnimDead(t);       break;
        }
    }

    // ── IDLE: gentle breathing sway
    void AnimIdle(float t)
    {
        float breath = Mathf.Sin(t * Mathf.PI * 1.2f);
        SetRot(_torso,    breath * 2f);
        SetRot(_head,     breath * 1f);
        SetRot(_hair,     breath * 4f + Mathf.Sin(t * 1.8f) * 2f);
        SetRot(_coatTail, breath * 3f);
        SetRot(_uArmR,   -10f + breath * 2f);
        SetRot(_uArmL,    10f - breath * 2f);
        SetRot(_uLegR, 0); SetRot(_uLegL, 0);
    }

    // ── WALK cycle: 24 frames ~0.5s
    void AnimWalk(float t)
    {
        float cycle = t / 0.50f;                   // normalized 0–1 per cycle
        float s = Mathf.Sin(cycle * Mathf.PI * 2f);
        float c = Mathf.Cos(cycle * Mathf.PI * 2f);

        SetRot(_torso,  s * 3f);
        SetRot(_hair,   s * 5f);
        SetRot(_coatTail, -s * 6f);Mathf.Sin(cycle);

        SetRot(_uArmR,  s *  30f - 10f);
        SetRot(_uArmL, -s *  30f + 10f);
        SetRot(_lArmR,  Mathf.Max(0, s * 15f));
        SetRot(_lArmL,  Mathf.Max(0, -s * 15f));

        SetRot(_uLegR,  s *  25f);
        SetRot(_uLegL, -s *  25f);
        SetRot(_lLegR,  Mathf.Max(0, -s * 20f));
        SetRot(_lLegL,  Mathf.Max(0,  s * 20f));

        // Slight bob
        _hips.localPosition = new Vector3(0, Mathf.Abs(s) * 0.03f - 0.02f, 0);
    }

    // ── RUN cycle: faster and bigger
    void AnimRun(float t)
    {
        float cycle = t / 0.30f;
        float s = Mathf.Sin(cycle * Mathf.PI * 2f);
        float c = Mathf.Cos(cycle * Mathf.PI * 2f);

        SetRot(_torso,  -8f + s * 5f);         // lean forward
        SetRot(_head,   -5f + s * 2f);
        SetRot(_hair,    s * 12f);
        SetRot(_coatTail, 15f - s * 10f);      // streams behind

        SetRot(_uArmR,  s *  55f - 10f);
        SetRot(_uArmL, -s *  55f + 10f);
        SetRot(_lArmR,  Mathf.Abs(s) * 30f);
        SetRot(_lArmL,  Mathf.Abs(s) * 30f);

        SetRot(_uLegR,  s *  45f);
        SetRot(_uLegL, -s *  45f);
        SetRot(_lLegR,  Mathf.Max(0, -s * 30f));
        SetRot(_lLegL,  Mathf.Max(0,  s * 30f));

        _hips.localPosition = new Vector3(0, Mathf.Abs(s) * 0.05f - 0.03f, 0);
    }

    // ── JUMP RISE: arms up, legs tuck
    void AnimJumpRise(float t)
    {
        float blend = Mathf.Clamp01(t / 0.15f);
        SetRot(_torso,  -5f);
        SetRot(_uArmR,  Lerp( -10f, -70f, blend));
        SetRot(_uArmL,  Lerp(  10f,  70f, blend));
        SetRot(_uLegR,  Lerp(   0f,  30f, blend));
        SetRot(_uLegL,  Lerp(   0f, -30f, blend));
        SetRot(_lLegR,  Lerp(   0f, -40f, blend));
        SetRot(_lLegL,  Lerp(   0f,  40f, blend));
        SetRot(_hair,   Lerp(   0f,  10f, blend));
        SetRot(_coatTail, Lerp(0f, 20f, blend));
    }

    // ── JUMP FALL: spread limbs
    void AnimJumpFall(float t)
    {
        SetRot(_torso,   5f);
        SetRot(_uArmR, -50f);
        SetRot(_uArmL,  50f);
        SetRot(_uLegR,  20f);
        SetRot(_uLegL, -20f);
        SetRot(_lLegR, -10f);
        SetRot(_lLegL,  10f);
        SetRot(_hair,   Mathf.Sin(t * 6f) * 5f);
    }

    // ── LAND: squish
    void AnimLand(float t)
    {
        float squish = Mathf.Clamp01(1f - t / 0.18f);
        SetRot(_torso,  15f * squish);
        SetRot(_uLegR,  40f * squish);
        SetRot(_uLegL, -40f * squish);
        SetRot(_lLegR, -35f * squish);
        SetRot(_lLegL,  35f * squish);
        SetRot(_uArmR, -30f * squish);
        SetRot(_uArmL,  30f * squish);
        _hips.localPosition = new Vector3(0, -0.06f * squish, 0);
    }

    // ── SHOOT: upper body snap + recoil
    void AnimShoot(float t)
    {
        float fire   = Mathf.Clamp01(t / 0.06f);
        float recoil = Mathf.Clamp01((t - 0.06f) / 0.12f);
        float r      = fire - recoil;

        SetRot(_torso,  -5f + r * -10f);
        SetRot(_uArmR,  -15f + r * -40f);   // extends gun arm
        SetRot(_lArmR,  r * -20f);
        SetRot(_uArmL,   10f);
        SetRot(_uLegR, 0); SetRot(_uLegL, 0);
        SetRot(_hair,   r * 3f);
    }

    // ── DASH ATTACK: leap with slash
    void AnimDashAttack(float t)
    {
        float leap  = Mathf.Clamp01(t / 0.15f);
        float slash = Mathf.Clamp01((t - 0.15f) / 0.20f);

        SetRot(_torso,  Lerp(0f, -20f, leap));
        SetRot(_head,   Lerp(0f, -10f, leap));
        SetRot(_hair,   Lerp(0f,  15f, leap));
        SetRot(_coatTail, Lerp(0f, 30f, leap));

        // Saber arm sweeps forward
        SetRot(_uArmL,  Lerp(10f,  120f, leap) - slash * 80f);
        SetRot(_lArmL,  Lerp(0f,   60f,  leap));

        // Gun arm back for balance
        SetRot(_uArmR,  Lerp(-10f, -60f, leap));

        // Kick pose
        SetRot(_uLegR,  Lerp(0f,   70f, leap));
        SetRot(_lLegR,  Lerp(0f,  -40f, leap));
        SetRot(_uLegL,  Lerp(0f,  -40f, leap));
    }

    // ── EVADE: backward roll
    void AnimEvade(float t)
    {
        float roll = Mathf.Clamp01(t / 0.35f);
        float spin = Mathf.Sin(roll * Mathf.PI) * -180f;  // full spin

        SetRot(_torso,    spin);
        SetRot(_uArmR,  -60f);
        SetRot(_uArmL,   60f);
        SetRot(_uLegR,   50f);
        SetRot(_uLegL,  -50f);
        SetRot(_hair,    spin * 0.6f);
    }

    // ── PARRY: guard up → riposte
    void AnimParry(float t)
    {
        bool riposte = t > 0.25f;
        float blend  = riposte ? Mathf.Clamp01((t - 0.25f) / 0.15f) : Mathf.Clamp01(t / 0.10f);

        if (!riposte)
        {
            // Guard up — cross arms
            SetRot(_uArmL,  Lerp(10f,   90f, blend));
            SetRot(_lArmL,  Lerp(0f,    60f, blend));
            SetRot(_uArmR,  Lerp(-10f, -60f, blend));
            SetRot(_torso,  Lerp(0f,    10f, blend));
        }
        else
        {
            // Riposte thrust
            SetRot(_uArmL,  Lerp(90f,  140f, blend));
            SetRot(_lArmL,  Lerp(60f,   90f, blend));
            SetRot(_torso,  Lerp(10f,  -15f, blend));
        }
        SetRot(_uLegR, 0); SetRot(_uLegL, 0);
    }

    // ── HURT: stagger back
    void AnimHurt(float t)
    {
        float s = Mathf.Sin(t * Mathf.PI / 0.30f);
        SetRot(_torso,   20f * s);
        SetRot(_head,    15f * s);
        SetRot(_uArmR,  -40f);
        SetRot(_uArmL,   40f);
    }

    // ── DEAD: collapse
    void AnimDead(float t)
    {
        float fall = Mathf.Clamp01(t / 0.50f);
        float ease = 1f - Mathf.Cos(fall * Mathf.PI * 0.5f);

        SetRot(_torso,   Lerp(0f, -90f, ease));
        SetRot(_head,    Lerp(0f, -20f, ease));
        SetRot(_uArmR,   Lerp(-10f, -110f, ease));
        SetRot(_uArmL,   Lerp(10f,   70f, ease));
        SetRot(_uLegR,   Lerp(0f,   30f, ease));
        SetRot(_uLegL,   Lerp(0f,  -20f, ease));
        _hips.localPosition = new Vector3(Lerp(0f, 0.3f, ease), 0, 0);
    }

    // ══════════════════════════════════════════════
    //  PHYSICS ACTIONS
    // ══════════════════════════════════════════════
    void StartDash()
    {
        _isDashing = true;
        _dashTimer = _dashDuration;
        float dir  = transform.localScale.x > 0f ? 1f : -1f;
        _rb.linearVelocity = new Vector2(dir * _dashForce, _rb.linearVelocity.y * 0.5f);
    }

    void DoEvade()
    {
        float dir = transform.localScale.x > 0f ? -1f : 1f;
        _rb.linearVelocity = new Vector2(dir * _runSpeed * 1.4f, 5f);
    }

    // ══════════════════════════════════════════════
    //  HELPERS
    // ══════════════════════════════════════════════
    void CheckGround()
    {
        Vector2 origin = (Vector2)transform.position + new Vector2(0, 0.05f);
        _isGrounded = Physics2D.Raycast(origin, Vector2.down, 0.12f + _groundCheckDist, _groundLayer);
    }

    void FlipCharacter()
    {
        if (Mathf.Abs(_moveInput.x) < 0.01f) return;
        Vector3 s  = transform.localScale;
        s.x        = _moveInput.x > 0f ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
        transform.localScale = s;
    }

    void CameraFollow()
    {
        if (_cam == null) return;
        Vector3 target = transform.position + new Vector3(0, 0.5f, -10f);
        _cam.position  = Vector3.Lerp(_cam.position, target, Time.deltaTime * 5f);
    }

    void TickCooldowns()
    {
        float dt = Time.deltaTime;
        _shootCd = Mathf.Max(0, _shootCd - dt);
        _dashCd  = Mathf.Max(0, _dashCd  - dt);
        _evadeCd = Mathf.Max(0, _evadeCd - dt);
        _parryCd = Mathf.Max(0, _parryCd - dt);

        if (_isDashing)
        {
            _dashTimer -= dt;
            if (_dashTimer <= 0f) _isDashing = false;
        }
    }

    static void SetRot(Transform t, float zDeg)
    {
        if (t == null) return;
        t.localRotation = Quaternion.Euler(0, 0, zDeg);
    }

    static float Lerp(float a, float b, float t) => Mathf.LerpUnclamped(a, b, t);

    // ══════════════════════════════════════════════
    //  ON-SCREEN HUD (debug overlay)
    // ══════════════════════════════════════════════
    void OnGUI()
    {
        if (_labelStyle == null)
        {
            _labelStyle            = new GUIStyle(GUI.skin.label);
            _labelStyle.fontSize   = 16;
            _labelStyle.normal.textColor = Color.white;
        }

        GUI.Label(new Rect(10, 10, 400, 30), $"State: {_stateLabel}", _labelStyle);
        GUI.Label(new Rect(10, 32, 400, 30), $"Grounded: {_isGrounded}  |  VSpeed: {_rb?.linearVelocity.y:F1}", _labelStyle);
        GUI.Label(new Rect(10, 54, 400, 30), "WASD/Arrows: Move  |  Shift: Run  |  Space: Jump", _labelStyle);
        GUI.Label(new Rect(10, 76, 400, 30), "J: Shoot  |  K: Dash Attack  |  L: Evade  |  I: Parry", _labelStyle);
        GUI.Label(new Rect(10, 98, 400, 30), "H: Hurt (test)  |  X: Death (test)", _labelStyle);
    }
}
