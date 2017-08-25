using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class PlayerWeaponController : MonoBehaviour, IWeaponControlable
{
    public string weaponName;

	[SerializeField] float damage = 10F;
	[SerializeField] float rateOfFire = 0.3F;
	[SerializeField] public int ammoClip = 30;
	[SerializeField] public int ammoAmount = 15;
	[SerializeField] float reloadTime = 2.0F;
    [SerializeField] GameObject muzzleEndGameObject;
    [SerializeField] GameObject shellPortGameObject;
    [SerializeField] Transform MuzzleEndTarget;
    [SerializeField] float weaponRecoil = 5.0F;
    [SerializeField] RuntimeAnimatorController controller;

    [Header("Decals")]
    [SerializeField] GameObject decal0;
    [SerializeField] GameObject decal1;
    [SerializeField] GameObject decal2;
    [SerializeField] GameObject decal3;

    [Header("Camera")]
    public Vector3 weaponPosition;
    public Vector3 aimPosition;

    [SerializeField] float hideWeaponTime;
    [SerializeField] float showWeaponTime;

    [NonSerialized] public float targetVectorOffset; 

    ParticleSystem muzzleEnd;
    ParticleSystem shellPort;

    PlayerController playerController;

	Ray ray;
    Ray UIray;

    Animator animator;

    RaycastHit hit;
    RaycastHit UIhit;

    int layerMask;
    
    float weaponOffset = 0;
    float timeToFire;
    float curWeaponRecoil;
    float weaponSlide = 2.0F;
    float curWeaponSlide;

    AudioSource audioSourceFire, audioSourceReload;

    Vector3 offset;

    Transform weaponAttachment;

    public delegate void VoidFunc();

    List<SkinnedMeshRenderer> mesh;

    UnityEvent FireEvent;

    public Vector3 hitCoord { get { return Physics.Raycast(UIray, out UIhit, Mathf.Infinity, layerMask) ? UIhit.point : UIray.GetPoint(50); } }

    public int curAmmoClip { get; private set; }
    public WeaponStates currentState { get; private set; }

    void Awake()
    {
        //animator = gameObject.AddComponent<Animator>();
        //animator.enabled = false;

        mesh = new List<SkinnedMeshRenderer>();
        mesh.AddRange(GetComponentsInChildren<SkinnedMeshRenderer>().OfType<SkinnedMeshRenderer>());

        currentState = WeaponStates.Disabled;
    }

    void Start () 
	{
        if (GetComponentInParent<PlayerController>() != null && currentState == WeaponStates.Disabled)
        {
            foreach (SkinnedMeshRenderer item in mesh)
            {
                item.enabled = false;
            }
        }

        FireEvent = new UnityEvent();

        playerController = GetComponentInParent<PlayerController>();

        curWeaponSlide = weaponSlide;

        muzzleEnd = muzzleEndGameObject.GetComponent<ParticleSystem>();
        shellPort = shellPortGameObject.GetComponent<ParticleSystem>();
        layerMask = 1 << 8 << 9;
		layerMask = ~layerMask;
		timeToFire = rateOfFire;
        curAmmoClip = ammoClip;
        audioSourceFire = GetComponent<AudioSource>();

        weaponRecoil /= 100;
        curWeaponRecoil = weaponRecoil;
    }

    public void Fire()
    {
        if (timeToFire >= rateOfFire && curAmmoClip > 0)
        {
            if (currentState == WeaponStates.Active)
            {
                FireEvent.Invoke();
            }         
        }
    }

    void OnFire()
    {
        timeToFire = 0;
        playerController.curRotation = playerController.curRotation - curWeaponRecoil * 5;
        currentState = WeaponStates.Fire;
        muzzleEnd.Play();
        shellPort.Play();
        curAmmoClip--;
        audioSourceFire.Play();

        if (hit.collider != null)
        {
            GameObject d;
            int m;
            try
            {
                m = hit.collider.GetComponent<material>().type;
            }
            catch
            {
                m = 0;
            }
            if (!hit.collider.gameObject.CompareTag("Enemy"))
            {
                switch (m)
                {
                    case (0):
                        d = Instantiate(decal0);
                        break;
                    case (1):
                        d = Instantiate(decal1);
                        break;
                    case (2):
                        d = Instantiate(decal2);
                        break;
                    default:
                        d = Instantiate(decal0);
                        break;
                }
            }
            else
            {
                d = Instantiate(decal3, hit.collider.GetComponent<Transform>());

                if (hit.collider.GetComponent<Rigidbody>() != null)
                {
                    hit.collider.GetComponent<Rigidbody>().AddForceAtPosition(transform.forward * 25, hit.point);
                }
            }

            d.transform.position = hit.point;
            d.transform.forward = hit.normal;
        }

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            try
            {
                hit.collider.GetComponentInParent<AIController>().BroadcastMessage("ReceiveDamage", damage);
            }
            catch
            {

            }
        }
    }
       
    public void Reload()
	{
        if (currentState == WeaponStates.Active && ammoAmount > 0 && curAmmoClip < ammoClip)
        {
            currentState = WeaponStates.Reload;

            playerController.isReloading = true;

            StartCoroutine(WaitAndRun(reloadTime, () => 
            {
                if (ammoAmount >= ammoClip - curAmmoClip)
                {
                    ammoAmount = ammoAmount - (ammoClip - curAmmoClip);
                    curAmmoClip = ammoClip;
                }
                else if (ammoAmount < ammoClip - curAmmoClip)
                {
                    curAmmoClip += ammoAmount;
                    ammoAmount = 0;
                }
                playerController.isReloading = false;
                currentState = WeaponStates.Active;
            }));
        }
    }


    public void Hide()
    {
        if (animator == null) animator = GetComponent<Animator>();

        animator.SetBool("is_active", false);

        currentState = WeaponStates.Disabling;

        StartCoroutine(WaitAndRun(hideWeaponTime, () => 
        {
            foreach (SkinnedMeshRenderer item in mesh)
            {
                item.enabled = false;
            }

            animator.enabled = false;

            FireEvent.RemoveListener(OnFire);

            currentState = WeaponStates.Disabled;
        }));
    }

    public void Show(Transform attachment=null)
    {
        if (attachment == null) attachment = weaponAttachment;

        weaponAttachment = attachment;

        currentState = WeaponStates.Activating;
        
        foreach (SkinnedMeshRenderer item in mesh)
        {
            item.enabled = true;
        }
        transform.localPosition = Vector3.zero;

        if (animator == null)
        {
            animator = gameObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
        }


        animator.enabled = true;

        animator.SetBool("is_active", true);  
        animator.PlayInFixedTime("Show");


        StartCoroutine(WaitAndRun(showWeaponTime, () => 
        {
            FireEvent.AddListener(OnFire);
            currentState = WeaponStates.Active;
        }));
    }

    void WeaponSway()
    {
        weaponAttachment.localRotation = Quaternion.Lerp(weaponAttachment.localRotation,
            Quaternion.Euler(new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), -Mathf.Clamp(Input.GetAxis("Mouse X") * 1.5F, -4, 4) * curWeaponSlide)), 0.15F);

        offset = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));
        offset = offset.normalized * Mathf.Clamp(offset.magnitude, 0, 0.03F);
    }

    IEnumerator WaitAndRun(float waitTime, VoidFunc func)
    {
        yield return new WaitForSeconds(waitTime);

        func();
    }

    void Update () 
	{
        Debug.Log(transform.localPosition);

        if (Time.timeScale == 0 || weaponAttachment == null) return;

        if (playerController == null) playerController = GetComponentInParent<PlayerController>();

        if (currentState == WeaponStates.Disabled) return;

        WeaponSway();

        if (playerController.isAiming && !playerController.isReloading)
        {
            weaponAttachment.localPosition = Vector3.Lerp(weaponAttachment.localPosition, aimPosition, 0.2F);
            curWeaponSlide = weaponSlide / 2;
        }
        else
        {
            weaponAttachment.localPosition = Vector3.Lerp(weaponAttachment.localPosition, weaponPosition + offset, 2.5F * Time.deltaTime);
            curWeaponSlide = weaponSlide;
        }

        if (weaponOffset > 0) weaponOffset = Mathf.Lerp(weaponOffset, 0, 0.1F);

		if ((curAmmoClip == 0) && (ammoAmount > 0 && curAmmoClip < ammoClip && currentState == WeaponStates.Active)) 
		{
            Reload();
		}

        targetVectorOffset = Mathf.Abs(playerController.WalkOffset) + Mathf.Abs(weaponOffset);

        if (!playerController.isAiming)
        {
            UIray = new Ray(MuzzleEndTarget.position, Camera.main.transform.forward);
            ray = new Ray(MuzzleEndTarget.position, Camera.main.transform.forward + new Vector3((UnityEngine.Random.value - 0.5F) * 2, (UnityEngine.Random.value - 0.5F) * 2, 0) * targetVectorOffset);

            curWeaponRecoil = weaponRecoil;
        }
        else
        {
            UIray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward + new Vector3((UnityEngine.Random.value - 0.5F) * 2, (UnityEngine.Random.value - 0.5F) * 2, 0) * targetVectorOffset);

            curWeaponRecoil = weaponRecoil / 5;
        }

        Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask);

        timeToFire += Time.deltaTime;

        animator.SetBool("is_reloading", playerController.isReloading);
        
        animator.SetBool("is_aiming", playerController.isAiming);
        animator.SetFloat("speed", playerController.animationSpeed);

        if (currentState == WeaponStates.Fire)
        {
            animator.SetBool("is_firing", true);
            weaponOffset += curWeaponRecoil;
        }
        else
        {
            animator.SetBool("is_firing", false);
        }

        if (!playerController.isReloading) StopCoroutine("Reload");

        if (timeToFire <= rateOfFire)
        {
            if (currentState == WeaponStates.Fire)
                currentState = WeaponStates.Active;
        }
    }
}
