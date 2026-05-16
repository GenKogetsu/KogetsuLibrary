# AGENT.md — AI Coding Guide

คู่มือนี้ให้ AI ของทุกคนในทีม generate code ได้ถูกต้องตาม convention ของ project นี้  
อ่านไฟล์นี้ก่อนทำงานทุกครั้ง — ห้าม guess ว่า style คืออะไร

---

## 1. Project Overview

| Item | Value |
|------|-------|
| Package | `com.genoverrei.library` |
| Unity | 6000.0+ (Unity 6) |
| C# | **10.0** (enforced via `csc.rsp`) |
| Root namespace | `Genoverrei.Library` |
| Game project | **Yanta** — Horror 3D, ผู้เล่นวาดยันต์ไทยเป็น spell |

Repository layout:
```
Runtime/
  DesignPatternCore/   ← StateMachine, EventBus, ObjectPool, Singleton
  Core/                ← MoveCore, InputCore, AudioCore, StatsCore …
Editor/
  Assistant/           ← Doc/Tag tools
```

---

## 2. Namespace Convention

> **Rule**: file-scoped namespace สำหรับทุกไฟล์ **ยกเว้น** MonoBehaviour และ ScriptableObject

```csharp
// ✅ Regular class / interface / enum / record struct → file-scoped
namespace Genoverrei.Library.DesignPatternCore;

public interface ISpellEffect { ... }
public record struct SpellCastEvent(SpellType Type, float Accuracy) : IEvent;
public enum SpellType { Stun, Heal, Invisible }
```

```csharp
// ✅ MonoBehaviour / ScriptableObject → block namespace (Unity requirement)
namespace Yanta.SpellSystem
{
    public class SpellCaster : MonoBehaviour { ... }
    public class SpellConfigSO : ScriptableObject { ... }
}
```

---

## 3. Naming Convention

| สิ่งที่ตั้งชื่อ | รูปแบบ | ตัวอย่าง |
|----------------|--------|----------|
| Private field | `_camelCase` | `_attackTimer`, `_currentState` |
| SerializeField | `[SerializeField]` + `_camelCase` | `[SerializeField] float _speed` |
| Property | `PascalCase` | `CurrentState { get; private set; }` |
| Method | `PascalCase` | `void OnEnter()`, `float GetSpeed()` |
| Interface | `I` + `PascalCase` | `ISpellEffect`, `IEnemy` |
| Event record | `PascalCase` + `Event` suffix | `SpellCastEvent`, `PlayerDamagedEvent` |
| Abstract class | ไม่มี prefix พิเศษ | `BaseState<T>`, `EnemyDecorator` |
| SO class | `PascalCase` + `SO` suffix | `SpellConfigSO`, `EnemyDataSO` |
| Enum member | `PascalCase` | `SpellType.Stun` |

---

## 4. C# 10 Syntax ที่ต้องใช้

### 4.1 Fields และ Properties

```csharp
// ✅ ใช้ [SerializeField] private — ไม่ใช้ public field เปล่า ๆ
[SerializeField] private float _moveSpeed = 5f;
[SerializeField] private Transform _target;

// ✅ Auto-property สำหรับ read-only จากภายนอก
public float CurrentHp { get; private set; }
public BaseState<TContext> CurrentState { get; private set; }

// ✅ Target-typed new
private readonly Dictionary<string, Action<IEvent>> _handlers = new();
private readonly List<ISpellEffect> _effects = new();
```

### 4.2 Events (IEvent)

```csharp
// ✅ record struct เสมอ — ห้ามใช้ class หรือ struct ธรรมดา
public record struct SpellCastEvent(SpellType Type, float Accuracy) : IEvent;
public record struct PlayerDamagedEvent(float Amount, float CurrentHp) : IEvent;
public record struct EnemyAlertEvent(Vector3 Position) : IEvent;
```

### 4.3 Pattern Matching

```csharp
// ✅ ใช้ is pattern แทน casting
if (CurrentState is IExitState exitState) exitState.OnExit();
if (energy is { Current: <= 0f }) return;

// ✅ switch expression
string label = spellType switch {
    SpellType.Stun      => "สตัน",
    SpellType.Heal      => "ฮีล",
    SpellType.Invisible => "ซ่อนตัว",
    _                   => "ไม่รู้จัก"
};
```

### 4.4 อื่น ๆ

