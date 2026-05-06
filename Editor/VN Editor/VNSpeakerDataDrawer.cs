#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Genoverrei.Library.Core;

namespace Genoverrei.Library.Editor //new: fixed namespace (was UIEditor)
{
    [CustomPropertyDrawer(typeof(VNSpeakerData))]
    public class VNSpeakerDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var characterProp = property.FindPropertyRelative("Character");
            var nameModeProp = property.FindPropertyRelative("NameDisplayMode"); //new: replaces ShowName
            var actionsProp = property.FindPropertyRelative("Actions");

            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            string displayName = "Speaker [Empty]";
            if (characterProp != null && characterProp.objectReferenceValue != null)
                displayName = $"Speaker [{characterProp.objectReferenceValue.name}]";

            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, displayName, true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                float y = position.y + EditorGUIUtility.singleLineHeight + 2f;

                if (characterProp != null)
                {
                    float h = EditorGUI.GetPropertyHeight(characterProp, true);
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, h), characterProp, new GUIContent("Character"), true);
                    y += h + 2f;
                }

                if (nameModeProp != null) //new: enum dropdown instead of bool toggle
                {
                    float h = EditorGUI.GetPropertyHeight(nameModeProp, true);
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, h), nameModeProp, new GUIContent("Name Display Mode"), true);
                    y += h + 2f;
                }

                if (actionsProp != null)
                    VNConversationNodeDrawer.DrawArrayWithCache(ref y, position, actionsProp, "Actions List", "Action");

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;

            float totalHeight = EditorGUIUtility.singleLineHeight + 2f;

            var characterProp = property.FindPropertyRelative("Character");
            var nameModeProp = property.FindPropertyRelative("NameDisplayMode"); //new
            var actionsProp = property.FindPropertyRelative("Actions");

            if (characterProp != null) totalHeight += EditorGUI.GetPropertyHeight(characterProp, true) + 2f;
            if (nameModeProp != null) totalHeight += EditorGUI.GetPropertyHeight(nameModeProp, true) + 2f;
            if (actionsProp != null) totalHeight += VNConversationNodeDrawer.GetArrayWithCacheHeight(actionsProp);

            return totalHeight;
        }
    }
}
#endif
