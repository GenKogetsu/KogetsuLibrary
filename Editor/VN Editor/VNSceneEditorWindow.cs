#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Genoverrei.Library.Core;

namespace Genoverrei.Library.Editor
{
    public class VNSceneEditorWindow : EditorWindow
    {
        private VNSceneSO _currentVNScene;
        private SerializedObject _serializedVNScene;
        private SerializedProperty _conversationsProp;

        private Vector2 _scrollPosition;

        [MenuItem("Window/GenoverreiLibrary/VN Scene Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<VNSceneEditorWindow>("VN Scene Editor");
            window.minSize = new Vector2(600, 400);
            window.CheckCurrentSelection(); //
            window.Show();
        }

        public static void OpenVNSceneWindow(VNSceneSO vnScene)
        {
            var window = GetWindow<VNSceneEditorWindow>("VN Scene Editor");
            window.minSize = new Vector2(600, 400);
            window.LoadVNScene(vnScene);
            window.Show();
        }

        private void LoadVNScene(VNSceneSO vnScene)
        {
            if (vnScene == null) return;

            _currentVNScene = vnScene;
            _serializedVNScene = new SerializedObject(_currentVNScene);
            _conversationsProp = _serializedVNScene.FindProperty("Conversations");
            Repaint();
        }

        private void OnEnable()
        {
            // [หัวใจสำคัญ] สมัครรับข้อมูลจาก Global Event ของ Unity
            // ทำให้หน้าต่างรับรู้การเปลี่ยนแปลงเสมอ แม้จะถูกซ่อนอยู่หลังแท็บอื่น!
            Selection.selectionChanged += CheckCurrentSelection;
            EditorApplication.update += ForceUpdateBackground;

            CheckCurrentSelection();
        }

        private void OnDisable()
        {
            // ยกเลิกการรับข้อมูลเมื่อหน้าต่างถูกปิด (ป้องกัน Memory Leak)
            Selection.selectionChanged -= CheckCurrentSelection;
            EditorApplication.update -= ForceUpdateBackground;
        }

        private void CheckCurrentSelection()
        {
            // ถ้าผู้เล่นคลิกเลือก VNVNSceneSO ตัวใหม่ในโปรเจกต์ 
            // ให้หน้าต่างอัปเดตข้อมูลตามทันที
            if (Selection.activeObject is VNSceneSO vnScene)
            {
                if (_currentVNScene != vnScene)
                {
                    LoadVNScene(vnScene);
                }
            }
        }

        private void ForceUpdateBackground()
        {
            // บังคับให้ SerializedObject อัปเดตและวาดหน้าต่างใหม่ตลอดเวลา 
            // ลบล้างระบบพักหน้าจอของ Unity
            if (_serializedVNScene != null)
            {
                _serializedVNScene.UpdateIfRequiredOrScript();
            }
            Repaint();
        }

        private void OnGUI()
        {
            if (_currentVNScene == null || _serializedVNScene == null)
            {
                EditorGUILayout.HelpBox("Please select a VNVNSceneSO in the Project window.", MessageType.Info);
                return;
            }

            _serializedVNScene.Update();

            EditorGUILayout.BeginVertical(EditorStyles.toolbar);
            GUILayout.Label($"Editing VNScene: {_currentVNScene.name}", EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            if (_conversationsProp != null)
            {
                EditorGUILayout.PropertyField(_conversationsProp, new GUIContent("All Conversations"), true);
            }

            EditorGUILayout.EndScrollView();

            _serializedVNScene.ApplyModifiedProperties();
        }
    }
}
#endif