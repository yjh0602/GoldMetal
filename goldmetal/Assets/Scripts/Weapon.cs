using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee, Range };
    public Type type;
    public int damage;
    public float rate; //공격속도
    public int maxAmmo; //최대 탄창
    public int curAmmo; //남은 탄창

    public BoxCollider meleeArea; //공격 범위
    public TrailRenderer trailEffect;// 공격 이펙트

    public Transform bulletPos; //총알이 나갈 위치
    public GameObject bullet;// 총알 생성
    public Transform bulletCasePos; //탄피가 나갈 위치
    public GameObject bulletCase;// 탄피 생성

    public void Use() // 플레이어가 무기 사용
    {
        if (type == Type.Melee)
        {
            StopCoroutine("Swing"); // 중지 코루틴 
            StartCoroutine("Swing"); // 코루틴 함수를 출력하는 스타트코루틴                                  
        }
        else if (type == Type.Range && curAmmo > 0)
        {
            curAmmo--; //1씩 마이너스
            StartCoroutine("Shot");
        }
    }
    IEnumerator Swing()
    {
        //결과를 전달하는 키워드
        // yield return null;//1프레임 대기
        //1
        yield return new WaitForSeconds(0.1f); //0.1초 대기
        meleeArea.enabled = true;
        trailEffect.enabled = true;
       
        yield return new WaitForSeconds(0.3f); //0.3초 대기후 melee 끄기
        meleeArea.enabled = false;
       
        yield return new WaitForSeconds(0.3f); // 0.3초 더 뒤에 이펙트 끄기
        trailEffect.enabled = false;
    }

    IEnumerator Shot()
    {
        //#1 총알 발사
        GameObject intantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = intantBullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletPos.forward * 50; // forward 방향 z축
        // Instantiate() 함수로 총알 인스턴스화 하기
        yield return null;


        //#2 탄피 배출
        GameObject intantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Rigidbody caseRigid = intantCase.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3);//랜덤화 해서 총알이 케이스에서 나올때 랜덤으로 나가게
        caseRigid.AddForce(caseVec, ForceMode.Impulse); // 총알이 나가는 힘 Impulse 즉각적인
        caseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse); // 총알이 휜다.


    }

    // 일반  함수 : Use() 메인 루틴 -> Swing() 서브루틴 -> Use() 메인 루틴
    // 코루틴 함수 : Use() 메인 루틴 + Swing() (Co.Op)


}
