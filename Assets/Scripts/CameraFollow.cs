using UnityEngine;

// no use, and didn't finish & test yet
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform player;            // 将 Player 拖拽到这里

    [Header("TP Setting")]
    public float distance = 5.0f;       // distance between camera and player
    public float height = 2.0f;         // cam higher than player
    public float smoothSpeed = 5.0f;    // move smoothly while follow
    public float lookAtHeight = 1.5f;   // cam look at higher

    [Header("FP Setting")]
    public float firstPersonHeight = 1.6f;  // height of player's eyes

    [Header("Switch FP/TP")]
    public KeyCode toggleKey = KeyCode.C;   // press key to switch
    private bool isFirstPerson = true;

    private Vector3 velocity = Vector3.zero;
    
    void LateUpdate()
    {
        if (player == null) return;

        if (isFirstPerson)
        {
            // FP
            // Cam sticked on targ pos
            transform.position = player.position + Vector3.up * firstPersonHeight;
            transform.rotation = player.rotation;
        }
        else
        {
            // TP
            Vector3 desiredPosition = player.position - player.forward * distance + Vector3.up * height;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, 1.0f / smoothSpeed);
            // cam look at player
            transform.LookAt(player.position + Vector3.up * 1.5f);
        }

        // switch
        // if (Input.GetKeyDown(toggleKey))
        // {
        //     isFirstPerson = !isFirstPerson;
        
        // }
        if (isFirstPerson)
            player.GetComponentInChildren<MeshRenderer>().enabled = !isFirstPerson;  // player's model invisible while FP
    }
    
}
