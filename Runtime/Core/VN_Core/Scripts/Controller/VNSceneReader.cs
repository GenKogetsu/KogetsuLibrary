using TMPro;
using System.Text;
using UnityEngine.UI;
using Genoverrei.Library.Attribute;
using Genoverrei.Library.DesignPatternCore;

namespace Genoverrei.Library.Core
{
	/// <summary>
	/// <para> Summary : </para>
	/// <para> (TH) : ตัวควบคุมหลักสำหรับระบบ Visual Novel ทำหน้าที่ประมวลผลโหนดเนื้อเรื่อง, จัดการ UI, ตัวละคร และเหตุการณ์ต่างๆ </para>
	/// <para> (EN) : Main controller for the Visual Novel system. Processes story nodes, manages UI, characters, and events. </para>
	/// </summary>
	[CreateHierarchyMenu("GenoverreiLibrary/Core")]
	public class VNSceneReader : MonoBehaviour
    {
        #region Field Region

        [Header("Story Data")]
        [Required]
        [SerializeField] private VNSceneSO _currentScene;

        [Header("Obsever Channels")]
        [Required]
        [SerializeField] private BasicMovementInputObserverSO _basicObserverChannel;

        [Required]
        [SerializeField] private AudioObserverSO _audioObserver;

        [Required]
        [SerializeField] private AnimationClip _hideDialogueClip, _showDialogueClip;

        [Header("UI References")]
        [SerializeField] private VNDialogueArea _standardDialogueArea;
        [SerializeField] private VNDialogueArea _logViewDialogueArea;
        [SerializeField] private VNDialogueArea _cinematicDialogueArea;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private VNChoicePanel _choicePanel; //new: choice button popup panel

        private readonly Queue<string> _logViewQueue = new();

        [Header("Runtime Data")]
        [ReadOnly]
        [SerializeField] private ushort _currentConversationIndex = 0;
        [SerializeField] private RectTransform _currentDialogueBox;
        [SerializeField] private Animator _currentDialogueAnimator;
        [SerializeField] private TextMeshProUGUI _currentDialogueTMP, _currentSpeakerNameTMP;
        [SerializeField] private Image _currentSpeakerIcon;
        [SerializeField] private Image _currentSpeakerNameIcon; //new: runtime-assigned name icon (Icon mode)

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

        private void OnEnable() => _basicObserverChannel.OnLeftClickChannel += HandleInput;
        private void OnDisable() => _basicObserverChannel.OnLeftClickChannel -= HandleInput;

        private void Start()
        {
            if (_currentScene == null) return;
            SetUp();
            StartVNScene(_currentScene);
        }

        #region Helper Functions Region

        /// <summary>
        /// <para> (TH) : ตั้งค่าเริ่มต้นให้กับตัวควบคุมหลัก โดยเชื่อมโยง UI พื้นฐานของโหมด Standard </para>
        /// <para> (EN) : Prepares initial setup, linking default text components for the Standard mode. </para>
        /// </summary>
        private void SetUp()
        {
            if (_standardDialogueArea == null) return;
            _currentDialogueTMP = _standardDialogueArea.DialogueTMP;
            _currentSpeakerNameTMP = _standardDialogueArea.SpeakerNameTMP;
            _currentSpeakerNameIcon = _standardDialogueArea.SpeakerNameIcon; //new
        }

        private VNDialogueArea GetDialogueArea(VNDialogueMode mode) => mode switch
        {
            VNDialogueMode.Standard => _standardDialogueArea,
            VNDialogueMode.LogView => _logViewDialogueArea,
            VNDialogueMode.Cinematic => _cinematicDialogueArea,
            _ => null
        };

        private void StartVNScene(VNSceneSO chapter)
        {
            if (chapter == null) return;
            _currentScene = chapter;
            _currentConversationIndex = 0;
            _logViewQueue.Clear();
            if (_currentScene.Conversations.Count <= 0) return;
            StartCoroutine(PlayNodeRoutine(_currentScene.Conversations[_currentConversationIndex]));
        }

