using TMPro;
using System;
using System.Text;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Genoverrei.Library.DesignPatternCore;

namespace Genoverrei.Library.Core
{
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ตัวควบคุมหลักสำหรับระบบ Visual Novel ทำหน้าที่ประมวลผลโหนดเนื้อเรื่อง, จัดการ UI, ตัวละคร และเหตุการณ์ต่างๆ </para>
    /// <para> (EN) : Main controller for the Visual Novel system. Processes story nodes, manages UI, characters, and events. </para>
    /// </summary>
    public class VNController : MonoBehaviour
    {
        #region Field Region

        [Header("Story Data")]
        [Required]
        [SerializeField] private VNChapterSO _currentChapter;

        [Header("Obsever Channels")]
        [Required]
        [SerializeField] private InputObserverChannelSO _inputObserverChannel;

        [Required]
        [SerializeField] private AudioChannelSO _audioChannel;

        [Required]
        [SerializeField] private AnimationClip _hideDialogueClip, _showDialogueClip;

        [Header("UI References")]
        [SerializeField] private VNDialogueArea _standardDialogueArea;
        [SerializeField] private VNDialogueArea _logViewDialogueArea;
        [SerializeField] private VNDialogueArea _cinematicDialogueArea;

        private readonly Queue<string> _logViewQueue = new();

        [Header("Runtime Data")]
        [ReadOnly]
        [SerializeField] private ushort _currentConversationIndex = 0;
        [SerializeField] private RectTransform _currentDialogueBox;
        [SerializeField] private Animator _currentDialogueAnimator;
        [SerializeField] private TextMeshProUGUI _currentDialogueTMP, _currentSpeakerNameTMP;
        [SerializeField] private Image _currentSpeakerIcon;

        [ReadOnly]
        [SerializeField] private VNDialogueMode _currentDialogueType = VNDialogueMode.None;

        [ReadOnly]
        [SerializeField] private VNCurrentPhase _currentPhase = VNCurrentPhase.None;

        [ReadOnly]
        [SerializeField] private bool _isTyping, _skipTyping, _waitForInput = false;

        [ReadOnly]
        [SerializeField] private bool _waitForChoiceInput = false;

        [ReadOnly]
        [SerializeField] private int _selectedAnswerNumber = 0;

        #endregion //End Field Region

        private void OnEnable()
        {
            _inputObserverChannel.OnInteractionChannel += HandleInput;
        }

        private void OnDisable()
        {
            _inputObserverChannel.OnInteractionChannel -= HandleInput;
        }

        private void Start()
        {
            if (_currentChapter == null) return;

            SetUp();
            StartChapter(_currentChapter);
        }

