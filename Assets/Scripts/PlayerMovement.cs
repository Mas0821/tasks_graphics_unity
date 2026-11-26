using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5.0f;
    public float rotationSpeed = 10f; // 转身速度

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        
        Vector3 move = new Vector3(h, 0, v);
        if (move.magnitude > 0)
        {
            transform.Translate(move.normalized * speed * Time.deltaTime, Space.World);

            Vector3 lookDir = new Vector3(h, 0, v);
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}