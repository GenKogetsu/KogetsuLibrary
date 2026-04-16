using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Genoverrei.Library.Attribute;
using Genoverrei.Library.DesignPatternCore;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Genoverrei.Library.Core
{
    [RequireComponent(typeof(PlayerInput))]
    [CreateHierarchyMenu("GenoverreiLibrary/Core/Manager")]
    public class GeneralInputManager : Singleton<GeneralInputManager>
    {
        public void OnPlayerInput(InputAction.CallbackContext context)
        {
            if (EventBus.Instance == null)
            {
                string assetPath = "Packages/com.genoverrei.library/Runtime/Assets/Prefab/DesignPattern/EventBus.prefab";

                UnityEngine.Object prefab = null;
#if UNITY_EDITOR
                prefab = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);

                Debug.LogError("<color=red>[Error]</color> <color=orange><b>Don't have EventBus in this scene.</b></color>" + Environment.NewLine
                    + "Please create <b>GenoverreiLibrary's EventBus</b> in this scene." + Environment.NewLine
                    + $"Path: <u><color=#87CEEB><i>{assetPath}</i></color></u>", prefab);
#endif

                return;
            }

            EventBus.Instance.Publish<InputEvent>(new InputEvent(context));
        }
    }
}