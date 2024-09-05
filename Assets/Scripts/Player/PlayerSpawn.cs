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
        Instantiate(PlayerPrefab, spawnPosition, PlayerPrefab.transform.rotation, transform);
        if (oldPlayer != null)
        {
            oldPlayer.gameObject.SetActive(false);
            Destroy(oldPlayer);
        }
    }
}
