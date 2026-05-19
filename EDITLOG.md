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
