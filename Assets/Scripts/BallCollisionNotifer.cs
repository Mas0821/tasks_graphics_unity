using UnityEngine;

public class BallCollisionNotifer : MonoBehaviour
{
    public CameraDirector cameraDirector;
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.Contains("TargetBlock"))
        {
            cameraDirector.StartFollowingBlocks();
        }
    }
}
