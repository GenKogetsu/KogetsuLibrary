#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Genoverrei.Library.Core;

namespace Genoverrei.Library.Editor
{
    [CustomPropertyDrawer(typeof(VNConversationNode))]
    public class VNConversationNodeDrawer : PropertyDrawer
    {
        internal static readonly Dictionary<string, bool> _previewStates = new();
        internal static readonly Dictionary<string, Color> _selectedColors = new();
        private static readonly Dictionary<string, int> _arraySizes = new();

        internal static readonly Color _enterColor = new(0.6f, 0.85f, 0.65f);
        internal static readonly Color _mainColor = new(0.55f, 0.75f, 0.95f);
        internal static readonly Color _exitColor = new(0.75f, 0.6f, 0.9f);

        internal static readonly Color _questionStateColor = new(1f, 0.85f, 0.6f);
        internal static readonly Color _answerStateColor = new(0.6f, 0.85f, 1f);
        internal static readonly Color _interactStateColor = new(0.9f, 0.7f, 1f);

        public static bool UseTint
        {
            get => EditorPrefs.GetBool("VNDrawer_UseTint", true);
            set => EditorPrefs.SetBool("VNDrawer_UseTint", value);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.contentColor = Color.white;
            CheckAndClearIfNewlyAdded(property);

            EditorGUI.BeginProperty(position, label, property);
            var index = GetIndex(property.propertyPath);
            float currentY = position.y;

            if (index == 1)
            {
                UseTint = EditorGUI.ToggleLeft(new Rect(position.x, currentY, 250, 20), "Enable Tinting", UseTint, EditorStyles.boldLabel);
                currentY += 24f;
            }

            float buttonWidth = 70f;
            float buttonSpacing = 10f;

            var foldoutRect = new Rect(position.x + 4f, currentY + 3f, position.width - (buttonWidth * 2) - buttonSpacing - 14f, EditorGUIUtility.singleLineHeight);
            var clearAllBtnRect = new Rect(position.x + position.width - (buttonWidth * 2) - buttonSpacing - 5f, currentY + 3f, buttonWidth, EditorGUIUtility.singleLineHeight);
            var recallAllBtnRect = new Rect(position.x + position.width - buttonWidth, currentY + 3f, buttonWidth, EditorGUIUtility.singleLineHeight);

            var typeProp = property.FindPropertyRelative("ConversationMode");

            if (typeProp == null)
            {
                EditorGUI.LabelField(new Rect(position.x, currentY, position.width, 20), "Error: Property 'ConversationMode' not found!");
                EditorGUI.EndProperty();
                return;
            }

            string typeName = typeProp.enumValueIndex == (int)VNConversationMode.ChoiceMode ? "Choice" : "Dialogue";
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, $"Conversation {index} [{typeName}]", true);

            if (GUI.Button(clearAllBtnRect, "Clear All", EditorStyles.miniButtonLeft))
            {
                VNCacheHelper.CacheNode(property);
                ResetNodeData(property);
                GUIUtility.keyboardControl = 0;
            }

            GUI.enabled = VNCacheHelper.NodeCache.ContainsKey(property.propertyPath);
            if (GUI.Button(recallAllBtnRect, "Recall All", EditorStyles.miniButtonRight))
            {
                VNCacheHelper.RecallNode(property);
                GUIUtility.keyboardControl = 0;
            }
            GUI.enabled = true;

            if (!property.isExpanded)
            {
                EditorGUI.EndProperty();
                return;
            }

            EditorGUI.indentLevel++;
            currentY += EditorGUIUtility.singleLineHeight + 7f;

            EditorGUI.PropertyField(new Rect(position.x, currentY, position.width, EditorGUIUtility.singleLineHeight), typeProp);
            currentY += EditorGUIUtility.singleLineHeight + 6f;

            if (typeProp.enumValueIndex == (int)VNConversationMode.DialogueMode)
            {
                var dialogueProp = property.FindPropertyRelative("DialogueNode");
                DrawPhase(ref currentY, position, dialogueProp.FindPropertyRelative("UseEnterPhase"), dialogueProp.FindPropertyRelative("EnterPhase"), "Enter Phase Effect", false, _enterColor);
                DrawPhase(ref currentY, position, null, dialogueProp.FindPropertyRelative("MainPhase"), "Main Phase", true, _mainColor);
                DrawPhase(ref currentY, position, dialogueProp.FindPropertyRelative("UseExitPhase"), dialogueProp.FindPropertyRelative("ExitPhase"), "Exit Phase Effect", false, _exitColor);
            }
            else if (typeProp.enumValueIndex == (int)VNConversationMode.ChoiceMode)
            {
                DrawChoiceNode(ref currentY, position, property.FindPropertyRelative("ChoiceNode"));
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }

        private void DrawChoiceNode(ref float currentY, Rect pos, SerializedProperty choiceProp)
        {
            var questionState = choiceProp.FindPropertyRelative("QuestionState");
            var answerState = choiceProp.FindPropertyRelative("AnswerState");
            var interactStates = choiceProp.FindPropertyRelative("InteractStates");

            GUIStyle stateHeaderStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 };
            Color prevContentColor = GUI.contentColor;

            GUI.contentColor = _questionStateColor;
            EditorGUI.LabelField(new Rect(pos.x, currentY, pos.width, EditorGUIUtility.singleLineHeight), "Question State", stateHeaderStyle);
            GUI.contentColor = prevContentColor;
            currentY += EditorGUIUtility.singleLineHeight + 4f;
            DrawPhase(ref currentY, pos, questionState.FindPropertyRelative("UseEnterPhase"), questionState.FindPropertyRelative("EnterPhase"), "Question Enter Phase", false, _enterColor);
            DrawPhase(ref currentY, pos, null, questionState.FindPropertyRelative("MainPhase"), "Question Main Phase", true, _mainColor);

