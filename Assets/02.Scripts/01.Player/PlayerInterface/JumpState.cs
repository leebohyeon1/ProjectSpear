using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpState : IState
{
    private PlayerController player;

    public JumpState(PlayerController player)
    {
        this.player = player;
    }

    public void Enter()
    {
        Debug.Log("����");
        //player.animator.SetTrigger("Jump");
        player.isGrounded = false;
        player.rb.velocity = new Vector2(player.rb.velocity.x, player.jumpForce);
    }

    public void Execute()
    {
        InputKey();

        if (player.isGrounded)
        {
            player.ChangeState(new IdleState(player));
        }
    }

    public void Exit()
    {
        Debug.Log("�̵�");
        // �ʿ� �� ���� �߰�
    }

    public void InputKey()
    {
        player.Move();
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
            Time.timeScale = 0.3f;
            if (Input.GetMouseButtonDown(0))
            {
                if (player.currentJavelins > 0)
                {
                    player.ThrowJavelin();
                }
            }         
        }
        if (Input.GetMouseButtonUp(1))
        {
            Time.timeScale = 1f;
        }
    }
}

