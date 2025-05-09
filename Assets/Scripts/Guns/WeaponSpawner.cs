using System.Collections.Generic;
using UnityEngine;

public class WeaponSpawner : MonoBehaviour
{
    private List<Vector2> allWeaponsPositions;
    private List<Vector2> rangedWeaponsPositions;
    private List<Vector2> meleeWeaponsPositions;
    private KdTree rangedStructure;
    private KdTree meleeStructure;

    void Start()
    {
        allWeaponsPositions = new List<Vector2>();
        rangedWeaponsPositions = new List<Vector2>();
        meleeWeaponsPositions = new List<Vector2>();
        GameObject[] weaponsInTheScene = GameObject.FindGameObjectsWithTag(Utils.Const.GUN_ON_THE_GROUND_TAG);

        foreach (GameObject obj in weaponsInTheScene)
        {
            IRestricted eq = obj.GetComponent<IRestricted>();

            // skip if the weapon is equippable by player only
            if (eq != null && eq.IsEquippableByPlayerOnly())
            {
                continue;
            }

            allWeaponsPositions.Add((Vector2)obj.transform.position); // they're not physical object so this is fine
            IPrimary weapon = obj.GetComponentInChildren<IPrimary>(); // all weapons extend IGun, sorry the name is a little bit misleading
            if (weapon is IRanged)
            {
                rangedWeaponsPositions.Add((Vector2)obj.transform.position);
                continue;
            }

            if (weapon is IMelee)
            {
                meleeWeaponsPositions.Add((Vector2)obj.transform.position);
                continue;
            }
        }
        this.rangedStructure = new KdTree(rangedWeaponsPositions.ToArray());
        this.meleeStructure = new KdTree(meleeWeaponsPositions.ToArray());
    }

    public KdTree GetRangedTree()
    {
        return rangedStructure;
    }

    public KdTree GetMeleeTree()
    {
        return meleeStructure;
    }

    public Vector2[] GetRangedOnTheGroundPosition()
    {
        return rangedWeaponsPositions.ToArray();
    }

    public Vector2[] GetMeleeOnTheGroundPosition()
    {
        return meleeWeaponsPositions.ToArray();
    }

    public Vector2[] GetAllWeaponsOnTheGroundPosition()
    {
        return allWeaponsPositions.ToArray();
    }

    public bool AddAvailableGunOnTheGroundPosition(Vector2 toAdd, IPrimary gunObject)
    {
        bool addedAll = !allWeaponsPositions.Contains(toAdd);
        if (addedAll) allWeaponsPositions.Add(toAdd);

        bool addedRanged = false;
        bool addedMelee = false;

        // Check for IRanged (no else-if; allows both checks to run)
        if (gunObject is IRanged)
        {
            addedRanged = !rangedWeaponsPositions.Contains(toAdd);
            if (addedRanged)
            {
                rangedWeaponsPositions.Add(toAdd);
                rangedStructure.UpdateVectorSetOnInsert(toAdd);
            }
        }

        // Check for IMelee separately (not mutually exclusive)
        if (gunObject is IMelee)
        {
            addedMelee = !meleeWeaponsPositions.Contains(toAdd);
            if (addedMelee)
            {
                meleeWeaponsPositions.Add(toAdd);
                meleeStructure.UpdateVectorSetOnInsert(toAdd);
            }
        }

        return addedAll | addedRanged | addedMelee;
    }


    public bool RemoveAGunFromTheGroundPosition(Vector2 toRemove)
    {
        bool removedAll = allWeaponsPositions.Remove(toRemove);

        bool removedRanged = rangedWeaponsPositions.Remove(toRemove)
                             && rangedStructure.UpdateVectorSetOnDeleteFirstOccurence(toRemove);

        bool removedMelee = meleeWeaponsPositions.Remove(toRemove)
                             && meleeStructure.UpdateVectorSetOnDeleteFirstOccurence(toRemove);

        // The single '|' ensures all three were executed; returns true if any succeeded.
        return removedAll | removedRanged | removedMelee;
    }
}
