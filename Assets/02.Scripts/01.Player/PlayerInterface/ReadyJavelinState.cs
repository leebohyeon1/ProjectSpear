using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadyJavelinState : IState
{
    private PlayerController player;

    public ReadyJavelinState(PlayerController player)
    {
        this.player = player;
    }

    public void Enter()
    {

    }

    public void Execute()
    {
        if (Input.GetMouseButtonDown(0))
        {
            player.ChangeState(new ThrowJavelinState(player));
        }
        else if(Input.GetMouseButtonUp(1))
        {
            player.ChangeState(new IdleState(player));
        }
    }

    public void Exit()
    {
        // 필요 시 로직 추가
    }
}
