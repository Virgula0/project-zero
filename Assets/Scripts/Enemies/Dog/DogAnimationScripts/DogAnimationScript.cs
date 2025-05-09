using UnityEngine;

public class DogAnimationScript : MonoBehaviour
{
    [SerializeField] private Sprite[] dogGunDeathSprites;
    [SerializeField] private Sprite[] dogBladeDeathSprites;

    private Animator animatorRef;
    private Rigidbody2D dogRigidbodyRef;
    private Vector3 lastPosition;
    private float speed;
    private SpriteRenderer dogSpriteRendererRef;

    void Start() {
        animatorRef = GetComponent<Animator>();
        dogRigidbodyRef = GetComponent<Rigidbody2D>();
        dogSpriteRendererRef = GetComponent<SpriteRenderer>();
    }

    void Update() {
        // Calculate movement delta
        speed = (transform.position - lastPosition).magnitude / Time.deltaTime;

        // Set Animator speed parameter
        animatorRef.SetFloat("speed", speed);

        // Store current position for next frame
        lastPosition = transform.position;
    }

    public void SetDogDeadSprite(IPrimary weapon)
    {
        animatorRef.enabled = false;

        // if weapon is null probably is because the object that hitted the enemy was not a bullet neither a swing
        // in this case, just choose a random from normal death sprites 
        
        Sprite[] chosenSprites = weapon is IRanged ? dogGunDeathSprites :
                               weapon is IMelee ? dogBladeDeathSprites : dogGunDeathSprites;

        int randomInt = Random.Range(0, chosenSprites.Length);
        dogSpriteRendererRef.sprite = chosenSprites[randomInt];
    }
}
