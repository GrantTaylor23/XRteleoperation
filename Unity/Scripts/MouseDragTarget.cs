using UnityEngine;
 
/// <summary>
/// Mouse Drag Target
/// =================
/// Lets the user click and drag the cube (EE target) in the Unity scene view
/// by moving it along a plane at the object's depth from the camera.
///
/// SETUP:
///   Attach this script to the Cube GameObject.
/// </summary>
public class MouseDragTarget : MonoBehaviour
{
    [Tooltip("How far the object can move per frame (clamps fast mouse movements)")]
    public float maxMoveSpeed = 5f;
 
    [Tooltip("Hold this key to move the cube up/down (Y axis) instead of XZ plane")]
    public KeyCode verticalModeKey = KeyCode.LeftShift;
 
    Camera mainCam;
    bool   isDragging = false;
    float  dragDepth;   // distance from camera to object when drag started
 
    // ─────────────────────────────────────────────────────────────────────────
    void Start()
    {
        mainCam = Camera.main;
    }
 
    // ─────────────────────────────────────────────────────────────────────────
    void OnMouseDown()
    {
        dragDepth  = Vector3.Distance(mainCam.transform.position, transform.position);
        isDragging = true;
    }
 
    // ─────────────────────────────────────────────────────────────────────────
    void OnMouseUp()
    {
        isDragging = false;
    }
 
    // ─────────────────────────────────────────────────────────────────────────
    void Update()
    {
        if (!isDragging) return;
 
        Ray   ray       = mainCam.ScreenPointToRay(Input.mousePosition);
        float rayDist;
 
        if (Input.GetKey(verticalModeKey))
        {
            // Shift held: move on vertical plane (XY) facing the camera
            Plane vertPlane = new Plane(
                Vector3.Cross(Vector3.up, mainCam.transform.right),
                transform.position
            );
            if (vertPlane.Raycast(ray, out rayDist))
            {
                Vector3 target = ray.GetPoint(rayDist);
                // Only allow Y movement in this mode
                target.x = transform.position.x;
                target.z = transform.position.z;
                MoveTo(target);
            }
        }
        else
        {
            // Normal: move on horizontal plane (XZ) at object's height
            Plane hPlane = new Plane(Vector3.up, transform.position);
            if (hPlane.Raycast(ray, out rayDist))
            {
                Vector3 target = ray.GetPoint(rayDist);
                target.y = transform.position.y; // lock Y
                MoveTo(target);
            }
        }
    }
 
    // ─────────────────────────────────────────────────────────────────────────
    void MoveTo(Vector3 target)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            maxMoveSpeed * Time.deltaTime
        );
    }
}