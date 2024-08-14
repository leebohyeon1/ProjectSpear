using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowJavelinState : IState
{
    private PlayerController player;

    public ThrowJavelinState(PlayerController player)
    {
        this.player = player;
    }

    public void Enter()
    {
        if (player.currentJavelins > 0)
        {
            player.ThrowJavelin();
            player.ChangeState(new IdleState(player));
        }
    }

    public void Execute()
    {
        // 필요 시 로직 추가
    }

    public void Exit()
    {
        // 필요 시 로직 추가
    }
}