        #region Helper Functions Region

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ตั้งค่าเริ่มต้นให้กับตัวควบคุมหลัก โดยเชื่อมโยง UI พื้นฐานของโหมด Standard </para>
        /// <para> (EN) : Prepares initial setup, linking default text components for the Standard mode. </para>
        /// </summary>
        private void SetUp()
        {
            if (_standardDialogueArea != null)
            {
                _currentDialogueTMP = _standardDialogueArea.DialogueTMP;
                _currentSpeakerNameTMP = _standardDialogueArea.SpeakerNameTMP;
            }
        }

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : รับโหมดการพูดคุยปัจจุบันและส่งคืนอ้างอิง UI ที่ตรงกับโหมดนั้น </para>
        /// <para> (EN) : Retrieves the corresponding Dialogue Area based on the requested mode. </para>
        /// </summary>
        private VNDialogueArea GetDialogueArea(VNDialogueMode mode)
        {
            return mode switch
            {
                VNDialogueMode.Standard => _standardDialogueArea,
                VNDialogueMode.LogView => _logViewDialogueArea,
                VNDialogueMode.Cinematic => _cinematicDialogueArea,
                _ => null
            };
        }

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : สั่งเริ่มเล่นเนื้อเรื่องจาก Chapter ที่ระบุ </para>
        /// <para> (EN) : Starts playing the story from the specified Chapter. </para>
        /// </summary>
        /// <param name="chapter">
        /// <para> (TH) : ข้อมูลเนื้อเรื่อง </para>
        /// <para> (EN) : Story chapter data. </para>
        /// </param>
        private void StartChapter(VNChapterSO chapter)
        {
            if (chapter == null) return;

            _currentChapter = chapter;
            _currentConversationIndex = 0;

            _logViewQueue.Clear();

            if (_currentChapter.Conversations.Count <= 0) return;
            StartCoroutine(PlayNodeRoutine(_currentChapter.Conversations[_currentConversationIndex]));
        }

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : จัดการการตอบสนองต่อการกดปุ่มของผู้เล่น (ข้ามเอฟเฟกต์การพิมพ์ หรือ ข้ามไป Phase ถัดไป) </para>
        /// <para> (EN) : Handles player input response (skipping typewriter effect or advancing to the next phase). </para>
        /// </summary>
        private void HandleInput()
        {
            if (_isTyping)
            {
                _skipTyping = true;
                return;
            }

            if (!_waitForInput) return;
            _waitForInput = false;
        }

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : รับค่าคำตอบที่ผู้เล่นเลือก และปลดล็อคสถานะเพื่อรันเนื้อเรื่องต่อไป </para>
        /// <para> (EN) : Receives the selected answer from the player and unlocks the state to continue the story. </para>
        /// </summary>
        /// <param name="answerNumber">
        /// <para> (TH) : หมายเลขคำตอบที่เลือก </para>
        /// <para> (EN) : Selected answer number. </para>
        /// </param>
        public void SelectChoice(int answerNumber)
        {
            if (!_waitForChoiceInput) return;

            _selectedAnswerNumber = answerNumber;
            _waitForChoiceInput = false;
        }

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : จัดรูปแบบและแสดงผลข้อความตามโหมดบทสนทนา (Standard, LogView, Cinematic) </para>
        /// <para> (EN) : Formats and displays dialogue text based on the active dialogue mode. </para>
        /// </summary>
        /// <param name="phase">
        /// <para> (TH) : ข้อมูล Phase ปัจจุบัน </para>
        /// <para> (EN) : Current phase data. </para>
        /// </param>
        /// <param name="namesBuilder">
        /// <para> (TH) : ชื่อตัวละครที่ถูกจัดรูปแบบแล้ว </para>
        /// <para> (EN) : Combined speaker names. </para>
        /// </param>
        /// <returns>
        /// <para> (TH) : จำนวนตัวอักษรตั้งต้นที่จะแสดง </para>
        /// <para> (EN) : Initial visible character count. </para>
        /// </returns>
        private int FormatDialogueText(VNDialoguePhaseData phase, StringBuilder namesBuilder)
        {
            int visibleCount = 0;

            switch (_currentDialogueType)
            {
                case VNDialogueMode.Standard:
                    if (_currentSpeakerNameTMP != null) _currentSpeakerNameTMP.text = namesBuilder.ToString();
                    _currentDialogueTMP.text = phase.DialogueText;
                    break;

                case VNDialogueMode.LogView:
                    string speakerPrefix = namesBuilder.Length > 0 ? $"{namesBuilder} : " : "";
                    string newMessage = $"{speakerPrefix}{phase.DialogueText}";

                    _logViewQueue.Enqueue(newMessage);

                    // รักษาจำนวน LogView ให้ไม่เกิน 3 ข้อความ
                    while (_logViewQueue.Count > 3)
                    {
                        _logViewQueue.Dequeue();
                    }

                    if (_logViewQueue.Count > 1)
                    {
                        var logsArray = _logViewQueue.ToArray();
                        string previousTexts = string.Join("\n\n", logsArray, 0, logsArray.Length - 1) + "\n\n";
                        _currentDialogueTMP.text = previousTexts;
                        _currentDialogueTMP.ForceMeshUpdate();
                        visibleCount = _currentDialogueTMP.textInfo.characterCount;
                    }

                    _currentDialogueTMP.text = string.Join("\n\n", _logViewQueue);
                    break;

                case VNDialogueMode.Cinematic:
                    if (_currentSpeakerNameTMP != null) _currentSpeakerNameTMP.text = string.Empty;
                    _currentDialogueTMP.text = phase.DialogueText;
                    break;
            }

            return visibleCount;
        }

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : รวบรวมรายชื่อผู้พูดและสั่งให้ตัวละครแต่ละตัวเริ่มประมวลผลคำสั่ง Action (ถ้าอยู่ในโหมดที่รองรับ) </para>
        /// <para> (EN) : Compiles speaker names and starts executing actions for each character (if mode supports it). </para>
        /// </summary>
        /// <param name="phase">
        /// <para> (TH) : ข้อมูล Phase ปัจจุบัน </para>
        /// <para> (EN) : Phase data. </para>
        /// </param>
        /// <returns>
        /// <para> (TH) : ข้อความชื่อผู้พูดที่รวมกันแล้ว </para>
        /// <para> (EN) : Formatted speaker names string builder. </para>
        /// </returns>
        private StringBuilder SetupSpeakers(VNDialoguePhaseData phase)
        {
            var namesBuilder = new StringBuilder();

            // ข้ามการตั้งค่าผู้พูดและ Action ทั้งหมดถ้าอยู่ในโหมดที่ไม่ได้ใช้งาน หรือ โหมด Cinematic / LogView (ตามเงื่อนไขที่ผู้เล่นไม่เห็น UI เหล่านี้)
            if (_currentDialogueType == VNDialogueMode.None || _currentDialogueType == VNDialogueMode.Cinematic || _currentDialogueType == VNDialogueMode.LogView)
                return namesBuilder;

            var addedNames = new HashSet<string>();

            foreach (var speakerData in phase.Speakers)
            {
                if (speakerData.Character == null) continue;

                var name = speakerData.ShowName ? speakerData.Character.CharacterName : "???";
                if (addedNames.Contains(name)) continue;

                if (namesBuilder.Length > 0) namesBuilder.Append(" , ");
                namesBuilder.Append(name);
                addedNames.Add(name);

                StartCoroutine(ExecuteActionsRoutine(speakerData, phase.Speakers.Count));
            }

            return namesBuilder;
        }

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ตรวจสอบและสั่งใช้งานเหตุการณ์บรรยากาศ (Ambient) หรือเพลงประกอบ (BGM) หากมีการตั้งค่าไว้ </para>
        /// <para> (EN) : Evaluates and triggers ambient events or background music if configured. </para>
        /// </summary>
        /// <param name="phase">
        /// <para> (TH) : ข้อมูล Phase ปัจจุบัน </para>
        /// <para> (EN) : Phase data. </para>
        /// </param>
        private void TriggerPhaseEvents(VNDialoguePhaseData phase)
        {
            if (phase.UseAmbientEvent)
            {
                if (phase.AmbientEventType != VNAmbientEventType.None)
                {
                    EventBus.Instance.Publish(new VNAmbientEvent(phase.AmbientEventType));
                }
#if UNITY_EDITOR
                else
                {
                    Debug.LogWarning($"<b><color=yellow>[Skiped AmbientEvent]</color></b>" + Environment.NewLine
                            + $"<b><color=orange>[because]</color></b> AmbientEventName in <b><color=cyan><i>[Conversation{_currentConversationIndex + 1} at {_currentPhase} On Chapter{_currentChapter.Index} {_currentChapter.name}]</color></i></b> is <b><color=yellow><i>Null or Empty!!</i></color></b>");
                }
#endif
            }

            if (phase.OverrideBmgClip)
            {
                if (phase.BmgClip != null)
                {
                    _audioChannel.SendBmgSignal(phase.BmgClip);
                }
#if UNITY_EDITOR
                else
                {
                    Debug.LogWarning($"<b><color=yellow>[Skiped BmgClip]</color></b>" + Environment.NewLine
                            + $"<b><color=orange>[because]</color></b> OverrideBmgClip in <b><color=cyan><i>[Conversation{_currentConversationIndex + 1} at {_currentPhase} On Chapter{_currentChapter.Index} {_currentChapter.name}]</color></i></b> is <b><color=yellow><i>Null!!</i></color></b>");
                }
#endif
            }

            // เสียงพูดให้เล่นก็ต่อเมื่อไม่ใช่โหมด None
            if (_currentDialogueType != VNDialogueMode.None && phase.UseVoiceoverClip)
            {
                if (phase.VoiceoverClip != null)
                {
                    _audioChannel.SendVoiceoverSignal(phase.VoiceoverClip);
                }
#if UNITY_EDITOR
                else
                {
                    Debug.LogWarning($"<b><color=yellow>[Skiped VoiceoverClip]</color></b>" + Environment.NewLine
                            + $"<b><color=orange>[because]</color></b> VoiceoverClip in <b><color=cyan><i>[Conversation{_currentConversationIndex + 1} at {_currentPhase} On Chapter{_currentChapter.Index} {_currentChapter.name}]</color></i></b> is <b><color=yellow><i>Null!!</i></color></b>");
                }
#endif
            }
        }

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ดำเนินการอัปเดตหน้าตาและอารมณ์ของตัวละครตามคำสั่ง (Action) ที่ตั้งไว้ </para>
        /// <para> (EN) : Processes and updates character emotions based on the defined action. </para>
        /// </summary>
        /// <param name="action">
        /// <para> (TH) : คำสั่งการกระทำ </para>
        /// <para> (EN) : Action command. </para>
        /// </param>
        /// <param name="speakerData">
        /// <para> (TH) : ข้อมูลตัวละคร </para>
        /// <para> (EN) : Speaker data. </para>
        /// </param>
        /// <param name="speakersCount">
        /// <para> (TH) : จำนวนคนพูดทั้งหมด </para>
        /// <para> (EN) : Total speakers count. </para>
        /// </param>
        /// <param name="actionIndex">
        /// <para> (TH) : ลำดับของคำสั่ง </para>
        /// <para> (EN) : Index of the action. </para>
        /// </param>
        private void ProcessEmotionAction(VNAction action, VNSpeakerData speakerData, int speakersCount, ushort actionIndex)
        {
            if (action.EmotionName == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"<b><color=yellow>[Skiped EmotionAction]</color></b>" + Environment.NewLine
                    + $"<b><color=orange>[because]</color></b> Emotion in <b><color=cyan><i>[Conversation{_currentConversationIndex + 1} at {_currentPhase} in Action{actionIndex + 1} On Chapter{_currentChapter.Index} {_currentChapter.name}]</color></i></b> is <b><color=yellow><i>Null!!</i></color></b>");
#endif
                return;
            }

