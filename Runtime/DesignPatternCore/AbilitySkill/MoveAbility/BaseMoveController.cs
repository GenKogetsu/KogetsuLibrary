using Genoverrei.Library.Attribute;
using Genoverrei.Library.DesignPatternCore;

[RequireComponent(typeof(StatsController))]
public abstract class BaseMoveController<TContext, TAbility> : MonoBehaviour
        where TContext : class, IMoveContext
        where TAbility : BaseMoveAbility<TContext>
{
    [ReadOnly]
    [SerializeField] protected StatsController StatsController;

    [Header("Observer Channels")]
    [SerializeField] protected InputObserverChannelSO InputObserverChannel;


    [Header("Abilities")]
    [SubclassSelector]
    [SerializeReference] protected TAbility StartingAbility;

    public Transform Transform => transform;
    public IMoveStatsProvider Stats { get; protected set; }

    protected StateMachine<TContext> StateMachine;
    protected Vector3 CurrentInput;

    protected virtual void OnValidate()
    {
        if (StateMachine == null) this.TryGetComponent<StatsController>(out StatsController);
    }

    protected virtual void Awake()
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

    protected abstract TContext GetContext();

    protected virtual void OnEnable()
    {
        if (InputObserverChannel != null) InputObserverChannel.OnMoveChannel += HandleMoveInput;
        if (InputObserverChannel != null) InputObserverChannel.OnJumpChannel += HandleJumpInput;
    }

    protected virtual void OnDisable()
    {
        if (InputObserverChannel != null) InputObserverChannel.OnMoveChannel -= HandleMoveInput;
        if (InputObserverChannel != null) InputObserverChannel.OnJumpChannel -= HandleJumpInput;
    }

    protected virtual void HandleMoveInput(Vector3 input) => CurrentInput = input;

    protected virtual void HandleJumpInput()
    {
        if (StateMachine.CurrentState is BaseMoveAbility<TContext> current) current.ExecuteJump();
    }

    protected virtual void Update()
    {
        if (StateMachine.CurrentState is BaseMoveAbility<TContext> current) current.SetInput(CurrentInput);
        StateMachine.Update();
    }

    protected virtual void FixedUpdate() => StateMachine.FixedUpdate();
}