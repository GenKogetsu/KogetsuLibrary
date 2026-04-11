#if UNITY_EDITOR

using UnityEditor;
using Genoverrei.Library.Core;

namespace Genoverrei.Library.Editor
{
    [CustomEditor(typeof(MoveController2D))]
    public class MoveController2DEditor : UnityEditor.Editor
    {
        private SerializedProperty _stats;
        private SerializedProperty _rb2;
        private SerializedProperty _inputObserverChannel;
        private SerializedProperty _directionMode;
        private SerializedProperty _moveAbility;
        private SerializedProperty _perspectiveMode;
        private SerializedProperty _perspectiveQuaternion2D;

        private SerializedProperty _currentDirection;
        private SerializedProperty _lastFacingDirection;
        private SerializedProperty _isGrounded;

        private bool _showDebug = false;
        private bool _showPerspective = true;

        private void OnEnable()
        {
            _stats = serializedObject.FindProperty("Stats");
            _rb2 = serializedObject.FindProperty("Rb2");
            _inputObserverChannel = serializedObject.FindProperty("InputObserverChannel");
            _directionMode = serializedObject.FindProperty("DirectionMode");
            _moveAbility = serializedObject.FindProperty("MoveAbility");

            _perspectiveMode = serializedObject.FindProperty("PerspectiveMode");
            _perspectiveQuaternion2D = serializedObject.FindProperty("PerspectiveQuaternion2D");

            _currentDirection = serializedObject.FindProperty("CurrentDirection");
            _lastFacingDirection = serializedObject.FindProperty("LastFacingDirection");
            _isGrounded = serializedObject.FindProperty("IsGrounded");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Script Field
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((UnityEngine.MonoBehaviour)target), typeof(MonoScript), false);
            GUI.enabled = true;

            // Stats & Rb2
            GUI.enabled = false;
            EditorGUILayout.PropertyField(_stats);
            EditorGUILayout.PropertyField(_rb2);
            GUI.enabled = true;

            EditorGUILayout.Space();

            // --- Assign Section ---
            // ให้ Unity วาด Header("Assign") จากตัวแปลต้นทางอัตโนมัติ ไม่ต้องเขียน LabelField ซ้ำแล้ว
            if (_inputObserverChannel.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Input Observer Channel is required!", MessageType.Error);
            }
            EditorGUILayout.PropertyField(_inputObserverChannel);
            EditorGUILayout.PropertyField(_directionMode);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_moveAbility);

            EditorGUILayout.Space();

            // --- Perspective Section ---
            if (!_perspectiveMode.boolValue)
            {
                // ถ้าติ๊กออก (False) โชว์เป็นแค่ Toggle ธรรมดา ซ่อนลูกศรและ Header ทิ้ง
                EditorGUILayout.PropertyField(_perspectiveMode);
            }
            else
            {
                // ถ้าติ๊กถูก (True) แปลงร่างเป็น Foldout Header แบบมี Toggle ซ้อนอยู่
                Rect perspectiveRect = EditorGUILayout.GetControlRect();

                // จัดตำแหน่ง Toggle ให้อยู่พอดีกับระยะ Label
                Rect toggleRect = new(perspectiveRect.x + EditorGUIUtility.labelWidth, perspectiveRect.y, 20f, perspectiveRect.height);

                // ดัก Event คลิกเม้าส์ที่ Checkbox เพื่อป้องกัน Foldout แย่งคลิก
                UnityEngine.Event e = UnityEngine.Event.current;
                if (e.type == UnityEngine.EventType.MouseDown && e.button == 0 && toggleRect.Contains(e.mousePosition))
                {
                    _perspectiveMode.boolValue = !_perspectiveMode.boolValue;
                    e.Use();
                }

                _showPerspective = EditorGUI.Foldout(perspectiveRect, _showPerspective, "Perspective Mode", true, EditorStyles.foldoutHeader);
                EditorGUI.PropertyField(toggleRect, _perspectiveMode, UnityEngine.GUIContent.none);

                // แสดง Property ด้านในเมื่อคลี่ออก
                if (_showPerspective)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_perspectiveQuaternion2D);
                    EditorGUI.indentLevel--;
                }
            }

            // --- Debug Section ---
            EditorGUILayout.Space();
            _showDebug = EditorGUILayout.Foldout(_showDebug, "Debug", true, EditorStyles.foldoutHeader);
            if (_showDebug)
            {
                EditorGUI.indentLevel++;

                GUI.enabled = false;
                EditorGUILayout.PropertyField(_currentDirection);
                EditorGUILayout.PropertyField(_lastFacingDirection);
                EditorGUILayout.PropertyField(_isGrounded);
                GUI.enabled = true;

                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif