# EDITLOG.md — AI Edit History

บันทึกการเปลี่ยนแปลงที่ AI ทำใน library นี้ทุก version  
เก็บสะสมไปเรื่อยๆ ห้ามลบ entry เก่า

---

## กฎการเขียน EDITLOG

### ต้องเขียนเมื่อไหร่
- AI เพิ่ม / แก้ไข / ลบ ไฟล์ใดก็ตามใน library
- รวมถึง AGENT.md, SKILL.md, EDITLOG.md เอง

### รูปแบบ entry

```
## [vX.Y.Z] — YYYY-MM-DD
**AI:** <ชื่อ model ที่ทำ>

### Added
- <สิ่งที่เพิ่มใหม่>

### Changed
- <สิ่งที่แก้ไข — ระบุว่าเปลี่ยนจากอะไรเป็นอะไร>

### Removed
- <สิ่งที่ลบออก>

### Fixed
- <bug หรือ issue ที่แก้>
```

### กฎ versioning

| เปลี่ยนอะไร | version bump |
|-------------|-------------|
| เพิ่ม Core System / Pattern ใหม่ | Minor `X.Y+1.0` |
| แก้ bug / เปลี่ยน implementation เล็กน้อย | Patch `X.Y.Z+1` |
| เปลี่ยน namespace, assembly, หรือ public API | Major `X+1.0.0` |
| แก้ doc / comment / AGENT.md เท่านั้น | Patch `X.Y.Z+1` |

Version ปัจจุบันดูได้ที่ `package.json` → `"version"`  
**อัปเดต `package.json` version พร้อมกันทุกครั้ง**

---

## [0.2.49] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Changed
- `AudioManager` — เปลี่ยนจาก `Input.GetMouseButtonDown` (legacy) เป็น subscribe `BasicMovementInputObserverSO.OnLeftClickChannel` (New Input System); ลบ `Update()` ออก; เพิ่ม `_basicObserverChannel` field

---

## [0.2.48] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Changed
- `AudioManager` — เพิ่ม `_typing` + `_click` AudioSource; subscribe `OnTypingSfxChannel`; เพิ่ม `OnDisable` unsubscribe ทุก channel; เพิ่ม `Update()` ตรวจ `Input.GetMouseButtonDown(0)` เพื่อเล่น `_clickSfx` globally (ทุก Scene คลิกที่ไหนก็ได้)
- `VNSceneReader` — ลบ `ClickSfx` field และ `SendClickSfxSignal` call ออก (AudioManager จัดการ click แทน)

### Added
- `AudioManager._clickSfx` (AudioClip, Inspector) — clip ที่เล่นทุกครั้งที่คลิกซ้าย

---

## [0.2.47] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Added
- `AudioObserverSO.OnTypingSfxChannel` (Action<AudioClip>) + `SendTypingSfxSignal` — channel แยกสำหรับเสียงพิมพ์ ควบคุม volume ได้อิสระจาก SFX ปกติ
- `AudioObserverSO.OnClickSfxChannel` (Action<AudioClip>) + `SendClickSfxSignal` — channel แยกสำหรับเสียงคลิก/continue
- `VNSceneReader.ClickSfx` (AudioClip) — ใส่เสียงคลิกใน inspector
- `VNSceneReader.PlayTypingEffect` — เรียก `SendTypingSfxSignal(TypingSfx)` ทุก character ขณะพิมพ์
- `VNSceneReader.HandleInput` — เรียก `SendClickSfxSignal(ClickSfx)` เมื่อผู้เล่นกด continue

---

## [0.2.46] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Fixed
- `VNCharacterController.OnDisable` — guard `if (EventBus.Instance != null)` ก่อน Unsubscribe เพื่อป้องกัน NullReferenceException เมื่อปิดเกมและ EventBus ถูก destroy ก่อน

---

## [0.2.45] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Changed
- `VNSceneReader` — เปลี่ยน relationship display จาก static `UpdateRelationshipHearts` เป็นระบบ blink-then-hide: หัวใจซ่อนตลอดเวลา, แสดงและกระพริบเฉพาะเมื่อ `ChangeRelationship` ถูกเรียก, หลังครบ `_blinkDuration` (default 20s) ซ่อนอัตโนมัติ
- `VNSceneReader.SetupSpeakers` — ลบการเรียก `UpdateRelationshipHearts` ออก (ไม่แสดงหัวใจเมื่อเปลี่ยน phase)
- `VNSceneReader.ChoiceModeRotine` — เปลี่ยนจาก `UpdateRelationshipHearts` เป็น `ShowRelationshipBlink`

### Added
- `VNSceneReader._relationshipDisplayRoot` (GameObject, optional) — parent container ของ hearts, ถ้าใส่จะ SetActive ทั้งกลุ่ม; ถ้าไม่ใส่จะ SetActive รายตัว
- `VNSceneReader._blinkSpeed` (float, default 2) — ความเร็วกระพริบ (ครั้งต่อวินาที)
- `VNSceneReader._blinkDuration` (float, default 20) — ระยะเวลาแสดงผล (วินาที)
- `VNSceneReader.ShowRelationshipBlink(int)` — เริ่ม coroutine, หยุด coroutine เก่าก่อนถ้ามี
- `VNSceneReader.RelationshipBlinkRoutine(int)` — แสดงหัวใจตาม value, loop กระพริบด้วย `Image.enabled`, ซ่อนหลังครบ duration

---

## [0.2.44] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Changed
- `VNSceneReader` — ลบ `_relationshipCharacter` field ออก; ใช้ speaker แรกของ phase เป็น source ของ `UpdateRelationshipHearts` แทน (ใน `SetupSpeakers`); อัปเดต hearts ทันทีหลัง `AddRelationship` ใน `ChoiceModeRotine`
- `VNSceneReader.SetupSpeakers` — เพิ่ม `VNNameDisplayMode.CustomText` case: ใช้ `speakerData.CustomName` เป็นชื่อแสดงผล

### Added
- `VNNameDisplayMode.CustomText` — mode ใหม่ให้พิมพ์ชื่อผู้พูดเองได้
- `VNSpeakerData.CustomName` (string) — แสดงในฟิลด์เมื่อเลือก `CustomText`
- `VNSpeakerDataDrawer` — แสดง `Custom Name` field เมื่อ mode เป็น `CustomText` + อัปเดต height

---

## [0.2.43] — 2026-05-20
**AI:** Claude Sonnet 4.6

### Changed
- ย้าย relationship display logic จาก `VNRelationshipDisplay` (ลบแล้ว) เข้า `VNSceneReader` โดยตรง
- `VNSceneReader` — เพิ่ม `_relationshipCharacter` (VNCharacterSO) + `_relationshipHearts` (List<Image>); subscribe `OnRelationshipChanged` ใน OnEnable/OnDisable; `UpdateRelationshipHearts(int)` loop SetActive

### Removed
- `VNRelationshipDisplay.cs` — ไม่ใช้แล้ว ย้าย logic เข้า VNSceneReader

---

## [0.2.42] — 2026-05-20
**AI:** Claude Sonnet 4.6

### Added
- `VNCharacterSO.RelationshipValue` (int 0–10, serialized) + `OnRelationshipChanged` (Action<int>) + `AddRelationship(int delta)` — clamp 0–10 แล้ว invoke channel
- `VNInteractState.ChangeRelationship` (bool) + `RelationshipCharacter` (VNCharacterSO) + `RelationshipDelta` (int) — เพิ่ม/ลดค่าความสัมพันธ์เมื่อผู้เล่นเลือก choice นั้น
- `VNSceneReader.ChoiceModeRotine` — เรียก `AddRelationship` ทันทีหลัง matchedInteract ถูกเลือก
- `VNRelationshipDisplay` — MonoBehaviour subscribe `OnRelationshipChanged`; loop `List<Image>` hearts SetActive(i < value)
- `VNInteractStateDrawer` — draw/reset/height สำหรับ ChangeRelationship + RelationshipCharacter + RelationshipDelta

---

## [0.2.41] — 2026-05-20
**AI:** Claude Sonnet 4.6

### Added
- `VNCutSceneMode` enum (`On` / `Off`) ใน Runtime/Core/VN_Core/Scripts/Enums/
- `VNChoicePhaseData.UseCutScene` (bool) + `CutSceneMode` (VNCutSceneMode) — เพิ่มหลัง `UseEnterName`
- `VNSceneReader.TriggerPhaseEvents` — publish `VNCutSceneEvent(CutSceneMode == On)` เมื่อ `UseCutScene = true`

### Fixed
- `VNConversationNodeDrawer.DrawPhaseContent` — แก้ `UseEnterName` ใช้ `EditorGUI.ToggleLeft` แทน `PropertyField` ให้ checkbox ตรงแถวเดียวกับ bool fields อื่น

---

## [0.2.40] — 2026-05-20
**AI:** Claude Sonnet 4.6

### Added
- `VNCutSceneEvent` — record struct `(bool IsCutScene) : IEvent` ใน Runtime/Core/VN_Core/Scripts/Event/
- `VNCharacterController.OnCutScene` — subscribe `VNCutSceneEvent` จาก EventBus: `IsCutScene = true` → ซ่อน `_characterSprite` GameObject, `false` → แสดงกลับ

---

## [0.2.39] — 2026-05-20
**AI:** Claude Sonnet 4.6

### Fixed
- `VNSceneReader.ChoiceModeRotine` — แก้ SubConversation เล่นแค่ node แรก: เปลี่ยนจาก `yield return PlayNodeRoutine(subNode)` เป็น `yield return PlaySubNodeRoutine(subNode)` ใน foreach loop
- เพิ่ม `PlaySubNodeRoutine` และ `PlaySubDialogueRoutine` — เล่น SubConversation node โดยไม่แตะ `_currentConversationIndex` และไม่ `StartCoroutine` main-list ต่อ ทำให้ foreach วน SubConversation ทุก node ได้ครบ

---

## [0.2.38] — 2026-05-20
**AI:** Claude Sonnet 4.6

### Fixed
- `VNSceneReader.ChoiceModeRotine` — แก้ `ReturnToChoicePhase` ไม่วนกลับ: เปลี่ยนจาก replay MainPhase ครั้งเดียว เป็น `while (returnToChoice)` loop ครอบทั้ง Question→Choices→SubConversation; EnterPhase ยังคงวิ่งครั้งเดียวก่อน loop
- `VNSceneReader.DialogueSetup` — แก้ข้อความคำถามหายตอนแสดง choices: `VNChoicePhaseData` (AnswerState phases) จะ `yield break` ก่อนยุ่ง dialogue box ทำให้ text คงอยู่จนเลือกได้
- `VNSceneReader.HandleInput` — แก้ Continue รั่วขณะ choices แสดง: เพิ่ม guard `if (_waitForChoiceInput || _waitForNameInput) return` ต้นฟังก์ชัน

---

## [0.2.34] — 2026-05-19
**AI:** Claude Sonnet 4.6

### Fixed
- `VNInteractStateDrawer` — แก้ bug `[SerializeReference]` shared reference ใน `List<VNInteractState>`: เมื่อ Unity duplicate element ใน array managed reference IDs จะ copy กัน ทำให้ทุก element ใน list ชี้ไปยัง `SubConversation` object เดียวกัน
- เพิ่ม `CheckAndClearIfNewlyAdded()` method ตรวจจับการเพิ่ม element ใหม่ (array size เพิ่มขึ้น) แล้วเรียก `ResetInteract()` บน element นั้นทันที เพื่อตัด shared reference และให้ข้อมูลเริ่มต้นสะอาด

