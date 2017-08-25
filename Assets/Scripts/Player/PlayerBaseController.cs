using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBaseController : MonoBehaviour
{
    protected Animator rifleMovement;

    protected bool isFiring = false;
    protected bool isReloading = false;
    protected bool isWeaponActive = true;

    protected bool isWalking = false;
    protected bool isAiming = false;
    protected bool isIdle = false;
    protected bool hasMissionObject = false;

    public bool IsReloading
    {
        get { return isReloading; }
    }

    public bool HasMissionObject
    {
        get { return hasMissionObject; }
    }

    public bool IsAiming
    {
        get { return isAiming; }
    }

    protected virtual void Start ()
    {
        rifleMovement = GetComponentInChildren<Animator>();
    }
}
