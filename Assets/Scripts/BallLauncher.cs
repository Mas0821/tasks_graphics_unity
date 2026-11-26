using UnityEngine;
using UnityEngine;

public class BallLauncher : MonoBehaviour
{
    [Header("Launch")]
    public float force_forward = 30f;
    public float force_downward = 2f;
    
    [Header("RollTorque")]
    public float torque_roll = 20f;
    // The torque is perpendicular to the direction of motion

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        if (rb == null)
        {
            Debug.LogError("Ball without Rigidbody");
            return;
        }

        rb.AddForce(Vector3.forward * force_forward, ForceMode.Impulse);    // go -z
        rb.AddTorque(Vector3.right * torque_roll, ForceMode.Impulse);       // torque on x
    }
}