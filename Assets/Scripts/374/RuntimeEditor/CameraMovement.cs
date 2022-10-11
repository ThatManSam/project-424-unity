using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraMovement : MonoBehaviour
{

    /* 
     * Code from
     * https://forum.unity.com/threads/how-to-make-camera-move-in-a-way-similar-to-editor-scene.524645/
     * https://gist.github.com/david-hodgetts/aee82a4e9f22b14b1b63
     */


    public float translationSensitivity = 2;
    public float zoomSensitiviy = 10;

    public float rotationSensitivity = 2;
    
    public float navigationSpeed = 2.4f;
    public float shiftMultiplier = 2f;


    public string mouseHorizontalAxisName = "Mouse X";
    public string mouseVerticalAxisName = "Mouse Y";
    public string scrollAxisName = "Mouse ScrollWheel";

    Camera _camera;

    private bool scrollEnabled = true;

    void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    void Update()
    {
        //  translation
        float translateX = 0;
        float translateY = 0;

        if (Input.GetMouseButton(2))
        {
            translateY = Input.GetAxis(mouseVerticalAxisName) * translationSensitivity;
            translateX = Input.GetAxis(mouseHorizontalAxisName) * translationSensitivity;
        }

        if (scrollEnabled)
        {           
            float zoom = Input.GetAxis(scrollAxisName) * zoomSensitiviy;

            transform.Translate(-translateX, -translateY, zoom);
        }
        

        // rotation

        float rotationX = 0;
        float rotationY = 0;

        if (Input.GetMouseButton(1))
        {
            rotationX = Input.GetAxis(mouseVerticalAxisName) * rotationSensitivity;
            rotationY = Input.GetAxis(mouseHorizontalAxisName) * rotationSensitivity;

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
            transform.Translate(move);
        }

        transform.Rotate(0, rotationY, 0, Space.World);
        transform.Rotate(-rotationX, 0, 0);

        // Focus
        if (Input.GetKeyDown(KeyCode.F))
        {
            Vector3 mp = Input.mousePosition;
            Ray ray = _camera.ScreenPointToRay(mp);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                FocusCameraOnGameObject(Camera.main, hit.transform.gameObject);
            }
        }
    }

    // http://answers.unity3d.com/questions/13267/how-can-i-mimic-the-frame-selected-f-camera-move-z.html
    Bounds CalculateBounds(GameObject go)
    {
        Bounds b = new Bounds(go.transform.position, Vector3.zero);
        Object[] rList = go.GetComponentsInChildren(typeof(Renderer));
        foreach (Renderer r in rList)
        {
            b.Encapsulate(r.bounds);
        }
        return b;
    }

    void FocusCameraOnGameObject(Camera c, GameObject go)
    {
        Bounds b = CalculateBounds(go);
        Vector3 max = b.size;
        float radius = Mathf.Max(max.x, Mathf.Max(max.y, max.z));
        float dist = radius / (Mathf.Sin(c.fieldOfView * Mathf.Deg2Rad / 2f));
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