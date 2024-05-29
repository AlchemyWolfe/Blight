using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    public GameSceneToolsSO Tools;
    public Player PlayerPrefab;

    void Awake()
    {
        //SpawnPlayer();
        Tools.OnGameClose += OnGameCloseReceived;
    }

    public void OnGameCloseReceived()
    {
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        var oldPlayer = Tools.Player;
        var spawnPosition = Tools.Player == null ? transform.position : Tools.Player.transform.position;
        Tools.Player = Instantiate(PlayerPrefab, spawnPosition, PlayerPrefab.transform.rotation, transform);
        if (oldPlayer != null)
        {
            oldPlayer.gameObject.SetActive(false);
            Destroy(oldPlayer);
        }
    }
}
