using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoaderToEditor : MonoBehaviour
{

    [SerializeField]
    private GameObject LoaderUI;

    [SerializeField]
    private GameObject LoaderCamera;

    [SerializeField]
    private GameObject EditorUI;

    [SerializeField]
    private GameObject EditorCamera;

    public void Click()
    {
        LoaderUI.SetActive(false);
        LoaderCamera.SetActive(false);
        EditorCamera.SetActive(true);
        //EditorCamera.transform.position = GISLoaderCamera.transform.position;
        //EditorCamera.transform.rotation = GISLoaderCamera.transform.rotation;
        EditorUI.SetActive(true);



        //GameObject roads = GameObject.Find("Terrains");
        //if(roads != null)
        //{
        //    Debug.Log("Roads found");
        //    ChangeLayer(roads, LayerMask.NameToLayer("Roads"));
        //}
    }

    private void ChangeLayer(GameObject root, int layer)
    {
        if(root.GetComponent<Terrain>() == null)
        {
            root.layer = layer;
        }
        
        foreach(Transform child in root.transform)
        {
            //child.gameObject.layer = layer;
            ChangeLayer(child.gameObject, layer);
        }
    }
}
