using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [HideInInspector] public Rigidbody2D rb;  // Rigidbody2D 컴포넌트를 저장
    //[HideInInspector] public Animator animator;  // Animator 컴포넌트를 저장
    public float moveSpeed = 5f;  // 이동 속도
    public float jumpForce = 10f;  // 점프 힘
    public float dashSpeed = 20f;  // 대쉬 속도

    public float flySpeed = 10f; // 날아가는 속도
    public float returnTime = 1f; // 창들이 플레이어에게 돌아오는 시간

    public int maxJavelins = 3;  // 최대 소지 가능한 창 개수
    public GameObject javelinPrefab;  // 창 프리팹
    public float dashDuration = 0.5f;  // 대쉬 지속 시간

    [HideInInspector] public int currentJavelins;  // 현재 소지 중인 창 개수
    [HideInInspector] public bool isGrounded = false;  // 플레이어가 땅에 있는지 여부
    [HideInInspector] public bool isDashing = false;  // 대쉬 중인지 여부

    private IState currentState;  // 현재 상태 저장
    private List<GameObject> stuckJavelins = new List<GameObject>();  // 박힌 창들의 리스트
    private List<Vector2> pinnedJavelinPositions = new List<Vector2>();
    private Coroutine currentFlyingCoroutine; // 현재 실행 중인 DelayBeforeFlying 코루틴을 추적

    //====================================================================================================

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();  // Rigidbody2D 컴포넌트를 초기화
        //animator = GetComponent<Animator>();  // Animator 컴포넌트를 초기화
       currentJavelins = maxJavelins;  // 초기 창 개수 설정
        ChangeState(new IdleState(this));  // 초기 상태를 Idle로 설정
    }

    void Update()
    {
        currentState.Execute();  // 현재 상태의 Execute 메서드 호출
    }

    //====================================================================================================

    public void ChangeState(IState newState)
    {
        if (currentState != null)
        {
            currentState.Exit();  // 이전 상태에서 벗어날 때 Exit 메서드 호출
        }

        currentState = newState;  // 새로운 상태로 전환

        if (currentState != null)
        {
            currentState.Enter();  // 새로운 상태로 진입할 때 Enter 메서드 호출
        }
    }

    public void Move()
    {
        // 좌우 이동 처리
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }

    public void Dash(float dir)
    {
        rb.velocity = new Vector2(dir * moveSpeed, 0);
    }

    #region Javelin
    public void ThrowJavelin()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - rb.position).normalized;

        int layerMask = ~LayerMask.GetMask("Player", "BackGround") ;

        RaycastHit2D hit = Physics2D.Raycast(rb.position, direction, Mathf.Infinity, layerMask);

        if (hit.collider != null)
        {
            // 창을 던질 위치를 결정
            Vector2 targetPosition = hit.point;

            // 창 생성 및 위치 설정
            GameObject javelin = Instantiate(javelinPrefab, rb.position, Quaternion.identity);
            javelin.transform.position = targetPosition;

            // 창의 방향을 플레이어를 향하도록 설정
            Vector2 toPlayer = rb.position - targetPosition;
            float angle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
            javelin.transform.rotation = Quaternion.AngleAxis(angle + 90f, Vector3.forward);

            // 창이 박힌 처리
            javelin.GetComponent<Javelin>().StickToTarget();
            
            stuckJavelins.Add(javelin);  
            currentJavelins--;  // 현재 소지한 창 개수 감소
        }
    }

    
    // 창이 박혔을 때 호출되는 메서드
    public void OnJavelinPinned(Vector2 position)
    {
        pinnedJavelinPositions.Add(position);

        if (pinnedJavelinPositions.Count > 0)
        {
            Vector2 targetPosition = CalculateMidpoint();

            if (currentState is FlyingState)
            {
                var flyingState = currentState as FlyingState;
                if (flyingState != null)
                {
                    flyingState.UpdateTargetPosition(targetPosition);
                }
            }
            else
            {
                // 기존 코루틴이 실행 중이면 중단
                if (currentFlyingCoroutine != null)
                {
                    StopCoroutine(currentFlyingCoroutine);
                }

                // 새 코루틴 시작
                currentFlyingCoroutine = StartCoroutine(DelayBeforeFlying(targetPosition));
            }
        }
    }

    // 창이 박힌 후 0.5초 딜레이를 주고 날아가는 코루틴
    private IEnumerator DelayBeforeFlying(Vector2 targetPosition)
    {
        // 0.5초 대기
        yield return new WaitForSeconds(0.7f);

        ChangeState(new FlyingState(this, targetPosition));  // 날아가는 상태로 전환

        // 코루틴 종료 시 참조를 null로 설정
        currentFlyingCoroutine = null;
    }

    // 창들을 플레이어에게 돌아오게 하는 코루틴
    public IEnumerator ReturnJavelins()
    {
        foreach (GameObject javelin in stuckJavelins)
        {
            StartCoroutine(ReturnJavelin(javelin));
        }

        // 모든 창이 돌아오는 시간 동안 대기
        yield return new WaitForSeconds(returnTime);

        ClearJavelin();

        // 플레이어 이동 잠금 해제
        ChangeState(new IdleState(this));  // Idle 상태로 전환
    }

    private IEnumerator ReturnJavelin(GameObject javelin)
    {
        Vector2 startPosition = javelin.transform.position;
        Vector2 endPosition = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < returnTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / returnTime;
            javelin.transform.position = Vector2.Lerp(startPosition, endPosition, t);

            // 창이 플레이어를 바라보도록 회전
            Vector2 direction = transform.position - javelin.transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            javelin.transform.rotation = Quaternion.AngleAxis(angle + 90f, Vector3.forward);

            yield return null;
        }
    }
    // 창이 박힌 위치들의 중간값을 계산
    public Vector2 CalculateMidpoint()
    {
        Vector2 sum = Vector2.zero;
        foreach (Vector2 pos in pinnedJavelinPositions)
        {
            sum += pos;
        }
        return sum / pinnedJavelinPositions.Count;
    }

    public void ClearJavelin()
    {
        for (int i = 0; i < stuckJavelins.Count; i++)
        {
            Destroy(stuckJavelins[i]);
        }

        stuckJavelins.Clear();
        pinnedJavelinPositions.Clear(); // 리스트 초기화
        currentJavelins = maxJavelins;
    }
    public int GetJavelinCount()
    {
        return pinnedJavelinPositions.Count;
    }
    #endregion
    //====================================================================================================

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Javelin"))
        {
            if (currentState is FlyingState)
            {
                var flyingState = currentState as FlyingState;
                if (flyingState != null)
                {
                    ClearJavelin();
                    flyingState.OnTriggerEnter2D(collision);
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(CalculateMidpoint(), 1.2f);
    }
}
