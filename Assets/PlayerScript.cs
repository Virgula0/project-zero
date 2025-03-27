using UnityEngine;

public class PlayerScript : MonoBehaviour
{

    [SerializeField]
    private float speed = 5f;

    [SerializeField]
    private Rigidbody2D playerRigidBody;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //empty for now
    }

    // Update is called once per frame
    void Update()
    {
     Movement();   
    }

    private void Movement(){
        if(Input.GetKey(KeyCode.W)){
            transform.position += transform.up * speed * Time.deltaTime;
        }
        if(Input.GetKey(KeyCode.S)){
            transform.position -= transform.up * speed * Time.deltaTime;
        }
        if(Input.GetKey(KeyCode.A)){
            transform.position -= transform.right * speed * Time.deltaTime;
        }
        if(Input.GetKey(KeyCode.D)){
            transform.position += transform.right * speed * Time.deltaTime;
        }
        Debug.Log(transform.position);
    }
}
