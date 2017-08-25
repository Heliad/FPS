using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalDestroyer : MonoBehaviour
{

    [SerializeField] float lifeTime = 5.0F;

    float t = 0;

    void Update()
    {
        t += Time.deltaTime;
        if ((Camera.main.WorldToViewportPoint(transform.position).x > 1 || Camera.main.WorldToViewportPoint(transform.position).x < 0 ||
            Camera.main.WorldToViewportPoint(transform.position).y > 1 || Camera.main.WorldToViewportPoint(transform.position).y > 1) && t >= lifeTime)
            Destroy(gameObject);
    }
}
