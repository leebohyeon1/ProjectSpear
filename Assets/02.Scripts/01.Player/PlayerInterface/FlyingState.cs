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
        // 플레이어 이동 잠금
        player.rb.gravityScale = 0f;
        player.StartCoroutine(FlyToPosition());
    }

    public void Execute()
    {
        // 창이 추가되면 목표 위치를 다시 계산
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
        Vector2 direction = (targetPosition - (Vector2)player.transform.position).normalized; // 목표 위치로 향하는 방향
        player.rb.velocity = direction * player.flySpeed; // 방향과 속도를 기반으로 velocity 설정

        // 목표 위치에 도달할 때까지 대기
        while (Vector2.Distance(player.transform.position, targetPosition) > 1.2f)
        {
            yield return null;
        }

        // 목표 위치에 도달하면 속도 0으로 설정하고 정확한 위치로 보정
        player.rb.velocity = Vector2.zero;
        player.transform.position = targetPosition;

        // ReturnJavelins 코루틴 시작 및 추적
        returnJavelinsCoroutine = player.StartCoroutine(player.ReturnJavelins());

    }

    // 충돌 처리 메서드
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Javelin"))
        {
            if (player.GetJavelinCount() == 1)
            {
                // 플레이어 이동 중지
                player.rb.velocity = Vector2.zero;

                // ReturnJavelins 코루틴이 실행 중이면 중지
                if (returnJavelinsCoroutine != null)
                {
                    player.StopCoroutine(returnJavelinsCoroutine);
                    returnJavelinsCoroutine = null;
                }

                // Idle 상태로 전환
                player.ChangeState(new IdleState(player));
            }
        }
    }
}
