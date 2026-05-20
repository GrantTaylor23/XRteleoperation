using UnityEngine;
 
/// <summary>
/// UR3e Jacobian IK Solver — Pick & Place Edition
/// ================================================
/// - Jacobian position IK drives the EE to the target cube
/// - Joint limits clamp each joint to realistic UR3e ranges
/// - Rest pose bias nudges the arm toward a top-down pick & place configuration
/// </summary>
public class UR3eIKSolver : MonoBehaviour
{
    [Header("Robot Links (base → wrist)")]
    public Transform link1;
    public Transform link2;
    public Transform link3;
    public Transform link4;
    public Transform link5;
    public Transform link6;
 
    [Header("Targets")]
    [Tooltip("The cube the user drags — IK tries to move EE here")]
    public Transform eeTarget;
    [Tooltip("The very tip of the robot (Link6 or a child of Link6)")]
    public Transform eeTip;
 
    [Header("IK Settings")]
    public float learningRate      = 10f;
    public float distanceThreshold = 0.01f;
    public float deltaAngle        = 0.01f;
    public int   maxIterations     = 10;
 
    [Header("Rest Pose Bias")]
    [Tooltip("How strongly to pull joints toward the rest pose (pick & place from above).\n" +
             "Increase if arm drifts into bad configurations.")]
    public float restPoseBias = 0.5f;
 
    [Header("Joint Limits (degrees)")]
    public Vector2 joint1Limits = new Vector2(-360f,  360f);
    public Vector2 joint2Limits = new Vector2(-180f,    0f);
    public Vector2 joint3Limits = new Vector2( -90f,  180f);
    public Vector2 joint4Limits = new Vector2(-180f,    0f);
    public Vector2 joint5Limits = new Vector2(-360f,  360f);
    public Vector2 joint6Limits = new Vector2(-360f,  360f);
 
    [Header("Rest Pose (degrees)")]
    public float restAngle1 =   0f;
    public float restAngle2 = -90f;
    public float restAngle3 =  90f;
    public float restAngle4 = -90f;
    public float restAngle5 =   0f;
    public float restAngle6 =   0f;
 
    readonly Vector3[] jointAxes = new Vector3[]
    {
        Vector3.up,       // link1: Y
        Vector3.forward,  // link2: Z
        Vector3.forward,  // link3: Z
        Vector3.forward,  // link4: Z
        Vector3.up,       // link5: Y
        Vector3.forward   // link6: Z
    };
 
    Transform[] joints;
    Vector2[]   limits;
    bool[]      usesY;
 
    // ─────────────────────────────────────────────────────────────────────────
    void Start()
    {
        joints = new Transform[] { link1, link2, link3, link4, link5, link6 };
        limits = new Vector2[] { joint1Limits, joint2Limits, joint3Limits,
                                 joint4Limits, joint5Limits, joint6Limits };
        usesY  = new bool[] { true, false, false, false, true, false };
    }
 
    // ─────────────────────────────────────────────────────────────────────────
    void Update()
    {
        if (eeTarget == null || eeTip == null) return;
 
        float dist = Vector3.Distance(eeTip.position, eeTarget.position);
        if (dist >= distanceThreshold)
        {
            for (int iter = 0; iter < maxIterations; iter++)
            {
                SolveJacobianStep();
                if (Vector3.Distance(eeTip.position, eeTarget.position) < distanceThreshold)
                    break;
            }
        }
 
        ApplyJointLimits();
    }
 
    // ─────────────────────────────────────────────────────────────────────────
    void SolveJacobianStep()
    {
        Vector3 error      = eeTarget.position - eeTip.position;
        float[] restAngles = { restAngle1, restAngle2, restAngle3,
                               restAngle4, restAngle5, restAngle6 };
 
        for (int i = 0; i < joints.Length; i++)
        {
            if (joints[i] == null) continue;
 
            Vector3 worldAxis = joints[i].TransformDirection(jointAxes[i]);
            Vector3 currentEE = eeTip.position;
 
            joints[i].Rotate(worldAxis, deltaAngle * Mathf.Rad2Deg, Space.World);
            Vector3 jacobianCol = (eeTip.position - currentEE) / deltaAngle;
            joints[i].Rotate(worldAxis, -deltaAngle * Mathf.Rad2Deg, Space.World);
 
            float deltaPos  = Vector3.Dot(jacobianCol, error) * learningRate * Time.deltaTime;
 
            float curAngle  = NormalizeAngle(usesY[i] ? joints[i].localEulerAngles.y
                                                       : joints[i].localEulerAngles.z);
            float biasDelta = (restAngles[i] - curAngle) * restPoseBias * Time.deltaTime;
 
            joints[i].Rotate(worldAxis, (deltaPos + biasDelta) * Mathf.Rad2Deg, Space.World);
        }
    }
 
    // ─────────────────────────────────────────────────────────────────────────
    void ApplyJointLimits()
    {
        for (int i = 0; i < joints.Length; i++)
        {
            if (joints[i] == null) continue;
 
            Vector3 euler = joints[i].localEulerAngles;
            float   angle = NormalizeAngle(usesY[i] ? euler.y : euler.z);
            angle = Mathf.Clamp(angle, limits[i].x, limits[i].y);
 
            if (usesY[i]) euler.y = angle;
            else          euler.z = angle;
 
            joints[i].localEulerAngles = euler;
        }
    }
 
    // ─────────────────────────────────────────────────────────────────────────
    float NormalizeAngle(float angle)
    {
        angle = angle % 360f;
        if (angle > 180f)  angle -= 360f;
        if (angle < -180f) angle += 360f;
        return angle;
    }
}