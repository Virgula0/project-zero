using UnityEngine;

public class BackgroundKillZoneScript : MonoBehaviour
{
    private LogicManager logic;
    private PlayerScript player;
    void Start()
    {
        logic = GameObject.FindGameObjectWithTag(Utils.Const.LOGIC_MANAGER_TAG).GetComponent<LogicManager>();
        player = GameObject.FindGameObjectWithTag(Utils.Const.PLAYER_TAG).GetComponent<PlayerScript>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (player.IsPlayerAlive())
            logic.GameOver();
    }
}