        private void HandleInput(ClickData data)
        {
            if (_isTyping) { _skipTyping = true; return; }
            if (!_waitForInput) return;
            _waitForInput = false;
        }

        /// <summary>
        /// <para> (TH) : รับค่าคำตอบที่ผู้เล่นเลือก และปลดล็อคสถานะเพื่อรันเนื้อเรื่องต่อไป </para>
        /// <para> (EN) : Receives the selected answer and unlocks the state to continue the story. </para>
        /// </summary>
        public void SelectChoice(int answerNumber)
        {
            if (!_waitForChoiceInput) return;
            _selectedAnswerNumber = answerNumber;
            _waitForChoiceInput = false;
        }

        /// <summary>
        /// <para> (TH) : จัดรูปแบบและแสดงผลข้อความตามโหมดบทสนทนา (Standard, LogView, Cinematic) </para>
        /// <para> (EN) : Formats and displays dialogue text based on the active dialogue mode. </para>
        /// </summary>
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
                    _logViewQueue.Enqueue($"{speakerPrefix}{phase.DialogueText}");

                    while (_logViewQueue.Count > 3) _logViewQueue.Dequeue();

                    if (_logViewQueue.Count > 1)
                    {
                        var logsArray = _logViewQueue.ToArray();
                        _currentDialogueTMP.text = string.Join("\n\n", logsArray, 0, logsArray.Length - 1) + "\n\n";
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
        /// <para> (TH) : รวบรวมรายชื่อผู้พูดและสั่งให้ตัวละครแต่ละตัวเริ่มประมวลผลคำสั่ง Action </para>
        /// <para> (EN) : Compiles speaker names and starts executing actions for each character. </para>
        /// </summary>
        /// <remarks>รับ VNChoicePhaseData เพราะ Speakers อยู่ใน base class</remarks>
        private StringBuilder SetupSpeakers(VNChoicePhaseData phase)
        {
            var namesBuilder = new StringBuilder();

            if (_currentDialogueType is VNDialogueMode.None or VNDialogueMode.Cinematic or VNDialogueMode.LogView)
                return namesBuilder;

            var addedNames = new HashSet<string>();

            foreach (var speakerData in phase.Speakers)
            {
                if (speakerData.Character == null) continue;

                if (speakerData.NameDisplayMode == VNNameDisplayMode.Text) //new: NameDisplayMode replaces ShowName
                {
                    var name = speakerData.Character.CharacterName;
                    if (!addedNames.Add(name)) continue;
                    if (namesBuilder.Length > 0) namesBuilder.Append(" , ");
                    namesBuilder.Append(name);
                }
                else if (speakerData.NameDisplayMode == VNNameDisplayMode.Icon) //new: show name icon sprite
                {
                    if (_currentSpeakerNameIcon != null && speakerData.Character.NameIcon != null)
                    {
                        _currentSpeakerNameIcon.sprite = speakerData.Character.NameIcon;
                        _currentSpeakerNameIcon.gameObject.SetActive(true);
                    }
                }

                StartCoroutine(ExecuteActionsRoutine(speakerData, phase.Speakers.Count));
            }

            return namesBuilder;
        }

        /// <summary>
        /// <para> (TH) : ตรวจสอบและสั่งใช้งานเหตุการณ์บรรยากาศ หรือเพลงประกอบ หากมีการตั้งค่าไว้ </para>
        /// <para> (EN) : Evaluates and triggers ambient events or background music if configured. </para>
        /// </summary>
        /// <remarks>รับ VNChoicePhaseData เพราะ event fields อยู่ใน base class</remarks>
        private void TriggerPhaseEvents(VNChoicePhaseData phase)
        {
            if (phase.UseAmbientEvent)
            {
                if (phase.AmbientEventType != VNAmbientEventType.None)
                    EventBus.Instance.Publish(new VNAmbientEvent(phase.AmbientEventType));
#if UNITY_EDITOR
                else Debug.LogWarning($"<b><color=yellow>[Skiped AmbientEvent]</color></b> AmbientEventType is None at Conversation {_currentConversationIndex + 1}");
#endif
            }

            if (phase.OverrideBmgClip)
            {
                if (phase.BmgClip != null) _audioObserver.SendBmgSignal(phase.BmgClip);
#if UNITY_EDITOR
                else Debug.LogWarning($"<b><color=yellow>[Skiped BmgClip]</color></b> BmgClip is null at Conversation {_currentConversationIndex + 1}");
#endif
            }

            if (_currentDialogueType != VNDialogueMode.None && phase.UseVoiceoverClip)
            {
                if (phase.VoiceoverClip != null) _audioObserver.SendVoiceoverSignal(phase.VoiceoverClip);
#if UNITY_EDITOR
                else Debug.LogWarning($"<b><color=yellow>[Skiped VoiceoverClip]</color></b> VoiceoverClip is null at Conversation {_currentConversationIndex + 1}");
#endif
            }
        }

        /// <summary>
        /// <para> (TH) : เปลี่ยนภาพพื้นหลังหากเปิดใช้งาน ChangeBackground ใน phase </para>
        /// <para> (EN) : Updates the background image if ChangeBackground is enabled for this phase. </para>
        /// </summary>
        private void ApplyBackground(VNChoicePhaseData phase)
        {
            if (!phase.ChangeBackground || _backgroundImage == null || phase.BackgroundSprite == null) return;
            _backgroundImage.sprite = phase.BackgroundSprite;
        }

        private void ProcessEmotionAction(VNAction action, VNSpeakerData speakerData, int speakersCount, ushort actionIndex)
        {
            if (action.EmotionName == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"<b><color=yellow>[Skiped EmotionAction]</color></b> EmotionName is null at [Conversation {_currentConversationIndex + 1}, {_currentPhase}, Action {actionIndex + 1}]");
#endif
                return;
            }

            var emotion = speakerData.Character.GetEmotion(action.EmotionName);
            if (emotion == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"<b><color=yellow>[Skiped EmotionAction]</color></b> Emotion '{action.EmotionName}' not found in '{speakerData.Character.CharacterName}'");
#endif
                return;
            }

