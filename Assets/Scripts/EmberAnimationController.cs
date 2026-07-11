using UnityEngine;
// UnityEngine.U2D.Animation is only needed when using Sprite Rigging (Step 4+)
// using UnityEngine.U2D.Animation;

/// <summary>
/// Controls all animation states for Ember character.
/// Attach to the root GameObject that has an Animator component.
/// </summary>
[RequireComponent(typeof(Animator))]
public class EmberAnimationController : MonoBehaviour
{
    // ──────────────────────────────────────────────
    //  Animator Parameter Hash IDs (cached for performance)
    // ──────────────────────────────────────────────
    static readonly int HASH_SPEED       = Animator.StringToHash("Speed");
    static readonly int HASH_VSPEED      = Animator.StringToHash("VSpeed");
    static readonly int HASH_GROUNDED    = Animator.StringToHash("IsGrounded");
    static readonly int HASH_DASHING     = Animator.StringToHash("IsDashing");
    static readonly int HASH_SHOOT       = Animator.StringToHash("Shoot");
    static readonly int HASH_DASH_ATTACK = Animator.StringToHash("DashAttack");
    static readonly int HASH_EVADE       = Animator.StringToHash("Evade");
    static readonly int HASH_PARRY       = Animator.StringToHash("Parry");
    static readonly int HASH_HURT        = Animator.StringToHash("Hurt");
    static readonly int HASH_DEATH       = Animator.StringToHash("Death");

    [Header("References")]
    [SerializeField] Animator _animator;

    [Header("Sprite Parts — Weapon Visibility")]
    [SerializeField] GameObject _steamBlaster;
    [SerializeField] GameObject _manaSaber;
    [SerializeField] SpriteRenderer _coatTail;

    [Header("VFX")]
    [SerializeField] ParticleSystem _dashTrailVFX;
    [SerializeField] ParticleSystem _manaSlashVFX;
    [SerializeField] ParticleSystem _shootMuzzleVFX;
    [SerializeField] ParticleSystem _parrySparkVFX;

    [Header("Tuning")]
    [SerializeField, Range(0f, 1f)] float _coatFlutterIntensity = 0.8f;
    [SerializeField] float _runThreshold = 4f;

    // ──────────────────────────────────────────────
    //  Internal State
    // ──────────────────────────────────────────────
    bool _isDead;

    void Awake()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();
    }

    // ══════════════════════════════════════════════
    //  PUBLIC API — called by EmberPlayerController
    // ══════════════════════════════════════════════

    /// <summary>Set horizontal movement speed (0 = idle, > runThreshold = run).</summary>
    public void SetSpeed(float speed)
    {
        if (_isDead) return;
        _animator.SetFloat(HASH_SPEED, Mathf.Abs(speed));
    }

    /// <summary>Set vertical velocity for jump/fall blend.</summary>
    public void SetVerticalSpeed(float vSpeed)
    {
        if (_isDead) return;
        _animator.SetFloat(HASH_VSPEED, vSpeed);
    }

    /// <summary>Sync grounded state each physics frame.</summary>
    public void SetGrounded(bool grounded)
    {
        if (_isDead) return;
        _animator.SetBool(HASH_GROUNDED, grounded);
    }

    /// <summary>Trigger one-shot: Shoot (Steam-Blaster).</summary>
    public void TriggerShoot()
    {
        if (_isDead) return;
        _animator.SetTrigger(HASH_SHOOT);
        if (_shootMuzzleVFX) _shootMuzzleVFX.Play();
    }

    /// <summary>Trigger Dash Attack — leap forward with Mana-Saber.</summary>
    public void TriggerDashAttack()
    {
        if (_isDead) return;
        _animator.SetTrigger(HASH_DASH_ATTACK);
        _animator.SetBool(HASH_DASHING, true);
        if (_dashTrailVFX)  _dashTrailVFX.Play();
        if (_manaSlashVFX)  _manaSlashVFX.Play();
    }

    /// <summary>Called by animation event when DashAttack ends.</summary>
    public void OnDashAttackEnd()
    {
        _animator.SetBool(HASH_DASHING, false);
        if (_dashTrailVFX) _dashTrailVFX.Stop();
    }

    /// <summary>Trigger Evade (side-roll dodge).</summary>
    public void TriggerEvade()
    {
        if (_isDead) return;
        _animator.SetTrigger(HASH_EVADE);
    }

    /// <summary>Trigger Parry — block + riposte pose.</summary>
    public void TriggerParry()
    {
        if (_isDead) return;
        _animator.SetTrigger(HASH_PARRY);
        if (_parrySparkVFX) _parrySparkVFX.Play();
    }

    /// <summary>Trigger hit reaction.</summary>
    public void TriggerHurt()
    {
        if (_isDead) return;
        _animator.SetTrigger(HASH_HURT);
    }

    /// <summary>Trigger death — locks all other transitions.</summary>
    public void TriggerDeath()
    {
        if (_isDead) return;
        _isDead = true;
        _animator.SetTrigger(HASH_DEATH);
        if (_dashTrailVFX) _dashTrailVFX.Stop();
    }

    /// <summary>Flip sprite to face movement direction.</summary>
    public void SetFacingDirection(float horizontalInput)
    {
        if (horizontalInput == 0f || _isDead) return;
        Vector3 scale = transform.localScale;
        scale.x = horizontalInput > 0f ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    // ══════════════════════════════════════════════
    //  ANIMATION EVENTS (called from .anim clips)
    // ══════════════════════════════════════════════

    // Called at frame 0 of Walk/Run — footstep left
    void AnimEvent_FootstepLeft()  => AudioManager.PlaySFX("footstep_boot");

    // Called at half-cycle of Walk/Run — footstep right
    void AnimEvent_FootstepRight() => AudioManager.PlaySFX("footstep_boot");

    // Called at jump apex frame
    void AnimEvent_JumpApex() { /* camera shake etc */ }

    // Called at land impact frame
    void AnimEvent_Land()          => AudioManager.PlaySFX("land_thud");

    // Called at shoot burst frame
    void AnimEvent_ShootFire()     => AudioManager.PlaySFX("steam_blaster_fire");

    // Called when DashAttack slash connects
    void AnimEvent_DashSlash()     => AudioManager.PlaySFX("mana_saber_slash");

    // Called at end of DashAttack clip
    void AnimEvent_DashEnd()       => OnDashAttackEnd();

    // Called at parry success frame
    void AnimEvent_ParrySuccess()  => AudioManager.PlaySFX("parry_clang");
}

// ──────────────────────────────────────────────────────────────────
//  Stub — replace with your own AudioManager implementation
// ──────────────────────────────────────────────────────────────────
public static class AudioManager
{
    public static void PlaySFX(string clipName) { /* implement */ }
}
