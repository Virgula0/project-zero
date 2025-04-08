using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI : MonoBehaviour
{
    private GameObject player; // we need the position and other ottributes of the player

    List<IMovement> movements;
    private IMovement currentMovement;
    private GameObject currentEnemy; 

    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private Vector2[] patrolWaypoints;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag(Utils.Const.PLAYER_TAG);
        currentEnemy = transform.parent.gameObject;
        IMovement patrol = new PatrolMovement(patrolWaypoints, patrolSpeed);

        // add movements to the list
        //movements.Add(patrol);
        currentMovement = patrol; // patrol movement when the object is spawned
    }

    void FixedUpdate()
    {
        currentMovement.Move(currentEnemy.transform);
    }

    // Debugging purposes
    private void OnDrawGizmos()
    {   
        float circleRadius = 0.8f;
        foreach (Vector2 point in patrolWaypoints){
            // Calculate the starting point and ending point of the cast
            Vector3 startPoint = point;
            Vector3 endPoint = startPoint + (Vector3)(new Vector2(0,0).normalized * circleRadius);

            // Set Gizmo color for visualization
            Gizmos.color = Color.cyan;
            
            // Draw the starting circle
            Gizmos.DrawWireSphere(startPoint, circleRadius);
            // Draw the ending circle
            Gizmos.DrawWireSphere(endPoint, circleRadius);
            
            // Draw a line connecting the two circles to illustrate the cast path
            Gizmos.DrawLine(startPoint, endPoint);
        }
    }
}