            var emotion = speakerData.Character.GetEmotion(action.EmotionName);

            if (emotion == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"<b><color=yellow>[Skiped EmotionAction]</color></b>" + Environment.NewLine
                     + $"<b><color=orange>[because]</color></b> Emotion '{action.EmotionName}' in <b><color=cyan><i>[Conversation{_currentConversationIndex + 1} at {_currentPhase} in Action{actionIndex + 1} On Chapter{_currentChapter.Index} {_currentChapter.name}]</color></i></b> not found in Character '{speakerData.Character.CharacterName}'");
#endif        
                return;
            }

            if (speakersCount == 1 && speakerData.ShowName && _currentDialogueType == VNDialogueMode.Standard)
            {
                if (_currentSpeakerIcon != null)
                {
                    _currentSpeakerIcon.sprite = emotion.Value.EmoteIcon;
                    _currentSpeakerIcon.gameObject.SetActive(emotion.Value.EmoteIcon != null);
                }
            }
            else
            {
                if (_currentSpeakerIcon != null)
                {
                    _currentSpeakerIcon.sprite = null;
                    _currentSpeakerIcon.gameObject.SetActive(false);
                }
            }

            var emotionName = emotion.Value.EmoteClip.name;
            speakerData.Character.SendVNEmotionSignel(emotionName);
        }

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ดำเนินการอัปเดตแอนิเมชันพฤติกรรมของตัวละครตามคำสั่ง (Action) ที่ตั้งไว้ </para>
        /// <para> (EN) : Processes and updates character animations based on the defined action. </para>
        /// </summary>
        /// <param name="action">
        /// <para> (TH) : คำสั่งการกระทำ </para>
        /// <para> (EN) : Action command. </para>
        /// </param>
        /// <param name="speakerData">
        /// <para> (TH) : ข้อมูลตัวละคร </para>
        /// <para> (EN) : Speaker data. </para>
        /// </param>
        /// <param name="actionIndex">
        /// <para> (TH) : ลำดับของคำสั่ง </para>
        /// <para> (EN) : Index of the action. </para>
        /// </param>
        private void ProcessAnimationAction(VNAction action, VNSpeakerData speakerData, ushort actionIndex)
        {
            if (action.BehaviorAnimationName == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"<b><color=yellow>[Skiped AnimationAction]</color></b>" + Environment.NewLine
                    + $"<b><color=orange>[because]</color></b> Animation in <b><color=cyan><i>[Conversation{_currentConversationIndex + 1} at {_currentPhase} in Action{actionIndex + 1} On Chapter{_currentChapter.Index} {_currentChapter.name}]</color></i></b> is <b><color=yellow><i>Null!!</i></color></b>");
#endif                
                return;
            }

            var animation = speakerData.Character.GetAnimation(action.BehaviorAnimationName);

            if (animation == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"<b><color=yellow>[Skiped AnimationAction]</color></b>" + Environment.NewLine
                    + $"<b><color=orange>[because]</color></b> animation '{action.BehaviorAnimationName}' in <b><color=cyan><i>[Conversation{_currentConversationIndex + 1} at {_currentPhase} in Action{actionIndex + 1} On Chapter{_currentChapter.Index} {_currentChapter.name}]</color></i></b> not found in Character '{speakerData.Character.CharacterName}'");
