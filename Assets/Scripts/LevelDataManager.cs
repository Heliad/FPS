using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDataManager : MonoBehaviour
{
    static public LevelDataManager instance = null;

    [NonSerialized] public GameObject player;
    [NonSerialized] public PlayerController playerController;
    [NonSerialized] public PlayerWeaponController playerWeaponController;
    [NonSerialized] public UserInterface userInterface;

    private void Awake()
    {
        if (instance == null && instance != this)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start ()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerController = player.GetComponent<PlayerController>();
        playerWeaponController = player.GetComponentInChildren<PlayerWeaponController>();
        userInterface = player.GetComponentInChildren<UserInterface>();
    }
}
