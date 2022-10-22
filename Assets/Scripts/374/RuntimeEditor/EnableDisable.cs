using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableDisable : MonoBehaviour
{

    [SerializeField]
    private List<GameObject> ToDisable = new List<GameObject>();

    [SerializeField]
    private List<GameObject> ToEnable = new List<GameObject>();

    public void ToggleGameObjects()
    {
        for(int i = 0; i < ToDisable.Count; i++)
        {
            ToDisable[i].SetActive(false);
        }

        for(int i =0; i < ToEnable.Count; i++)
        {
            ToEnable[i].SetActive(true);
        }
    }
}
