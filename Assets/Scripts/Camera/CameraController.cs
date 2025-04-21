using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float displacementMultiplier = 0.15f;

    [SerializeField] private float zPos = -6f;

    private PlayerScript playerScript;

    void Start()
    {
        this.playerScript = GameObject.FindGameObjectWithTag(Utils.Const.PLAYER_TAG).GetComponent<PlayerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerScript.IsPlayerAlive()){
            return;
        }

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 cameraDisplacement = (mousePos - playerTransform.position) * displacementMultiplier;

        Vector3 finalCameraPos = playerTransform.position + cameraDisplacement;
        finalCameraPos.z = zPos;
        transform.position = finalCameraPos;
    }
}