            if (speakersCount == 1 && speakerData.NameDisplayMode != VNNameDisplayMode.None && _currentDialogueType == VNDialogueMode.Standard) //new: NameDisplayMode replaces ShowName
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

            speakerData.Character.SendVNEmotionSignel(emotion.Value.EmoteClip.name);
        }

        private void ProcessAnimationAction(VNAction action, VNSpeakerData speakerData, ushort actionIndex)
        {
            if (action.BehaviorAnimationName == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"<b><color=yellow>[Skiped AnimationAction]</color></b> AnimationName is null at [Conversation {_currentConversationIndex + 1}, {_currentPhase}, Action {actionIndex + 1}]");
#endif
                return;
            }

            var animation = speakerData.Character.GetAnimation(action.BehaviorAnimationName);
            if (animation == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"<b><color=yellow>[Skiped AnimationAction]</color></b> Animation '{action.BehaviorAnimationName}' not found in '{speakerData.Character.CharacterName}'");
#endif
                return;
            }

            speakerData.Character.SendVNAnimationSignal(animation.Value.BehaviorAnimationClip.name);
        }

        private void ProcessSoundEffectAction(VNAction action, VNSpeakerData speakerData, ushort actionIndex)
        {
            if (action.SoundEffectName == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"<b><color=yellow>[Skiped ActionSoundEffect]</color></b> SoundEffectName is null at [Conversation {_currentConversationIndex + 1}, {_currentPhase}, Action {actionIndex + 1}]");
#endif
                return;
            }

            var soundEffect = speakerData.Character.GetSoundEffect(action.SoundEffectName);
            if (soundEffect == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"<b><color=yellow>[Skiped ActionSoundEffect]</color></b> SoundEffect '{action.SoundEffectName}' not found in '{speakerData.Character.CharacterName}'");
