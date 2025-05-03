using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float displacementMultiplier = 0.15f;
    private float originalDisplacementMultiplier;

    [SerializeField] private float zPos = -6f;
    [SerializeField] private KeyCode panKey = KeyCode.LeftShift;  // Hold to pan

    private PlayerScript playerScript;
    private Camera cam;

    void Awake()
    {
        DontDestroyOnLoad(gameObject); // don't destory when switching scene
        cam = GetComponent<Camera>();
    }

    void Start()
    {
        originalDisplacementMultiplier = displacementMultiplier;
        playerScript = GameObject.FindGameObjectWithTag(Utils.Const.PLAYER_TAG).GetComponent<PlayerScript>();
    }

    void Update()
    {
        if (cam == null)
        {
            Debug.LogError("Camera in camera controller is null");
            return;
        }

        if (!playerScript.IsPlayerAlive()) return;

        if (Input.GetKey(panKey))
            displacementMultiplier = 0.40f;
        else
            displacementMultiplier = originalDisplacementMultiplier;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 camDelta = (mouseWorld - playerTransform.position) * displacementMultiplier;
        Vector3 finalCamPos = playerTransform.position + camDelta;
        finalCamPos.z = zPos;

        // --- CLAMPING STEP ---
        // half‐sizes of ortho view
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        // ensure player stays within view rectangle
        // camera center must stay between player ± halfExtents
        finalCamPos.x = Mathf.Clamp(finalCamPos.x, playerTransform.position.x - halfW, playerTransform.position.x + halfW);
        finalCamPos.y = Mathf.Clamp(finalCamPos.y, playerTransform.position.y - halfH, playerTransform.position.y + halfH);

        transform.position = finalCamPos;
    }
}
