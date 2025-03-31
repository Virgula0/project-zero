using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float displacementMultiplier = 0.15f;

    [SerializeField] private float zPos = -6f; 

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 cameraDisplacement = (mousePos - playerTransform.position) * displacementMultiplier;

        Vector3 finalCameraPos = playerTransform.position + cameraDisplacement;
        finalCameraPos.z = zPos;
        transform.position = finalCameraPos;
    }
}