            currentY += 8f;
            GUI.contentColor = _answerStateColor;
            EditorGUI.LabelField(new Rect(pos.x, currentY, pos.width, EditorGUIUtility.singleLineHeight), "Answer State", stateHeaderStyle);
            GUI.contentColor = prevContentColor;
            currentY += EditorGUIUtility.singleLineHeight + 4f;
            DrawPhase(ref currentY, pos, answerState.FindPropertyRelative("UseEnterPhase"), answerState.FindPropertyRelative("EnterPhase"), "Answer Enter Phase", false, _enterColor);
            DrawPhase(ref currentY, pos, null, answerState.FindPropertyRelative("MainPhase"), "Answer Main Phase", true, _mainColor);
            DrawArrayWithCache(ref currentY, pos, answerState.FindPropertyRelative("Choices"), "Choices List", "Choice");
            DrawPhase(ref currentY, pos, answerState.FindPropertyRelative("UseExitPhase"), answerState.FindPropertyRelative("ExitPhase"), "Answer Exit Phase", false, _exitColor);

            currentY += 8f;
            GUI.contentColor = _interactStateColor;
            EditorGUI.LabelField(new Rect(pos.x, currentY, pos.width, EditorGUIUtility.singleLineHeight), "Interact State", stateHeaderStyle);
            GUI.contentColor = prevContentColor;
            currentY += EditorGUIUtility.singleLineHeight + 4f;
            DrawArrayWithCache(ref currentY, pos, interactStates, "Interact States List", "Interact");
        }

        public static void DrawArrayWithCache(ref float currentY, Rect pos, SerializedProperty arrayProp, string label, string elementPrefix = null)
        {
            if (arrayProp == null) return;

            float listHeight = EditorGUI.GetPropertyHeight(arrayProp, true);

            // เลื่อนกรอบของ Array ไปทางขวาเพิ่มเป็น 10 หน่วย เพื่อเพิ่ม padding ให้ = และ สามเหลี่ยม
            Rect arrayRect = new Rect(pos.x + 10f, currentY, pos.width - 10f, listHeight);

            // คำนวณความกว้างของข้อความ Label เพื่อให้ปุ่มมาต่อท้ายข้อความเลย ป้องกันการทับตัวเลข Size ทางขวาสุด
            GUIStyle labelStyle = EditorStyles.foldout;
            float foldoutWidth = labelStyle.CalcSize(new GUIContent(label)).x;
            float indentSpace = EditorGUI.indentLevel * 15f;
            float buttonsStartX = pos.x + indentSpace + foldoutWidth + 25f; // เว้นระยะห่างจากชื่อ List เล็กน้อย

            float buttonWidth = 50f;
            float headerHeight = EditorGUIUtility.singleLineHeight;

            var clearRect = new Rect(buttonsStartX, currentY + 1f, buttonWidth, headerHeight - 2f);
            var recallRect = new Rect(clearRect.xMax + 2f, currentY + 1f, buttonWidth, headerHeight - 2f);

            bool canRecall = VNCacheHelper.ArrayCache.ContainsKey(arrayProp.propertyPath);

            // ดักจับ Event เมาส์คลิก "ก่อน" ที่ EditorGUI.PropertyField จะขโมย Event ไป
            Event e = Event.current;
            bool doClear = false;
            bool doRecall = false;

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                if (clearRect.Contains(e.mousePosition))
                {
                    doClear = true;
                    e.Use(); // กิน Event ไว้ไม่ให้กล่อง Array แย่งไป
                }
                else if (canRecall && recallRect.Contains(e.mousePosition))
                {
                    doRecall = true;
                    e.Use(); // กิน Event ไว้ไม่ให้กล่อง Array แย่งไป
                }
            }

            // วาดตัว Array
            EditorGUI.PropertyField(arrayRect, arrayProp, new GUIContent(label), true);

            int oldGUIIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // วาดปุ่ม (ใช้ Event ที่เราดักมาสั่งการทำงาน)
            if (GUI.Button(clearRect, "Clear", EditorStyles.miniButtonLeft) || doClear)
            {
                VNCacheHelper.CacheArray(arrayProp);
                arrayProp.ClearArray(); // ล้างข้อมูลภายใน
                arrayProp.arraySize = 0; // ลบขนาดสมาชิกให้เป็น 0
                arrayProp.serializedObject.ApplyModifiedProperties();
                GUIUtility.keyboardControl = 0;
            }

            GUI.enabled = canRecall;
            if (GUI.Button(recallRect, "Recall", EditorStyles.miniButtonRight) || doRecall)
            {
                VNCacheHelper.RecallArray(arrayProp);
                arrayProp.serializedObject.ApplyModifiedProperties();
                GUIUtility.keyboardControl = 0;
            }
            GUI.enabled = true;
            EditorGUI.indentLevel = oldGUIIndent;

