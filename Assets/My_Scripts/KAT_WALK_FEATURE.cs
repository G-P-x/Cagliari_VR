using UnityEngine;

public class KAT_WALK_FEATURE : MonoBehaviour
{
    public float walkSpeed = 2.0f;
    public GameObject centralEyeCamera;

    private readonly OVRInput.Axis2D thumbstickAxis = OVRInput.Axis2D.PrimaryThumbstick;

    // Update is called once per frame
    void LateUpdate()
    {
        Vector2 thumbstickInput = OVRInput.Get(thumbstickAxis);
        if (thumbstickInput.magnitude > 0.01f)
        {
            Debug.Log($"Left Thumbstick Input: {thumbstickInput}");
            Vector3 direction = new(thumbstickInput.x, 0, thumbstickInput.y);
            direction = centralEyeCamera.transform.TransformDirection(direction);
            direction.y = 0; // Keep movement horizontal

            transform.position += Time.deltaTime * walkSpeed * direction.normalized;
        }

        Vector2 rightThumbstickInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        if (rightThumbstickInput.magnitude > 0.01f)
        {
            Debug.Log($"Right Thumbstick Input: {rightThumbstickInput}");
        }
    }
}
