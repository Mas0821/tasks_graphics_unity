using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class CameraDirector : MonoBehaviour
{
    // 定义相机状态
    private enum CameraState
    {
        FOLLOW_BALL,
        FOLLOW_BLOCKS
    }

    [Header("Target")]
    public Transform ball;
    public Transform blocksParent;     // from Spawner

    [Header("阶段一：跟随小球")]
    public Vector3 look_offset_ball = new Vector3(0, 5, -10); // 相机在小球后上方的偏移
    public float ballSmoothTime = 0.3f;                 // 跟随小球的平滑时间

    [Header("阶段二：沿轨迹移动")]
    public Vector3 look_offset_blocks = new Vector3(0, 2f, 0); 
    public float blockMoveSpeed = 5f;                   // 相机沿轨迹移动的速度
    public float blockSmoothTime = 0.5f;                // 沿轨迹移动的平滑时间
    public float blockHeight = 8f;                      // 相机看着方块的悬挂高度

    private CameraState currentState = CameraState.FOLLOW_BALL;
    private Vector3 velocity = Vector3.zero;            // 供 SmoothDamp 使用的速度缓存
    private List<Transform> blockList = new List<Transform>();
    private List<Rigidbody> blockRbs = new List<Rigidbody>();
    private int currentBlockIndex = 0;

    void Start()
    {
        int count = 0;

        if (blocksParent != null)
        {
            for (int i = 0; i < blocksParent.childCount; i++)
            {
                blockList.Add(blocksParent.GetChild(i));
                Rigidbody rb = blocksParent.GetChild(i).GetComponent<Rigidbody>();
                if (rb != null) blockRbs.Add(rb);
                count ++;
            }
        }
        Debug.Log($"blocksParent has {count} blocks in blockList");

        // status at start
        if (ball != null)
        {
            transform.position = ball.position + look_offset_ball;
            transform.LookAt(ball);
        }
    }

    void LateUpdate()
    {
        if (ball == null) return;

        // 调试用：按空格键手动切换状态
        if (Keyboard.current.spaceKey.wasPressedThisFrame) 
        {
            SwitchCamMode();
        }

        switch (currentState)
        {
            case CameraState.FOLLOW_BALL:
                UpdateFollowBall();
                Debug.Log($"switch to FollowBall");
                break;

            case CameraState.FOLLOW_BLOCKS:
                // UpdateFollowBlocks();
                UpdateFollowBlocksDynamically();
                Debug.Log($"switch to FollowBlocks");
                break;
        }
    }

    private void UpdateFollowBall()
    {
        Vector3 desiredPosition = ball.position + look_offset_ball;
        
        // 平滑移动位置
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, ballSmoothTime);
        
        // 平滑旋转朝向小球
        Quaternion targetRotation = Quaternion.LookRotation(ball.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    private void UpdateFollowBlocks()
    {
        if (blockList.Count == 0) return;

        // 获取当前目标方块的位置
        Transform targetBlock = blockList[currentBlockIndex];
        Debug.Log($"FollowBlocks: currentBlockIndex = {currentBlockIndex}");
        
        // 相机保持在目标方块的正上方偏后一点
        Vector3 desiredPosition = targetBlock.position + new Vector3(0, blockHeight, -10);

        // 平滑移动
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, blockSmoothTime);
        
        // 相机看向前方的方块（避免只看脚底）
        Vector3 lookTarget = targetBlock.position;
        Quaternion targetRotation = Quaternion.LookRotation(lookTarget - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);

        // 判断是否移动到了当前目标方块附近，如果是，则切换到下一个方块
        if (Vector3.Distance(transform.position, desiredPosition) < 2f)
        {
            if (currentBlockIndex < blockList.Count - 1)
            {
                currentBlockIndex++;
            }
        }
    }

    private void UpdateFollowBlocksDynamically()
    {
        if (blockRbs.Count == 0) return;

        Vector3 focusPoint = Vector3.zero;
        int activeBlockCount = 0;

        // 遍历所有方块，找出那些正在“动”的方块（即已经倒下的方块）
        foreach (var rb in blockRbs)
        {
            if(rb.position.x < 0)
                // 如果方块的速度大于极小值，或者它的倾斜角度很大
                if (rb.linearVelocity.sqrMagnitude > 0.1f || rb.angularVelocity.sqrMagnitude > 0.1f)
                {
                    focusPoint += rb.position;
                    activeBlockCount++;
                }
        }

        // 如果没有任何方块在动（说明还没倒，或者全倒了），就让相机原地等待或保持最后位置
        if (activeBlockCount == 0)
        {
            // 可选：如果没有方块在动，让相机慢慢看向小球，或者什么都不做
            return;
        }

        // 计算当前所有正在倒下的方块的中心点
        focusPoint /= activeBlockCount;

        // 期望相机位置：方块中心点的正上方偏后
        Vector3 desiredPosition = focusPoint + new Vector3(0, blockHeight, -10);
        
        // 平滑移动相机
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, blockSmoothTime);

        // 让相机看向这个重心点（加上一点高度偏移）
        Quaternion targetRotation = Quaternion.LookRotation(focusPoint + look_offset_blocks - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    public void SwitchCamMode()
    {
        if (currentState == CameraState.FOLLOW_BALL)
        {
            currentState = CameraState.FOLLOW_BLOCKS;
            // currentBlockIndex = 0; // 从头开始走
            // velocity = Vector3.zero; // 重置平滑速度，防止惯性导致镜头猛冲
            UpdateFollowBlocksDynamically();
            Debug.Log("cam switch to FOLLOW_BLOCKS");
        }
        else
        {
            currentState = CameraState.FOLLOW_BALL;
            UpdateFollowBall();
            Debug.Log("cam switch to FOLLOW_BALL");
        }
    }

    public void StartFollowingBlocks()
    {
        if (currentState == CameraState.FOLLOW_BALL)
        {
            currentState = CameraState.FOLLOW_BLOCKS;
            velocity = Vector3.zero; // 重置平滑速度
            Debug.Log("do StartFollowingBlocks()");
        }
    }
}