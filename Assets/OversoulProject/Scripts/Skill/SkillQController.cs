
using System;
using Kogetsu.Library.Core;
using UnityEngine.InputSystem;

public sealed class SkillQController : BaseSkillMono
{
    [SerializeField] private AnimationClip _holdStateAnimation;
    [SerializeField] private AnimationClip _releaseStateAnimation;

    protected override void EffectSkill(InputAction.CallbackContext context)
    {
        Action action = context.phase switch
        {
            InputActionPhase.Started => HandleStart, 
            InputActionPhase.Canceled => HandleCancel,
            InputActionPhase.Performed => HandlePerform,
            _ => null
        };

        action();
    }

    private void HandleStart()
    {
        ButtonAnimator.Play(_holdStateAnimation.name);
    }

    private void HandlePerform()
    {

    }

    private void HandleCancel()
    {

    }
}
