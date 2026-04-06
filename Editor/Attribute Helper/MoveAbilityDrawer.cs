#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Genoverrei.Library.Attribute;

namespace Genoverrei.Library.Editor
{
    [CustomPropertyDrawer(typeof(MoveAbilitySelector))]
    public class MoveAbilityDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUI.LabelField(position, label.text, "Use [SubclassSelector] with [SerializeReference] only.");
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            var foldoutRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            var dropdownRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);

            string fullTypeName = property.managedReferenceFullTypename;
            string displayTypeName = string.IsNullOrEmpty(fullTypeName) ? "None (Null)" : fullTypeName.Split('.').Last();

            if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(displayTypeName), FocusType.Keyboard, EditorStyles.popup))
            {
                ShowTypeMenu(property);
            }

            if (property.isExpanded && !string.IsNullOrEmpty(fullTypeName))
            {
                EditorGUI.indentLevel++;
                float currentY = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                SerializedProperty child = property.Copy();
                SerializedProperty endProperty = property.GetEndProperty();

                bool hasNext = child.NextVisible(true);
                bool hasFields = false;

                while (hasNext && !SerializedProperty.EqualContents(child, endProperty))
                {
                    hasFields = true;
                    float childHeight = EditorGUI.GetPropertyHeight(child, true);
                    var childRect = new Rect(position.x, currentY, position.width, childHeight);

                    EditorGUI.PropertyField(childRect, child, true);
                    currentY += childHeight + EditorGUIUtility.standardVerticalSpacing;

                    hasNext = child.NextVisible(false); 
                }

                if (!hasFields)
                {
                    var emptyRect = new Rect(position.x, currentY, position.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.LabelField(emptyRect, "No Settings", EditorStyles.miniLabel);
                }
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        private void ShowTypeMenu(SerializedProperty property)
        {
            GenericMenu menu = new();

            Type baseType = GetBaseType(property);

            menu.AddItem(new GUIContent("None"), string.IsNullOrEmpty(property.managedReferenceFullTypename), () =>
            {
                property.managedReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();
            });

            if (baseType == null)
            {
                menu.ShowAsContext();
                return;
            }

            menu.AddSeparator("");

            var types = TypeCache.GetTypesDerivedFrom(baseType)
                .Where(t => !t.IsAbstract && !t.IsInterface);

            foreach (var type in types)
            {
                menu.AddItem(new GUIContent(type.Name), false, () =>
                {
                    property.managedReferenceValue = Activator.CreateInstance(type);
                    property.isExpanded = true;
                    property.serializedObject.ApplyModifiedProperties();
                });
            }
            menu.ShowAsContext();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float totalHeight = EditorGUIUtility.singleLineHeight;

            if (property.isExpanded && !string.IsNullOrEmpty(property.managedReferenceFullTypename))
            {
                SerializedProperty child = property.Copy();
                SerializedProperty endProperty = property.GetEndProperty();

                bool hasNext = child.NextVisible(true);
                bool hasFields = false;

                while (hasNext && !SerializedProperty.EqualContents(child, endProperty))
                {
                    hasFields = true;
                    totalHeight += EditorGUI.GetPropertyHeight(child, true) + EditorGUIUtility.standardVerticalSpacing;
                    hasNext = child.NextVisible(false);
                }

                if (!hasFields)
                {
                    totalHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }
            }

            return totalHeight;
        }

        private Type GetBaseType(SerializedProperty property)
        {
            string typeName = property.managedReferenceFieldTypename;
            if (string.IsNullOrEmpty(typeName)) return null;

            string[] parts = typeName.Split(' ');
            if (parts.Length == 2)
            {
                var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == parts[0]);
                if (assembly != null) return assembly.GetType(parts[1]);
            }
            return null;
        }
    }
}
#endif