using UnityEngine;

public class SpawnerBlocks : MonoBehaviour
{
    [Header("Setting for auto create blocks")]
    public float spacing = 20f;      // space between blocks
    public Vector3 position_start;
    public int r = 250;
    // 1. go -z for r
    // 2. go circle of r
    public Vector3 direction = Vector3.forward;
    public Vector3 scale;           // = new Vector3{10, 30, 5};
    public float mass;              //= 5f;
    public float angular_damping;   //= 1f;
    public Vector3 center_mass;     //= new Vector3(0, -5f, 0);
    public Material block_mat; 

    // switch on board Inspector ...
    [ContextMenu("Auto Create Blocks")]     // should next to its func
    public void GenerateBlocks()
    {
        // clean previous
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        int count_r = (int)(r / spacing);
        int count_c = (int)(2 * 3.14 * r / spacing);
        Debug.Log($"todo: create {count_r} and {count_c} blocks in r = {r} circle");

        // 1
        for (int i = 0; i < count_r; i++)
        {
            // 1. 计算位置：起点 + 方向 * 间距 * 索引
            Vector3 pos = position_start + direction.normalized * i * spacing;
            pos.y = scale.y / 2; 

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "TargetBlock_" + i;
            
            cube.transform.SetParent(this.transform);
            cube.transform.position = pos;
            cube.transform.localScale = scale;
            
            Rigidbody rb = cube.AddComponent<Rigidbody>();
            rb.mass = mass;
            rb.angularDamping = angular_damping;
            rb.centerOfMass = center_mass;

            Renderer rend = cube.GetComponent<Renderer>();
            rend.sharedMaterial = block_mat;
        }
        Debug.Log($"create {count_r} blocks in z-direction");
        // 2
        for (int i = 0; i < count_c; i++)
        {
            float angle = i * Mathf.PI * 2 / count_c;   // radian
            angle = (angle + 180) % 360;                // for CameraDirections::UpdateFollowBlocks(), no necessary now
            // polar --> cartesian
            float x = Mathf.Cos(angle) * r;
            float z = Mathf.Sin(angle) * r;
            Vector3 pos = new Vector3(x, scale.y / 2f, z);
            // Debug.Log($"create block x:y = {x}:{z}in circle. pos = {pos}");
            
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "TargetBlock_" + (i + count_r);
            cube.transform.SetParent(this.transform);
            cube.transform.position = pos;
            cube.transform.localScale = scale;
            
            // Vector3 center = new Vector3(0, pos.y, 0);
            // cube.transform.LookAt(center);
            // cube.transform.Rotate(0, 90f, 0); 
            Vector3 tangent_direction = new Vector3(pos.z, 0, -pos.x).normalized;
            cube.transform.rotation = Quaternion.LookRotation(tangent_direction);
            if(pos.x < 0) cube.transform.Rotate(0, 45f, 0); 
            else cube.transform.Rotate(0, -45f, 0); 
            
            Rigidbody rb = cube.AddComponent<Rigidbody>();
            rb.mass = mass;
            rb.angularDamping = angular_damping;
            rb.centerOfMass = center_mass;
            
            Renderer rend = cube.GetComponent<Renderer>();
            rend.sharedMaterial = block_mat;
        }
        Debug.Log($"create {count_c} blocks in circle. O = {position_start}");
    }
}