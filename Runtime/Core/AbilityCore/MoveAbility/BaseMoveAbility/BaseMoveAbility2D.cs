using System;
using System.Collections;
using Kogetsu.Library.DesignPatternCore;
using UnityEngine;

namespace Kogetsu.Library.Core;

[Serializable]
public abstract class BaseMoveAbility2D : BaseMoveAbility<IMoveContext2D>
{
    // ─── Context Shortcuts ────────────────────────────────────────────────
    /// <summary>True when both Context and Stats are non-null and safe to use.</summary>
    protected bool HasStats => Context?.Stats != null;

    /// <summary>Current move speed from StatsController.</summary>
    protected float MoveSpeed => Context.Stats.GetMoveSpeed();

    /// <summary>Jump force from StatsController.</summary>
    protected float JumpForce => Context.Stats.GetJumpForce();

    // ─── Damage Flash ─────────────────────────────────────────────────────
    [Header("Damage Flash")]
    [Tooltip("ถ้าว่างจะหา SpriteRenderer บน Context Transform แล้ว fallback ไป children อัตโนมัติ")]
    [SerializeField] private SpriteRenderer _flashTarget;
    [SerializeField] private Color          _flashColor    = Color.red;
    [Min(0.01f)]
    [SerializeField] private float          _flashDuration = 0.2f;

    private Color        _originalColor;
    private Coroutine    _flashCoroutine;
    private MonoBehaviour _flashRunner;

    public override void Initialize(IMoveContext2D context)
    {
        base.Initialize(context);

        _flashRunner = context as MonoBehaviour;

        if (_flashTarget == null)
        {
            if (!context.Transform.TryGetComponent(out _flashTarget))
                _flashTarget = context.Transform.GetComponentInChildren<SpriteRenderer>();
        }

        _originalColor = _flashTarget != null ? _flashTarget.color : Color.white;
    }

    /// <summary>Trigger damage flash — เรียกจาก damage handler หรือ ability ใดก็ได้</summary>
    protected void Flash()
    {
        if (_flashRunner == null || _flashTarget == null) return;
        if (_flashCoroutine != null) _flashRunner.StopCoroutine(_flashCoroutine);
        _flashCoroutine = _flashRunner.StartCoroutine(Flash2DRoutine());
    }

    private IEnumerator Flash2DRoutine()
    {
        _flashTarget.color = _flashColor;

        float elapsed = 0f;
        while (elapsed < _flashDuration)
        {
            elapsed += Time.deltaTime;
            _flashTarget.color = Color.Lerp(_flashColor, _originalColor, elapsed / _flashDuration);
            yield return null;
        }

        _flashTarget.color = _originalColor;
        _flashCoroutine    = null;
    }

    // ─── Direction ────────────────────────────────────────────────────────
    /// <summary>
    /// Sets <see cref="IMoveContext.CurrentDirection"/> to <paramref name="direction"/>.
    /// Also updates <see cref="IMoveContext.LastFacingDirection"/> when direction is non-zero,
    /// so the flip system always knows the last facing axis.
    /// </summary>
    protected void ApplyDirection(Vector3 direction)
    {
        Context.CurrentDirection = direction;
        if (direction != Vector3.zero)
            Context.LastFacingDirection = direction;
    }

    // ─── Velocity ─────────────────────────────────────────────────────────
    /// <summary>Set full 2D velocity (X and Y).</summary>
    protected void SetVelocity(Vector2 velocity) =>
        Context.Rb2.linearVelocity = velocity;

    /// <summary>Set X-axis velocity only — preserves Y so gravity and jump are unaffected.</summary>
    protected void SetVelocityX(float x) =>
        Context.Rb2.linearVelocity = new Vector2(x, Context.Rb2.linearVelocity.y);

    /// <summary>Zero out Rigidbody2D velocity.</summary>
    protected void StopVelocity() =>
        Context.Rb2.linearVelocity = Vector2.zero;

    // ─── Input Pipeline ───────────────────────────────────────────────────
    /// <summary>
    /// Full keyboard/gamepad input pipeline:
    /// ClampMagnitude(1) → TransformInput → SnapDirection.
    /// Returns the final snapped direction used for both velocity and <see cref="ApplyDirection"/>.
    /// </summary>
    protected Vector3 ProcessInput2D(Vector2 rawInput)
    {
        Vector2 clamped     = Vector2.ClampMagnitude(rawInput, 1f);
        Vector3 transformed = Context.TransformInput(new Vector3(clamped.x, clamped.y, 0f));
        return Context.SnapDirection(transformed);
    }

    /// <summary>
    /// Partial input pipeline: ClampMagnitude(1) → TransformInput only (no snap).
    /// Use when you need the raw transformed direction for velocity
    /// while snapping only for the display direction.
    /// </summary>
    protected Vector3 TransformInput2D(Vector2 rawInput)
    {
        Vector2 clamped = Vector2.ClampMagnitude(rawInput, 1f);
        return Context.TransformInput(new Vector3(clamped.x, clamped.y, 0f));
    }
}
