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
    public Transform blocksParent;     // Spawner

    [Header("cam stage I: follow ball")]
    public Vector3 look_offset_ball = new Vector3(0, 5, -10);   // Camera locates at back-above the ball
    public float ballSmoothTime = 0.3f;

    [Header("cam stage II: follow blocks")]
    public Vector3 look_offset_blocks = new Vector3(0, 2f, 0); 
    public float blockMoveSpeed = 5f;                           // move speed of camera
    public float blockSmoothTime = 0.5f;
    public float blockHeight = 8f;                              // hight of camera (while following blocks)

    private CameraState currentState = CameraState.FOLLOW_BALL;
    private Vector3 velocity = Vector3.zero;                    // for SmoothDamp
    private List<Transform> blockList = new List<Transform>();  // can be removed
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

        // for test
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
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, ballSmoothTime);
        Quaternion targetRotation = Quaternion.LookRotation(ball.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    // This function has been deprecated, bad performance, might remove later
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

        // find blocks which moving heavily
        foreach (var rb in blockRbs)
        {
            if(rb.position.x < 0)   // only follow one side of circle
                if (rb.linearVelocity.sqrMagnitude > 0.1f || rb.angularVelocity.sqrMagnitude > 0.1f)
                {
                    focusPoint += rb.position;
                    activeBlockCount++;
                }
        }

        if (activeBlockCount == 0)
        {
            // camera keep staying in the final position
            return;
        }

        focusPoint /= activeBlockCount;
        
        // todo:
        // think focusPoint is better calculated by the last n active blocks
        // manage using queues

        Vector3 desiredPosition = focusPoint + new Vector3(0, blockHeight, -10);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, blockSmoothTime);

        Quaternion targetRotation = Quaternion.LookRotation(focusPoint + look_offset_blocks - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    public void SwitchCamMode()
    {
        if (currentState == CameraState.FOLLOW_BALL)
        {
            currentState = CameraState.FOLLOW_BLOCKS;
            // currentBlockIndex = 0; // 从头开始走
            // velocity = Vector3.zero;
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
            velocity = Vector3.zero;    // reset, prevent inertia causing
            Debug.Log("do StartFollowingBlocks()");
        }
    }
}