---

## [0.2.33] — 2026-05-19
**AI:** Claude Sonnet 4.6

### Added
- `BasicMoveAbility3D` — `BypassCameraTransform` bool: เมื่อ `true` จะใช้ input เป็น world-space ตรงๆ ไม่แปลงผ่าน camera transform เหมาะสำหรับ AI ที่ส่ง direction เป็น world-space โดยตรง

---

## [0.2.32] — 2026-05-18
**AI:** Claude Sonnet 4.6

### Removed
- `BaseMoveAbility2D` — ลบ Damage Flash ออก (ซ้ำกับ StatsController Color Lerp)
- `BaseMoveAbility3D` — ลบ Damage Flash ออก (ซ้ำกับ StatsController Color Lerp)

---

## [0.2.31] — 2026-05-18
**AI:** Claude Sonnet 4.6

### Added
- `MoveController3D` — Ground Check: `_enableJump`, `_groundCheck`, `_groundRadius`, `_groundLayer`, `UpdateGroundCheck()` ใน FixedUpdate ด้วย `Physics.CheckSphere`
- `MoveController3D` — Gizmo sphere (สีเขียว = grounded, แดง = airborne) เหมือน 2D

---

## [0.2.30] — 2026-05-18
**AI:** Claude Sonnet 4.6

### Added
- `BaseMoveAbility2D` — Damage Flash สำหรับ SpriteRenderer: `Flash()` protected method, coroutine lerp, Inspector fields (_flashTarget, _flashColor, _flashDuration), auto-cache ใน `Initialize()`
- `BaseMoveAbility3D` — Damage Flash สำหรับ MeshRenderer + MaterialPropertyBlock: ไม่ GC spike, รองรับ custom shader property name

### Removed
- `DirectionMode.OneDiraction` — ซ้ำซ้อนกับ `None` (SnapDirection คืน rawInput เหมือนกันทั้งคู่)
- `DirectionModeExtension.ToByte()` case `OneDiraction => 1`

---

## [0.2.28] — 2026-05-17
**AI:** Claude Sonnet 4.6

### Changed
- Renamed namespace root `Genoverrei.Library.*` → `Kogetsu.Library.*` ทุกไฟล์ใน Runtime และ Editor
- Renamed assembly `GenoverreiLibrary` → `KogetsuLibrary`, `GenoverreiLibrary.Editor` → `KogetsuLibrary.Editor`
- Updated `rootNamespace` ใน asmdef ทั้งสองตัว
- Updated `m_EditorClassIdentifier` ใน scene/prefab/asset files ใน Assets/
- Updated `using` statements ใน `ProjectGlobalUsing.cs` และ `SkillQController.cs`
- Updated tags `*.GenoverreiLibrary` → `*.KogetsuLibrary` ใน TagManager.asset และ prefabs
- Updated `productName` ใน ProjectSettings.asset: `GenoverreiLibary6` → `KogetsuLibrary`
- Updated `package.json` name: `com.genoverrei.library` → `com.kogetsu.library`

### Added
- `ignore.conf` — UVC ignore rules สำหรับ `.claude/`, git, IDE files
- `SKILL.md` — Claude Code skill สำหรับสร้าง module ใหม่ตาม library convention
- `EDITLOG.md` — ไฟล์นี้ (AI edit history)
- Section 0 "Library Overview" ใน AGENT.md — สรุปสถาปัตยกรรม, Observer SO pattern, EventBus flow

### Fixed
- แก้ `com.kogetsu.library` ที่ถูก git track เป็น submodule → เปลี่ยนเป็น regular files ใน main repo
- แก้ git remote URL จาก `genoverrei/GenoverreiLibary6` → `GenKogetsu/KogetsuLibrary`
- แก้ Windows Credential Manager ให้ใช้ account `GenKogetsu` แทน `genoverrei`
