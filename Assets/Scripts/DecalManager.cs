using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalManager : MonoBehaviour
{
    [SerializeField] float updateTime = 5.0F;

    GameObject[] decals;

    private IEnumerator FindDecals(float time)
    {
        yield return new WaitForSeconds(time);
        decals = GameObject.FindGameObjectsWithTag("Decal");
        if (decals.Length > 100)
        {
            for (int i = 0; i <= decals.Length - 100; i++)
            {
                Destroy(decals[i].gameObject);
            }
        }
    }

    private IEnumerator Start()
    {
        while (true)
            yield return StartCoroutine(FindDecals(updateTime));          
    }
}