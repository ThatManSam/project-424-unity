using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GroundPlacementController : MonoBehaviour
{

    [SerializeField]
    private GameObject SceneController;

    [SerializeField]
    private GameObject PerspectiveCamera;

    [Header("Can be a prefab or an object already in the scene")]
    [SerializeField]
    protected GameObject ObjectToPlace;

    [SerializeField]
    private KeyCode cancelPlacementKey = KeyCode.Escape;

    private KeyCode rotateKey = KeyCode.R;

    protected bool followTerrainAngle = false;

    protected GameObject currentPlaceableObject;

    private float mouseRotation;

    protected int count = 0;

    // Scripts for reference
    HierarchyManager hm;
    CameraMovement cm;
    SelectionManager sm;

    // Get scripts for reference
    public void Start()
    {
        hm = SceneController.GetComponent<HierarchyManager>();
        cm = PerspectiveCamera.GetComponent<CameraMovement>();
        sm = SceneController.GetComponent<SelectionManager>();
        rotateKey = sm.rotateKey;

    }

    public virtual void ButtonClick()
    {
        // Make sure the object exists
        if( currentPlaceableObject == null )
        {
            // Instatiate new instance of prefab
            currentPlaceableObject = Instantiate(ObjectToPlace);
            // Give it a new unique name
            currentPlaceableObject.name = ObjectToPlace.name + "_" + count;
            // Change whether it follow terrain angle or not
            SceneObject script = currentPlaceableObject.GetComponent<SceneObject>();
            script.followTerrainAngle = followTerrainAngle;

            count++;

            // Disable collider so raycast doesnt break
            if (currentPlaceableObject.GetComponent<Collider>() != null)
            {
                currentPlaceableObject.GetComponent<Collider>().enabled = false;
            }

            Debug.Log("Create Object");
            
        }
        else
        {
            Destroy(currentPlaceableObject);
            Debug.Log("Destroy Object");
        }
    }

    // Update is called once per frame
    private void Update()
    {
        // Make sure the object exists
        if (currentPlaceableObject != null)
        {
            MoveCurrentPlaceableObjectToMouse();
            RotatePlaceable();
            ReleaseIfClicked();
            CancelPlacement();
        }

        // Update if prefab should follow terrain angle
        if (this.GetComponentInChildren<Toggle>() != null)
        {
            followTerrainAngle = this.GetComponentInChildren<Toggle>().isOn;
        }
        else followTerrainAngle = true;

    }

    private void CancelPlacement()
    {
        if (Input.GetKeyDown(cancelPlacementKey))
        {
            // Destroy instatiated object if canceled
            Destroy(currentPlaceableObject);
        }
    }

    public virtual void ReleaseIfClicked()
    {
        // Check for mouse click left
        if (Input.GetMouseButtonDown(0))
        {
            // Re-enable collider
            if (currentPlaceableObject.GetComponent<Collider>() != null)
            {
                currentPlaceableObject.GetComponent<Collider>().enabled = true;
            }
            // Add object to hierarchy
            hm.AddObject(currentPlaceableObject);
            currentPlaceableObject = null;
        }
    }

    private void RotatePlaceable()
    {
        // Disable scroll for camera to use for prefab placement rotation
        if (Input.GetKeyDown(rotateKey)) cm.disableScroll();
        else if (Input.GetKeyUp(rotateKey)) cm.enableScroll();

        // While rotate key is held use mouse scroll to update rotation
        if (Input.GetKey(rotateKey))
        {
            // Add additional Y rotation if following terrain angle
            if(followTerrainAngle) mouseRotation += Input.mouseScrollDelta.y;
            else mouseRotation = Input.mouseScrollDelta.y;
        }
        // Update object position
        currentPlaceableObject.transform.Rotate(Vector3.up, mouseRotation * 10f);

    }

    private void MoveCurrentPlaceableObjectToMouse()
    {
        // Create ray from camera to mouse pos
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hitInfo;
        // Check if raycast has hit something
        if(Physics.Raycast(ray, out hitInfo))
        {
            // Move current selection to when ray hit
            currentPlaceableObject.transform.position = hitInfo.point;
            // Update rotation if follow terrain angle enabled
            if (followTerrainAngle)
            {
                currentPlaceableObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
            }
        }
    }
}
