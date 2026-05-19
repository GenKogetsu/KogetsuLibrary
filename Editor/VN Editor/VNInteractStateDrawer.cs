#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Kogetsu.Library.Core;

namespace Kogetsu.Library.Editor
{
    [CustomPropertyDrawer(typeof(VNInteractState))]
    public class VNInteractStateDrawer : PropertyDrawer
    {
        private static readonly Dictionary<string, int> _arraySizes = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            CheckAndClearIfNewlyAdded(property);
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

                var subConvProp = property.FindPropertyRelative("SubConversation");

                //new: auto-init null managed reference elements (from [SerializeReference] + button)
                if (subConvProp != null && subConvProp.isArray)
                {
                    bool changed = false;
                    for (int i = 0; i < subConvProp.arraySize; i++)
                    {
                        var elem = subConvProp.GetArrayElementAtIndex(i);
                        if (elem.managedReferenceValue == null)
                        {
                            elem.managedReferenceValue = new VNConversationNode();
                            changed = true;
                        }
                    }
                    if (changed) property.serializedObject.ApplyModifiedProperties();
                }

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
            if (subConvProp != null) { subConvProp.ClearArray(); subConvProp.arraySize = 0; }

            prop.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded) return EditorGUIUtility.singleLineHeight + 4f;

            float h = EditorGUIUtility.singleLineHeight + 6f;
            h += EditorGUIUtility.singleLineHeight + 2f;
            h += EditorGUIUtility.singleLineHeight + 2f;
            h += EditorGUIUtility.singleLineHeight + 6f;
            h += VNConversationNodeDrawer.GetArrayWithCacheHeight(property.FindPropertyRelative("SubConversation"));
            return h + 4f;
        }

        private void CheckAndClearIfNewlyAdded(SerializedProperty property)
        {
            string path = property.propertyPath;

            // Unity element paths always end with ".Array.data[n]"
            // We need to strip that suffix to get a path FindProperty understands.
            int dotArrayData = path.LastIndexOf(".Array.data[");
            if (dotArrayData < 0) return;

            int lastBracket = path.LastIndexOf('[');
            int lastClose   = path.LastIndexOf(']');
            if (!int.TryParse(path[(lastBracket + 1)..lastClose], out int currentIndex)) return;

            string arrayFieldPath = path[..dotArrayData];   // e.g. "ChoiceNode.InteractStates"
            var arrayProp = property.serializedObject.FindProperty(arrayFieldPath);
            if (arrayProp == null || !arrayProp.isArray) return;

            int currentSize = arrayProp.arraySize;

            if (_arraySizes.TryGetValue(arrayFieldPath, out int prevSize))
            {
                if (currentSize > prevSize && currentIndex == currentSize - 1)
                {
                    ResetInteract(property);
                    _arraySizes[arrayFieldPath] = currentSize;
                    GUIUtility.ExitGUI();
                    return;
                }
            }

            if (currentIndex == currentSize - 1)
                _arraySizes[arrayFieldPath] = currentSize;
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
