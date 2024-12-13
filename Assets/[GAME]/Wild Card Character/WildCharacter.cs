using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildCharacter : MonoBehaviour
{
    public GameObject laser;
    public GameObject FirePoint;
    public GameObject FirePoint1;
    public GameObject stoneObj;

    public Transform stoneObjPosRef;

    public Transform _parent;
    public Animator _animator;

    public ParticleSystem leftBreathParitcle;
    public ParticleSystem rightBreathParitcle;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(3.2f);

        //GameObject Instance = Instantiate(laser, FirePoint.transform.position, FirePoint.transform.rotation);
        //Instance.transform.parent = _parent;
        //Instance.transform.localScale = Vector3.one;

        //GameObject Instance1 = Instantiate(laser, FirePoint1.transform.position, FirePoint1.transform.rotation);
        //Instance1.transform.parent = _parent;
        //Instance1.transform.localScale = Vector3.one;
    }

    public void ShootLaser()
    {
        leftBreathParitcle.Play();
        rightBreathParitcle.Play();

        GameObject _laser = Instantiate(laser, FirePoint.transform.position, FirePoint.transform.rotation);
        _laser.transform.parent = _parent;
        _laser.transform.localScale = Vector3.one;
        Destroy(_laser, 0.2f);

        GameObject _laser_1 = Instantiate(laser, FirePoint1.transform.position, FirePoint1.transform.rotation);
        _laser_1.transform.parent = _parent;
        _laser_1.transform.localScale = Vector3.one;
        Destroy(_laser_1, 0.2f);

        SoundManager.OnPlaySound(SoundType.laser);

        StartCoroutine(BonusRespinsController.instance.ShowOwlEffet(0.2f));

        Invoke(nameof(ResetNormalPos), 0.5f);
    }

    void ResetNormalPos()
    {
        _animator.Play("Laser Shoot Idle Pos");
    }

    void PlayIdlePosAnim()
    {
        StartCoroutine(BonusRespinsController.instance.ShowCloudAndLightning());

        Invoke(nameof(PlayIdelAnimation), 0.3f);
    }

    void PlayIdelAnimation()
    {
        _animator.Play("Idel");
    }
}
