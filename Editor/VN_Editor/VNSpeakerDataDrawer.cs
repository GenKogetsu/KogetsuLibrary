#if UNITY_EDITOR
using UnityEditor;
using Genoverrei.Library.Core;

namespace Genoverrei.Library.Editor
{
    [CustomPropertyDrawer(typeof(VNSpeakerData))]
    public class VNSpeakerDataDrawer : PropertyDrawer
    {
        private static readonly System.Collections.Generic.HashSet<string> _initializedPaths = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            if (!_initializedPaths.Contains(property.propertyPath))
            {
                var showNameProp = property.FindPropertyRelative("ShowName");
                if (showNameProp != null && !showNameProp.boolValue)
                {
                    var charProp = property.FindPropertyRelative("Character");
                    if (charProp != null && charProp.objectReferenceValue == null)
                    {
                        showNameProp.boolValue = true;
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }
                _initializedPaths.Add(property.propertyPath);
            }

            var foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            string displayName = "Speaker [Empty]";
            var characterProp = property.FindPropertyRelative("Character");
            if (characterProp != null && characterProp.objectReferenceValue != null)
            {
                displayName = $"Speaker [{characterProp.objectReferenceValue.name}]";
            }

            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, displayName, true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                float y = position.y + EditorGUIUtility.singleLineHeight + 2f;

                EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), characterProp);
                y += EditorGUIUtility.singleLineHeight + 2f;

                var showNameField = property.FindPropertyRelative("ShowName");
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), showNameField);
                y += EditorGUIUtility.singleLineHeight + 2f;

                // new: วาด Actions แบบมี Cache (ปุ่ม Clear/Recall)
                var actionsProp = property.FindPropertyRelative("Actions");
                VNConversationNodeDrawer.DrawArrayWithCache(ref y, position, actionsProp, "Actions List", "Action");

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;

            float h = EditorGUIUtility.singleLineHeight + 2f;
            h += EditorGUIUtility.singleLineHeight + 2f;
            h += EditorGUIUtility.singleLineHeight + 2f;

            var actionsProp = property.FindPropertyRelative("Actions");
            if (actionsProp != null)
            {
                h += VNConversationNodeDrawer.GetArrayWithCacheHeight(actionsProp);
            }

            return h;
        }
    }
}
#endif