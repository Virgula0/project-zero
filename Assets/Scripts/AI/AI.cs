using UnityEngine;

public class AI : MonoBehaviour
{
    private GameObject player; // we need the position and other ottributes of the player

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag(Utils.Const.PLAYER_TAG);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject GetPlayer() {
        return player;
    }
}
