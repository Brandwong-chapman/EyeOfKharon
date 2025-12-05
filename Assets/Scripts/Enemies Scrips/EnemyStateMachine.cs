public class EnemyStateMachine
{
    private IEnemyState _currentState;
    private readonly EnemyController _enemy;

    public EnemyStateMachine(EnemyController enemy)
    {
        _enemy = enemy;
        //ChangeState(new IdleState());
    }

    public void Update()
    {
        _currentState?.Update();
    }

    public void ChangeState(IEnemyState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter(_enemy);
    }
}

public interface IEnemyState
{
    void Enter(EnemyController enemy);
    void Update();
    void Exit();
}
