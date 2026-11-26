using UnityEngine;

public class CameraFixed : MonoBehaviour
{
    [Header("Target LookAt")]
    public Transform target;

    void LateUpdate()
    {
        if (target == null) return;
        // fixed camera
        transform.LookAt(target);
    }
    
}
