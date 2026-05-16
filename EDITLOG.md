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