```csharp
// ✅ Expression body สำหรับ method บรรทัดเดียว
public virtual void Initialize(TContext ctx) => Context = ctx;
public float GetDuration() => _duration * _accuracyScale;

// ✅ Null-coalescing
private Transform _cachedTransform;
public Transform GetTransform() => _cachedTransform ??= transform;
```

---

## 5. Design Pattern Core — วิธีใช้ที่ถูกต้อง

### 5.1 StateMachine\<TContext\>

```csharp
// TContext = MonoBehaviour ที่เป็นเจ้าของ FSM
// state ทุกตัว extend BaseState<TContext>

namespace Yanta.EnemySystem
{
    public class EnemyControllerLv1 : MonoBehaviour
    {
        private StateMachine<EnemyControllerLv1> _fsm = new();

        private void Start()
        {
            var patrol = new PatrolState();
            patrol.Initialize(this);        // ส่ง context ครั้งเดียว
            _fsm.ChangeState(patrol);
        }

        private void Update()   => _fsm.Update();
        private void FixedUpdate() => _fsm.FixedUpdate();
    }
}
```

```csharp
// ✅ State ที่ถูกต้อง — implement เฉพาะ interface ที่ต้องการ
namespace Yanta.EnemySystem;

public class PatrolState : BaseState<EnemyControllerLv1>, IEnterState, IUpdateState, IExitState
{
    private float _waitTimer;

    public void OnEnter()
    {
        _waitTimer = 0f;
        // เข้าถึง context ผ่าน this.Context (ไม่มี parameter)
        Context.NavAgent.isStopped = false;
    }

    public void OnUpdate()
    {
        _waitTimer += Time.deltaTime;
        if (Context.DetectPlayer()) Context.Fsm.ChangeState(Context.AlertState);
    }

    public void OnExit() => Context.NavAgent.isStopped = true;
}
```

```csharp
// ❌ ห้ามทำแบบนี้ — context-per-call style (ของเก่าที่ไม่ใช้แล้ว)
public interface IEnemyState {
    void Enter(EnemyController ctx);   // ❌ wrong
    void Update(EnemyController ctx);  // ❌ wrong
}
```

### 5.2 HFSM (Hierarchical FSM) — Enemy Lv2

```csharp
// CombatSuperState เป็น BaseState ของ outer FSM
// มี StateMachine<T> ภายในสำหรับ sub-states
namespace Yanta.EnemySystem;

public class CombatSuperState : BaseState<EnemyControllerLv2>, IEnterState, IUpdateState, IExitState
{
    private StateMachine<EnemyControllerLv2> _subFsm = new();
    private AttackSubState _attackSub = new();
    private DodgeSubState _dodgeSub = new();

    public override void Initialize(EnemyControllerLv2 ctx)
    {
        base.Initialize(ctx);
        _attackSub.Initialize(ctx);
        _dodgeSub.Initialize(ctx);
    }

    public void OnEnter()  => _subFsm.ChangeState(_attackSub);
    public void OnUpdate() => _subFsm.Update();
    public void OnExit()   { }
}
```

### 5.3 EventBus

```csharp
// Subscribe ใน OnEnable, Unsubscribe ใน OnDisable เสมอ
private void OnEnable()
{
    EventBus.Instance.Subscribe<SpellCastEvent>(OnSpellCast);
    EventBus.Instance.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
}

private void OnDisable()
{
    EventBus.Instance.Unsubscribe<SpellCastEvent>(OnSpellCast);
    EventBus.Instance.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamaged);
}

private void OnSpellCast(SpellCastEvent e) => PlaySFX($"spell_{e.Type}");
```

```csharp
// Publish
EventBus.Instance.Publish(new SpellCastEvent(SpellType.Stun, accuracy: 0.85f));
```

### 5.4 ObjectPoolManager

```csharp
// ต้องมี ObjectPoolManager GameObject ใน Scene
// ต้องมี PoolTableDataSO assign ไว้
Enemy enemy = ObjectPoolManager.Instance.Get<Enemy>(enemyType.ToString(), pos, rot);
ObjectPoolManager.Instance.Release(key, enemyComponent);
```

### 5.5 Singleton\<T\>

```csharp
namespace Yanta.SpellSystem
{
    public class SpellCaster : Singleton<SpellCaster>
    {
        // ใช้งาน: SpellCaster.Instance.Cast(...)
    }
}
```

---

## 6. Yanta — Game Events ทั้งหมด

