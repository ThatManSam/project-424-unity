using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraMovement : MonoBehaviour
{

    // Camera movement preferences
    public float cameraTranslationSensitivity = 2;
    public float cameraZoomSensitivity = 10;
    public float cameraRotationSensitivity = 2;
    public float keysSpeed = 2.4f;
    public float movementMultiplier = 2f;

    private string mouseXAxisName = "Mouse X";
    private string mouseYAxisName = "Mouse Y";
    private string scrollAxisName = "Mouse ScrollWheel";
    // Enable disable scroll when other scene
    // elements use scroll wheel
    private bool scrollEnabled = true;

    // Camera component script is attatched to
    Camera _camera;

    void Awake()
    {
        // Get camera component
        _camera = GetComponent<Camera>();
    }

    // Update once per frame
    void Update()
    {

        float moveX = 0;
        float moveY = 0;
        // Move camera along plane when middle mouse is held
        if (Input.GetMouseButton(2))
        {
            moveY = Input.GetAxis(mouseYAxisName) * cameraTranslationSensitivity;
            moveX = Input.GetAxis(mouseXAxisName) * cameraTranslationSensitivity;
        }
        // Zoom in / out with scroll wheel
        if (scrollEnabled)
        {           
            float zoom = Input.GetAxis(scrollAxisName) * cameraZoomSensitivity;
            transform.Translate(-moveX, -moveY, zoom);
        }
        
        float rotateX = 0;
        float rotateY = 0;
        // Move camera when right mouse is held
        if (Input.GetMouseButton(1))
        {
            // Get change in mouse pos to use for rotation
            rotateX = Input.GetAxis(mouseYAxisName) * cameraRotationSensitivity;
            rotateY = Input.GetAxis(mouseXAxisName) * cameraRotationSensitivity;
            
        }

        // Rotate camera to new position
        transform.Rotate(0, rotateY, 0, Space.World);
        transform.Rotate(-rotateX, 0, 0);

        // Get new vector position for camera
        Vector3 position = Vector3.zero;
        float speed = keysSpeed * (Input.GetKey(KeyCode.LeftShift) ? movementMultiplier : 1f) * Time.deltaTime * 9.1f;
        if (Input.GetKey(KeyCode.W)) position += Vector3.forward * speed;
        if (Input.GetKey(KeyCode.S)) position -= Vector3.forward * speed;
        if (Input.GetKey(KeyCode.D)) position += Vector3.right * speed;
        if (Input.GetKey(KeyCode.A)) position -= Vector3.right * speed;
        if (Input.GetKey(KeyCode.E)) position += Vector3.up * speed;
        if (Input.GetKey(KeyCode.Q)) position -= Vector3.up * speed;

        // Move camera to new position
        transform.Translate(position);

        // When F is pressed current selected object will be 'focused'
        if (Input.GetKeyDown(KeyCode.F))
        {
            // Get vector pos of mouse
            Vector3 mousePos = Input.mousePosition;
            // Create ray to mouse pos from camera
            Ray ray = _camera.ScreenPointToRay(mousePos);
            RaycastHit hit;
            // Check for raycast hit to get object to focus
            // Check layer mask to only hit gameobjects not ground
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, (1 << 11)))
            {
                FocusCameraOnGameObject(Camera.main, hit.transform.gameObject);
            }
        }
    }

    // Calculate bounding box around GameObject
    Bounds CalculateObjectBounds(GameObject go)
    {
        // Create bounding box at selected object
        Bounds box = new Bounds(go.transform.position, Vector3.zero);
        // Get renderer components in object
        Object[] renderers = go.GetComponentsInChildren(typeof(Renderer));
        foreach (Renderer r in renderers)
        {
            box.Encapsulate(r.bounds);
        }
        return box;
    }

    // Move camera to focus on GameObject
    void FocusCameraOnGameObject(Camera c, GameObject go)
    {
        // Get bounding box around sekected object
        Bounds box = CalculateObjectBounds(go);
        // Get total size of bounding box
        Vector3 max = box.size;
        // Get radius from of bounding box
        float radius = Mathf.Max(max.x, Mathf.Max(max.y, max.z));
        // Calc distance from camera
        float dist = radius / (Mathf.Sin(c.fieldOfView * Mathf.Deg2Rad / 2f));
        // Move camera to focus object
        c.transform.position = go.transform.position + transform.rotation * Vector3.forward * -dist;
    }

    // Toggle scrolling capability of camera
    public void disableScroll()
    {
        scrollEnabled = false;
    }
    public void enableScroll()
    {
        scrollEnabled = true;
    }
}