using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;

public class WeaponFinderMovement : MonoBehaviour, IMovement
{
    private WeaponSpawner spawner;
    private float speed;
    private KdTree kdTree;
    private BFSPathfinder bfs;
    private List<Type> typesThatCanBeEquipped;
    private EnemyWeaponManager enemyWeaponManager;
    private bool busy = false;
    private bool atLeastAWeaponAvailable = true;
    private Coroutine _finderCoroutine;
    private Detector playerDetector;

    public IMovement New(KdTree tree, BFSPathfinder bfs, List<Type> typesThatCanBeEquipped, Detector playerDetector, WeaponSpawner spawner, EnemyWeaponManager enemyWeaponManager, float speed)
    {
        this.playerDetector = playerDetector;
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
    public Vector2? CloserWeaponToEnemy(Vector2 enemyPos, Vector2? closerWeaponToEnemy)
    {
        foreach (Type t in typesThatCanBeEquipped)
        {
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
            atLeastAWeaponAvailable = false;
            Debug.LogWarning("Closer weapon to enemy is null, nothing found");
            return;
        }

        busy = true;
        atLeastAWeaponAvailable = true;
        _finderCoroutine = StartCoroutine(MoveToThePointCoroutine(enemyTransform, (Vector2)closerEquippableWeapon));
    }

    public bool GetIsAtLeastAWeaponAvailable()
    {
        return this.atLeastAWeaponAvailable;
    }

    public Vector2 FindClosestWayPoint(Rigidbody2D enemyRigidbody, Vector2[] toExclude, out int index)
    {
        if (kdTree == null)
        {
            throw new InvalidOperationException("KdTree is not built. Make sure doorWayPoint array is assigned.");
        }

        return kdTree.FindNearestExcluding(enemyRigidbody.position, toExclude, out index);
    }

    private IEnumerator MoveToThePointCoroutine(Rigidbody2D enemyRB, Vector2 closerEquippableWeapon)
    {
        Vector2 targetWaypoint = kdTree.FindNearest(closerEquippableWeapon, out _);

        bool clearPath = false;
        Vector2 closestPoint = new();
        List<Vector2> vectorsToExclude = new List<Vector2>();
        int maxIterations = 200; // stop after 200 iterations
        int currentIteration = 0;
        while (busy && !clearPath && ++currentIteration < maxIterations)
        {
            closestPoint = FindClosestWayPoint(enemyRB, vectorsToExclude.ToArray(), out _);
            Vector2 directionToClosest = (closestPoint - enemyRB.position).normalized;
            float distanceToClosest = Vector2.Distance(enemyRB.position, closestPoint);
            RaycastHit2D hit = Physics2D.Raycast(enemyRB.position, directionToClosest, distanceToClosest, playerDetector.GetObstacleLayers());
            clearPath = hit.collider == null;

            if (!clearPath)
            {
                vectorsToExclude.Add(closestPoint);
                Debug.Log("Obstacle detected between enemy and closest waypoint while trying to finding weapon. Recalculating.");
            }
        }

        if (currentIteration >= maxIterations)
        {
            StopCoroutines(true);
            Debug.LogWarning("WARNING! Cannot find clearest closer waypoint while coward");
        }

        //Vector2 enemyCloserWaypoint = kdTree.FindNearest(enemyRB.position, out _);
        Vector2[] path = bfs.PathToPoint(closestPoint, targetWaypoint);

        // walk the path
        foreach (Vector2 waypoint in path)
            yield return MoveToDestinationWithChecks(enemyRB, waypoint, closerEquippableWeapon);

        // final leg: go to the actual weapon
        yield return MoveToDestinationWithChecks(enemyRB, closerEquippableWeapon, closerEquippableWeapon);

        busy = false;
    }

    /// Moves towards 'destination', but on each FixedUpdate checks:
    /// 1) if we still need to find a weapon  
    /// 2) if the weapon is still on the ground  
    /// Exits early (and clears busy) if either check fails.
    private IEnumerator MoveToDestinationWithChecks(Rigidbody2D enemyRB, Vector2 destination, Vector2 weaponPosition)
    {
        while (busy && Vector2.Distance(enemyRB.position, destination) > 0.1f)
        {
            if (!enemyWeaponManager.NeedsToFindAWeapon())
            {
                busy = false;
                yield break;
            }

            if (!spawner.GetAllWeaponsOnTheGroundPosition().Contains(weaponPosition))
            {
                Debug.Log("Weapon does not exist anymore");
                busy = false;
                yield break;
            }

            MoveTowardsTarget(enemyRB, destination);
            yield return new WaitForFixedUpdate();
        }
    }


    public void NeedsRepositioning(bool reposition)
    {
        return;
    }

    public void StopCoroutines(bool stop)
    {
        if (!stop || _finderCoroutine == null)
        {
            return;
        }
        busy = false;
        StopCoroutine(_finderCoroutine);
        _finderCoroutine = null;
    }
}