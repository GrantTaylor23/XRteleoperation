using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
public class JointPublisher : MonoBehaviour
{
    [Header("ROS")]
    public string jointStatesTopic = "/g8_joint_states";
    [Header("Joint Links")]
    public Transform joint1;   // shoulder_pan  — Y axis
    public Transform joint2;   // shoulder_lift — Z axis, offset -π/2
    public Transform joint3;   // elbow         — Z axis
    public Transform joint4;   // wrist_1       — Z axis, offset -π/2
    public Transform joint5;   // wrist_2       — Y axis
    public Transform joint6;   // wrist_3       — Z axis
    ROSConnection ros;
    bool  registered = false;
    float timer      = 0f;
    static readonly string[] JOINT_NAMES = new string[]
    {
        "Joint_1",
        "Joint_2",
        "Joint_3",
        "Joint_4",
        "Joint_5",
        "Joint_6"
    };
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Connect("127.0.0.1", 10000);
        Debug.Log("[JointPublisher] Connected to ROS");
    }
    float publishInterval = 0.5f;
    float publishTimer = 0f;
	void Update()
	{
	    timer += Time.deltaTime;
	    if (timer < 1.5f) return;
	    if (!registered)
	    {
		ros.RegisterPublisher<JointStateMsg>(jointStatesTopic);
		registered = true;
		return;
	    }
	    publishTimer += Time.deltaTime;
	    if (publishTimer < publishInterval) return;
	    publishTimer = 0f;
	    PublishJointStates();
	}
    void PublishJointStates()
    {
        double[] q = new double[6];
        q[0] = NormalizeAngle(joint1 != null ? joint1.localEulerAngles.y : 0f);
        q[1] = NormalizeAngle(joint2 != null ? joint2.localEulerAngles.z : 0f) - 1.5708;
        q[2] = NormalizeAngle(joint3 != null ? joint3.localEulerAngles.z : 0f);
        q[3] = NormalizeAngle(joint4 != null ? joint4.localEulerAngles.z : 0f) - 1.5708;
        q[4] = NormalizeAngle(joint5 != null ? joint5.localEulerAngles.y : 0f);
        q[5] = joint6 != null ? NormalizeAngle(joint6.localEulerAngles.z) : 0.0;
        var msg = new JointStateMsg
        {
            header = new HeaderMsg
            {
                frame_id = "base_link",
                stamp = new RosMessageTypes.BuiltinInterfaces.TimeMsg
                {
                    sec  = (int)Time.time,
                    nanosec = (uint)((Time.time - (int)Time.time) * 1e9)
                }
            },
            name     = JOINT_NAMES,
            position = q,
            velocity = new double[6],
            effort   = new double[6]
        };
        ros.Publish(jointStatesTopic, msg);
    }
    float NormalizeAngle(float degrees)
    {
        float angle = degrees % 360f;
        if (angle > 180f)  angle -= 360f;
        if (angle < -180f) angle += 360f;
        return angle * Mathf.Deg2Rad;
    }
}