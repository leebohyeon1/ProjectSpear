using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashState : IState
{
    private PlayerController player;
    private float dashTime;
    private float direction;

    public DashState(PlayerController player)
    {
        this.player = player;
    }

    public void Enter()
    {
        player.isDashing = true;
        //player.animator.SetTrigger("Dash");
        player.moveSpeed *= player.dashSpeed;  // 대쉬 속도 적용
        dashTime = player.dashDuration;  // 대쉬 시간 설정
        direction = Input.GetAxisRaw("Horizontal");
    }

    public void Execute()
    {
        dashTime -= Time.deltaTime;
        player.Dash(direction);
        if (dashTime <= 0)
        {
            player.ChangeState(new IdleState(player));
        }
    }

    public void Exit()
    {
        player.isDashing = false;
        player.moveSpeed /= player.dashSpeed;  // 대쉬 속도 해제
        
    }

}
