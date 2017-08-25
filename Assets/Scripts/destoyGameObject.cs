using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destoyGameObject : MonoBehaviour {

    [SerializeField] float time = 0.5F;

	void Update ()
    {
        time -= Time.deltaTime;
        if (time <= 0)
            Destroy(gameObject);
	}
}
