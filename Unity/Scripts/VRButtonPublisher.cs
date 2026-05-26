using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Std;
 
public class VRButtonPublisher : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "/g8_gripper_cmd";
    private Float32Msg clawMsg;
 
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<Float32Msg>(topicName);
        clawMsg = new Float32Msg(0.0f); // Default to open
    }
 
    void Update()
    {
        bool primaryButtonPressed = false;
        bool secondaryButtonPressed = false;
        bool keyboardPrimaryPressed = Keyboard.current != null && Keyboard.current.aKey.isPressed;
        bool keyboardSecondaryPressed = Keyboard.current != null && Keyboard.current.sKey.isPressed;
 
        // Get right hand controller
        var rightHandDevice = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.RightHand);
       
        // Check primary button (A/X)
        rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out primaryButtonPressed);
       
        // Check secondary button (B/Y)
        rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out secondaryButtonPressed);
 
        // Determine whether either button is pressed
        bool primaryPressed = primaryButtonPressed || keyboardPrimaryPressed;
        bool secondaryPressed = secondaryButtonPressed || keyboardSecondaryPressed;
        float newClawState = clawMsg.data;
 
        if (primaryPressed)
        {
            newClawState += 0.0001f; // Close incrementally
        }
        else if (secondaryPressed)
        {
            newClawState -= 0.0001f; // Open incrementally
        }
 
        if (primaryPressed || secondaryPressed)
        {
            newClawState = Mathf.Clamp(newClawState, 0.0f, 0.1f);
            clawMsg.data = newClawState;
            ros.Publish(topicName, clawMsg);
        }
    }
}
 