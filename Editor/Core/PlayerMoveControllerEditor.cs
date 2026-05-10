#if UNITY_EDITOR

using UnityEditor;
using Genoverrei.Library.Core;

namespace Genoverrei.Library.Editor
{
    [CustomEditor(typeof(MoveController2D))]
    public class MoveController2DEditor : UnityEditor.Editor
    {
        // ── Base (BaseMoveController) ─────────────────────────────────────
        SerializedProperty _stats, _rb2;
        SerializedProperty _inputObserverChannel, _directionMode, _moveAbility;
        SerializedProperty _perspectiveMode, _perspectiveQuaternion2D;
        SerializedProperty _currentDirection, _lastFacingDirection, _isGrounded;

        // ── Roll ─────────────────────────────────────────────────────────
        SerializedProperty _playerObserver, _rollAbility;

        // ── Jump ─────────────────────────────────────────────────────────
        SerializedProperty _enableJump;
        SerializedProperty _groundCheck, _groundRadius, _groundLayer;

        // ── Rig ──────────────────────────────────────────────────────────
        SerializedProperty _rigMode, _mainRenderer, _animatorController, _rigRoot;

        // ── Foldouts ─────────────────────────────────────────────────────
        bool _foldMove = true, _foldJump = true, _foldRoll = true;
        bool _foldRig  = true, _foldDebug = false, _foldPerspective = false;

        // ─────────────────────────────────────────────────────────────────
        void OnEnable()
        {
            _stats                   = serializedObject.FindProperty("Stats");
            _rb2                     = serializedObject.FindProperty("Rb2");
            _inputObserverChannel    = serializedObject.FindProperty("InputObserverChannel");
            _directionMode           = serializedObject.FindProperty("DirectionMode");
            _moveAbility             = serializedObject.FindProperty("MoveAbility");
            _perspectiveMode         = serializedObject.FindProperty("PerspectiveMode");
            _perspectiveQuaternion2D = serializedObject.FindProperty("PerspectiveQuaternion2D");
            _currentDirection        = serializedObject.FindProperty("CurrentDirection");
            _lastFacingDirection     = serializedObject.FindProperty("LastFacingDirection");
            _isGrounded              = serializedObject.FindProperty("IsGrounded");

            _playerObserver = serializedObject.FindProperty("_playerObserver");
            _rollAbility    = serializedObject.FindProperty("_rollAbility");

            _enableJump   = serializedObject.FindProperty("_enableJump");
            _groundCheck  = serializedObject.FindProperty("_groundCheck");
            _groundRadius = serializedObject.FindProperty("_groundRadius");
            _groundLayer  = serializedObject.FindProperty("_groundLayer");

            _rigMode            = serializedObject.FindProperty("_rigMode");
            _mainRenderer       = serializedObject.FindProperty("_mainRenderer");
            _animatorController = serializedObject.FindProperty("_animatorController");
            _rigRoot            = serializedObject.FindProperty("_rigRoot");
        }

        // ─────────────────────────────────────────────────────────────────
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Script field (read-only)
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script",
                MonoScript.FromMonoBehaviour((UnityEngine.MonoBehaviour)target),
                typeof(MonoScript), false);
            GUI.enabled = true;

            EditorGUILayout.Space(4);

            // ── Movement ─────────────────────────────────────────────────
            if (Section(ref _foldMove, "⚙  Movement"))
            {
                if (_inputObserverChannel.objectReferenceValue == null)
                    EditorGUILayout.HelpBox("Input Observer Channel is required!", MessageType.Error);
                EditorGUILayout.PropertyField(_inputObserverChannel);
                EditorGUILayout.PropertyField(_directionMode);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(_moveAbility);
                EditorGUILayout.HelpBox(
                    "Use BasicMoveAbility2D.\n" +
                    "• SideViewMode OFF → top-down (full velocity)\n" +
                    "• SideViewMode ON  → platformer (X-only + coyote/buffer/variable jump)",
                    MessageType.Info);
            }

