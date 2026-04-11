#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Genoverrei.Library.Core;

namespace Genoverrei.Library.Editor
{
    [CustomPropertyDrawer(typeof(VNInteractState))]
    public class VNInteractStateDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Color bgColor = VNConversationNodeDrawer.UseTint ? new Color(0.98f, 0.92f, 0.95f) : Color.white;
            GUI.color = bgColor;
            GUI.Box(new Rect(position.x - 5f, position.y, position.width + 5f, GetPropertyHeight(property, label)), "", EditorStyles.helpBox);
            GUI.color = Color.white;

            var index = GetIndex(property.propertyPath);

            var foldoutRect = new Rect(position.x + 20f, position.y + 2f, position.width - 130f, EditorGUIUtility.singleLineHeight);
            var clearBtnRect = new Rect(position.xMax - 105f, position.y + 2f, 50f, EditorGUIUtility.singleLineHeight);
            var recallBtnRect = new Rect(position.xMax - 50f, position.y + 2f, 50f, EditorGUIUtility.singleLineHeight);

            GUIStyle headerStyle = new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold };
            headerStyle.padding.right += 2;

            if (VNConversationNodeDrawer.UseTint) headerStyle.normal.textColor = new Color(0.6f, 0.2f, 0.4f);

            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, $"Interact {index}", true, headerStyle);

            if (GUI.Button(clearBtnRect, "Clear", EditorStyles.miniButtonLeft))
            {
                VNCacheHelper.CacheElement(property);
                ResetInteract(property);
                GUIUtility.keyboardControl = 0;
            }

            GUI.enabled = VNCacheHelper.ElementCache.ContainsKey(property.propertyPath);
            if (GUI.Button(recallBtnRect, "Recall", EditorStyles.miniButtonRight))
            {
                VNCacheHelper.RecallElement(property);
                GUIUtility.keyboardControl = 0;
            }
            GUI.enabled = true;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                var y = position.y + EditorGUIUtility.singleLineHeight + 6f;

                EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("TargetAnswerNumber"));
                y += EditorGUIUtility.singleLineHeight + 2f;

                EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("ReturnValue"));
                y += EditorGUIUtility.singleLineHeight + 2f;

                EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("ReturnToChoicePhase"));
                y += EditorGUIUtility.singleLineHeight + 6f;

                // วาด SubConversation ด้วย Helper ตัวเดียวกับโหนดหลัก รองรับการ Cache อย่างสมบูรณ์
                var subConvProp = property.FindPropertyRelative("SubConversation");
                VNConversationNodeDrawer.DrawArrayWithCache(ref y, position, subConvProp, "Sub Conversation List", "Node");

                EditorGUI.indentLevel--;
            }
            EditorGUI.EndProperty();
        }

        private void ResetInteract(SerializedProperty prop)
        {
            prop.FindPropertyRelative("TargetAnswerNumber").intValue = 0;
            prop.FindPropertyRelative("ReturnValue").floatValue = 0f;
            prop.FindPropertyRelative("ReturnToChoicePhase").boolValue = false;

            var subConvProp = prop.FindPropertyRelative("SubConversation");
            if (subConvProp != null)
            {
                subConvProp.ClearArray();
                subConvProp.arraySize = 0;
            }

            prop.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded) return EditorGUIUtility.singleLineHeight + 4f;

            float h = EditorGUIUtility.singleLineHeight + 6f; // Header
            h += EditorGUIUtility.singleLineHeight + 2f; // TargetAnswerNumber
            h += EditorGUIUtility.singleLineHeight + 2f; // ReturnValue
            h += EditorGUIUtility.singleLineHeight + 6f; // ReturnToChoicePhase

            var subConvProp = property.FindPropertyRelative("SubConversation");
            h += VNConversationNodeDrawer.GetArrayWithCacheHeight(subConvProp); // รวมความสูงของ SubConversation List

            return h + 4f;
        }

        private int GetIndex(string path)
        {
            var s = path.LastIndexOf('[') + 1;
            var e = path.LastIndexOf(']');
            return (s > 0 && e > s && int.TryParse(path[s..e], out var i)) ? i + 1 : 1;
        }
    }
}
#endif