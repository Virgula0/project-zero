using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI : MonoBehaviour
{
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float chaseSpeed;
    [SerializeField] private float stoppingDistance = 5f;
    [SerializeField] private Vector2[] patrolWaypoints;
    [SerializeField] private Vector2[] exitWaypoints;

    private GameObject player; // we need the position and other ottributes of the player
    private IMovement currentMovement;
    private IMovement patrolMovement;
    private IMovement chaseMovement;
    private GameObject currentEnemy;
    private EnemyWeaponManager weaponManager;
    private PlayerDetector playerDetector;
    private Rigidbody2D body;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.chaseSpeed = patrolSpeed * 3;
        this.body = transform.parent.GetComponentInChildren<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag(Utils.Const.PLAYER_TAG);
        playerDetector = gameObject.GetComponent<PlayerDetector>();
        currentEnemy = transform.parent.gameObject;

        // this trick is needed beacuse it needs MohoBehaviour for coroutines
        // and we can't even initialize a new object in unity if it inherits MonoBehaviour
        // this is a little bit shit and I need to do this trick if i want to add manually the script as component 
        // within the game object logic
        patrolMovement = gameObject.AddComponent<PatrolMovement>().New(patrolWaypoints, exitWaypoints, patrolSpeed, playerDetector);

        // instantiate normally
        chaseMovement = new ChaseMovement(player, playerDetector, chaseSpeed, stoppingDistance, exitWaypoints);

        // movements
        currentMovement = patrolMovement; // patrol movement when the object is spawned
        weaponManager = gameObject.transform.parent.GetComponentInChildren<EnemyWeaponManager>();
    }

    void FixedUpdate()
    {
        if (!playerDetector.GetIsEnemyAwareOfPlayer())
        {
            currentMovement = patrolMovement;
            currentMovement.Move(body); // if the enemy does not know about the player
            weaponManager.ChangeEnemyStatus(false);
            return;
        }

        // if here enemy detected player, start shooting and chasing!
        currentMovement.CustomSetter(varToSet: true); // CustomSetter will try to set the needsRepositioning to true
        currentMovement = chaseMovement;
        currentMovement?.Move(body); // ?. means that Move will called if currentMovement is not null
        weaponManager.ChangeEnemyStatus(true);
    }

    // Debugging purposes
    private void OnDrawGizmos()
    {
        if (transform.position == null)
            return;
            
        float circleRadius = 0.8f;
        foreach (Vector2 point in patrolWaypoints)
        {
            // Calculate the starting point and ending point of the cast
            Vector3 startPoint = point;
            Vector3 endPoint = startPoint + (Vector3)(new Vector2(0, 0).normalized * circleRadius);

            // Set Gizmo color for visualization
            Gizmos.color = Color.cyan;

            // Draw the starting circle
            Gizmos.DrawWireSphere(startPoint, circleRadius);
            // Draw the ending circle
            Gizmos.DrawWireSphere(endPoint, circleRadius);

            // Draw a line connecting the two circles to illustrate the cast path
            Gizmos.DrawLine(startPoint, endPoint);
        }

        foreach (Vector2 point in exitWaypoints)
        {
            // Calculate the starting point and ending point of the cast
            Vector3 startPoint = point;
            Vector3 endPoint = startPoint + (Vector3)(new Vector2(0, 0).normalized * circleRadius);

            // Set Gizmo color for visualization
            Gizmos.color = Color.white;

            // Draw the starting circle
            Gizmos.DrawWireSphere(startPoint, circleRadius);
            // Draw the ending circle
            Gizmos.DrawWireSphere(endPoint, circleRadius);

            // Draw a line connecting the two circles to illustrate the cast path
            Gizmos.DrawLine(startPoint, endPoint);
        }
    }
}
