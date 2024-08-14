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
        // 공격 후 대기 상태로 전환
        player.ChangeState(new IdleState(player));
    }

    public void Exit()
    {
        // 필요 시 로직 추가
    }
}
