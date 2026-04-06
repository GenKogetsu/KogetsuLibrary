using Genoverrei.Library.Attribute;
using Genoverrei.Library.Core;
using Genoverrei.Library.DesignPatternCore;

[RequireComponent(typeof(StatsController))]
public abstract class BaseMoveController<TContext, TAbility> : MonoBehaviour
        where TContext : class, IMoveContext
        where TAbility : BaseMoveAbility<TContext>
{
    [ReadOnly]
    [SerializeField] protected StatsController StatsController;

    [Header("Abilities")]
    [MoveAbilitySelector]
    [SerializeReference] protected TAbility StartingAbility;

    public Transform Transform => transform;
    public IMoveStatsProvider Stats { get; protected set; }

    protected StateMachine<TContext> StateMachine;
    protected abstract TContext GetContext();

    protected virtual void AbilitySetUp()
    {
        if (StateMachine == null) this.TryGetComponent<StatsController>(out StatsController);

        StateMachine = new StateMachine<TContext>();
        Stats = StatsController;

        if (StartingAbility != null)
        {
            StartingAbility.Initialize(GetContext());
            StateMachine.ChangeState(StartingAbility);
        }
    }

    protected virtual void OnValidate()
    {
        if (StateMachine == null) this.TryGetComponent<StatsController>(out StatsController);
    }

    protected virtual void Awake()
    {
        AbilitySetUp();
    }

    protected virtual void Update()
    {
        StateMachine.Update();
    }

    protected virtual void FixedUpdate() => StateMachine.FixedUpdate();
}