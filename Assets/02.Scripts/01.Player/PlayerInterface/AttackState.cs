using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : IState
{
    private PlayerController player;

    public AttackState(PlayerController player)
    {
        this.player = player;
    }

    public void Enter()
    {
        //player.animator.SetTrigger("Attack");
    }

    public void Execute()
    {
        // ���� �� ��� ���·� ��ȯ
        player.ChangeState(new IdleState(player));
    }

    public void Exit()
    {
        // �ʿ� �� ���� �߰�
    }
}
