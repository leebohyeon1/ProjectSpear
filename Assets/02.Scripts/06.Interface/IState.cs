public interface IState
{
    void Enter();    // ���¿� ������ �� ����Ǵ� �޼���
    void Execute();  // ���°� �����Ǵ� ���� ����Ǵ� �޼���
    void Exit();     // ���¿��� ��� �� ����Ǵ� �޼���
}
