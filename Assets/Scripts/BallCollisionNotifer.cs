using UnityEngine;

public class BallCollisionNotifer : MonoBehaviour
{
    public CameraDirector cameraDirector;
    void OnCollisionEnter(Collision collision)
    {
        // 简单判断碰到的是不是方块 (根据你的命名规则)
        if (collision.gameObject.name.Contains("TargetBlock"))
        {
            // 触发相机的切换逻辑，并把撞到的方块传过去
            cameraDirector.StartFollowingBlocks();
        }
    }
}
