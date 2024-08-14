using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Javelin : MonoBehaviour
{
    //private bool isStuck = false;  // â�� ��ü�� �������� ����

    //====================================================================================================

    public void Start()
    {

    }

    //====================================================================================================

    public void StickToTarget()
    {
        // ��ü�� ���� â�� �ʱ�ȭ ó��
        GetComponent<Rigidbody2D>().isKinematic = true;  // â�� ���������� ����
        //isStuck = true;
    }

    //====================================================================================================

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ���� ������ ����
        if (!collision.gameObject.CompareTag("Player"))
        {
            GetComponent<Rigidbody2D>().isKinematic = true;
            GetComponent<Collider2D>().enabled = false;

            // PlayerController���� OnJavelinPinned �޼��带 ȣ���Ͽ� â�� ��ġ�� ����
            PlayerController player = FindObjectOfType<PlayerController>();
            player.OnJavelinPinned(transform.position);
        }
    }
    
}
