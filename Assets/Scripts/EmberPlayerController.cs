using UnityEngine;
// Input System needed for production build — EmberProtoController uses legacy Input for prototype
// using UnityEngine.InputSystem;

/// <summary>
/// Handles physics movement and routes input to EmberAnimationController.
/// Requires: Rigidbody2D, CapsuleCollider2D, EmberAnimationController.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(EmberAnimationController))]
public class EmberPlayerController : MonoBehaviour
{
    // ──────────────────────────────────────────────
    //  Inspector Settings
    // ──────────────────────────────────────────────
    [Header("Movement")]
    [SerializeField] float _walkSpeed    = 3.5f;
    [SerializeField] float _runSpeed     = 7f;
    [SerializeField] float _jumpForce    = 12f;
    [SerializeField] float _dashForce    = 18f;
    [SerializeField] float _dashDuration = 0.18f;
    [SerializeField] float _dashCooldown = 0.8f;

    [Header("Ground Check")]
    [SerializeField] LayerMask _groundLayer;
    [SerializeField] Transform _groundCheck;
    [SerializeField] float     _groundCheckRadius = 0.15f;

    [Header("Combat Cooldowns (seconds)")]
    [SerializeField] float _shootCooldown       = 0.35f;
    [SerializeField] float _dashAttackCooldown  = 1.0f;
    [SerializeField] float _evadeCooldown       = 0.6f;
    [SerializeField] float _parryCooldown       = 0.5f;

    // ──────────────────────────────────────────────
    //  Runtime State
    // ──────────────────────────────────────────────
    Rigidbody2D             _rb;
    EmberAnimationController _anim;

    Vector2 _moveInput;
    bool    _isGrounded;
    bool    _isDashing;
    bool    _isRunHeld;

    float _dashTimer;
    float _dashCooldownTimer;
    float _shootTimer;
    float _dashAttackTimer;
    float _evadeTimer;
    float _parryTimer;

    // ──────────────────────────────────────────────
    //  Unity Lifecycle
    // ──────────────────────────────────────────────
    void Awake()
    {
        _rb   = GetComponent<Rigidbody2D>();
        _anim = GetComponent<EmberAnimationController>();
    }

    void Update()
    {
        TickCooldowns();
        CheckGround();
        HandleJumpInput();
        HandleCombatInput();
        SyncAnimator();
    }

    void FixedUpdate()
    {
        if (_isDashing) return;   // physics locked during dash
        ApplyMovement();
    }

    // ══════════════════════════════════════════════
    //  INPUT SYSTEM CALLBACKS (New Input System)
    // ══════════════════════════════════════════════

    // Bind these via Player Input component → Send Messages

    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }

    public void OnRun(InputValue value)
    {
        _isRunHeld = value.isPressed;
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && _isGrounded)
            Jump();
    }

    public void OnShoot(InputValue value)
    {
        if (value.isPressed && _shootTimer <= 0f)
            Shoot();
    }

    public void OnDashAttack(InputValue value)
    {
        if (value.isPressed && _dashAttackTimer <= 0f)
            StartDashAttack();
    }

    public void OnEvade(InputValue value)
    {
        if (value.isPressed && _evadeTimer <= 0f)
            Evade();
    }

    public void OnParry(InputValue value)
    {
        if (value.isPressed && _parryTimer <= 0f)
            Parry();
    }

    // ══════════════════════════════════════════════
    //  MOVEMENT
    // ══════════════════════════════════════════════

    void ApplyMovement()
    {
        float speed = _isRunHeld ? _runSpeed : _walkSpeed;
        _rb.linearVelocity = new Vector2(_moveInput.x * speed, _rb.linearVelocity.y);
        _anim.SetFacingDirection(_moveInput.x);
    }

    void Jump()
    {
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
    }

    // ══════════════════════════════════════════════
    //  COMBAT ACTIONS
    // ══════════════════════════════════════════════

    void Shoot()
    {
        _shootTimer = _shootCooldown;
        _anim.TriggerShoot();
    }

    void StartDashAttack()
    {
        _dashAttackTimer = _dashAttackCooldown;
        _isDashing = true;
        _dashTimer = _dashDuration;

        float dir = transform.localScale.x > 0f ? 1f : -1f;
        _rb.linearVelocity = new Vector2(dir * _dashForce, _rb.linearVelocity.y);

        _anim.TriggerDashAttack();
    }

    void Evade()
    {
        _evadeTimer = _evadeCooldown;
        _anim.TriggerEvade();
        // Quick backward hop
        float dir = transform.localScale.x > 0f ? -1f : 1f;
        _rb.linearVelocity = new Vector2(dir * _runSpeed * 1.5f, _rb.linearVelocity.y);
    }

    void Parry()
    {
        _parryTimer = _parryCooldown;
        _anim.TriggerParry();
    }

    // ══════════════════════════════════════════════
    //  HELPERS
    // ══════════════════════════════════════════════

    void CheckGround()
    {
        bool wasGrounded = _isGrounded;
        _isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);
        _anim.SetGrounded(_isGrounded);
    }

    void HandleJumpInput() { /* handled via OnJump callback above */ }

    void HandleCombatInput() { /* handled via Input System callbacks above */ }

    void TickCooldowns()
    {
        float dt = Time.deltaTime;
        _shootTimer        = Mathf.Max(0f, _shootTimer        - dt);
        _dashAttackTimer   = Mathf.Max(0f, _dashAttackTimer   - dt);
        _evadeTimer        = Mathf.Max(0f, _evadeTimer        - dt);
        _parryTimer        = Mathf.Max(0f, _parryTimer        - dt);
        _dashCooldownTimer = Mathf.Max(0f, _dashCooldownTimer - dt);

        if (_isDashing)
        {
            _dashTimer -= dt;
            if (_dashTimer <= 0f)
            {
                _isDashing = false;
                _dashCooldownTimer = _dashCooldown;
            }
        }
    }

    void SyncAnimator()
    {
        _anim.SetSpeed(_rb.linearVelocity.x);
        _anim.SetVerticalSpeed(_rb.linearVelocity.y);
    }

    // ──────────────────────────────────────────────
    //  Gizmo: visualize ground check radius in editor
    // ──────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        if (_groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
    }

    // ══════════════════════════════════════════════
    //  PUBLIC — called by game systems
    // ══════════════════════════════════════════════

    public void TakeDamage()  => _anim.TriggerHurt();
    public void Die()         => _anim.TriggerDeath();
}
