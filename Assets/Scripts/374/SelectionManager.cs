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
        if (Input.GetMouseButtonDown(0))
        {
            if (currentSelection != null)
            {
                currentSelection.GetComponentInChildren<Renderer>().material.color = startcolor;
            }

            if (moving)
            {
                moving = false;

                currentSelection.GetComponent<Collider>().enabled = true;

                currentSelection.GetComponentInChildren<Renderer>().material.color = startcolor;

                currentSelection = null;

                if (currentHierarchyItem != null) currentHierarchyItem.GetComponentInChildren<Image>().color = Color.white;

                MoveText.SetActive(false);

            }
            else
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastHit hitInfo;

                if (Physics.Raycast(ray, out hitInfo, 50000.0f, (1 << 11)))
                {

                    Debug.Log("Object Selected: " + hitInfo.transform.name);

                    currentSelection = hitInfo.transform.gameObject;

                    if (currentSelection.GetComponentInChildren<Renderer>().material.color != selectedColor)
                    {

                        startcolor = currentSelection.GetComponentInChildren<Renderer>().material.color;

                        currentSelection.GetComponentInChildren<Renderer>().material.color = selectedColor;

                    }

                    if (currentHierarchyItem != null) currentHierarchyItem.GetComponentInChildren<Image>().color = Color.white;

                    currentHierarchyItem = GameObject.Find("HierarchyItem_" + currentSelection.name);

                    //currentScrollViewItem.GetComponentInChildren<Selectable>().Select();

                    currentHierarchyItem.GetComponentInChildren<Image>().color = selectedColor;

                    MoveText.SetActive(true);

                }
                else
                {
                    if (currentHierarchyItem != null) currentHierarchyItem.GetComponentInChildren<Image>().color = Color.white;

                    currentSelection = null;

                    MoveText.SetActive(false);
                }
            }
        }

        if (manualSelection)
        {
            if (currentSelection.GetComponentInChildren<Renderer>().material.color != selectedColor)
            {
                startcolor = currentSelection.GetComponentInChildren<Renderer>().material.color;
                currentSelection.GetComponentInChildren<Renderer>().material.color = selectedColor;
            }
            manualSelection = false;
            MoveText.SetActive(true);
        }

        if (currentSelection != null)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                moving = true;
            }

            if (moving)
            {

                MoveCurrentPlaceableObjectToMouse();
                RotatePlaceable();

            }
     
            if (Input.GetKey(KeyCode.Delete))
            {
                //ScrollManager.RemoveObject(currentSelection);
                //GameController.GetComponent<ScrollManager>().goList.AddObject(currentSelection);
                HierarchyManager script = SceneController.GetComponent<HierarchyManager>();
                script.RemoveObject(currentSelection);

                Destroy(currentSelection);
            }
        }
    }

    public void SetSelection(GameObject go)
    {
        currentSelection = go;
        manualSelection = true;
        Debug.Log("MANUAL SELECTION: " + go.name);
    }

    private void RotatePlaceable()
    {
        SceneObject so = currentSelection.GetComponent<SceneObject>();
        followTerrainAngle = so.followTerrainAngle;

        CameraMovement script = PerspectiveCamera.GetComponent<CameraMovement>();
        // Disable scroll for camera to use for rotation
        if (Input.GetKeyDown(rotateKey)) script.disableScroll();
        else if (Input.GetKeyUp(rotateKey)) script.enableScroll();

        // While rotate key is held use mouse scroll to update rotation
        if (Input.GetKey(rotateKey))
        {
            if (followTerrainAngle) mouseRotation += Input.mouseScrollDelta.y;
            else mouseRotation = Input.mouseScrollDelta.y;
        }
        currentSelection.transform.Rotate(Vector3.up, mouseRotation * 10f);

    }

    private void MoveCurrentPlaceableObjectToMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo))
        {
            currentSelection.transform.position = hitInfo.point;
            currentSelection.GetComponent<Collider>().enabled = false;
            if (followTerrainAngle)
            {
                currentSelection.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
            }
        }
    }
}