#endif
                return;
            }

            var animationName = animation.Value.BehaviorAnimationClip.name;
            speakerData.Character.SendVNAnimationSignal(animationName);
        }

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ดำเนินการเล่นเสียงประกอบของตัวละครตามคำสั่ง (Action) ที่ตั้งไว้ </para>
        /// <para> (EN) : Processes and plays character sound effects based on the defined action. </para>
        /// </summary>
        /// <param name="action">
        /// <para> (TH) : คำสั่งการกระทำ </para>
        /// <para> (EN) : Action command. </para>
        /// </param>
        /// <param name="speakerData">
        /// <para> (TH) : ข้อมูลตัวละคร </para>
        /// <para> (EN) : Speaker data. </para>
        /// </param>
        /// <param name="actionIndex">
        /// <para> (TH) : ลำดับของคำสั่ง </para>
        /// <para> (EN) : Index of the action. </para>
        /// </param>
        private void ProcessSoundEffectAction(VNAction action, VNSpeakerData speakerData, ushort actionIndex)
        {
            if (action.SoundEffectName == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"<b><color=yellow>[Skiped ActionSoundEffect]</color></b>" + Environment.NewLine
                    + $"<b><color=orange>[because]</color></b> SoundEffect in <b><color=cyan><i>[Conversation{_currentConversationIndex + 1} at {_currentPhase} in Action{actionIndex + 1} On Chapter{_currentChapter.Index} {_currentChapter.name}]</color></i></b> is <b><color=yellow><i>Null!!</i></color></b>");
#endif
                return;
            }

            var soundEffect = speakerData.Character.GetSoundEffect(action.SoundEffectName);

            if (soundEffect == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"<b><color=yellow>[Skiped ActionSoundEffect]</color></b>" + Environment.NewLine
                    + $"<b><color=orange>[because]</color></b> SoundEffect '{action.SoundEffectName}' in <b><color=cyan><i>[Conversation{_currentConversationIndex + 1} at {_currentPhase} in Action{actionIndex + 1} On Chapter{_currentChapter.Index} {_currentChapter.name}]</color></i></b> not found in Character '{speakerData.Character.CharacterName}'");