#endif
                return;
            }

            speakerData.Character.SendVNSoundEffectSignel(soundEffect.Value.SoundEffectClip.name);
        }

        private void ApplyTextSettings(VNTextSettings settings)
        {
            if (settings == null || !settings.OverrideGlobalSettings || _currentDialogueTMP == null) return;
            if (settings.CustomFont != null) _currentDialogueTMP.font = settings.CustomFont;
            _currentDialogueTMP.fontSize = settings.FontSize;
            _currentDialogueTMP.color = settings.FontColor;
            _currentDialogueTMP.alignment = settings.Alignment;
        }

        #endregion //End Helper Functions Region

        #region Corotines Region

        /// <summary>
        /// <para> (TH) : ตรวจสอบประเภทของโหนดบทสนทนา (Dialogue หรือ Choice) และเริ่มการทำงานตามรูปแบบนั้น </para>
        /// <para> (EN) : Evaluates the conversation node type and routes to the appropriate execution. </para>
        /// </summary>
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

            if (_currentConversationIndex < _currentScene.Conversations.Count)
                StartCoroutine(PlayNodeRoutine(_currentScene.Conversations[_currentConversationIndex]));
        }

        /// <summary>
        /// <para> (TH) : ประมวลผลโหนดในรูปแบบการพูดคุยมาตรฐาน (Enter → Main → Exit) </para>
        /// <para> (EN) : Processes a standard dialogue node through phases (Enter → Main → Exit). </para>
        /// </summary>
        private IEnumerator DialogueModeRotine(VNDialogueNode node)
        {
            if (node.UseEnterPhase) yield return PlayPhaseRoutine(node.EnterPhase, VNCurrentPhase.EnterPhase);

            yield return PlayPhaseRoutine(node.MainPhase, VNCurrentPhase.MainPhase);

            if (node.UseExitPhase) yield return PlayPhaseRoutine(node.ExitPhase, VNCurrentPhase.ExitPhase);

            _currentConversationIndex++;

            if (_currentConversationIndex >= _currentScene.Conversations.Count)
            {
#if UNITY_EDITOR
                Debug.Log("<b><color=#A5D6A7>[VN Engine]</color></b> VNScene Finished!");
#endif
                yield break;
            }
        }

        /// <summary>
        /// <para> (TH) : ประมวลผลโหนดตัวเลือก QuestionState → AnswerState → รอเลือก → InteractState → SubConversation </para>
        /// <para> (EN) : Processes a choice node: Question → Answer (show choices) → wait → InteractState → SubConversation. </para>
        /// </summary>
        private IEnumerator ChoiceModeRotine(VNChoiceNode node)
        {
            // --- Question Phase ---
            if (node.QuestionState.UseEnterPhase)
                yield return PlayPhaseRoutine(node.QuestionState.EnterPhase, VNCurrentPhase.EnterPhase);

            yield return PlayPhaseRoutine(node.QuestionState.MainPhase, VNCurrentPhase.MainPhase);

            // --- Answer Phase (VNChoicePhaseData – ไม่มี DialogueText, ใช้แสดง choice buttons) ---
            if (node.AnswerState.UseEnterPhase)
                yield return PlayPhaseRoutine(node.AnswerState.EnterPhase, VNCurrentPhase.EnterPhase);

            yield return PlayPhaseRoutine(node.AnswerState.MainPhase, VNCurrentPhase.MainPhase);

            _choicePanel?.ShowChoices(node.AnswerState.Choices, SelectChoice); //new: show choice buttons to player
            _waitForChoiceInput = true;
            while (_waitForChoiceInput) yield return null;
            _choicePanel?.HideChoices(); //new: hide panel after selection

            if (node.AnswerState.UseExitPhase)
                yield return PlayPhaseRoutine(node.AnswerState.ExitPhase, VNCurrentPhase.ExitPhase);

            // --- หาค่า InteractState ที่ตรงกับคำตอบที่เลือก ---
            bool hasInteractMatch = false;
            VNInteractState matchedInteract = default;

            foreach (var interact in node.InteractStates)
            {
                if (interact.TargetAnswerNumber != _selectedAnswerNumber) continue;
                matchedInteract = interact;
                hasInteractMatch = true;
                break;
            }

            if (!hasInteractMatch) yield break;

            // --- เล่น SubConversation ตามคำตอบที่เลือก ---
            foreach (var subNode in matchedInteract.SubConversation)
                yield return PlayNodeRoutine(subNode);

            // --- วนกลับไปถามใหม่ถ้าตั้งค่าไว้ ---
            if (matchedInteract.ReturnToChoicePhase)
                yield return PlayPhaseRoutine(node.QuestionState.MainPhase, VNCurrentPhase.MainPhase);

            _currentConversationIndex++;

            if (_currentConversationIndex >= _currentScene.Conversations.Count)
            {
#if UNITY_EDITOR
                Debug.Log("<b><color=#A5D6A7>[VN Engine]</color></b> VNScene Finished!");
#endif
                yield break;
            }
        }

        /// <summary>
        /// <para> (TH) : สลับกล่องบทสนทนาและเตรียม UI ก่อนเริ่ม Phase </para>
        /// <para> (EN) : Toggles dialogue boxes and prepares UI before starting the phase. </para>
        /// </summary>
        /// <remarks>รับ VNChoicePhaseData เป็น base เพื่อรองรับทั้ง VNDialoguePhaseData และ VNChoicePhaseData</remarks>
        private IEnumerator DialogueSetup(VNChoicePhaseData phase)
        {
            // TextSettings และ DialogueText มีเฉพาะใน VNDialoguePhaseData (subclass)
            if (phase is VNDialoguePhaseData dialoguePhase)
                ApplyTextSettings(dialoguePhase.TextSettings);

            ApplyBackground(phase);

            bool isModeChanged = _currentDialogueType != phase.DialogueMode;

            if (isModeChanged && _currentDialogueType != VNDialogueMode.None && _currentDialogueAnimator != null)
            {
                var clipName = (phase.OverrideDialogueBoxAnimation && phase.DialogueBoxAnimation != null)
                    ? phase.DialogueBoxAnimation.name
                    : _hideDialogueClip?.name;

                if (clipName != null) _currentDialogueAnimator.Play(clipName);
                yield return new WaitForSeconds(1.2f);
            }

            _standardDialogueArea?.DialogueBox?.gameObject.SetActive(false);
            _logViewDialogueArea?.DialogueBox?.gameObject.SetActive(false);
            _cinematicDialogueArea?.DialogueBox?.gameObject.SetActive(false);

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

            _currentDialogueBox = dialogueArea.DialogueBox;
            _currentDialogueAnimator = dialogueArea.DialogueAnimator;
            _currentDialogueTMP = dialogueArea.DialogueTMP;
            _currentSpeakerNameTMP = dialogueArea.SpeakerNameTMP;
            _currentSpeakerNameIcon = dialogueArea.SpeakerNameIcon; //new
            _currentDialogueType = phase.DialogueMode;

            if (_currentDialogueTMP != null) { _currentDialogueTMP.text = string.Empty; _currentDialogueTMP.maxVisibleCharacters = 0; }
            if (_currentSpeakerNameTMP != null) _currentSpeakerNameTMP.text = string.Empty;
            if (_currentSpeakerIcon != null) { _currentSpeakerIcon.sprite = null; _currentSpeakerIcon.gameObject.SetActive(false); }
            if (_currentSpeakerNameIcon != null) { _currentSpeakerNameIcon.sprite = null; _currentSpeakerNameIcon.gameObject.SetActive(false); } //new
            if (_currentDialogueBox != null) _currentDialogueBox.gameObject.SetActive(true);

            if (isModeChanged && _currentDialogueAnimator != null)
            {
                var clipName = (phase.OverrideDialogueBoxAnimation && phase.DialogueBoxAnimation != null)
                    ? phase.DialogueBoxAnimation.name
                    : _showDialogueClip?.name;

                if (clipName != null) _currentDialogueAnimator.Play(clipName);
                yield return new WaitForSeconds(1.2f);
            }
        }

        /// <summary>
        /// <para> (TH) : ประมวลผลการทำงานทั้งหมดภายใน Phase เดียว (ตัวละคร, ข้อความ, เหตุการณ์) </para>
        /// <para> (EN) : Processes all logic within a single phase (characters, text, events). </para>
        /// </summary>
        /// <remarks>
        /// รับ VNChoicePhaseData เป็น base — ถ้าเป็น VNDialoguePhaseData จะแสดง dialogue text ด้วย
        /// ถ้าเป็นแค่ VNChoicePhaseData (เช่น AnswerState.MainPhase) จะข้าม typing effect
        /// </remarks>
        private IEnumerator PlayPhaseRoutine(VNChoicePhaseData phase, VNCurrentPhase currentPhaseEnum)
        {
            _currentPhase = currentPhaseEnum;

            yield return DialogueSetup(phase);

            var namesBuilder = SetupSpeakers(phase);
            TriggerPhaseEvents(phase);

            // แสดง dialogue text เฉพาะเมื่อ phase เป็น VNDialoguePhaseData และโหมดพร้อมแสดงผล
            if (phase is VNDialoguePhaseData dialoguePhase
                && _currentDialogueType != VNDialogueMode.None
                && _currentDialogueTMP != null)
            {
                int visibleCount = FormatDialogueText(dialoguePhase, namesBuilder);
                yield return PlayTypingEffect(dialoguePhase, visibleCount);
            }
            else
            {
                yield return null;
            }
        }

        /// <summary>
        /// <para> (TH) : รันเอฟเฟกต์เครื่องพิมพ์ดีดสำหรับข้อความ พร้อมรอให้ผู้เล่นกดยืนยัน </para>
        /// <para> (EN) : Runs the typewriter text effect and waits for player confirmation input. </para>
        /// </summary>
        private IEnumerator PlayTypingEffect(VNDialoguePhaseData phase, int visibleCount)
        {
            _isTyping = true;
            _skipTyping = false;

            _currentDialogueTMP.ForceMeshUpdate();
            int totalVisibleCharacters = _currentDialogueTMP.textInfo.characterCount;

            while (visibleCount < totalVisibleCharacters)
            {
                if (_skipTyping)
                {
                    _currentDialogueTMP.maxVisibleCharacters = totalVisibleCharacters;
                    _audioObserver.SendSfxSignal(null);
                    _audioObserver.SendVoiceoverSignal(null);
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
        /// <para> (TH) : วนลูปอ่านและใช้งานคำสั่ง (Action) ต่างๆ ของตัวละครภายในเฟสนั้นๆ </para>
        /// <para> (EN) : Iterates and executes the list of actions defined for a character within the current phase. </para>
        /// </summary>
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
                        Debug.LogWarning($"<b><color=yellow>[Skiped ActionDelay]</color></b> DelayTime is 0 or Negative at [Conversation {_currentConversationIndex + 1}, {_currentPhase}, Action {actionIndex + 1}]");
#endif
                    }
                    else yield return new WaitForSeconds(action.DelayTime);
                }

                actionIndex++;
                yield return null;
            }
        }

        #endregion //End Corotines Region
    }
}