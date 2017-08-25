using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Health : MonoBehaviour {

	[SerializeField] public float health = 100.0F;

    bool isDead = false;

	void ReceiveDamage(float dmg)
	{
		health -= dmg;
	}

	void Update()
	{
        if (health <= 0 && !isDead)
        {
            GameObject weapon = GameObject.Find("Rifle");
            weapon.GetComponent<PlayerWeaponController>().enabled = false;
            foreach (var item in weapon.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                item.enabled = false;
            }

            isDead = true;
        }
    }
}
