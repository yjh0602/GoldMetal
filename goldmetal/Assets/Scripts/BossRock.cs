using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRock : Bullet
{
    // 리지드 바디 변수와 회전파워 , 크기 숫자값 변수 생성
    Rigidbody rigid;
    float angularPower = 2;
    float scaleValue = 0.1f;
    //기를 모으고 쏘는 타이밍을 관리한 bool 변수 추가
    bool isShoot;
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        StartCoroutine(GainPowerTimer()); 
        StartCoroutine(GainPower());
    }

    IEnumerator GainPowerTimer()
    {
        yield return new WaitForSeconds(2.2f); //2.2초 뒤에 발사
        isShoot = true;
    }

    IEnumerator GainPower()
    {
        // while 문에는 꼭 yield return null 포함
        while (!isShoot)
        {
            // while 문에서 증가된 값을 트랜스폼, 리지드바디에 적용
            angularPower += 0.02f;
            scaleValue += 0.005f;

            transform.localScale = Vector3.one * scaleValue; // 크기를 증가

            rigid.AddTorque(transform.right * angularPower, ForceMode.Acceleration); // 회전
            yield return null;
        }
    }
}