#endif
                return;
            }

            var audioName = soundEffect.Value.SoundEffectClip.name;
            speakerData.Character.SendVNSoundEffectSignel(audioName);
        }

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : นำการตั้งค่าตัวอักษรพิเศษมาใช้งาน หากตั้งค่าเปิดให้เขียนทับค่าเริ่มต้น </para>
        /// <para> (EN) : Applies custom text settings if override is enabled. </para>
        /// </summary>
        /// <param name="settings">
        /// <para> (TH) : ข้อมูลการตั้งค่าตัวอักษร </para>
        /// <para> (EN) : Text settings data. </para>
        /// </param>
        private void ApplyTextSettings(VNTextSettings settings)
        {
            if (settings == null || !settings.OverrideGlobalSettings) return;

            if (settings.CustomFont != null) _currentDialogueTMP.font = settings.CustomFont;

            _currentDialogueTMP.fontSize = settings.FontSize;
            _currentDialogueTMP.color = settings.FontColor;
            _currentDialogueTMP.alignment = settings.Alignment;
        }

        #endregion //End Helper Functions Region

        #region Corotines Region

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ตรวจสอบประเภทของโหนดบทสนทนา (Dialogue หรือ Choice) และเริ่มการทำงานตามรูปแบบนั้น </para>
        /// <para> (EN) : Evaluates the conversation node type (Dialogue or Choice) and routes to the appropriate execution. </para>
        /// </summary>
        /// <param name="node">
        /// <para> (TH) : ข้อมูลโหนดปัจจุบัน </para>
        /// <para> (EN) : Current node data. </para>
        /// </param>
        /// <returns>
        /// <para> (TH) : สถานะ Coroutine </para>
        /// <para> (EN) : Coroutine state. </para>
        /// </returns>
        private IEnumerator PlayNodeRoutine(VNConversationNode node)
        {
            switch (node.ConversationMode)
            {
                case VNConversationMode.DialogueMode:
                    yield return DialogueModeRotine(node.DialogueNode);
                    break;
                case VNConversationMode.ChoiceMode:
                    yield return ChoiceModeRotine(node.ChoiceNode);
                    break;
            }

            if (_currentConversationIndex < _currentChapter.Conversations.Count)
            {
                StartCoroutine(PlayNodeRoutine(_currentChapter.Conversations[_currentConversationIndex]));
            }
        }

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ประมวลผลโหนดในรูปแบบการพูดคุยมาตรฐานตามลำดับ Phase (Enter -> Main -> Exit) </para>
        /// <para> (EN) : Processes a standard dialogue node sequentially through phases (Enter -> Main -> Exit). </para>
        /// </summary>
        /// <param name="node">
        /// <para> (TH) : ข้อมูลโหนดพูดคุย </para>
        /// <para> (EN) : Dialogue node data. </para>
        /// </param>
        /// <returns>
        /// <para> (TH) : สถานะ Coroutine </para>
        /// <para> (EN) : Coroutine state. </para>
        /// </returns>
        private IEnumerator DialogueModeRotine(VNDialogueNode node)
        {
            if (node.UseEnterPhase) yield return PlayPhaseRoutine(node.EnterPhase, VNCurrentPhase.EnterPhase);

            yield return PlayPhaseRoutine(node.MainPhase, VNCurrentPhase.MainPhase);

            if (node.UseExitPhase) yield return PlayPhaseRoutine(node.ExitPhase, VNCurrentPhase.ExitPhase);

            _currentConversationIndex++;

            if (_currentConversationIndex >= _currentChapter.Conversations.Count)
            {
#if UNITY_EDITOR
                Debug.Log("<b><color=#A5D6A7>[VN Engine]</color></b> Chapter Finished!");
#endif
                yield break;
            }
        }

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ประมวลผลโหนดตัวเลือก โดยไล่ลำดับตั้งแต่ตั้งคำถาม -> รอผู้เล่นตอบ -> แสดงผลลัพธ์ </para>
        /// <para> (EN) : Processes a choice node sequentially (Question -> Wait for answer -> Interact). </para>
        /// </summary>
        /// <param name="node">
        /// <para> (TH) : ข้อมูลโหนดตัวเลือก </para>
        /// <para> (EN) : Choice node data. </para>
        /// </param>
        /// <returns>
        /// <para> (TH) : สถานะ Coroutine </para>
        /// <para> (EN) : Coroutine state. </para>
        /// </returns>
        private IEnumerator ChoiceModeRotine(VNChoiceNode node)
        {
            if (node.QuestionState.UseEnterPhase)
                yield return PlayPhaseRoutine(node.QuestionState.EnterPhase, VNCurrentPhase.EnterPhase);

            yield return PlayPhaseRoutine(node.QuestionState.MainPhase, VNCurrentPhase.MainPhase);

            if (node.AnswerState.UseEnterPhase)
                yield return PlayPhaseRoutine(node.AnswerState.EnterPhase, VNCurrentPhase.EnterPhase);

            yield return PlayPhaseRoutine(node.AnswerState.MainPhase as VNDialoguePhaseData, VNCurrentPhase.MainPhase);

            _waitForChoiceInput = true;

            while (_waitForChoiceInput) yield return null;

            if (node.AnswerState.UseExitPhase)
                yield return PlayPhaseRoutine(node.AnswerState.ExitPhase as VNDialoguePhaseData, VNCurrentPhase.ExitPhase);

            bool hasInteractMatch = false;
            VNInteractState matchedInteract = default;

            foreach (var interact in node.InteractStates)
            {
                if (interact.TargetAnswerNumber == _selectedAnswerNumber)
                {
                    matchedInteract = interact;
                    hasInteractMatch = true;
                    break;
                }
            }

            if (hasInteractMatch)
            {
                if (matchedInteract.UseEnterPhase)
                    yield return PlayPhaseRoutine(matchedInteract.EnterPhase, VNCurrentPhase.EnterPhase);

                yield return PlayPhaseRoutine(matchedInteract.MainPhase, VNCurrentPhase.MainPhase);

                if (matchedInteract.UseExitPhase)
                    yield return PlayPhaseRoutine(matchedInteract.ExitPhase as VNDialoguePhaseData, VNCurrentPhase.ExitPhase);
            }

            _currentConversationIndex++;

            if (_currentConversationIndex >= _currentChapter.Conversations.Count)
            {
#if UNITY_EDITOR
                Debug.Log("<b><color=#A5D6A7>[VN Engine]</color></b> Chapter Finished!");
#endif
                yield break;
            }
        }

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : สลับกล่องบทสนทนาและเตรียมความพร้อมของแอนิเมชัน UI ก่อนเริ่ม Phase พร้อมเพิ่มจังหวะรอ Fade 1.2 วินาทีเมื่อมีการสลับโหมด </para>
        /// <para> (EN) : Toggles dialogue boxes and prepares UI animations before starting the phase, with a 1.2s delay for cross-fading modes. </para>
        /// </summary>
        /// <param name="phase">
        /// <para> (TH) : ข้อมูล Phase ที่จะใช้งาน </para>
        /// <para> (EN) : Phase data to setup. </para>
        /// </param>
        /// <returns>
        /// <para> (TH) : สถานะ Coroutine (รอให้แอนิเมชันเล่นจนจบ) </para>
        /// <para> (EN) : Coroutine state (waits for animation). </para>
        /// </returns>
        private IEnumerator DialogueSetup(VNDialoguePhaseData phase)
        {
            ApplyTextSettings(phase.TextSettings);

            // เช็คว่ามีการเปลี่ยนโหมดหรือไม่
            bool isModeChanged = _currentDialogueType != phase.DialogueMode;

            // กรณีเปลี่ยนโหมด (หรือกำลังจะปิดเป็นโหมด None) ให้เล่น Fade ปิดของกล่องเก่าก่อน
            if (isModeChanged && _currentDialogueType != VNDialogueMode.None && _currentDialogueAnimator != null)
            {
                if (phase.OverrideDialogueAnimation && phase.DialogueBoxAnimation != null)
                {
                    _currentDialogueAnimator.Play(phase.DialogueBoxAnimation.name);
                }
                else if (_hideDialogueClip != null)
                {
                    _currentDialogueAnimator.Play(_hideDialogueClip.name);
                }

                // หน่วงเวลา Fade out ตามกำหนด (ขั้นต่ำ 1.2 วินาที)
                yield return new WaitForSeconds(1.2f);
            }

            // ซ่อนกล่องเดิมให้หมดเพื่อเตรียมเปิดกล่องของโหมดใหม่
            if (_standardDialogueArea?.DialogueBox != null) _standardDialogueArea.DialogueBox.gameObject.SetActive(false);
            if (_logViewDialogueArea?.DialogueBox != null) _logViewDialogueArea.DialogueBox.gameObject.SetActive(false);
            if (_cinematicDialogueArea?.DialogueBox != null) _cinematicDialogueArea.DialogueBox.gameObject.SetActive(false);

            var dialogueArea = GetDialogueArea(phase.DialogueMode);

            if (phase.DialogueMode == VNDialogueMode.None || dialogueArea == null)
            {
                _currentDialogueType = VNDialogueMode.None;
                _currentDialogueBox = null;
                _currentDialogueAnimator = null;
                _currentDialogueTMP = null;
                _currentSpeakerNameTMP = null;
                yield break;
            }

            // ตั้งค่า UI ตัวใหม่ตามโหมด
            _currentDialogueBox = dialogueArea.DialogueBox;
            _currentDialogueAnimator = dialogueArea.DialogueAnimator;
            _currentDialogueTMP = dialogueArea.DialogueTMP;
            _currentSpeakerNameTMP = dialogueArea.SpeakerNameTMP;
            _currentDialogueType = phase.DialogueMode;

            if (_currentDialogueTMP != null)
            {
                _currentDialogueTMP.text = string.Empty;
                _currentDialogueTMP.maxVisibleCharacters = 0;
            }

            if (_currentSpeakerNameTMP != null)
            {
                _currentSpeakerNameTMP.text = string.Empty;
            }

            if (_currentSpeakerIcon != null)
            {
                _currentSpeakerIcon.sprite = null;
                _currentSpeakerIcon.gameObject.SetActive(false);
            }

            if (_currentDialogueBox != null)
            {
                _currentDialogueBox.gameObject.SetActive(true);
            }

            // ถ้าเป็นการสลับโหมดจากโหมดอื่น ให้เล่น Fade in ของกล่องใหม่
            if (isModeChanged && _currentDialogueAnimator != null)
            {
                if (phase.OverrideDialogueAnimation && phase.DialogueBoxAnimation != null)
                {
                    _currentDialogueAnimator.Play(phase.DialogueBoxAnimation.name);
                }
                else if (_showDialogueClip != null)
                {
                    _currentDialogueAnimator.Play(_showDialogueClip.name);
                }

                yield return new WaitForSeconds(1.2f);
            }
        }

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ประมวลผลการทำงานทั้งหมดภายในเฟสเดียว (เริ่มจากการเตรียมตัวละคร, พิมพ์ข้อความ, และรันคำสั่งต่างๆ) </para>
        /// <para> (EN) : Processes all logic within a single phase (setting up characters, typing text, executing actions). </para>
        /// </summary>
        /// <param name="phase">
        /// <para> (TH) : ข้อมูล Phase ที่จะเล่น </para>
        /// <para> (EN) : Phase data to play. </para>
        /// </param>
        /// <param name="currentPhaseEnum">
        /// <para> (TH) : ประเภทของ Phase ปัจจุบัน </para>
        /// <para> (EN) : Current phase type enum. </para>
        /// </param>
        /// <returns>
        /// <para> (TH) : สถานะ Coroutine </para>
        /// <para> (EN) : Coroutine state. </para>
        /// </returns>
        private IEnumerator PlayPhaseRoutine(VNDialoguePhaseData phase, VNCurrentPhase currentPhaseEnum)
        {
            _currentPhase = currentPhaseEnum;

            yield return DialogueSetup(phase);

            StringBuilder namesBuilder = SetupSpeakers(phase);

            TriggerPhaseEvents(phase);

            if (_currentDialogueType != VNDialogueMode.None && _currentDialogueTMP != null)
            {
                int visibleCount = FormatDialogueText(phase, namesBuilder);
                yield return PlayTypingEffect(phase, visibleCount);
            }
            else
            {
                // ถ้าโหมดเป็น None ให้ข้ามและรอคอยครู่หนึ่ง
                yield return null;
            }
        }

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : รันเอฟเฟกต์เครื่องพิมพ์ดีดสำหรับข้อความ (ทยอยแสดงทีละตัวอักษร) พร้อมรอให้ผู้เล่นกดยืนยัน </para>
        /// <para> (EN) : Runs the typewriter text effect, revealing characters one by one, and waits for confirmation input. </para>
        /// </summary>
        /// <param name="phase">
        /// <para> (TH) : ข้อมูล Phase ปัจจุบัน </para>
        /// <para> (EN) : Phase data. </para>
        /// </param>
        /// <param name="visibleCount">
        /// <para> (TH) : จำนวนอักษรที่แสดงอยู่แล้ว (ใช้กับโหมด LogView) </para>
        /// <para> (EN) : Already visible character count (used in LogView). </para>
        /// </param>
        /// <returns>
        /// <para> (TH) : สถานะ Coroutine </para>
        /// <para> (EN) : Coroutine state. </para>
        /// </returns>
        private IEnumerator PlayTypingEffect(VNDialoguePhaseData phase, int visibleCount)
        {
            _isTyping = true;
            _skipTyping = false;

            _currentDialogueTMP.ForceMeshUpdate();
            var totalVisibleCharacters = _currentDialogueTMP.textInfo.characterCount;

            while (visibleCount < totalVisibleCharacters)
            {
                if (_skipTyping)
                {
                    _currentDialogueTMP.maxVisibleCharacters = totalVisibleCharacters;
                    _audioChannel.SendSfxSignal(null);
                    _audioChannel.SendVoiceoverSignal(null);
                    EventBus.Instance.Publish(new VNTypingSkipEvent(_currentConversationIndex, _currentPhase));
                    break;
                }

                visibleCount++;
                _currentDialogueTMP.maxVisibleCharacters = visibleCount;
                yield return new WaitForSeconds(1f / phase.TextSettings.TypingSpeed);
            }

            _isTyping = false;
            _waitForInput = true;

            while (_waitForInput) yield return null;
        }

        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : วนลูปอ่านและใช้งานคำสั่ง (Action) ต่างๆ ของตัวละครภายในเฟสนั้นๆ </para>
        /// <para> (EN) : Iterates and executes the list of actions defined for a character within the current phase. </para>
        /// </summary>
        /// <param name="speakerData">
        /// <para> (TH) : ข้อมูลผู้พูดและคำสั่ง Action </para>
        /// <para> (EN) : Speaker and action data. </para>
        /// </param>
        /// <param name="speakersCount">
        /// <para> (TH) : จำนวนคนพูดร่วมทั้งหมดในเฟส </para>
        /// <para> (EN) : Total speakers count. </para>
        /// </param>
        /// <returns>
        /// <para> (TH) : สถานะ Coroutine </para>
        /// <para> (EN) : Coroutine state. </para>
        /// </returns>
        private IEnumerator ExecuteActionsRoutine(VNSpeakerData speakerData, int speakersCount)
        {
            ushort actionIndex = 0;

            foreach (var action in speakerData.Actions)
            {
                if (action.UseEmotion) ProcessEmotionAction(action, speakerData, speakersCount, actionIndex);
                if (action.UseBehaviorAnimation) ProcessAnimationAction(action, speakerData, actionIndex);
                if (action.UseSoundEffect) ProcessSoundEffectAction(action, speakerData, actionIndex);

                if (action.UseDelay)
                {
                    if (action.DelayTime <= 0f)
                    {
#if UNITY_EDITOR
                        Debug.LogWarning($"<b><color=yellow>[Skiped ActionDelay]</color></b>" + Environment.NewLine
                            + $"<b><color=orange>[because]</color></b> DelayTime in <b><color=cyan><i>[Conversation{_currentConversationIndex + 1} at {_currentPhase} in Action{actionIndex + 1} On Chapter{_currentChapter.Index} {_currentChapter.name}]</color></i></b> is <b><color=yellow><i>0 or Negative!!</i></color></b>");
#endif
                    }
                    else
                    {
                        yield return new WaitForSeconds(action.DelayTime);
                    }
                }

                actionIndex++;
                yield return null;
            }
        }

        #endregion //End Corotines Region
    }
}