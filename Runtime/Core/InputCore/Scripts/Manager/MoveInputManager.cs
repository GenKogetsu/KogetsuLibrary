using UnityEngine.InputSystem;
using Genoverrei.Library.Attribute;
using Genoverrei.Library.DesignPatternCore;

namespace Genoverrei.Library.Core
{
    [RequireComponent(typeof(PlayerInput))]
    [CreateHierarchyMenu("GenoverreiLibrary/Core")]
    public class BasicInputManager : MonoBehaviour, IAbility
    {
        [Header("ObserverChannels")]
        [Required]
        [SerializeField] protected BasicObserverChannelSO BasicObserverChannel;

        public void OnMoveInput(InputAction.CallbackContext context)
        {
            if (BasicObserverChannel == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("MoveInputChannelSO is not Assign");
#endif
                return;
            }

            BasicObserverChannel.SendMoveSignal(context.ReadValue<Vector3>());
        }

        public void OnJumpInput(InputAction.CallbackContext context)
        {
            if (BasicObserverChannel == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("JumpEventChannelSO is not Assign");
#endif
                return;
            }

            BasicObserverChannel.SendJumpSignal();
        }

        public void OnInteractionInput(InputAction.CallbackContext context)
        {
            if (BasicObserverChannel == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("InteractionInputSO is not Assign");
#endif
                return;
            }

            BasicObserverChannel.SendInteractionSignal();
        }

        public void OnLeftClickInput(InputAction.CallbackContext context)
        {
            if (!context.performed || BasicObserverChannel == null) return;

            Vector2 clickPos = Mouse.current.position.ReadValue();
            Debug.Log($"Left Click Pos : {clickPos}");
            BasicObserverChannel.SendLeftClickSignal(new ClickData(clickPos));
        }

        public void OnMiddelClickInput(InputAction.CallbackContext context)
        {
            if (!context.performed || BasicObserverChannel == null) return;

            Vector2 clickPos = Mouse.current.position.ReadValue();
            Debug.Log($"Middel Click Pos : {clickPos}");
            BasicObserverChannel.SendMiddleClickSignal(new ClickData(clickPos));
        }

        public void OnRightClickInput(InputAction.CallbackContext context)
        {
            if (!context.performed || BasicObserverChannel == null) return;
            Vector2 clickPos = Mouse.current.position.ReadValue();
            Debug.Log($"Right Click Pos : {clickPos}");
            BasicObserverChannel.SendRightClickSignal(new ClickData(clickPos));
        }
    }
}
