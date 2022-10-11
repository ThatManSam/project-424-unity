using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DisableCamMovements : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private GameObject PerspectiveCamera;

    CameraMovement cm;

    void Start()
    {
        cm = PerspectiveCamera.GetComponent<CameraMovement>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Enter");
        cm.disableScroll();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Exit");
        cm.enableScroll();
    }



}
