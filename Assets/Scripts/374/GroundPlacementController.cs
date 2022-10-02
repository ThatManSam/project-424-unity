using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Adapted from
// https://www.youtube.com/watch?v=YI6F1x4pzpg&t=0s

public class GroundPlacementController : MonoBehaviour
{

    [SerializeField]
    private GameObject SceneController;

    [SerializeField]
    private GameObject PerspectiveCamera;

    [SerializeField]
    private GameObject prefab;

    [SerializeField]
    private KeyCode cancelPlacementKey = KeyCode.Escape;

    private KeyCode rotateKey = KeyCode.R;

    public bool followTerrainAngle = false;

    private GameObject currentPlaceableObject;

    private float mouseRotation;

    private int count = 0;

    HierarchyManager hm;
    CameraMovement cm;
    SelectionManager sm;

    public void Start()
    {
        hm = SceneController.GetComponent<HierarchyManager>();
        cm = PerspectiveCamera.GetComponent<CameraMovement>();
        sm = SceneController.GetComponent<SelectionManager>();
        rotateKey = sm.rotateKey;

    }

    public void ButtonClick()
    {

        if( currentPlaceableObject == null )
        {
            currentPlaceableObject = Instantiate(prefab);
            currentPlaceableObject.name = prefab.name + "_" + count;
            //currentPlaceableObject.AddComponent("SceneObject");
            SceneObject script = currentPlaceableObject.GetComponent<SceneObject>();
            script.followTerrainAngle = followTerrainAngle;

            count++;

            //// Disable collider so raycast doesnt break
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
        
        if(currentPlaceableObject != null)
        {
            MoveCurrentPlaceableObjectToMouse();
            RotatePlaceable();
            ReleaseIfClicked();
            CancelPlacement();
        }

    }

    private void CancelPlacement()
    {
        if (Input.GetKeyDown(cancelPlacementKey))
        {
            Destroy(currentPlaceableObject);
        }
    }

    private void ReleaseIfClicked()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Re-enable collider
            if (currentPlaceableObject.GetComponent<Collider>() != null)
            {
                currentPlaceableObject.GetComponent<Collider>().enabled = true;
            }

            //HierarchyManager script = SceneController.GetComponent<HierarchyManager>();
            hm.AddObject(currentPlaceableObject);

            currentPlaceableObject = null;
        }
    }

    private void RotatePlaceable()
    {
        //cm = PerspectiveCamera.GetComponent<CameraMovement>();
        // Disable scroll for camera to use for rotation
        if (Input.GetKeyDown(rotateKey)) cm.disableScroll();
        else if (Input.GetKeyUp(rotateKey)) cm.enableScroll();

        // While rotate key is held use mouse scroll to update rotation
        if (Input.GetKey(rotateKey))
        {
            if(followTerrainAngle) mouseRotation += Input.mouseScrollDelta.y;
            else mouseRotation = Input.mouseScrollDelta.y;
        }
        //Debug.Log("Mouse Rotation: " + mouseRotation);
        currentPlaceableObject.transform.Rotate(Vector3.up, mouseRotation * 10f);

    }

    private void MoveCurrentPlaceableObjectToMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hitInfo;
        if(Physics.Raycast(ray, out hitInfo))
        {
            currentPlaceableObject.transform.position = hitInfo.point;
            if (followTerrainAngle)
            {
                currentPlaceableObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
            }
        }
    }
}
