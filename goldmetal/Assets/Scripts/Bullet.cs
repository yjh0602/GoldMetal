using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage;
    public bool isMelee;
    public bool isRock; // 돌 변수

    void OnCollisionEnter(Collision collision) // 에서 각각 충돌 로직 생성
    {
        if(!isRock && collision.gameObject.tag == "Floor")
        {
            Destroy(gameObject, 3); //바닥과 만나면 3초뒤에 사라진다
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (!isMelee && other.gameObject.tag == "Wall") // 근접공격이 아닐경우에 +
        {
            Destroy(gameObject); //벽과 만나면 바로 사라진다
        }
    }

}
