using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static string spawningAt;
    private Transform player;

    private GameObject spawnPointObj;
    private Transform spawnPoint;

    // Start is called before the first frame update
    void OnEnable()
    {
        if (PlayerMovement.instance != null)
        {
            player = PlayerMovement.instance.transform;
        }
        if (player != null && spawningAt != null && !GameSaver.loading)
        {
            spawnPointObj = GameObject.Find(spawningAt);
            if (spawnPointObj != null)
            {
                spawnPoint = spawnPointObj.transform;
                player.position = new Vector3(spawnPoint.position.x, spawnPoint.position.y, spawnPoint.position.z);
            }
            transform.position = player.position;
        }
        GameSaver.loading = false;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
