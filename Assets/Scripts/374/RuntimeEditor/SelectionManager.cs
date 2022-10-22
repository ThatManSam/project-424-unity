using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionManager : MonoBehaviour
{
    [SerializeField]
    private GameObject SceneController;

    [SerializeField]
    private GameObject PerspectiveCamera;

    [SerializeField]
    private GameObject MoveText;

    public KeyCode rotateKey = KeyCode.R;

    GameObject currentSelection;

    GameObject currentHierarchyItem;

    public Color selectedColor = Color.blue;

    private Color startcolor;

    private bool followTerrainAngle = false;

    private float mouseRotation;

    bool moving = false;

    bool manualSelection = false;

    // Update is called once per frame
    void Update()
    {
        // Check for mouse left click
        if (Input.GetMouseButtonDown(0))
        {
            // Make sure object exists
            if (currentSelection != null)
            {
                // Update colour to original colour
                currentSelection.GetComponentInChildren<Renderer>().material.color = startcolor;
            }
            // Check if selected object is being moved
            if (moving)
            {
                moving = false;
                // Reenable collider
                currentSelection.GetComponent<Collider>().enabled = true;
                // Update colour to orignal colour
                currentSelection.GetComponentInChildren<Renderer>().material.color = startcolor;
                // Deselect 
                currentSelection = null;
                // Reflect selection in hierarchy
                if (currentHierarchyItem != null) currentHierarchyItem.GetComponentInChildren<Image>().color = Color.white;
                // Disable instructions
                MoveText.SetActive(false);

            }
            else
            {
                // Create ray from camera to mouse
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;
                // Raycast out only to SceneObjects (11) layer
                if (Physics.Raycast(ray, out hitInfo, 50000.0f, (1 << 11)))
                {
                    Debug.Log("Object Selected: " + hitInfo.transform.name);
                    // Update selection
                    currentSelection = hitInfo.transform.gameObject;
                    // Make sure selected object hasnt already had the colour changed
                    if (currentSelection.GetComponentInChildren<Renderer>().material.color != selectedColor)
                    {
                        // Save start colour for deselection
                        startcolor = currentSelection.GetComponentInChildren<Renderer>().material.color;
                        // Change select object colour to reflect selection
                        currentSelection.GetComponentInChildren<Renderer>().material.color = selectedColor;
                    }
                    // Reflect selection in hierarchy
                    if (currentHierarchyItem != null) currentHierarchyItem.GetComponentInChildren<Image>().color = Color.white;                    
                    currentHierarchyItem = GameObject.Find("HierarchyItem_" + currentSelection.name);
                    currentHierarchyItem.GetComponentInChildren<Image>().color = selectedColor;
                    // Enable instructions
                    MoveText.SetActive(true);
                }
                else
                {
                    // If raycast did not hit anything, deselect
                    if (currentHierarchyItem != null) currentHierarchyItem.GetComponentInChildren<Image>().color = Color.white;
                    currentSelection = null;
                    MoveText.SetActive(false);
                }
            }
        }
        // Check for manual selection
        if (manualSelection)
        {
            // Make sure selected object hasnt already had the colour changed
            if (currentSelection.GetComponentInChildren<Renderer>().material.color != selectedColor)
            {
                // Save start colour for deselection
                startcolor = currentSelection.GetComponentInChildren<Renderer>().material.color;
                // Change select object colour to reflect selection
                currentSelection.GetComponentInChildren<Renderer>().material.color = selectedColor;
            }
            manualSelection = false;
            MoveText.SetActive(true);
        }
        // Make sure object exists
        if (currentSelection != null)
        {
            // Check for Space key click
            if (Input.GetKey(KeyCode.Space))
            {
                // Enable object to move
                moving = true;
            }
            // Check if object is moving
            if (moving)
            {
                // Update object position
                MoveCurrentPlaceableObjectToMouse();
                RotatePlaceable();
            }
            // Check for Delete key click
            if (Input.GetKey(KeyCode.Delete))
            {
                // Get Hierarchy Manager script
                HierarchyManager script = SceneController.GetComponent<HierarchyManager>();
                // Remove object from hierarchy
                script.RemoveObject(currentSelection);
                // Delete prefab
                Destroy(currentSelection);
            }
        }
    }

    public void SetSelection(GameObject go)
    {
        // Set manual selection
        currentSelection = go;
        manualSelection = true;
        Debug.Log("MANUAL SELECTION: " + go.name);
    }

    private void RotatePlaceable()
    {
        // Get object script
        SceneObject so = currentSelection.GetComponent<SceneObject>();
        // Get whether object follows terrian angle or not
        followTerrainAngle = so.followTerrainAngle;
        // Get camera movement script
        CameraMovement script = PerspectiveCamera.GetComponent<CameraMovement>();
        // Disable scroll for camera to use for rotation
        if (Input.GetKeyDown(rotateKey)) script.disableScroll();
        else if (Input.GetKeyUp(rotateKey)) script.enableScroll();
        // While rotate key is held use mouse scroll to update rotation
        if (Input.GetKey(rotateKey))
        {
            // Update Y delta
            if (followTerrainAngle) mouseRotation += Input.mouseScrollDelta.y;
            else mouseRotation = Input.mouseScrollDelta.y;
        }
        // Update rotation
        currentSelection.transform.Rotate(Vector3.up, mouseRotation * 10f);

    }

    private void MoveCurrentPlaceableObjectToMouse()
    {
        // Create ray from camera to mouse
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        // Check if raycast hit anything
        if (Physics.Raycast(ray, out hitInfo))
        {
            // Set object pos to where raycast hit
            currentSelection.transform.position = hitInfo.point;
            currentSelection.GetComponent<Collider>().enabled = false;
            // Update angle to terrain
            if (followTerrainAngle)
            {
                currentSelection.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
            }
        }
    }
}
