using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacePERRIN : GroundPlacementController
{

    public override void ButtonClick()
    {

        if (currentPlaceableObject == null)
        {
            currentPlaceableObject = ObjectToPlace;
            currentPlaceableObject.name = ObjectToPlace.name + "_" + count;

            followTerrainAngle = true;

            count++;

            //// Disable colliders so raycast doesnt break
            var collidersObj = currentPlaceableObject.GetComponentsInChildren<Collider>();
            for (var index = 0; index < collidersObj.Length; index++)
            {
                var colliderItem = collidersObj[index];
                colliderItem.enabled = false;
            }

            Debug.Log("Create Object");

        }
        else
        {
            Destroy(currentPlaceableObject);
            Debug.Log("Destroy Object");
        }
    }

    public override void ReleaseIfClicked()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Re-enable colliders
            var collidersObj = currentPlaceableObject.GetComponentsInChildren<Collider>();
            for (var index = 0; index < collidersObj.Length; index++)
            {
                var colliderItem = collidersObj[index];
                colliderItem.enabled = true;
            }

            currentPlaceableObject = null;
        }
    }

}
