public interface IState
{
    void Enter();    // 상태에 진입할 때 실행되는 메서드
    void Execute();  // 상태가 유지되는 동안 실행되는 메서드
    void Exit();     // 상태에서 벗어날 때 실행되는 메서드
}
