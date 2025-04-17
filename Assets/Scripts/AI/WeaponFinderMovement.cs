using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public class WeaponFinderMovement : MonoBehaviour, IMovement
{
    private WeaponSpawner spawner;
    private float speed;
    private KdTree kdTree;
    private BFSPathfinder bfs;
    private List<Type> typesThatCanBeEquipped;
    private EnemyWeaponManager enemyWeaponManager;
    private bool busy = false;

    public IMovement New(KdTree tree, BFSPathfinder bfs, List<Type> typesThatCanBeEquipped, WeaponSpawner spawner, EnemyWeaponManager enemyWeaponManager, float speed)
    {
        this.enemyWeaponManager = enemyWeaponManager;
        this.spawner = spawner;
        this.kdTree = tree;
        this.bfs = bfs;
        this.speed = speed;
        this.typesThatCanBeEquipped = typesThatCanBeEquipped;
        return this;
    }


    private void MoveTowardsTarget(Rigidbody2D enemyRB, Vector2 targetPosition, float additionalSpeed = 1)
    {
        Vector2 enemyPos = enemyRB.position;
        Vector2 newPos = Vector2.MoveTowards(enemyPos, targetPosition, speed * additionalSpeed * Time.fixedDeltaTime);
        enemyRB.MovePosition(newPos);
    }

    // this functions using tree of ranged or melee weapons returns the coordintates of the weapoon closer to the enemy
    // this ignores bodies within the enemy and weapoins at the moment
    // also, as it is coded it gives priority to the first Type declared to typesThatCanBeEquipped, so the order matters
    private Vector2? CloserWeaponToEnemy(Vector2 enemyPos, Vector2? closerWeaponToEnemy)
    {
        foreach (Type t in typesThatCanBeEquipped)
        {
            Debug.Log(t);
            if (closerWeaponToEnemy != null)
            {
                return closerWeaponToEnemy;
            }

            // update from the weapon spawner the current available guns on the ground
            if (typeof(IRanged).IsAssignableFrom(t) && spawner.GetRangedOnTheGroundPosition().Length > 0)
            {
                return spawner.GetRangedTree().FindNearest(enemyPos, out _);
            }

            if (typeof(IMelee).IsAssignableFrom(t) && spawner.GetMeleeOnTheGroundPosition().Length > 0)
            {
                return spawner.GetMeleeTree().FindNearest(enemyPos, out _);
            }
        }

        return null;
    }

    public void Move(Rigidbody2D enemyTransform)
    {
        if (typesThatCanBeEquipped == null || typesThatCanBeEquipped.Count == 0)
        {
            return;
        }

        if (busy)
        {
            return;
        }

        Vector2? closerEquippableWeapon = null;
        Vector2 enemyPos = enemyTransform.position;

        closerEquippableWeapon = CloserWeaponToEnemy(enemyPos, closerEquippableWeapon);

        if (closerEquippableWeapon == null)
        {
            Debug.LogWarning("closer weapon to enemy is null, nothing found");
            return;
        }

        busy = true;
        StartCoroutine(MoveToThePointCoroutine(enemyTransform, (Vector2)closerEquippableWeapon));
    }

    private IEnumerator MoveToThePointCoroutine(Rigidbody2D enemyRB, Vector2 closerEquippableWeapon)
    {
        Vector2 targetWaypoint = kdTree.FindNearest(closerEquippableWeapon, out _);
        Vector2 enemyCloserWaypoint = kdTree.FindNearest(enemyRB.position, out _);
        Vector2[] path = bfs.PathToPoint(enemyCloserWaypoint, targetWaypoint);
        foreach (Vector2 waypoint in path)
        {
            while (Vector2.Distance(enemyRB.position, waypoint) > 0.1f)
            {
                if (!enemyWeaponManager.NeedsToFindAWeapon())
                {
                    busy = false;
                    yield break;
                }
                // TODO: check it the gun still exist on the ground
                MoveTowardsTarget(enemyRB, waypoint);
                yield return new WaitForFixedUpdate();
            }
        }
        // and then move towards the targetWaypoint
        while (Vector2.Distance(enemyRB.position, closerEquippableWeapon) > 0.1f)
        {
            if (!enemyWeaponManager.NeedsToFindAWeapon())
            {
                busy = false;
                yield break;
            }
            MoveTowardsTarget(enemyRB, closerEquippableWeapon);
            yield return new WaitForFixedUpdate();
        }
        busy = false;
    }

    public void NeedsRepositioning(bool reposition)
    {
        return;
    }
}