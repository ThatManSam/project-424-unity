using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableDisable : MonoBehaviour
{

    //[SerializeField]
    //private GameObject currentUI;

    //[SerializeField]
    //private GameObject currentCamera;

    //[SerializeField]
    //private GameObject nextUI;

    //[SerializeField]
    //private GameObject nextCamera;

    [SerializeField]
    private List<GameObject> ToDisable = new List<GameObject>();

    [SerializeField]
    private List<GameObject> ToEnable = new List<GameObject>();

    public void Click()
    {
        //currentUI.SetActive(false);
        //currentCamera.SetActive(false);
        //nextCamera.SetActive(true);
        //nextUI.SetActive(true);

        for(int i = 0; i < ToDisable.Count; i++)
        {
            ToDisable[i].SetActive(false);
        }

        for(int i =0; i < ToEnable.Count; i++)
        {
            ToEnable[i].SetActive(true);
        }



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
