using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    public GameObject[] weapons;
    public bool[] hasWeapons;
    public GameObject[] grenades;
    public int hasGrenades;  // 슈류탄
    public GameObject grenadeObj;
    public Camera followCamera;

    public int ammo;  // 총알
    public int coin;   // 코인
    public int health;  // 하트


    public int maxAmmo;  // 총알
    public int maxCoin;   // 코인
    public int maxHealth;  // 하트
    public int maxHasGrenade;  // 슈류탄

    float hAxis;
    float vAxis;

    bool wDown;
    bool jDown;
    bool fDown; // 파이어 누르다
    bool gDown; // 슈류탄 던지기
    bool rDown; // 재장전
    bool iDown;
    bool sDown1;
    bool sDown2;
    bool sDown3;

    bool isJump;
    bool isDodge;
    bool isSwap;
    bool isReload;
    bool isFireReady = true; //공격준비
    bool isBorder;
    bool isDamage;

    Vector3 moveVec;
    Vector3 dodgeVec;

    Rigidbody rigid;
    Animator anim;

    MeshRenderer[] meshs; //배열로 가져온다.

    GameObject nearObject;
    Weapon equipWeapon;

    int equipWeaponIndex = -1;
    float fireDelay;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();//animator 변수를 초기화
        meshs = GetComponentsInChildren<MeshRenderer>(); //s가 붙는다 모든 자식컴포넌트를 다 가져온다.

    }


    void Update()
    {
        GetInput(); // 초기화된 이동값.
        Move();
        Turn();
        Jump();
        Grenade();
        Attack();
        Reload();
        Dodge();
        Swap();
        Interation();


    }


    void GetInput()
    {

        //edit 프로젝트 세팅에 인풋 매니저에 가면 있다. 키관리하는 기본 설정
        hAxis = Input.GetAxisRaw("Horizontal");//axis값을 정수로 반환하는 함수
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        fDown = Input.GetButton("Fire1"); //마우스 왼쪽
        gDown = Input.GetButtonDown("Fire2"); //마우스 오른쪽
        rDown = Input.GetButtonDown("Reload");
        iDown = Input.GetButtonDown("Interation");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
    }
    void Move()
    {

        //normalized 어떤 값이든 1로 해준다.
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if (isDodge)
            moveVec = dodgeVec;

        if (isSwap || isReload || !isFireReady) // 바꾸는 중이거나 장전중 때리는 중!
            moveVec = Vector3.zero;

        if(!isBorder)
           transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;

        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);
    }
    void Turn()
    {
        //#1. 키보드에 의한 회전
        transform.LookAt(transform.position + moveVec);

        //#2 .마우스에 의한 회전
        if (fDown)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);// 스크린에서 월드로 ray를 쏘는 함수
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100)) // out : return 처럼 반환값을 주어진 변수에 저장하는 키워드
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0; // 높은곳을 안바라본다. 0으로 초기화
                transform.LookAt(transform.position + nextVec);
            }
        }

    }

    void Jump()
    {
        if (jDown && moveVec == Vector3.zero && !isJump && !isDodge) // 점프키랑 isJump가 false 일때 작동
        {
            rigid.AddForce(Vector3.up * 15, ForceMode.Impulse);//impulse 즉발적인 힘
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }

    void Grenade()
    {
        if (hasGrenades == 0)
            return;

        if(gDown && !isReload && !isSwap) // 장전중이나 스왑중일떄 발동안함
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);// 스크린에서 월드로 ray를 쏘는 함수
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100)) // out : return 처럼 반환값을 주어진 변수에 저장하는 키워드
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 10; // 위로 10 던진다.


                GameObject instantGrenade = Instantiate(grenadeObj, transform.position, transform.rotation);
                Rigidbody rigidGrenade = instantGrenade.GetComponent<Rigidbody>(); // 생선된 수류탄의 리지드바디를 활용하여 던지는 로직 구현
                rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
                rigidGrenade.AddTorque(Vector3.back * 10, ForceMode.Impulse);

                hasGrenades--; // 수류탄 던지면 빼기
                grenades[hasGrenades].SetActive(false);
            }
        }
    }
    void Attack() //공격딜레이에 시간을 더해주고 공격가능 여부를 확인.
    {
        if (equipWeapon == null)
        {
            return; //손에 아무것도 없으면 리턴!
        }
        fireDelay += Time.deltaTime; // 델타타임이 게속 더해진다.
        isFireReady = equipWeapon.rate < fireDelay; // 달려있는 무기의 공격속도보다 fireDelay가 크다!

        if (fDown && isFireReady && !isDodge && !isSwap) // 공격
        {
            equipWeapon.Use();
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
            //들고있는 무기 타입이 스윙이냐 샷이냐!
            fireDelay = 0; // 공격딜레이를 0으로 돌려서 다음 공격까지 대기
        }
    }

    void Reload()
    {
        if (equipWeapon == null) // 무기가 있는지
            return;

        if (equipWeapon.type == Weapon.Type.Melee) // 무기타입이 맞는지
            return;

        if (ammo == 0) // 총알이 0 발일때
            return;

        if (rDown && !isJump && !isDodge && !isSwap && isFireReady)
        {
            anim.SetTrigger("doReload");
            isReload = true;

            Invoke("ReloadOut", 2.5f); // 장전 2.5초
        }
    }

    void ReloadOut()
    {
        int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;
        equipWeapon.curAmmo = reAmmo;

        ammo -= reAmmo;
        isReload = false;
    }

    void Dodge()
    {
        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge) // 점프키랑 isJump가 false 일때 작동
        {
            dodgeVec = moveVec;
            speed *= 2;
            anim.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.5f);  //시간차 함수 호출 Invoke();
        }
    }

    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }

    void Swap()
    {
        if (sDown1 && (!hasWeapons[0] || equipWeaponIndex == 0))
            return;
        if (sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1))
            return;
        if (sDown3 && (!hasWeapons[2] || equipWeaponIndex == 2))
            return;

        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;

        if ((sDown1 || sDown2 || sDown3) && !isJump && !isDodge && !isSwap)// 셋중하나만 눌러도 발동
        {
            if (equipWeapon != null) //빈손이 아닐경우에만 이 로직이 실행
            {
                equipWeapon.gameObject.SetActive(false);
            }

            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

            anim.SetTrigger("doSwap");

            isSwap = true;

            Invoke("SwapOut", 0.4f);
        }
    }
    void SwapOut()
    {
        isSwap = false;
    }

    void Interation()
    {
        if (iDown && nearObject != null && !isJump && !isDodge) //nearobject가 비어있지 않다!
        {
            if (nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true; // 아이템 정보를 가져와서 해당 무기 입수 체크

                Destroy(nearObject);
            }
        }
    }



    void FreezeRotation()
    {
        rigid.angularVelocity = Vector3.zero; // angularVelocity 물리 회전 속도
    }
    void StopToWall()
    {
        Debug.DrawRay(transform.position, transform.forward * 5, Color.green);
        isBorder = Physics.Raycast(transform.position, transform.forward, 5, LayerMask.GetMask("Wall")); // raycast : ray를 쏘아 닿는 오브젝트를 감지하는 함수
    }
    void FixedUpdate() 
    {
        FreezeRotation();
        StopToWall();
    }




    void OnCollisionEnter(Collision collision) // 이벤트 함수로 착지 구현
    {
        if(collision.gameObject.tag == "Floor") // 바닥에 닿으면 false
        {
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }
     void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if(ammo > maxAmmo)
                      {ammo = maxAmmo;}
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxCoin)
                    { coin = maxCoin; }
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxHealth)
                    { health = maxHealth; }
                    break;
                case Item.Type.Grenade:
                    grenades[hasGrenades].SetActive(true);
                    //수류탄 개수대로 공전체가 활성화 되도록 구현
                    hasGrenades += item.value;
                    if (hasGrenades > maxHasGrenade)
                    { hasGrenades = maxHasGrenade; }
                    break;
            }
            Destroy(other.gameObject);
        }
        else if(other.tag == "EnemyBullet")
        {
            if (!isDamage)
            {
                Bullet enemyBullet = other.GetComponent<Bullet>();
                health -= enemyBullet.damage;

                bool isBossAtk = other.name == "Boss Melee Area";
                StartCoroutine(OnDamge(isBossAtk));
            }

            if (other.GetComponent<Rigidbody>() != null) //리지드바디 유무를 조건으로 하여 Destroy()호출
                Destroy(other.gameObject);

        }
    }

    IEnumerator OnDamge(bool isBossAtk) // 리액션을 위한 코루틴 생성
    {
        isDamage = true;

        foreach (MeshRenderer mesh in meshs)  // 맞으면 노란색으로
        {
            mesh.material.color = Color.yellow;
        }

        if(isBossAtk)
        {
            rigid.AddForce(transform.forward * -25, ForceMode.Impulse);
        }

        yield return new WaitForSeconds(1f); //1초동안 무적

        isDamage = false;

        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.white;
        }

        if(isBossAtk)
        {
            rigid.velocity = Vector3.zero;
        }
    }





    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Weapon")
            nearObject = other.gameObject;

      
    }
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
            nearObject = null;
    }

    
}
