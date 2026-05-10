using System;

namespace Genoverrei.Library.Core;

/// <summary>
/// Roll/Dodge ability สำหรับ 2D Side-view
/// - ดัน Rigidbody ทันทีที่ Enter แล้วรอจบ duration ใน FixedUpdate
/// - cooldown ถูก tick โดย MoveController2D.Update() ตลอดเวลา
/// </summary>
[Serializable]
public class RollMoveAbility2D : BaseMoveAbility2D
{
    [Header("Roll Settings")]
    [SerializeField] private float _rollForce    = 14f;
    [SerializeField] private float _rollDuration = 0.28f;
    [SerializeField] private float _cooldown     = 1.5f;

    private float            _rollTimer;
    private float            _cooldownTimer;
    private MoveController2D _owner;

    public bool  IsOnCooldown  => _cooldownTimer > 0f;
    public float CooldownRatio => _cooldown > 0f ? Mathf.Clamp01(_cooldownTimer / _cooldown) : 0f;

    public override void Initialize(IMoveContext2D context)
    {
        base.Initialize(context);
        _owner = context as MoveController2D;
    }

    protected override void OnEnter()
    {
        _rollTimer     = _rollDuration;
        _cooldownTimer = _cooldown;

        Vector2 dir = Context.CurrentDirection != Vector3.zero
            ? (Vector2)Context.CurrentDirection
            : (Vector2)Context.LastFacingDirection;

        if (dir == Vector2.zero) dir = Vector2.right;

        Context.Rb2.linearVelocity = dir.normalized * _rollForce;
    }

    protected override void OnFixedUpdate()
    {
        _rollTimer -= Time.fixedDeltaTime;
        if (_rollTimer <= 0f && _owner != null)
            _owner.ReturnToNormalMovement();
    }

    protected override void OnExit() { }

    /// <summary>เรียกจาก MoveController2D.Update() ทุก frame ไม่ว่า state ไหนจะ active</summary>
    public void TickCooldown(float dt)
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= dt;
    }
}
