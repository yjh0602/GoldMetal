using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public GameObject mashObj;
    public GameObject effectObj;
    public Rigidbody rigid;



    void Start()
    {
        StartCoroutine(Explosion());
    }

    IEnumerator Explosion()
    {
        yield return new WaitForSeconds(2f); // 2초뒤 폭발
        rigid.velocity = Vector3.zero; // 물리적 속도를 모두 vector3.zero 로 초기화.
        rigid.angularVelocity = Vector3.zero;
        mashObj.SetActive(false);
        effectObj.SetActive(true);


        RaycastHit[] rayHits = Physics.SphereCastAll(
            transform.position, // 시작 위치
            15, //반지름
            Vector3.up, //쏘는 방향 늘 중심으로만 터진다 (상관없음)
            0f,  // 0으로 해야 그 자리에서 터진다.
            LayerMask.GetMask("Enemy")); // 어떤 오브젝트에 닿았을때
        // 구체 모양의 레이캐스팅 (구체에 닿는 모든 오브젝트)

        foreach(RaycastHit hitObj in rayHits) // foreach 문으로 수류탄 범위 적들의 피격함수를 호출
        {
            hitObj.transform.GetComponent<Enemy>().HitByGrenade(transform.position);
        }

        //수류탄은 파티클이 사라지는 시간을 고려하여 디스토로이 함수를 호출
        Destroy(gameObject, 5);

    }
}