```csharp
namespace Yanta.Events;

// Player
public record struct PlayerDamagedEvent(float Amount, float CurrentHp)  : IEvent;
public record struct PlayerHealedEvent(float Amount, float CurrentHp)   : IEvent;
public record struct PlayerInvisibleEvent(bool IsActive)                 : IEvent;

// Spell
public record struct SpellCastEvent(SpellType Type, float Accuracy)      : IEvent;
public record struct SpellCancelledEvent(SpellType Type)                 : IEvent;
public record struct DrawingCompleteEvent(SpellType Type, float Accuracy): IEvent;

// Enemy
public record struct EnemyAlertEvent(Vector3 Position)                   : IEvent;
public record struct EnemyDespawnedEvent(EnemyType Type)                 : IEvent;

// Game
public record struct PuzzleSolvedEvent(string PuzzleId)                  : IEvent;
public record struct GameStateChangedEvent(GameState State)              : IEvent;
```

---

## 7. Spell System — ISpellEffect

```csharp
namespace Yanta.SpellSystem;

public interface ISpellEffect
{
    void Apply(Transform target);
    void Cancel();
    float GetDuration();
}

// ตัวอย่าง concrete effect
public class StunSpellEffect : ISpellEffect
{
    private readonly float _duration;
    private readonly float _accuracyScale;

    public StunSpellEffect(float baseDuration, float accuracy)
    {
        _duration = baseDuration;
        _accuracyScale = accuracy;
    }

    public void Apply(Transform target) { /* stun logic */ }
    public void Cancel()               { /* remove stun */ }
    public float GetDuration()         => _duration * _accuracyScale;
}
```

---

## 8. ScriptableObject Convention

```csharp
// SO สำหรับ config data — ชื่อลงท้าย SO
namespace Yanta.SpellSystem
{
    [CreateAssetMenu(menuName = "Yanta/SpellConfig", fileName = "SpellConfig")]
    public class SpellConfigSO : ScriptableObject
    {
        public SpellType spellType;        // public field ไม่มี _ (SO data field)
        public float baseDuration;
        public Sprite icon;
        public Texture2D drawPattern;

        // Prototype clone — ถ้า SO นั้นใช้เป็น template
        public SpellData Clone() => new() {
            spellType    = this.spellType,
            baseDuration = this.baseDuration,
        };
    }
}
```

> SO fields ใช้ `public` แบบไม่มี underscore (Unity serialization convention)  
> ต่างจาก MonoBehaviour ที่ใช้ `[SerializeField] private _camelCase`

---

## 9. Comment Policy

```csharp
// ✅ เขียน comment เฉพาะเมื่อ WHY ไม่ชัดเจน
// Unity NavMeshAgent ต้องให้ isStopped = true ก่อน destroy ไม่งั้น crash
agent.isStopped = true;

// ❌ ห้าม comment อธิบาย WHAT (ชื่อ method บอกอยู่แล้ว)
// Reset the timer   ← ไม่จำเป็น
_timer = 0f;

// ❌ ห้าม multi-line comment block / XML doc สำหรับ method ทั่วไป
```

---

## 10. ไฟล์อ้างอิงสำคัญ

| ไฟล์ | ประโยชน์ |
|------|---------|
| `README.md` | ภาพรวม library + Yanta showcase |
| `LibrarySkill.md` (memory) | API reference ครบทุก core — อ่านแทนโค้ด |
| `C:/Users/genzo/Downloads/yanta-class-diagrams_2.html` | Class diagram SVG ทั้ง 10 ระบบ (dark blue theme) |
| `Runtime/DesignPatternCore/StateMachine/` | StateMachine, BaseState, IState interfaces |
| `Runtime/DesignPatternCore/EventBus/` | EventBus, IEvent |
| `Runtime/DesignPatternCore/ObjectPool/` | ObjectPoolManager, ObjectPool\<T\> |
| `Runtime/DesignPatternCore/Singleton/` | Singleton\<T\> |

---

## 11. ก่อน Generate Code — Checklist

- [ ] namespace ถูก? (file-scoped vs block ตาม base class)
- [ ] field ใช้ `_camelCase` + `[SerializeField]` ถูก?
- [ ] event เป็น `record struct : IEvent`?
- [ ] state ไม่มี context parameter ใน method (ใช้ `Context` property แทน)?
- [ ] Subscribe/Unsubscribe EventBus จับคู่ใน OnEnable/OnDisable?
- [ ] SO field เป็น `public` ไม่มี underscore?
- [ ] comment มีเฉพาะที่ WHY ไม่ชัดเจน?
