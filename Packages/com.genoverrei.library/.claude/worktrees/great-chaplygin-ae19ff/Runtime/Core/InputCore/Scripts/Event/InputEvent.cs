using UnityEngine;
using UnityEngine.InputSystem;
using Genoverrei.Library.DesignPatternCore;

namespace Genoverrei.Library
{
    public record struct InputEvent(InputAction.CallbackContext Context) : IEvent;

}
