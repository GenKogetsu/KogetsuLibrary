using System;
using Genoverrei.Library.DesignPatternCore;

namespace Genoverrei.Library.Core
{
    /// <summary>
    /// 2D move ability รองรับ 2 โหมด:
    ///
    ///  SideViewMode = false (default — top-down / free-move)
    ///    • ตั้ง linearVelocity เต็มทิศ (x + y)
    ///    • กระโดดแบบ AddForce ทันที ไม่มี coyote/buffer
    ///
    ///  SideViewMode = true (platformer)
    ///    • ตั้งเฉพาะ velocity.x ไม่แตะ y → gravity ทำงานตามปกติ
    ///    • ระบบกระโดดเต็ม: coyote time / jump buffer / variable height
    ///    • EnableJump ต้อง true จึงจะ subscribe jump channels
    ///    • EnableJump ถูก set โดย MoveController2D.AbilitySetUp() ก่อน OnEnter()
    /// </summary>
    [Serializable]
    public class BasicMoveAbility2D : BaseMoveAbility2D
    {
        // ─── Mode ──────────────────────────────────────────────────────────
        [Header("Mode")]
        [Tooltip("Platformer/Side-view: X-only velocity + full jump system.\n" +
                 "Off = top-down free-move (full velocity, instant jump).")]
        [SerializeField] private bool _sideViewMode = false;

        // ─── Jump ──────────────────────────────────────────────────────────
        [Header("Jump")]
        [Tooltip("Enable jump input. In top-down mode jump is instant AddForce.\n" +
                 "In side-view mode the full coyote/buffer/variable system is used.\n" +
                 "Set automatically by MoveController2D from its inspector toggle.")]
        [SerializeField] internal bool _enableJump = true;

        [Header("Jump Tuning (Side-View only)")]
        [Tooltip("Seconds after leaving a ledge that the player can still jump.")]
        [SerializeField] private float _coyoteTime = 0.12f;

        [Tooltip("Seconds in advance a jump input can be buffered.")]
        [SerializeField] private float _jumpBuffer = 0.12f;

        [Tooltip("Release jump mid-air to cut jump height.")]
        [SerializeField] private bool _variableJump = true;

        [Range(0f, 1f)]
        [Tooltip("velocity.y is multiplied by this when jump is released mid-air.")]
        [SerializeField] private float _jumpCutMultiplier = 0.45f;

        // ─── Runtime ───────────────────────────────────────────────────────
        private float _coyoteTimer;
        private float _jumpBufferTimer;
        private bool  _wasGrounded;
        private bool  _isJumping;
        private bool  _jumpHeld;

        // ─── Subscribe / Unsubscribe ───────────────────────────────────────
        protected override void OnEnter()
        {
            if (Context.InputObserverChannel == null) return;
            Context.InputObserverChannel.OnMoveChannel += SetInput;

            if (!_enableJump) return;

            if (_sideViewMode)
            {
                Context.InputObserverChannel.OnJumpChannel         += OnJumpPressed;
                Context.InputObserverChannel.OnJumpReleasedChannel += OnJumpReleased;
            }
            else
            {
                Context.InputObserverChannel.OnJumpChannel += ExecuteJump;
            }
        }

        protected override void OnExit()
        {
            if (Context.InputObserverChannel == null) return;
            Context.InputObserverChannel.OnMoveChannel -= SetInput;

            if (!_enableJump) return;

            if (_sideViewMode)
            {
                Context.InputObserverChannel.OnJumpChannel         -= OnJumpPressed;
                Context.InputObserverChannel.OnJumpReleasedChannel -= OnJumpReleased;
            }
            else
            {
                Context.InputObserverChannel.OnJumpChannel -= ExecuteJump;
            }
        }

        // ─── FixedUpdate ───────────────────────────────────────────────────
        protected override void OnFixedUpdate()
        {
            if (Context?.Stats == null) return;

            if (_sideViewMode)
            {
                if (_enableJump) TickJump();
                ApplySideViewMovement();
            }
            else
            {
                ApplyFullMovement();
            }
        }

        // ─── Movement ──────────────────────────────────────────────────────
        /// <summary>Top-down: sets full velocity (x + y).</summary>
        private void ApplyFullMovement()
        {
            Vector2 clamped = Vector2.ClampMagnitude(CurrentInput, 1f);
            Vector3 rawDir  = Context.TransformInput(new Vector3(clamped.x, clamped.y));
            Vector3 snapped = Context.SnapDirection(rawDir);

            Context.CurrentDirection = snapped;
            if (snapped != Vector3.zero)
                Context.LastFacingDirection = snapped;

            Context.Rb2.linearVelocity = (Vector2)rawDir * Context.Stats.GetMoveSpeed();
        }

        /// <summary>Side-view: sets X only, preserves Y for gravity/jump.</summary>
        private void ApplySideViewMovement()
        {
            Vector2 clamped     = Vector2.ClampMagnitude(CurrentInput, 1f);
            Vector3 transformed = Context.TransformInput(new Vector3(clamped.x, clamped.y, 0f));
            Vector3 snapped     = Context.SnapDirection(transformed);

            float xSpeed = snapped.x * Context.Stats.GetMoveSpeed();
            Context.Rb2.linearVelocity = new Vector2(xSpeed, Context.Rb2.linearVelocity.y);

            Context.CurrentDirection = snapped;
            if (snapped != Vector3.zero)
                Context.LastFacingDirection = snapped;
        }

        // ─── Jump System (Side-View) ────────────────────────────────────────
        private void TickJump()
        {
            bool grounded = Context.IsGrounded;

            if (grounded)
            {
                _coyoteTimer = _coyoteTime;
                if (!_wasGrounded)
                {
                    _isJumping = false;
                    if (EventBus.Instance)
                        EventBus.Instance.Publish(new PlayerLandEvent());
                }
            }
            else if (_coyoteTimer > 0f)
            {
                _coyoteTimer -= Time.fixedDeltaTime;
            }

            _wasGrounded = grounded;

            if (_jumpBufferTimer > 0f)
                _jumpBufferTimer -= Time.fixedDeltaTime;

            // Execute jump when buffer + coyote both active
            if (_jumpBufferTimer > 0f && _coyoteTimer > 0f)
            {
                _jumpBufferTimer   = 0f;
                _coyoteTimer       = 0f;
                _isJumping         = true;
                Context.IsGrounded = false;

                var vel = Context.Rb2.linearVelocity;
                Context.Rb2.linearVelocity = new Vector2(vel.x, 0f);
                Context.Rb2.AddForce(Vector2.up * Context.Stats.GetJumpForce(), ForceMode2D.Impulse);

                if (EventBus.Instance)
                    EventBus.Instance.Publish(new PlayerJumpEvent());
            }

            // Variable-height cut
            if (_variableJump && _isJumping && !_jumpHeld && Context.Rb2.linearVelocity.y > 0f)
            {
                var vel = Context.Rb2.linearVelocity;
                Context.Rb2.linearVelocity = new Vector2(vel.x, vel.y * _jumpCutMultiplier);
            }
        }

        // ─── Top-Down Jump (instant) ────────────────────────────────────────
        private void ExecuteJump()
        {
            if (Context?.Stats == null) return;
            Context.Rb2.AddForce(Vector2.up * Context.Stats.GetJumpForce(), ForceMode2D.Impulse);
        }

        // ─── Input Callbacks ───────────────────────────────────────────────
        private void OnJumpPressed()
        {
            _jumpBufferTimer = _jumpBuffer;
            _jumpHeld        = true;
        }

        private void OnJumpReleased() => _jumpHeld = false;
    }
}