            // ── Jump ─────────────────────────────────────────────────────
            if (Section(ref _foldJump, "🦘  Jump"))
            {
                EditorGUILayout.PropertyField(_enableJump,
                    new UnityEngine.GUIContent("Enable Jump",
                        "Pushed to BasicMoveAbility2D._enableJump before OnEnter().\n" +
                        "Disable for top-down maps."));

                if (_enableJump.boolValue)
                {
                    EditorGUILayout.Space(2);

                    if (_groundCheck.objectReferenceValue == null)
                        EditorGUILayout.HelpBox(
                            "Assign Ground Check — Empty GO ปลายเท้า", MessageType.Warning);

                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_groundCheck,
                        new UnityEngine.GUIContent("Ground Check Point"));
                    EditorGUILayout.PropertyField(_groundRadius);
                    EditorGUILayout.PropertyField(_groundLayer);
                    EditorGUI.indentLevel--;

                    EditorGUILayout.Space(2);
                    EditorGUILayout.HelpBox(
                        "Jump tuning (Coyote / Buffer / Variable-height) อยู่ใน Move Ability ด้านบน",
                        MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("Jump disabled — ground check ไม่จำเป็น", MessageType.None);
                }
            }

            // ── Roll ─────────────────────────────────────────────────────
            if (Section(ref _foldRoll, "🔄  Roll / Dodge"))
            {
                EditorGUILayout.PropertyField(_playerObserver,
                    new UnityEngine.GUIContent("Player Observer",
                        "PlayerInputObserverSO ที่มี OnRollChannel\n" +
                        "ปล่อยว่างถ้าไม่ต้องการ roll"));
                EditorGUILayout.PropertyField(_rollAbility);
            }

            // ── Rig Mode ─────────────────────────────────────────────────
            if (Section(ref _foldRig, "🎨  Rig Mode"))
            {
                EditorGUILayout.PropertyField(_rigMode);
                EditorGUI.indentLevel++;

                int mode = _rigMode.enumValueIndex;
                if (mode == (int)RigMode2D.Spritesheet)
                {
                    EditorGUILayout.PropertyField(_mainRenderer,
                        new UnityEngine.GUIContent("Main Renderer",
                            "SpriteRenderer เดียวสำหรับ sprite sheet"));
                    EditorGUILayout.PropertyField(_animatorController);
                }
                else
                {
                    EditorGUILayout.PropertyField(_rigRoot,
                        new UnityEngine.GUIContent("Rig Root",
                            "Parent ของ bone ทั้งหมดใน 2D rig"));
                    EditorGUILayout.HelpBox(
                        "RiggedHurtFlash จะ collect SpriteRenderer ทั้งหมดจาก Rig Root อัตโนมัติ",
                        MessageType.Info);
                }
                EditorGUI.indentLevel--;
            }

            // ── Perspective ──────────────────────────────────────────────
            if (Section(ref _foldPerspective, "📐  Perspective"))
            {
                EditorGUILayout.PropertyField(_perspectiveMode);
                if (_perspectiveMode.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_perspectiveQuaternion2D);
                    EditorGUI.indentLevel--;
                }
            }

            // ── Debug ────────────────────────────────────────────────────
            if (Section(ref _foldDebug, "🐛  Debug"))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_stats);
                EditorGUILayout.PropertyField(_rb2);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(_currentDirection);
                EditorGUILayout.PropertyField(_lastFacingDirection);
                EditorGUILayout.PropertyField(_isGrounded);
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }

        // ─────────────────────────────────────────────────────────────────
        static bool Section(ref bool foldout, string label)
        {
            EditorGUILayout.Space(2);
            foldout = EditorGUILayout.Foldout(foldout, label, true, EditorStyles.foldoutHeader);
            return foldout;
        }
    }
}

#endif
