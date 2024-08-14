using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Javelin : MonoBehaviour
{
    //private bool isStuck = false;  // 창이 물체에 박혔는지 여부

    //====================================================================================================

    public void Start()
    {

    }

    //====================================================================================================

    public void StickToTarget()
    {
        // 물체에 박힌 창의 초기화 처리
        GetComponent<Rigidbody2D>().isKinematic = true;  // 창을 물리적으로 고정
        //isStuck = true;
    }

    //====================================================================================================

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 벽에 박히는 로직
        if (!collision.gameObject.CompareTag("Player"))
        {
            GetComponent<Rigidbody2D>().isKinematic = true;
            GetComponent<Collider2D>().enabled = false;

            // PlayerController에서 OnJavelinPinned 메서드를 호출하여 창의 위치를 전달
            PlayerController player = FindObjectOfType<PlayerController>();
            player.OnJavelinPinned(transform.position);
        }
    }
    
}
