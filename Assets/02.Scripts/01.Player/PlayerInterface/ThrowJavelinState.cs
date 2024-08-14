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
        // �ʿ� �� ���� �߰�
    }

    public void Exit()
    {
        // �ʿ� �� ���� �߰�
    }
}

