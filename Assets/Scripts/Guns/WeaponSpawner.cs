using System.Collections.Generic;
using UnityEngine;

public class WeaponSpawner : MonoBehaviour
{
    private List<Vector2> allWeaponsPositions;
    private List<Vector2> rangedWeaponsPositions;
    private List<Vector2> meleeWeaponsPositions;

    void Start()
    {
        allWeaponsPositions = new List<Vector2>();
        rangedWeaponsPositions = new List<Vector2>();
        meleeWeaponsPositions = new List<Vector2>();
        GameObject[] weaponsInTheScene = GameObject.FindGameObjectsWithTag(Utils.Const.GUN_ON_THE_GROUND_TAG);
        foreach (GameObject obj in weaponsInTheScene){
            allWeaponsPositions.Add((Vector2)obj.transform.position); // they're not physical object so this is fine
            IGun weapon = obj.GetComponentInChildren<IGun>(); // all weapons extend IGun, sorry the name is a little bit misleading
            if (weapon is IRanged)
            {
                rangedWeaponsPositions.Add((Vector2)obj.transform.position);
                continue;
            }

            if (weapon is IMelee){
                meleeWeaponsPositions.Add((Vector2)obj.transform.position);
            }
        }
    }

    public Vector2[] GetRangedOnTheGroundPosition(){
        return rangedWeaponsPositions.ToArray();
    }

    public Vector2[] GetMeleeOnTheGroundPosition(){
        return meleeWeaponsPositions.ToArray();
    }

    public Vector2[] GetAllWeaponsOnTheGroundPosition(){
        return allWeaponsPositions.ToArray();
    }

    public bool RemoveAGunFromTheGroundPosition(Vector2 toRemove){
        return allWeaponsPositions.Remove(toRemove) 
                | rangedWeaponsPositions.Remove(toRemove) // | single | executes also the other removes
                | meleeWeaponsPositions.Remove(toRemove);
    }
}
