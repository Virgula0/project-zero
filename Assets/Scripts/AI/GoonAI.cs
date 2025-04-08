using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AI : MonoBehaviour
{
    private GameObject player; // we need the position and other ottributes of the player

    List<IMovement> movements;
    private IMovement currentMovement;
    private GameObject currentEnemy; 

    private float patrolSpeed = 3f;
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

    // Update is called once per frame
    void FixedUpdate()
    {
        currentMovement.Move(currentEnemy.transform);
    }

}
