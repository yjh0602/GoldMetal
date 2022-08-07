using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum Type { Ammo , Coin, Grenade , Heart , Weapon};//열거형 타입
    public Type type;
    public int value;

    Rigidbody rigid;
    SphereCollider sphereCollider;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>(); // 첫번째 컴포넌트만 가져옴 

    }
    void Update()
    {
        transform.Rotate(Vector3.up * 2 * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision) //함수로 변수를 호출하여 물리효과 변경
    {
        if(collision.gameObject.tag == "Floor")
        {
            rigid.isKinematic = true;
            sphereCollider.enabled = false;
        }
    }
}
