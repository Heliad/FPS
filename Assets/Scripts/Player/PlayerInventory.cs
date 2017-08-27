using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PlayerController))]
public class PlayerInventory : MonoBehaviour
{
    [SerializeField] List<GameObject> weapons;
    [SerializeField] List<GameObject> objects;

    [NonSerialized] public PlayerWeaponController currentWeapon;

    Transform weaponAttachment;

    List<PlayerWeaponController> attachedWeapons;

    UserInterface userInterface;

    PlayerController playerController;

    UnityEvent ChangeWeapons;

    int currentWeaponIndex = -1;

    bool isChange = false;

    void Start ()
    {
        ChangeWeapons = new UnityEvent();

        playerController = GetComponentInParent<PlayerController>();
        userInterface = GetComponentInChildren<UserInterface>();

        ChangeWeapons.AddListener(playerController.OnChangeWeapons);
        ChangeWeapons.AddListener(userInterface.OnChangeWeapons);

        attachedWeapons = new List<PlayerWeaponController>();

        weaponAttachment = GameObject.Find("WeaponAttachment").GetComponent<Transform>();

		if (weapons != null && weapons.Count > 0)
        {
            foreach (var weapon in weapons)
            {
                if (weapon.GetComponent<PlayerWeaponController>() != null) AddObject(weapon, true);
            }
            ChangeCurrentWeapon(0, true);
        }
	}

    public void AddObject(GameObject obj, bool isPrefab=false)
    {
        if (obj == null || obj.GetComponent<InteractableObject>() == null) return;

        if (obj.GetComponent<InteractableObject>().type == InventoryObjectType.Weapon)
        {
            if (isPrefab)
            {
                GameObject w = Instantiate(obj, weaponAttachment, false);
                w.name = w.GetComponent<PlayerWeaponController>().weaponName;
                obj = w;
            }

            obj.transform.parent = weaponAttachment;
            obj.transform.position = weaponAttachment.position;
            obj.transform.localRotation = weaponAttachment.localRotation;
            obj.transform.localRotation = Quaternion.Euler(0, 90, 0);
            obj.name = obj.GetComponent<PlayerWeaponController>().weaponName;
            obj.GetComponent<BoxCollider>().enabled = false;
            obj.layer = 8;

            foreach (Transform item in obj.transform)
            {
                item.gameObject.layer = 8;
            }

            foreach (SkinnedMeshRenderer mesh in obj.GetComponentsInChildren<SkinnedMeshRenderer>().OfType<SkinnedMeshRenderer>())
            {
                mesh.enabled = false;
            }
            attachedWeapons.Add(obj.GetComponent<PlayerWeaponController>());
        }
        else if (obj.GetComponent<InteractableObject>().type == InventoryObjectType.Object || obj.GetComponent<InteractableObject>().type == InventoryObjectType.MissionObject)
        {   
            if (!objects.Contains(obj))
                objects.Add(obj);
            obj.SetActive(false);
        }
    }

    void RemoveObject(GameObject obj)
    {

    }

    public bool HasTypeObject(InventoryObjectType objectType)
    {
        foreach (GameObject item in objects)
        {
            if (item.GetComponent<InteractableObject>().type == objectType)
            {
                return true;
            }
        }
        return false;
    }

    void ChangeCurrentWeapon(int index, bool initial=false)
    {
        index = Mathf.Clamp(index, 0, attachedWeapons.Count - 1);

        if (index == currentWeaponIndex && currentWeapon.currentState != WeaponStates.Disabled) return;

        if (initial)
        {
            if (attachedWeapons[index].currentState == WeaponStates.Disabled)
            {
                attachedWeapons[index].Show(weaponAttachment);
                currentWeapon = attachedWeapons[index];
                currentWeaponIndex = index;
                ChangeWeapons.Invoke();
            }
        }
        else
        {
            if (currentWeapon.currentState == WeaponStates.Active)
            {
                currentWeapon.Hide();
                currentWeaponIndex = index;
                isChange = true;
            }
            else if (currentWeapon.currentState == WeaponStates.Disabled)
            {
                currentWeaponIndex = index;
                isChange = true;
            }
        }
    }

    void Update ()
    {
        if (attachedWeapons == null || attachedWeapons.Count == 0 || Input.GetKey(KeyCode.LeftShift)) return;
        
        if (attachedWeapons != null && attachedWeapons.Count > 0 && currentWeapon == null)
        {
            ChangeCurrentWeapon(0, true);
        }

        if (currentWeapon.currentState == WeaponStates.Disabled && isChange)
        {
            attachedWeapons[currentWeaponIndex].Show(weaponAttachment);
            currentWeapon = attachedWeapons[currentWeaponIndex];
            ChangeWeapons.Invoke();
            isChange = false;
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0 && !Input.GetButton("Fire1"))
        {
            ChangeCurrentWeapon(currentWeaponIndex + 1);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 && !Input.GetButton("Fire1"))
        {
            ChangeCurrentWeapon(currentWeaponIndex - 1);
        }

        int index;

        if (int.TryParse(Input.inputString, out index))
        {
            ChangeCurrentWeapon(index - 1);
        }
	}
}