            currentY += listHeight + 4f;
        }

        internal static void DrawPhase(ref float currentY, Rect pos, SerializedProperty toggleProp, SerializedProperty phaseProp, string label, bool isMandatory, Color phaseColor)
        {
            if (phaseProp == null) return;

            Color originalColor = GUI.contentColor;
            if (UseTint) GUI.contentColor = phaseColor;

            GUIStyle headerStyle = new(EditorStyles.boldLabel) { fontSize = 13 };
            bool isActive = isMandatory || (toggleProp != null && toggleProp.boolValue);

            var arrowRect = new Rect(pos.x, currentY, 15f, EditorGUIUtility.singleLineHeight);
            var toggleRect = new Rect(pos.x + 15f, currentY, pos.width - 65f, EditorGUIUtility.singleLineHeight);
            var phaseClearBtnRect = new Rect(pos.x + pos.width - 45f, currentY, 45f, EditorGUIUtility.singleLineHeight);

            if (isActive)
            {
                phaseProp.isExpanded = EditorGUI.Foldout(arrowRect, phaseProp.isExpanded, GUIContent.none, true);
            }
            else
            {
                phaseProp.isExpanded = false;
            }

            if (isMandatory)
            {
                GUI.enabled = false;
                EditorGUI.Toggle(new Rect(toggleRect.x, toggleRect.y, 16f, toggleRect.height), true);
                GUI.enabled = true;
                EditorGUI.LabelField(new Rect(toggleRect.x + 16f, toggleRect.y, toggleRect.width - 16f, toggleRect.height), label, headerStyle);
            }
            else
            {
                if (toggleProp != null)
                {
                    EditorGUI.BeginChangeCheck();
                    toggleProp.boolValue = EditorGUI.ToggleLeft(toggleRect, label, toggleProp.boolValue, headerStyle);
                    if (EditorGUI.EndChangeCheck()) GUIUtility.keyboardControl = 0;
                }
            }

            if (GUI.Button(phaseClearBtnRect, "Clear", EditorStyles.miniButton))
            {
                ResetPhaseData(phaseProp);
                if (toggleProp != null) toggleProp.boolValue = false;
                phaseProp.serializedObject.ApplyModifiedProperties();
                GUIUtility.keyboardControl = 0;
            }

            currentY += EditorGUIUtility.singleLineHeight + 4f;

            if (isActive && phaseProp.isExpanded)
            {
                EditorGUI.indentLevel++;
                DrawPhaseContent(ref currentY, pos, phaseProp, isMandatory);
                currentY += 6f;
                EditorGUI.indentLevel--;
            }

            GUI.contentColor = originalColor;
        }

        internal static void DrawPhaseContent(ref float currentY, Rect pos, SerializedProperty phaseProp, bool isMainPhase)
        {
            var dialogueModeProp = phaseProp.FindPropertyRelative("DialogueMode");
            bool isNoneMode = dialogueModeProp != null && dialogueModeProp.enumValueIndex == (int)VNDialogueMode.None;

            var endProperty = phaseProp.GetEndProperty();
            var child = phaseProp.Copy();

            if (!child.NextVisible(true)) return;

            while (!SerializedProperty.EqualContents(child, endProperty))
            {
                if (isNoneMode && child.name != "DialogueMode" && child.name != "OverrideBmgClip" && child.name != "BmgClip" && child.name != "UseAmbientEvent" && child.name != "AmbientEventName" && child.name != "AmbientEventType")
                {
                    if (!child.NextVisible(false)) break;
                    continue;
                }

                if (child.name == "Speakers")
                {
                    DrawArrayWithCache(ref currentY, pos, child, "Speakers List", "Speaker");
                }
                else if (child.name == "DialogueText")
                {
                    float indentOffset = EditorGUI.indentLevel * 15f;
                    float headerHeight = EditorGUIUtility.singleLineHeight;

                    EditorGUI.LabelField(new Rect(pos.x + indentOffset, currentY, pos.width - indentOffset, headerHeight), "Dialogue Text", EditorStyles.boldLabel);

                    DrawRichTextEditorControls(ref currentY, pos, child, phaseProp, false);
                }
                else if (child.name == "UseDialogueText")
                {
                    // ข้าม
                }
                else if (child.name == "OverrideDialogueAnimation" || child.name == "UseAmbientEvent" || child.name == "UseVoiceoverClip" || child.name == "OverrideBmgClip")
                {
                    float indentOffset = EditorGUI.indentLevel * 15f;
                    float labelWidth = 210f;

                    var toggleRect = new Rect(pos.x + indentOffset, currentY, labelWidth, EditorGUIUtility.singleLineHeight);

                    string lblText = child.name == "OverrideDialogueAnimation" ? "Override Dialogue Animation" :
                                     child.name == "UseAmbientEvent" ? "Use Ambient Event" :
                                     child.name == "UseVoiceoverClip" ? "Use Voiceover Clip" : "Override Bmg Clip";

                    child.boolValue = EditorGUI.ToggleLeft(toggleRect, lblText, child.boolValue);

                    if (child.boolValue)
                    {
                        string clipName = child.name == "OverrideDialogueAnimation" ? "DialogueBoxAnimation" :
                                          child.name == "UseAmbientEvent" ? "AmbientEventType" :
                                          child.name == "UseVoiceoverClip" ? "VoiceoverClip" : "BmgClip";

                        var clipProp = phaseProp.FindPropertyRelative(clipName);

                        if (clipProp == null && child.name == "UseAmbientEvent")
                        {
                            clipProp = phaseProp.FindPropertyRelative("AmbientEventName");
                        }

                        if (clipProp != null)
                        {
                            float fieldStartX = toggleRect.xMax + 5f;
                            float fieldWidth = pos.width - (fieldStartX - pos.x) - 10f;

                            int oldIndent = EditorGUI.indentLevel;
                            EditorGUI.indentLevel = 0;

                            EditorGUI.PropertyField(new Rect(fieldStartX, currentY, fieldWidth, EditorGUIUtility.singleLineHeight), clipProp, GUIContent.none);

                            EditorGUI.indentLevel = oldIndent;
                        }
                    }

                    currentY += EditorGUIUtility.singleLineHeight + 2f;
                }
                else if (child.name == "DialogueBoxAnimation" || child.name == "AmbientEventName" || child.name == "AmbientEventType" || child.name == "VoiceoverClip" || child.name == "BmgClip")
                {
                    // ข้ามการวาดบรรทัดใหม่
                }
                else if (child.name == "TextSettings")
                {
                    int oldIndent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 0;

                    float baseStartX = pos.x + (oldIndent * 15f);
                    var overrideProp = child.FindPropertyRelative("OverrideGlobalSettings");

                    float preciseHeight = EditorGUIUtility.singleLineHeight;

                    var arrowRect = new Rect(baseStartX, currentY, 15f, preciseHeight);
                    var toggleRect = new Rect(baseStartX + 15f, currentY, 16f, preciseHeight);
                    var labelRect = new Rect(toggleRect.xMax, currentY, 140f, preciseHeight);

                    int oldDepth = GUI.depth;
                    GUI.depth = -10;

                    if (overrideProp != null)
                    {
                        if (overrideProp.boolValue)
                        {
                            child.isExpanded = EditorGUI.Foldout(arrowRect, child.isExpanded, GUIContent.none, true);
                        }
                        else
                        {
                            child.isExpanded = false;
                        }

                        overrideProp.boolValue = EditorGUI.Toggle(toggleRect, overrideProp.boolValue);

                        if (GUI.Button(labelRect, GUIContent.none, GUIStyle.none))
                        {
                            overrideProp.boolValue = !overrideProp.boolValue;
                            GUIUtility.keyboardControl = 0;
                        }
                        EditorGUI.LabelField(labelRect, "Override Text Settings");
                    }
                    else
                    {
                        child.isExpanded = EditorGUI.Foldout(new Rect(baseStartX, currentY, pos.width, preciseHeight), child.isExpanded, "Text Settings", true);
                    }

                    GUI.depth = oldDepth;
                    EditorGUI.indentLevel = oldIndent;
                    currentY += EditorGUIUtility.singleLineHeight + 4f;

                    if ((overrideProp == null || overrideProp.boolValue) && child.isExpanded)
                    {
                        EditorGUI.indentLevel++;

                        var subChild = child.Copy();
                        var textSettingsEndProp = subChild.GetEndProperty();

                        if (subChild.NextVisible(true))
                        {
                            do
                            {
                                if (SerializedProperty.EqualContents(subChild, textSettingsEndProp)) break;
                                if (subChild.name == "OverrideGlobalSettings") continue;

                                float h = EditorGUI.GetPropertyHeight(subChild, true);
                                EditorGUI.PropertyField(new Rect(pos.x, currentY, pos.width, h), subChild, true);
                                currentY += h + 2f;

                            } while (subChild.NextVisible(false));
                        }

                        EditorGUI.indentLevel--;
                    }
                }
                else
                {
                    var h = EditorGUI.GetPropertyHeight(child, true);
                    EditorGUI.PropertyField(new Rect(pos.x, currentY, pos.width, h), child, true);
                    currentY += h + 2f;
                }

                if (!child.NextVisible(false)) break;
            }
        }

        internal static void DrawRichTextEditorControls(ref float currentY, Rect pos, SerializedProperty property, SerializedProperty phaseProp, bool hasCheckbox)
        {
            float buttonWidth = 75f;
            float buttonSpacing = 10f;
            float headerHeight = EditorGUIUtility.singleLineHeight;

            int oldGUIIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var clearRect = new Rect(pos.x + pos.width - (buttonWidth * 2) - buttonSpacing, currentY, buttonWidth, headerHeight - 2f);
            var recallRect = new Rect(pos.x + pos.width - buttonWidth, currentY, buttonWidth, headerHeight - 2f);

            if (GUI.Button(clearRect, "Clear Text", EditorStyles.miniButtonLeft))
            {
                VNCacheHelper.TextCache[property.propertyPath] = property.stringValue;
                property.stringValue = "";
                GUIUtility.keyboardControl = 0;
            }

            GUI.enabled = VNCacheHelper.TextCache.ContainsKey(property.propertyPath);
            if (GUI.Button(recallRect, "Recall Text", EditorStyles.miniButtonRight))
            {
                property.stringValue = VNCacheHelper.TextCache[property.propertyPath];
                GUIUtility.keyboardControl = 0;
            }
            GUI.enabled = true;
            EditorGUI.indentLevel = oldGUIIndent;

            currentY += headerHeight + 6f;

            var toolbarRect = new Rect(pos.x, currentY, pos.width, EditorGUIUtility.singleLineHeight);
            DrawToolbar(toolbarRect, property);
            currentY += EditorGUIUtility.singleLineHeight + 4f;

            var textAreaHeight = 90f;
            var textAreaRect = new Rect(pos.x, currentY, pos.width, textAreaHeight);
            string controlName = "RT_" + property.propertyPath;

            var e = Event.current;
            if (e.type == EventType.ContextClick && textAreaRect.Contains(e.mousePosition))
            {
                var editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                int min = 0, max = 0;
                string currentText = property.stringValue ?? "";

                if (editor != null && GUI.GetNameOfFocusedControl() == controlName)
                {
                    min = Mathf.Clamp(Mathf.Min(editor.cursorIndex, editor.selectIndex), 0, currentText.Length);
                    max = Mathf.Clamp(Mathf.Max(editor.cursorIndex, editor.selectIndex), 0, currentText.Length);
                }

                bool hasSelection = max > min;
                GenericMenu menu = new();
                string path = property.propertyPath;
                SerializedObject so = property.serializedObject;

                menu.AddItem(new GUIContent("Format/Bold <b>"), false, () => ApplyMenuFormat(so, path, min, max, "b", "b", editor));
                menu.AddItem(new GUIContent("Format/Italic <i>"), false, () => ApplyMenuFormat(so, path, min, max, "i", "i", editor));
                menu.AddItem(new GUIContent("Format/Underline <u>"), false, () => ApplyMenuFormat(so, path, min, max, "u", "u", editor));

                var hex = ColorUtility.ToHtmlStringRGB(GetColorState(path));
                menu.AddItem(new GUIContent($"Format/Color <color=#{hex}>"), false, () => ApplyMenuFormat(so, path, min, max, $"color=#{hex}", "color", editor));

                menu.AddSeparator("");

                if (hasSelection)
                {
                    menu.AddItem(new GUIContent("Cut"), false, () =>
                    {
                        EditorGUIUtility.systemCopyBuffer = currentText[min..max];
                        ApplyMenuFormat(so, path, min, max, "", "", editor, true);
                    });
                    menu.AddItem(new GUIContent("Copy"), false, () =>
                    {
                        EditorGUIUtility.systemCopyBuffer = currentText[min..max];
                    });
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent("Cut"));
                    menu.AddDisabledItem(new GUIContent("Copy"));
                }

                menu.AddItem(new GUIContent("Paste"), false, () =>
                {
                    string clipboard = EditorGUIUtility.systemCopyBuffer;
                    if (!string.IsNullOrEmpty(clipboard))
                    {
                        so.Update();
                        var prop = so.FindProperty(path);
                        string t = prop.stringValue ?? "";
                        prop.stringValue = $"{t[..Mathf.Clamp(min, 0, t.Length)]}{clipboard}{t[Mathf.Clamp(max, 0, t.Length)..]}";
                        so.ApplyModifiedProperties();

                        if (editor != null)
                        {
                            editor.cursorIndex = min + clipboard.Length;
                            editor.selectIndex = editor.cursorIndex;
                        }
                        EditorApplication.delayCall += () => UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                    }
                });

                menu.ShowAsContext();
                e.Use();
            }

            GUI.SetNextControlName(controlName);
            Color prevColor = GUI.contentColor;
            GUI.contentColor = Color.white;

            property.stringValue = EditorGUI.TextArea(textAreaRect, property.stringValue, EditorStyles.textArea);

            GUI.contentColor = prevColor;
            currentY += textAreaHeight + 2f;

            if (GetPreviewState(property.propertyPath))
            {
                GUI.contentColor = Color.white;
                var previewStyle = new GUIStyle(EditorStyles.label) { richText = true, wordWrap = true, fontSize = 12, padding = new RectOffset(5, 5, 5, 5) };

                var textSettings = phaseProp.FindPropertyRelative("TextSettings");
                if (textSettings != null)
                {
                    var overrideProp = textSettings.FindPropertyRelative("OverrideGlobalSettings");
                    if (overrideProp != null && overrideProp.boolValue)
                    {
                        var alignProp = textSettings.FindPropertyRelative("Alignment");
                        if (alignProp != null)
                        {
                            int enumIdx = alignProp.enumValueIndex;
                            if (enumIdx >= 0 && enumIdx < alignProp.enumNames.Length)
                            {
                                string alignName = alignProp.enumNames[enumIdx];
                                previewStyle.alignment = GetAnchorFromTMProAlignment(alignName);
                            }
                        }
                    }
                }

                var pHeight = Mathf.Max(40f, previewStyle.CalcHeight(new GUIContent(property.stringValue), pos.width));
                GUI.Box(new Rect(pos.x, currentY, pos.width, pHeight + 4f), GUIContent.none, EditorStyles.helpBox);
                EditorGUI.LabelField(new Rect(pos.x + 4f, currentY + 2f, pos.width - 8f, pHeight), property.stringValue, previewStyle);
                currentY += pHeight + 8f;

                GUI.contentColor = prevColor;
            }
        }

        internal static TextAnchor GetAnchorFromTMProAlignment(string alignName)
        {
            if (string.IsNullOrEmpty(alignName)) return TextAnchor.UpperLeft;

            bool isRight = alignName.Contains("Right");
            bool isCenter = alignName.Contains("Center") || alignName.Contains("Mid") || alignName.Contains("Geo") || alignName.Contains("Justified") || alignName.Contains("Flush") || alignName == "Top" || alignName == "Bottom" || alignName == "Baseline" || alignName == "Capline";
            bool isBottom = alignName.Contains("Bottom");
            bool isMiddle = alignName.Contains("Midline") || alignName.Contains("Baseline") || alignName.Contains("Capline");

            if (isBottom) { if (isRight) return TextAnchor.LowerRight; if (isCenter) return TextAnchor.LowerCenter; return TextAnchor.LowerLeft; }
            if (isMiddle) { if (isRight) return TextAnchor.MiddleRight; if (isCenter) return TextAnchor.MiddleCenter; return TextAnchor.MiddleLeft; }
            if (isRight) return TextAnchor.UpperRight; if (isCenter) return TextAnchor.UpperCenter; return TextAnchor.UpperLeft;
        }

        internal static void DrawToolbar(Rect pos, SerializedProperty property)
        {
            var x = pos.x;
            string controlName = "RT_" + property.propertyPath;
            var editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);

            int min = 0, max = 0;
            if (editor != null && GUI.GetNameOfFocusedControl() == controlName)
            {
                min = Mathf.Clamp(Mathf.Min(editor.cursorIndex, editor.selectIndex), 0, property.stringValue.Length);
                max = Mathf.Clamp(Mathf.Max(editor.cursorIndex, editor.selectIndex), 0, property.stringValue.Length);
            }

            if (DrawToolButton(new Rect(x, pos.y, 25, 20), "B", EditorStyles.miniButtonLeft)) ApplyMenuFormat(property.serializedObject, property.propertyPath, min, max, "b", "b", editor);
            x += 25;
            if (DrawToolButton(new Rect(x, pos.y, 25, 20), "I", EditorStyles.miniButtonMid)) ApplyMenuFormat(property.serializedObject, property.propertyPath, min, max, "i", "i", editor);
            x += 25;
            if (DrawToolButton(new Rect(x, pos.y, 25, 20), "U", EditorStyles.miniButtonRight)) ApplyMenuFormat(property.serializedObject, property.propertyPath, min, max, "u", "u", editor);
            x += 30;

            var currentColor = GetColorState(property.propertyPath);
            var newColor = EditorGUI.ColorField(new Rect(x, pos.y, 50, 20), GUIContent.none, currentColor, false, false, false);
            if (newColor != currentColor) SetColorState(property.propertyPath, newColor);
            x += 52;

            if (DrawToolButton(new Rect(x, pos.y, 50, 20), "Color", EditorStyles.miniButton))
            {
                var hex = ColorUtility.ToHtmlStringRGB(newColor);
                ApplyMenuFormat(property.serializedObject, property.propertyPath, min, max, $"color=#{hex}", "color", editor);
            }

            float previewWidth = 40f; // ขยายขนาดปุ่มเพื่อรองรับข้อความ "View"
            var previewRect = new Rect(pos.x + pos.width - previewWidth, pos.y, previewWidth, 20);

            bool currentPreviewState = GetPreviewState(property.propertyPath);
            bool newPreviewState = GUI.Toggle(previewRect, currentPreviewState, "View", EditorStyles.miniButton); // ลบ Emoji ออก

            if (newPreviewState != currentPreviewState)
            {
                SetPreviewState(property.propertyPath, newPreviewState);
            }
        }

        internal static bool DrawToolButton(Rect rect, string text, GUIStyle style)
        {
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition)) { Event.current.Use(); return true; }
            if (Event.current.type == EventType.Repaint) style.Draw(rect, new GUIContent(text), false, false, false, false);
            return false;
        }

        internal static void ApplyMenuFormat(SerializedObject so, string path, int min, int max, string startTag, string endTag, TextEditor editor, bool isCut = false)
        {
            so.Update();
            var prop = so.FindProperty(path);
            if (prop == null) return;

            string t = prop.stringValue ?? "";
            min = Mathf.Clamp(min, 0, t.Length);
            max = Mathf.Clamp(max, 0, t.Length);

            if (isCut)
            {
                prop.stringValue = t.Remove(min, max - min);
                if (editor != null) { editor.cursorIndex = min; editor.selectIndex = min; }
            }
            else
            {
                if (max > min)
                {
                    prop.stringValue = t.Insert(max, $"</{endTag}>").Insert(min, $"<{startTag}>");
                    if (editor != null)
                    {
                        int newPos = max + startTag.Length + 2;
                        editor.cursorIndex = newPos;
                        editor.selectIndex = newPos;
                    }
                }
                else
                {
                    prop.stringValue = t.Insert(min, $"<{startTag}></{endTag}>");
                    if (editor != null)
                    {
                        int startTagFullLength = startTag.Length + 2;
                        int newPos = min + startTagFullLength;

                        editor.cursorIndex = newPos;
                        editor.selectIndex = newPos;
                    }
                }
            }

            so.ApplyModifiedProperties();
            GUIUtility.keyboardControl = 0;
            EditorApplication.delayCall += () => UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }

        private void CheckAndClearIfNewlyAdded(SerializedProperty property)
        {
            string path = property.propertyPath;
            int lastBracket = path.LastIndexOf('[');
            if (lastBracket < 0) return;

            string arrayPath = path[..lastBracket];
            var arrayProp = property.serializedObject.FindProperty(arrayPath);

            if (arrayProp != null && arrayProp.isArray)
            {
                int currentSize = arrayProp.arraySize;
                int currentIndex = GetIndex(path) - 1;

                if (_arraySizes.TryGetValue(arrayPath, out int prevSize))
                {
                    if (currentSize > prevSize && currentIndex == currentSize - 1)
                    {
                        ResetNodeData(property);
                    }
                }

                if (currentIndex == currentSize - 1)
                {
                    _arraySizes[arrayPath] = currentSize;
                }
            }
        }

        private void ResetNodeData(SerializedProperty nodeProp)
        {
            var dialogueProp = nodeProp.FindPropertyRelative("DialogueNode");
            if (dialogueProp != null)
            {
                dialogueProp.FindPropertyRelative("UseEnterPhase").boolValue = false;
                dialogueProp.FindPropertyRelative("UseExitPhase").boolValue = false;
                ResetPhaseData(dialogueProp.FindPropertyRelative("EnterPhase"));
                ResetPhaseData(dialogueProp.FindPropertyRelative("MainPhase"));
                ResetPhaseData(dialogueProp.FindPropertyRelative("ExitPhase"));
            }

            var choiceProp = nodeProp.FindPropertyRelative("ChoiceNode");
            if (choiceProp != null)
            {
                var qState = choiceProp.FindPropertyRelative("QuestionState");
                qState.FindPropertyRelative("UseEnterPhase").boolValue = false;
                ResetPhaseData(qState.FindPropertyRelative("EnterPhase"));
                ResetPhaseData(qState.FindPropertyRelative("MainPhase"));

                var aState = choiceProp.FindPropertyRelative("AnswerState");
                aState.FindPropertyRelative("UseEnterPhase").boolValue = false;
                aState.FindPropertyRelative("UseExitPhase").boolValue = false;
                ResetPhaseData(aState.FindPropertyRelative("EnterPhase"));
                ResetPhaseData(aState.FindPropertyRelative("MainPhase"));
                ResetPhaseData(aState.FindPropertyRelative("ExitPhase"));

                var choicesProp = aState.FindPropertyRelative("Choices");
                if (choicesProp != null)
                {
                    choicesProp.ClearArray();
                    choicesProp.arraySize = 0;
                }

                var interactStatesProp = choiceProp.FindPropertyRelative("InteractStates");
                if (interactStatesProp != null)
                {
                    interactStatesProp.ClearArray();
                    interactStatesProp.arraySize = 0;
                }
            }

            nodeProp.serializedObject.ApplyModifiedProperties();
        }

        internal static void ResetPhaseData(SerializedProperty phaseProp)
        {
            if (phaseProp == null) return;

            var dialogueTextProp = phaseProp.FindPropertyRelative("DialogueText");
            if (dialogueTextProp != null) dialogueTextProp.stringValue = "";

            var pOverrideDialogueAnim = phaseProp.FindPropertyRelative("OverrideDialogueAnimation");
            if (pOverrideDialogueAnim != null) pOverrideDialogueAnim.boolValue = false;

            var pDialogueBoxAnim = phaseProp.FindPropertyRelative("DialogueBoxAnimation");
            if (pDialogueBoxAnim != null) pDialogueBoxAnim.objectReferenceValue = null;

            var pAmbient = phaseProp.FindPropertyRelative("UseAmbientEvent");
            if (pAmbient != null) pAmbient.boolValue = false;

            var pAmbientName = phaseProp.FindPropertyRelative("AmbientEventName");
            if (pAmbientName != null) pAmbientName.stringValue = "";

            var pAmbientType = phaseProp.FindPropertyRelative("AmbientEventType");
            if (pAmbientType != null) pAmbientType.enumValueIndex = 0;

            var pVoice = phaseProp.FindPropertyRelative("UseVoiceoverClip");
            if (pVoice != null) pVoice.boolValue = false;

            var pVoiceClip = phaseProp.FindPropertyRelative("VoiceoverClip");
            if (pVoiceClip != null) pVoiceClip.objectReferenceValue = null;

            var pBmg = phaseProp.FindPropertyRelative("OverrideBmgClip");
            if (pBmg != null) pBmg.boolValue = false;

            var pBmgClip = phaseProp.FindPropertyRelative("BmgClip");
            if (pBmgClip != null) pBmgClip.objectReferenceValue = null;

            var textSettings = phaseProp.FindPropertyRelative("TextSettings");
            if (textSettings != null)
            {
                var pOverride = textSettings.FindPropertyRelative("OverrideGlobalSettings");
                if (pOverride != null) pOverride.boolValue = false;

                var pFont = textSettings.FindPropertyRelative("CustomFont");
                if (pFont != null) pFont.objectReferenceValue = null;

                var pSize = textSettings.FindPropertyRelative("FontSize");
                if (pSize != null) pSize.floatValue = 36f;

                var pColor = textSettings.FindPropertyRelative("FontColor");
                if (pColor != null) pColor.colorValue = Color.white;

                var pAlign = textSettings.FindPropertyRelative("Alignment");
                if (pAlign != null) pAlign.enumValueIndex = 0;

                var pSpeed = textSettings.FindPropertyRelative("TypingSpeed");
                if (pSpeed != null) pSpeed.floatValue = 30f;
            }

            var speakersProp = phaseProp.FindPropertyRelative("Speakers");
            if (speakersProp != null)
            {
                speakersProp.ClearArray();
                speakersProp.arraySize = 0;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var h = EditorGUIUtility.singleLineHeight + 6f;
            var index = GetIndex(property.propertyPath);

            if (index == 1) h += 24f;

            if (!property.isExpanded) return h;

            h += 4f;

            var typeProp = property.FindPropertyRelative("ConversationMode");
            if (typeProp == null) return h;

            h += EditorGUIUtility.singleLineHeight + 6f;

            if (typeProp.enumValueIndex == (int)VNConversationMode.DialogueMode)
            {
                var dialogueProp = property.FindPropertyRelative("DialogueNode");
                h += GetPhaseHeight(dialogueProp.FindPropertyRelative("UseEnterPhase"), dialogueProp.FindPropertyRelative("EnterPhase"), false);
                h += GetPhaseHeight(null, dialogueProp.FindPropertyRelative("MainPhase"), true);
                h += GetPhaseHeight(dialogueProp.FindPropertyRelative("UseExitPhase"), dialogueProp.FindPropertyRelative("ExitPhase"), false);
            }
            else if (typeProp.enumValueIndex == (int)VNConversationMode.ChoiceMode)
            {
                var choiceProp = property.FindPropertyRelative("ChoiceNode");

                var qState = choiceProp.FindPropertyRelative("QuestionState");
                h += EditorGUIUtility.singleLineHeight + 4f;
                h += GetPhaseHeight(qState.FindPropertyRelative("UseEnterPhase"), qState.FindPropertyRelative("EnterPhase"), false);
                h += GetPhaseHeight(null, qState.FindPropertyRelative("MainPhase"), true);

                var aState = choiceProp.FindPropertyRelative("AnswerState");
                h += EditorGUIUtility.singleLineHeight + 12f;
                h += GetPhaseHeight(aState.FindPropertyRelative("UseEnterPhase"), aState.FindPropertyRelative("EnterPhase"), false);
                h += GetPhaseHeight(null, aState.FindPropertyRelative("MainPhase"), true);
                h += GetArrayWithCacheHeight(aState.FindPropertyRelative("Choices"));
                h += GetPhaseHeight(aState.FindPropertyRelative("UseExitPhase"), aState.FindPropertyRelative("ExitPhase"), false);

                var iStates = choiceProp.FindPropertyRelative("InteractStates");
                h += EditorGUIUtility.singleLineHeight + 12f;
                h += GetArrayWithCacheHeight(iStates);
            }

            return h + 6f;
        }

        internal static float GetArrayWithCacheHeight(SerializedProperty arrayProp)
        {
            if (arrayProp == null) return 0f;
            return EditorGUI.GetPropertyHeight(arrayProp, true) + 4f;
        }

        internal static float GetPhaseHeight(SerializedProperty toggle, SerializedProperty phase, bool isMainPhase)
        {
            float h = EditorGUIUtility.singleLineHeight + 4f;
            if (phase != null && phase.isExpanded && (toggle == null || toggle.boolValue))
            {
                h += CalculateActualPhaseHeight(phase, isMainPhase) + 6f;
            }
            return h;
        }

        internal static float CalculateActualPhaseHeight(SerializedProperty phase, bool isMainPhase)
        {
            var dialogueModeProp = phase.FindPropertyRelative("DialogueMode");
            bool isNoneMode = dialogueModeProp != null && dialogueModeProp.enumValueIndex == (int)VNDialogueMode.None;

            float currentHeight = 0f;
            var end = phase.GetEndProperty();
            var child = phase.Copy();
            if (!child.NextVisible(true)) return currentHeight;

            while (!SerializedProperty.EqualContents(child, end))
            {
                if (isNoneMode && child.name != "DialogueMode" && child.name != "OverrideBmgClip" && child.name != "BmgClip" && child.name != "UseAmbientEvent" && child.name != "AmbientEventName" && child.name != "AmbientEventType")
                {
                    if (!child.NextVisible(false)) break;
                    continue;
                }

                if (child.name == "Speakers")
                {
                    currentHeight += GetArrayWithCacheHeight(child);
                }
                else if (child.name == "DialogueText")
                {
                    currentHeight += EditorGUIUtility.singleLineHeight + 2f;
                    currentHeight += 4f + EditorGUIUtility.singleLineHeight + 4f + 90f + 2f;

                    if (GetPreviewState(child.propertyPath))
                    {
                        var previewStyle = new GUIStyle(EditorStyles.label) { richText = true, wordWrap = true, fontSize = 12, padding = new RectOffset(5, 5, 5, 5) };

                        var textSettings = phase.FindPropertyRelative("TextSettings");
                        if (textSettings != null)
                        {
                            var overrideProp = textSettings.FindPropertyRelative("OverrideGlobalSettings");
                            if (overrideProp != null && overrideProp.boolValue)
                            {
                                var alignProp = textSettings.FindPropertyRelative("Alignment");
                                if (alignProp != null)
                                {
                                    int enumIdx = alignProp.enumValueIndex;
                                    if (enumIdx >= 0 && enumIdx < alignProp.enumNames.Length)
                                    {
                                        string alignName = alignProp.enumNames[enumIdx];
                                        previewStyle.alignment = GetAnchorFromTMProAlignment(alignName);
                                    }
                                }
                            }
                        }

                        currentHeight += Mathf.Max(40f, previewStyle.CalcHeight(new GUIContent(child.stringValue), EditorGUIUtility.currentViewWidth - 80)) + 8f;
                    }
                }
                else if (child.name == "UseDialogueText")
                {
                    // ข้าม
                }
                else if (child.name == "OverrideDialogueAnimation" || child.name == "UseAmbientEvent" || child.name == "UseVoiceoverClip" || child.name == "OverrideBmgClip")
                {
                    currentHeight += EditorGUIUtility.singleLineHeight + 2f;
                }
                else if (child.name == "DialogueBoxAnimation" || child.name == "AmbientEventName" || child.name == "AmbientEventType" || child.name == "VoiceoverClip" || child.name == "BmgClip")
                {
                    // ข้าม
                }
                else if (child.name == "TextSettings")
                {
                    currentHeight += EditorGUIUtility.singleLineHeight + 4f;
                    var overrideProp = child.FindPropertyRelative("OverrideGlobalSettings");
                    if ((overrideProp == null || overrideProp.boolValue) && child.isExpanded)
                    {
                        var subChild = child.Copy();
                        var textSettingsEndProp = subChild.GetEndProperty();

                        if (subChild.NextVisible(true))
                        {
                            do
                            {
                                if (SerializedProperty.EqualContents(subChild, textSettingsEndProp)) break;
                                if (subChild.name == "OverrideGlobalSettings") continue;

                                currentHeight += EditorGUI.GetPropertyHeight(subChild, true) + 2f;
                            } while (subChild.NextVisible(false));
                        }
                    }
                }
                else
                {
                    currentHeight += EditorGUI.GetPropertyHeight(child, true) + 2f;
                }
                if (!child.NextVisible(false)) break;
            }
            return currentHeight;
        }

        private int GetIndex(string path) { int s = path.LastIndexOf('[') + 1; var e = path.LastIndexOf(']'); return (s > 0 && e > s && int.TryParse(path[s..e], out var i)) ? i + 1 : 1; }

        internal static bool GetPreviewState(string path)
        {
            if (_previewStates.TryGetValue(path, out bool state)) return state;
            return false;
        }

        internal static void SetPreviewState(string path, bool state)
        {
            _previewStates[path] = state;
        }

        internal static Color GetColorState(string path)
        {
            if (_selectedColors.TryGetValue(path, out Color color)) return color;
            return Color.white;
        }

        internal static void SetColorState(string path, Color color)
        {
            _selectedColors[path] = color;
        }
    }
}
#endif