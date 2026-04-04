using UnityEngine.InputSystem;
using Genoverrei.Library.DesignPatternCore;

namespace Genoverrei.Library.Core
{
    [RequireComponent(typeof(PlayerInput))]
    public class BasicInputManager : MonoBehaviour , IAbility    
    {
        [Required]
        [SerializeField] protected InputObserverChannelSO InputObserverChannel;

        public void OnMoveInput(InputAction.CallbackContext context)
        {
            if (InputObserverChannel == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("MoveInputChannelSO is not Assign");
#endif
                return;
            }

            InputObserverChannel.SendMoveSignal(context.ReadValue<Vector3>());
        }

        public void OnJumpInput(InputAction.CallbackContext context)
        {
            if (InputObserverChannel == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("JumpEventChannelSO is not Assign");
#endif
                return;
            }

            InputObserverChannel.SendJumpSignal();
        }

        public void OnInteractionInput(InputAction.CallbackContext context)
        {
            if (InputObserverChannel == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("InteractionInputSO is not Assign");
#endif
                return;
            }

            InputObserverChannel.SendInteractionSignal();
        }
    }

}
