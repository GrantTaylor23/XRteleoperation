using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class Colourchange : MonoBehaviour
{
    public Color defaultColor = Color.red;
    public Color hoverColor = Color.yellow;
    public Color grabColor = Color.green;

    [Tooltip("Tag to identify the VR controller collider entering the cube.")]
    public string controllerTag = "VRController";

    [Tooltip("Fallback button used to simulate grab in editor or when the back button is not mapped.")]
    public KeyCode grabButton = KeyCode.JoystickButton6;

    private Renderer cubeRenderer;
    private bool controllerInside;
    private bool grabbed;

    private void Awake()
    {
        cubeRenderer = GetComponent<Renderer>();
        SetCubeColor(defaultColor);
    }

    private void Update()
    {
        if (controllerInside && !grabbed && IsGrabButtonPressed())
        {
            GrabCube();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsControllerCollider(other))
        {
            controllerInside = true;
            if (!grabbed)
            {
                SetCubeColor(hoverColor);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsControllerCollider(other))
        {
            controllerInside = false;
            if (!grabbed)
            {
                SetCubeColor(defaultColor);
            }
        }
    }

    private bool IsControllerCollider(Collider other)
    {
        if (!string.IsNullOrEmpty(controllerTag) && other.CompareTag(controllerTag))
        {
            return true;
        }

        return other.name.ToLower().Contains("controller") || other.name.ToLower().Contains("hand");
    }

    private bool IsGrabButtonPressed()
    {
        return Input.GetKeyDown(grabButton) || Input.GetKeyDown(KeyCode.Escape);
    }

    private void GrabCube()
    {
        grabbed = true;
        SetCubeColor(grabColor);
    }

    private void SetCubeColor(Color color)
    {
        if (cubeRenderer != null)
        {
            cubeRenderer.material.color = color;
        }
    }

    public void ResetCube()
    {
        grabbed = false;
        controllerInside = false;
        SetCubeColor(defaultColor);
    }

    public void SimulateGrab()
    {
        if (controllerInside && !grabbed)
        {
            GrabCube();
        }
    }
}
