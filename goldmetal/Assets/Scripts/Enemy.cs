using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Enemy : MonoBehaviour
{


    public enum Type { A, B, C, D}; // enum으로 타입을 나눈다
    public Type enemyType; // 그것을 지정할 변수 생성
    public int maxHealth;
    public int curHealth;
    public Transform target;
    public BoxCollider melleeArea;
    public GameObject bullet; // 미사일
    public bool isChase; // 추적을 결정하는 bool변수
    public bool isAttack;
    public bool isDead;
   

    public Rigidbody rigid;
    public BoxCollider boxCollider;
    public MeshRenderer[] meshs;
    public NavMeshAgent nav;  //navmash : navagent가 경로를 그리기 위한 바탕
    public Animator anim;
   
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        meshs = GetComponentsInChildren<MeshRenderer>(); //Material은 MeshRenderer 컴포넌트에서 접근 가능!
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        if(enemyType != Type.D) // 보스가 아닐경우에만
            Invoke("ChaseStart", 2);
    }


    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("isWalk", true);
    }

    void Update()
    {
        if (nav.enabled && enemyType != Type.D) // 네비가 활성화 되어있을때만
        {
         nav.SetDestination(target.position);
         nav.isStopped = !isChase;// 완벽하게 멈춘다. isStopped
        }
    }
    void FreezeVelocity()
    {
        if (isChase)
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
        
    }
    void Targerting()
    {
        if (!isDead && enemyType != Type.D) // 사망 아니고 보스가 아닐때
        {
            float targetRadius = 0;
            float targetRange = 0;

            switch (enemyType)
            {
                case Type.A:
                    targetRadius = 1.5f;
                    targetRange = 3f;
                    break;
                case Type.B:
                    targetRadius = 1f;
                    targetRange = 12f;
                    break;
                case Type.C:
                    targetRadius = 0.5f;
                    targetRange = 25f;
                    break;
            }

            RaycastHit[] rayHits = Physics.SphereCastAll(
                transform.position, // 시작 위치
                targetRadius, //반지름
                transform.forward, //쏘는 방향 앞쪽
                targetRange,  // 거리(범위)
                LayerMask.GetMask("Player")); // 어떤 오브젝트에 닿았을때

            if (rayHits.Length > 0 && !isAttack) // rayHits 변수에 데이터가 들어오면 공격 코루틴 실행
            {
                StartCoroutine(Attack());
            }
        }
    }
    IEnumerator Attack()
    {
        isChase = false; // 정지를 먼저 한 다음 , 애니메이션과 함께 공격범위 활성화
        isAttack = true;
        anim.SetBool("isAttack", true); // 애니메이션 활성화

        switch (enemyType)
        {
            case Type.A:
                yield return new WaitForSeconds(0.2f);
                melleeArea.enabled = true;

                yield return new WaitForSeconds(1f);
                melleeArea.enabled = false;

                yield return new WaitForSeconds(1f); //1초를 더 딜레이 

                break;
            case Type.B:

                yield return new WaitForSeconds(0.1f);
                rigid.AddForce(transform.forward * 20, ForceMode.Impulse); //힘을 가한다 앞으로 즉발적인 힘
                melleeArea.enabled = true;

                yield return new WaitForSeconds(0.5f);
                rigid.velocity = Vector3.zero; //velocity 제로 로 속도를 제어
                melleeArea.enabled = false;


                yield return new WaitForSeconds(2f); // 돌격후 2초 멈춤
                break;
            case Type.C:
                yield return new WaitForSeconds(0.5f);
                GameObject instantBullet = Instantiate(bullet, transform.position, transform.rotation); //미사일 복사

                Rigidbody rigidBullet = instantBullet.GetComponent<Rigidbody>();
                rigidBullet.velocity = transform.forward * 20;

                yield return new WaitForSeconds(2f);

                break;
        }
       

        isChase = true; // 다시 쫒아가게
        isAttack = false; // 공격이 끝났다
        anim.SetBool("isAttack", false); // 애니메이션 비활성화
    }

    void FixedUpdate()
    {
        Targerting();
        FreezeVelocity();
       
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Melee")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;           
            Vector3 reactVec = transform.position - other.transform.position;

            StartCoroutine(onDamage(reactVec, false));
        }
        
        else if( other.tag == "Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();
            curHealth -= bullet.damage;
            
            Vector3 reactVec = transform.position - other.transform.position;

            Destroy(other.gameObject);// 총알의 경우 , 적과 닿았을 때 삭제되도록 destroy()호출

            StartCoroutine(onDamage(reactVec, false));
        }
    }
    public void HitByGrenade(Vector3 explosionPos)
    {
        curHealth -= 100;
        Vector3 reactVec = transform.position - explosionPos;

        StartCoroutine(onDamage(reactVec, true));
    }
    

    // 슈류탄 만의 리액션을 위해 bool 매개변수 추가.
    IEnumerator onDamage(Vector3 reactVec , bool isGrenade)
    {
        foreach(MeshRenderer mesh in meshs)
            mesh.material.color = Color.red; //맞으면 빨강

        yield return new WaitForSeconds(0.1f);

        if(curHealth > 0)
        {
            foreach (MeshRenderer mesh in meshs)
                mesh.material.color = Color.white;
        }
        else
        {
            foreach (MeshRenderer mesh in meshs)
                mesh.material.color = Color.gray; // 죽었을때 회색

            gameObject.layer = 14; // 죽었을때 반응안하는 14번 죽음
            isDead = true;
            isChase = false;
            nav.enabled = false; // 사망 리액션을 유지하기 위해 navAgent를 비활성
            anim.SetTrigger("doDie");

            if(isGrenade)
            {
                reactVec = reactVec.normalized; // 1로 통일 
                reactVec += Vector3.up * 5; // 반대방향으로


                rigid.freezeRotation = false; // 체크를 푼다
                rigid.AddForce(reactVec * 5, ForceMode.Impulse); //넉백당한다.
                rigid.AddTorque(reactVec * 15, ForceMode.Impulse); // 수류탄의 의한 사망 리액션은 큰 힘과 회전을 추가
            }
            else
            {
                reactVec = reactVec.normalized; // 1로 통일 
                reactVec += Vector3.up; // 반대방향으로

                rigid.AddForce(reactVec * 5, ForceMode.Impulse); //넉백당한다. 
            }


           if(enemyType != Type.D)
            Destroy(gameObject, 4); //4초뒤에 사라짐
        }
    }
}
