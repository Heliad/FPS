using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(Health), typeof(PlayerInventory))]
public class PlayerController : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] float walkSpeed = 1.5F;
    [SerializeField] float playerSpeed = 3;
	[SerializeField] float sprintSpeed = 6;
	[SerializeField] float rotationSpeed = 150;
	[SerializeField] float jumpSpeed = 8;

    [Header("Camera FOV")]
    [SerializeField] [Range(0, 180)] float weaponCameraFov = 54;
    [SerializeField] [Range(0, 180)] float aimCameraFov = 30;

    [Header("Character Controller")]
    [SerializeField] float radius = 0.5F;
    [SerializeField] float height = 2;
    [SerializeField] [Range(0, 180)] float slopeLimit = 45;
    
    [NonSerialized] public bool isReloading = false;
    [NonSerialized] public bool isAiming = false;
    [NonSerialized] public bool hasMissionObject = false;

    [NonSerialized] public float curRotation;
    [NonSerialized] public float animationSpeed;

    Camera weaponCamera;

    CharacterController characterController;

    Vector3 moveDirection = Vector3.zero;

    PlayerWeaponController playerWeaponController;

    GameObject missionObject;

    PlayerInventory playerInventory;

    Transform weaponAttachment;

    public float WalkOffset{ get { return rSpeed / 80; } }

    bool isIdle = false;
    bool isWalking = false;

    float curSetSpeed;
    float curSpeed;
    float boundRotation = 90;
    float idleTime;
    float rSpeed;

    IWeaponControlable weapon;

    const float gravity = 20;

    void Start () 
	{
        playerInventory = GetComponent<PlayerInventory>();
        characterController = GetComponent<CharacterController>();
        playerWeaponController = GetComponentInChildren<PlayerWeaponController>();
        weaponAttachment = GameObject.Find("WeaponAttachment").GetComponent<Transform>();
        weaponCamera = GameObject.Find("Gun Camera").GetComponent<Camera>();

        if (!LevelParameters.mode && missionObject != null)
        {
            missionObject.SetActive(false);
        }

        weapon = playerInventory.currentWeapon;
		
        curSetSpeed = playerSpeed;

        CharacterControllerInitialization();
    }

    void CharacterControllerInitialization()
    {
        characterController.slopeLimit = slopeLimit;
        characterController.height = height;
        characterController.radius = radius;
    }

    void PlayerRotation()
    {
        transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X"), 0) * rotationSpeed * Time.deltaTime);
        curRotation -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

        curRotation = Mathf.Clamp(curRotation, -boundRotation, boundRotation);

        Camera.main.transform.localRotation = Quaternion.Euler(curRotation, 0, 0);
    }


    void PlayerMovement()
    {
        if (characterController.isGrounded)
        {
            if (Input.GetButton("Fire2") && playerInventory.currentWeapon.currentState == WeaponStates.Active) isAiming = true;
            else isAiming = false;

            if (isAiming && !isReloading)
            {
                curSetSpeed = walkSpeed;
                weaponCamera.fieldOfView = Mathf.Lerp(weaponCamera.fieldOfView, aimCameraFov, 25F * Time.deltaTime);
            }
            else
            {
                if (curSetSpeed == walkSpeed)
                {
                    curSetSpeed = playerSpeed;
                }
                weaponCamera.fieldOfView = Mathf.Lerp(weaponCamera.fieldOfView, weaponCameraFov, 25F * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.LeftShift) && !isAiming && !isReloading)
            {
                if (Vector3.Angle(transform.forward, moveDirection) < 60)
                {
                    curSetSpeed = sprintSpeed;
                }
                else
                {
                    curSetSpeed = playerSpeed;
                }
            }
            else if (!isAiming)
            {
                curSetSpeed = playerSpeed;
            }
            if (Input.GetKey(KeyCode.C))
            {
                characterController.height = 1;
                curSetSpeed = walkSpeed;
            }
            else if (characterController.height != 2)
            {
                characterController.height = Mathf.Lerp(characterController.height, 2, 0.3F);
            }

            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= curSetSpeed;

            if (Input.GetButton("Jump")) moveDirection.y = jumpSpeed;

            if (moveDirection.magnitude > curSetSpeed && !Input.GetButton("Jump"))
            {
                moveDirection = moveDirection.normalized * Mathf.Clamp(moveDirection.magnitude, 0, curSetSpeed);
            }
        }

        moveDirection.y -= gravity * Time.deltaTime;
        rSpeed = moveDirection.magnitude;
     
        curSpeed = Mathf.Lerp(curSpeed, rSpeed, 0.05F);

        characterController.Move(moveDirection.normalized * curSpeed * Time.deltaTime);

        animationSpeed = curSpeed / sprintSpeed;
    }

    public void OnChangeWeapons()
    {
        if (playerInventory == null) playerInventory = GetComponent<PlayerInventory>();

        weapon = playerInventory.currentWeapon;
    }

    void FixedUpdate()
    {
        PlayerRotation();
    }

    void Update() 
	{
        PlayerMovement();

        if (weapon == null) return;

        if (Input.GetButton("Fire1") && !Input.GetKey(KeyCode.LeftShift))
        {
            weapon.Fire();       
        }
        else if (Input.GetKey(KeyCode.R))
        {
            weapon.Reload();
        }
        else if (Input.GetKey(KeyCode.Z))
        {
            if (weapon.currentState == WeaponStates.Active)
            {
                weapon.Hide();
            }
            else if (weapon.currentState == WeaponStates.Disabled)
            {
                weapon.Show(null);
            }
        }
    }
}