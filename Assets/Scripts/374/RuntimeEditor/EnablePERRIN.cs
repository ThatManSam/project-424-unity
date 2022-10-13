using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnablePERRIN : MonoBehaviour
{

    [SerializeField]
    private GameObject CameraController;

    [SerializeField]
    private GameObject CameraToDisable;


    void Click()
    {
        CameraToDisable.SetActive(false);
        CameraController.SetActive(true);     
    }
}
