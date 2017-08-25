using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour
{
	[SerializeField] Texture2D sight;
    [SerializeField] Texture2D sightHor;

    Camera uiCam;
	Vector3 sightCoord;
	Vector2 camSightCoord;
    Health h;

    Text ammo;
    Text weaponName;

    GameObject currentHealthImage;

    PlayerController playerController;
    PlayerWeaponController playerWeaponController;
    PlayerInventory playerInventory;

    float offset;
    float healthBarWidth;
    float currentPercentageHealth;
    float maxHealth;
    float healthBarOffset;
    float lerpedAlphaLevel = 255.0F;

    int currentAlphaLevel = 255;
    
	void Start () 
	{
        playerController = GetComponentInParent<PlayerController>();
        playerInventory = GetComponentInParent<PlayerInventory>();

        if (GameObject.Find("UI") == null) return;

        ammo = GameObject.Find("AmmoText").GetComponent<Text>();
        weaponName = GameObject.Find("WeaponNameText").GetComponent<Text>();
        currentHealthImage = GameObject.Find("CurrentHealth");
        healthBarWidth = currentHealthImage.GetComponent<RectTransform>().rect.width;
		uiCam = GetComponent<Camera> ();

        h = GameObject.Find("Player").GetComponent<Health>();

        maxHealth = h.health;
        healthBarOffset = currentHealthImage.transform.localPosition.x;
    }

    void Update()
    {
        if (Time.timeScale == 1)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (playerWeaponController == null || GameObject.Find("UI") == null) return;

        if (playerWeaponController.currentState != WeaponStates.Disabled)
        {
            ammo.text = playerWeaponController.curAmmoClip.ToString() + "/" + playerWeaponController.ammoAmount.ToString();
            weaponName.text = playerWeaponController.weaponName;
        }
        else
        {
            ammo.text = "";
            weaponName.text = "";
        }
        

        currentPercentageHealth = 1 - h.health / maxHealth;

        currentHealthImage.transform.localPosition = new Vector3(healthBarOffset - healthBarWidth * currentPercentageHealth, currentHealthImage.transform.localPosition.y, currentHealthImage.transform.localPosition.z);
    }

    public void OnChangeWeapons()
    {
        if (playerInventory == null) playerInventory = GetComponentInParent<PlayerInventory>();
        playerWeaponController = playerInventory.currentWeapon;
    }

    void OnGUI()
	{

        if (playerWeaponController == null || GameObject.Find("UI") == null) return;

        GUI.color = new Color32(255, 255, 255, (byte)currentAlphaLevel);

        if (!playerController.isAiming && playerWeaponController.currentState == WeaponStates.Active)
        {
            currentAlphaLevel = (int)Mathf.Lerp(currentAlphaLevel, 255, 0.2F);
        }
        else
        {
            currentAlphaLevel = (int)Mathf.Lerp(currentAlphaLevel, 0, 0.2F);
        }

        if (currentAlphaLevel > 0)
        {
            camSightCoord = uiCam.WorldToScreenPoint(playerWeaponController.hitCoord);
            camSightCoord.y = Screen.height - camSightCoord.y;

            offset = Mathf.Lerp(offset, playerWeaponController.targetVectorOffset * 1000, 0.25F);

            if (!playerController.isReloading && Time.timeScale == 1)
            {
                GUI.DrawTexture(new Rect(camSightCoord.x + 3 + offset, camSightCoord.y - 5, 30, 6), sightHor);
                GUI.DrawTexture(new Rect(camSightCoord.x - 33 - offset, camSightCoord.y - 5, 30, 6), sightHor);
                GUI.DrawTexture(new Rect(camSightCoord.x - 3, camSightCoord.y + offset, 6, 30), sight);
                GUI.DrawTexture(new Rect(camSightCoord.x - 3, camSightCoord.y - 34 - offset, 6, 30), sight);
            }
    }
}
}
