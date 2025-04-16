using System.Collections.Generic;
using UnityEngine;

public class WeaponSpawner : MonoBehaviour
{

    private List<Vector2> positions;

    void Start()
    {
        positions = new List<Vector2>();
        GameObject[] weaponsInTheScene = GameObject.FindGameObjectsWithTag(Utils.Const.GUN_ON_THE_GROUND_TAG);
        foreach (GameObject obj in weaponsInTheScene){
            positions.Add((Vector2)obj.transform.position); // they're not physical object so this is fine
        }
    }

    public Vector2[] GetWeaponsOnTheGroundPosition(){
        return positions.ToArray();
    }

    public bool RemoveAGunFromTheGroundPosition(Vector2 toRemove){
        return positions.Remove(toRemove);
    }
}
