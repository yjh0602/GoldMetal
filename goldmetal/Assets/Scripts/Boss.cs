using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Boss : Enemy
{
    public GameObject missile;
    public Transform missilePortA;
    public Transform missilePortB;
    public bool isLook; // 플레이어 바라보는 플래그 변수 생성

    Vector3 lookVec;
    Vector3 tauntVec;
   

    //Awake()함수는 자식 스크립트만 단독 실행
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        meshs = GetComponentsInChildren<MeshRenderer>(); //Material은 MeshRenderer 컴포넌트에서 접근 가능!
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        nav.isStopped = true;
        StartCoroutine(Think());
    }

    
    void Update()
    {
        if(isDead) // 죽음 플래그 bool 변수를 활용하여 패턴 정지 로직 작성
        {
            StopAllCoroutines(); // 지금 모든 코루틴이 다 정지
            return;
        }

        if (isLook)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            lookVec = new Vector3(h, 0, v) * 5f;//플레이어 입력값으로 예측 벡터값 생성
            transform.LookAt(target.position + lookVec);
        }
        else
            nav.SetDestination(tauntVec); //점프 공격을 할때 목표지점으로 이동하도록 로직
    }

    IEnumerator Think()
    {
        yield return new WaitForSeconds(0.1f); //보스가 반응하는 속도 조절

        int ranAction = Random.Range(0, 5); // 행동 패턴을 만들기 위해 Random.Range 함수 호출
        switch(ranAction)
        {
            //미사일 패턴 0 ,1 
            case 0:                
            case 1:
                StartCoroutine(MissileShot());
                break;
            //돌 굴러가기 패턴 2,3
            case 2:
            case 3:              
                StartCoroutine(RockShot());
                break;
            case 4:
                //점프 공격 패턴
                StartCoroutine(Taunt());
                break;
        }
    }
    IEnumerator MissileShot()
    {
        anim.SetTrigger("doShot");
        yield return new WaitForSeconds(0.2f);
        //미사일 스크립트까지 접근하여 목표물 설정해주기
        GameObject instantMissileA = Instantiate(missile, missilePortA.position, missilePortA.rotation);
        BossMissile bossMissileA = instantMissileA.GetComponent<BossMissile>();
        bossMissileA.target = target;

        yield return new WaitForSeconds(0.3f);
        //미사일 스크립트까지 접근하여 목표물 설정해주기
        GameObject instantMissileB = Instantiate(missile, missilePortB.position, missilePortB.rotation);
        BossMissile bossMissileB = instantMissileB.GetComponent<BossMissile>();
        bossMissileB.target = target;

        yield return new WaitForSeconds(2f);

        StartCoroutine(Think()); // 끝나고 다시 생각
    }
    IEnumerator RockShot()
    {
        
        isLook = false; // 기모을때는 바라보기 중지!
        anim.SetTrigger("doBigShot");
        Instantiate(bullet, transform.position, transform.rotation);
        yield return new WaitForSeconds(3f);

        isLook = true; // 꺼둔 바라보기 bool 다시 바라보게 돌려놓기!
        StartCoroutine(Think());
    }
    IEnumerator Taunt()
    {
        //점프 공격을 할 위치를 변수에 저장!
        tauntVec = target.position + lookVec;

        isLook = false; //바라보기 중지!
        nav.isStopped = false; 
        boxCollider.enabled = false; // 점프하는 중에 콜라이더가 플레이어를 밀지 않도록 비활성화
        anim.SetTrigger("doTaunt");

        yield return new WaitForSeconds(1.5f); // 공격 범위 활성화 후 끄기
        melleeArea.enabled = true;

        yield return new WaitForSeconds(0.5f);
        melleeArea.enabled = false;

        yield return new WaitForSeconds(1f);
        isLook = true;
        nav.isStopped = true;
        boxCollider.enabled = true;

        StartCoroutine(Think());
    }
}
