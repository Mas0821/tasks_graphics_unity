using UnityEngine;

public class CameraFixed : MonoBehaviour
{
    [Header("Target LookAt")]
    public Transform target;

    void LateUpdate()
    {
        // 如果目标为空，直接返回，保持原有姿态
        if (target == null) return;

        // 位置是固定的，什么都不用写。只改变旋转。
        // LookAt 会让相机的 Z 轴（前方）永远指向目标点
        transform.LookAt(target);
    }
    
}
