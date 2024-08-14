using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingState : IState
{
    private PlayerController player;
    private Vector2 targetPosition;
    private Coroutine returnJavelinsCoroutine;

    public FlyingState(PlayerController playerController, Vector2 targetPosition)
    {
        this.player = playerController;
        this.targetPosition = targetPosition;
    }

    public void Enter()
    {
        // �÷��̾� �̵� ���
        player.rb.gravityScale = 0f;
        player.StartCoroutine(FlyToPosition());
    }

    public void Execute()
    {
        // â�� �߰��Ǹ� ��ǥ ��ġ�� �ٽ� ���
        Vector2 newTargetPosition = player.CalculateMidpoint();
        if (newTargetPosition != targetPosition)
        {
            targetPosition = newTargetPosition;
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

    public void Exit()
    {
        player.rb.gravityScale = 1f;
    }

    public void UpdateTargetPosition(Vector2 newTargetPosition)
    {
        targetPosition = newTargetPosition;
        player.StopCoroutine(FlyToPosition());
        player.StartCoroutine(FlyToPosition());
    }

    private IEnumerator FlyToPosition()
    {
        Vector2 direction = (targetPosition - (Vector2)player.transform.position).normalized; // ��ǥ ��ġ�� ���ϴ� ����
        player.rb.velocity = direction * player.flySpeed; // ����� �ӵ��� ������� velocity ����

        // ��ǥ ��ġ�� ������ ������ ���
        while (Vector2.Distance(player.transform.position, targetPosition) > 1.2f)
        {
            yield return null;
        }

        // ��ǥ ��ġ�� �����ϸ� �ӵ� 0���� �����ϰ� ��Ȯ�� ��ġ�� ����
        player.rb.velocity = Vector2.zero;
        player.transform.position = targetPosition;

        // ReturnJavelins �ڷ�ƾ ���� �� ����
        returnJavelinsCoroutine = player.StartCoroutine(player.ReturnJavelins());

    }

    // �浹 ó�� �޼���
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Javelin"))
        {
            if (player.GetJavelinCount() == 1)
            {
                // �÷��̾� �̵� ����
                player.rb.velocity = Vector2.zero;

                // ReturnJavelins �ڷ�ƾ�� ���� ���̸� ����
                if (returnJavelinsCoroutine != null)
                {
                    player.StopCoroutine(returnJavelinsCoroutine);
                    returnJavelinsCoroutine = null;
                }

                // Idle ���·� ��ȯ
                player.ChangeState(new IdleState(player));
            }
        }
    }
}
