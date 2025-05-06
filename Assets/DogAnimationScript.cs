using UnityEngine;

public class DogAnimationScript : MonoBehaviour
{
    private Animator animatorRef;
    private Rigidbody2D dogRigidbodyRef;
    private Vector3 lastPosition;
    private float speed;

    void Start() {
        animatorRef = GetComponent<Animator>();
        dogRigidbodyRef = GetComponent<Rigidbody2D>();
    }

    void Update() {
        // Calculate movement delta
        speed = (transform.position - lastPosition).magnitude / Time.deltaTime;

        Debug.Log(speed);
        // Set Animator speed parameter
        animatorRef.SetFloat("speed", speed);

        // Store current position for next frame
        lastPosition = transform.position;
    }
}
