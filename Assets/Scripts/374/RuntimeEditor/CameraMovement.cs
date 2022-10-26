using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraMovement : MonoBehaviour
{

    // Camera movement preferences
    public float translationSensitivity = 2;
    public float zoomSensitiviy = 10;
    public float rotationSensitivity = 2;
    public float navigationSpeed = 2.4f;
    public float shiftMultiplier = 2f;

    private string mouseHorizontalAxisName = "Mouse X";
    private string mouseVerticalAxisName = "Mouse Y";
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

    void Update()
    {

        float translateX = 0;
        float translateY = 0;
        // Move camera along plane when middle mouse is held
        if (Input.GetMouseButton(2))
        {
            translateY = Input.GetAxis(mouseVerticalAxisName) * translationSensitivity;
            translateX = Input.GetAxis(mouseHorizontalAxisName) * translationSensitivity;
        }
        // Zoom in / out with scroll wheel
        if (scrollEnabled)
        {           
            float zoom = Input.GetAxis(scrollAxisName) * zoomSensitiviy;
            transform.Translate(-translateX, -translateY, zoom);
        }
        
        float rotationX = 0;
        float rotationY = 0;
        // Move camera when right mouse is held
        if (Input.GetMouseButton(1))
        {
            // Get change in mouse pos to use for rotation
            rotationX = Input.GetAxis(mouseVerticalAxisName) * rotationSensitivity;
            rotationY = Input.GetAxis(mouseHorizontalAxisName) * rotationSensitivity;
            
        }

        // Rotate camera to new position
        transform.Rotate(0, rotationY, 0, Space.World);
        transform.Rotate(-rotationX, 0, 0);

        // Get new vector position for camera
        Vector3 move = Vector3.zero;
        float speed = navigationSpeed * (Input.GetKey(KeyCode.LeftShift) ? shiftMultiplier : 1f) * Time.deltaTime * 9.1f;
        if (Input.GetKey(KeyCode.W))
            move += Vector3.forward * speed;
        if (Input.GetKey(KeyCode.S))
            move -= Vector3.forward * speed;
        if (Input.GetKey(KeyCode.D))
            move += Vector3.right * speed;
        if (Input.GetKey(KeyCode.A))
            move -= Vector3.right * speed;
        if (Input.GetKey(KeyCode.E))
            move += Vector3.up * speed;
        if (Input.GetKey(KeyCode.Q))
            move -= Vector3.up * speed;

        // Move camera to new position
        transform.Translate(move);

        // When F is pressed current selected object will be 'focused'
        if (Input.GetKeyDown(KeyCode.F))
        {
            // Get vector pos of mouse
            Vector3 mp = Input.mousePosition;
            // Create ray to mouse pos from camera
            Ray ray = _camera.ScreenPointToRay(mp);
            RaycastHit hit;
            // Check for raycast hit to get object to focus
            // Check layer mask to only hit gameobjects not ground
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, (1 << 11)))
            {
                FocusCameraOnGameObject(Camera.main, hit.transform.gameObject);
            }
        }
    }

    // http://answers.unity3d.com/questions/13267/how-can-i-mimic-the-frame-selected-f-camera-move-z.html
    Bounds CalculateBounds(GameObject go)
    {
        // Get bounding box around selected object
        Bounds b = new Bounds(go.transform.position, Vector3.zero);
        // Get renderer components in object
        Object[] rList = go.GetComponentsInChildren(typeof(Renderer));
        foreach (Renderer r in rList)
        {
            b.Encapsulate(r.bounds);
        }
        return b;
    }

    void FocusCameraOnGameObject(Camera c, GameObject go)
    {
        // Get bounding box around sekected object
        Bounds b = CalculateBounds(go);
        // Get total size of bounding box
        Vector3 max = b.size;
        // Get radius from of bounding box
        float radius = Mathf.Max(max.x, Mathf.Max(max.y, max.z));
        // Calc distance from camera
        float dist = radius / (Mathf.Sin(c.fieldOfView * Mathf.Deg2Rad / 2f));
        // Move camera to focus object
        c.transform.position = go.transform.position + transform.rotation * Vector3.forward * -dist;
    }

    public void disableScroll()
    {
        scrollEnabled = false;
    }

    public void enableScroll()
    {
        scrollEnabled = true;
    }
}