using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [HideInInspector] public Rigidbody2D rb;  // Rigidbody2D ������Ʈ�� ����
    //[HideInInspector] public Animator animator;  // Animator ������Ʈ�� ����
    public float moveSpeed = 5f;  // �̵� �ӵ�
    public float jumpForce = 10f;  // ���� ��
    public float dashSpeed = 20f;  // �뽬 �ӵ�

    public float flySpeed = 10f; // ���ư��� �ӵ�
    public float returnTime = 1f; // â���� �÷��̾�� ���ƿ��� �ð�

    public int maxJavelins = 3;  // �ִ� ���� ������ â ����
    public GameObject javelinPrefab;  // â ������
    public float dashDuration = 0.5f;  // �뽬 ���� �ð�

    [HideInInspector] public int currentJavelins;  // ���� ���� ���� â ����
    [HideInInspector] public bool isGrounded = false;  // �÷��̾ ���� �ִ��� ����
    [HideInInspector] public bool isDashing = false;  // �뽬 ������ ����

    private IState currentState;  // ���� ���� ����
    private List<GameObject> stuckJavelins = new List<GameObject>();  // ���� â���� ����Ʈ
    private List<Vector2> pinnedJavelinPositions = new List<Vector2>();
    private Coroutine currentFlyingCoroutine; // ���� ���� ���� DelayBeforeFlying �ڷ�ƾ�� ����

    //====================================================================================================

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();  // Rigidbody2D ������Ʈ�� �ʱ�ȭ
        //animator = GetComponent<Animator>();  // Animator ������Ʈ�� �ʱ�ȭ
       currentJavelins = maxJavelins;  // �ʱ� â ���� ����
        ChangeState(new IdleState(this));  // �ʱ� ���¸� Idle�� ����
    }

    void Update()
    {
        currentState.Execute();  // ���� ������ Execute �޼��� ȣ��
    }

    //====================================================================================================

    public void ChangeState(IState newState)
    {
        if (currentState != null)
        {
            currentState.Exit();  // ���� ���¿��� ��� �� Exit �޼��� ȣ��
        }

        currentState = newState;  // ���ο� ���·� ��ȯ

        if (currentState != null)
        {
            currentState.Enter();  // ���ο� ���·� ������ �� Enter �޼��� ȣ��
        }
    }

    public void Move()
    {
        // �¿� �̵� ó��
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
            // â�� ���� ��ġ�� ����
            Vector2 targetPosition = hit.point;

            // â ���� �� ��ġ ����
            GameObject javelin = Instantiate(javelinPrefab, rb.position, Quaternion.identity);
            javelin.transform.position = targetPosition;

            // â�� ������ �÷��̾ ���ϵ��� ����
            Vector2 toPlayer = rb.position - targetPosition;
            float angle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
            javelin.transform.rotation = Quaternion.AngleAxis(angle + 90f, Vector3.forward);

            // â�� ���� ó��
            javelin.GetComponent<Javelin>().StickToTarget();
            
            stuckJavelins.Add(javelin);  
            currentJavelins--;  // ���� ������ â ���� ����
        }
    }

    
    // â�� ������ �� ȣ��Ǵ� �޼���
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
                // ���� �ڷ�ƾ�� ���� ���̸� �ߴ�
                if (currentFlyingCoroutine != null)
                {
                    StopCoroutine(currentFlyingCoroutine);
                }

                // �� �ڷ�ƾ ����
                currentFlyingCoroutine = StartCoroutine(DelayBeforeFlying(targetPosition));
            }
        }
    }

    // â�� ���� �� 0.5�� �����̸� �ְ� ���ư��� �ڷ�ƾ
    private IEnumerator DelayBeforeFlying(Vector2 targetPosition)
    {
        // 0.5�� ���
        yield return new WaitForSeconds(0.7f);

        ChangeState(new FlyingState(this, targetPosition));  // ���ư��� ���·� ��ȯ

        // �ڷ�ƾ ���� �� ������ null�� ����
        currentFlyingCoroutine = null;
    }

    // â���� �÷��̾�� ���ƿ��� �ϴ� �ڷ�ƾ
    public IEnumerator ReturnJavelins()
    {
        foreach (GameObject javelin in stuckJavelins)
        {
            StartCoroutine(ReturnJavelin(javelin));
        }

        // ��� â�� ���ƿ��� �ð� ���� ���
        yield return new WaitForSeconds(returnTime);

        ClearJavelin();

        // �÷��̾� �̵� ��� ����
        ChangeState(new IdleState(this));  // Idle ���·� ��ȯ
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

            // â�� �÷��̾ �ٶ󺸵��� ȸ��
            Vector2 direction = transform.position - javelin.transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            javelin.transform.rotation = Quaternion.AngleAxis(angle + 90f, Vector3.forward);

            yield return null;
        }
    }
    // â�� ���� ��ġ���� �߰����� ���
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
        pinnedJavelinPositions.Clear(); // ����Ʈ �ʱ�ȭ
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
