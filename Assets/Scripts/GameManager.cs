using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject GameOverMenu;
    [SerializeField] GameObject WinMenu;
    [SerializeField] GameObject background;

    [SerializeField] Transform missionWaypoint;

    [SerializeField] bool testLevel = false;

    Health h;

    GameObject player;

    List<AIController> enemys = new List<AIController>();

    PlayerInventory playerInventory;


    void Start ()
    {
        if (testLevel) return;

        player = GameObject.Find("Player");
        playerInventory = player.GetComponent<PlayerInventory>();

        h = player.GetComponent<Health>();

        foreach (var item in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (item.GetComponent<AIController>() != null)
            {
                enemys.Add(item.GetComponent<AIController>());
            }
        }

        GameOverMenu.SetActive(false);
        WinMenu.SetActive(false);

        StartCoroutine(EnemyCountCheck());
	}

    IEnumerator EnemyCountCheck()
    {
        while (true)
        {
            yield return new WaitForSeconds(2F);

            List<AIController> temp = new List<AIController>(enemys);

            foreach (var item in enemys)
            {
                if (item.CurrentStateLog() == "Dead")
                {
                    temp.Remove(item);
                }
            }

            enemys = new List<AIController>(temp);
        }
    }

	void Update ()
    {
        if (testLevel) return;

		if (h.health <= 0)
        {
            background.SetActive(true);
            GameOverMenu.SetActive(true);
            Time.timeScale = 0;
        }

        if (!LevelParameters.mode)
        {
            if (enemys.Count == 0)
            {
                background.SetActive(true);
                WinMenu.SetActive(true);
                Time.timeScale = 0;
            }
        }
        else
        {
            if ((player.transform.position - missionWaypoint.position).magnitude < 3)
            {
                if (playerInventory.HasTypeObject(InventoryObjectType.MissionObject))
                {
                    background.SetActive(true);
                    WinMenu.SetActive(true);
                    Time.timeScale = 0;
                }
            }
        }
	}
}
