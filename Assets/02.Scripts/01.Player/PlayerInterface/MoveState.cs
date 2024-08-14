using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState : IState
{
    private PlayerController player;

    public MoveState(PlayerController player)
    {
        this.player = player;
    }

    public void Enter()
    {
       
        //player.animator.SetBool("isRunning", true);
    }

    public void Execute()
    {
        InputKey();

    }

    public void Exit()
    {
        //player.animator.SetBool("isRunning", false);
    }

    public void InputKey()
    {
        player.Move();  // �÷��̾� �̵�

        if (Input.GetAxisRaw("Horizontal") == 0)
        {
            player.ChangeState(new IdleState(player));
        }

        if (Input.GetKeyDown(KeyCode.Space) && player.isGrounded)
        {
            player.ChangeState(new JumpState(player));
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            player.ChangeState(new DashState(player));
        }

        if (Input.GetMouseButtonDown(0))
        {
            player.ChangeState(new AttackState(player));
        }

        if (Input.GetMouseButton(1))
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (player.currentJavelins > 0)
                {
                    player.ThrowJavelin();
                }
            }
        }
    }
}

