using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabsScroll : MonoBehaviour
{
    [SerializeField]
    private GameObject SceneController;

    public void ScrollRight()
    {
        SceneController.GetComponent<PrefabsManager>().Scroll(1);
    }

    public void ScrollLeft()
    {
        SceneController.GetComponent<PrefabsManager>().Scroll(0);
    }
}
