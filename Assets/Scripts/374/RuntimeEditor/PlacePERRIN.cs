using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Placement is similar to GroundPlacementController
// so it extends from it and overrides some functionality

public class PlacePERRIN : GroundPlacementController
{

    public override void ButtonClick()
    {
        // Make sure object is not null
        if (currentPlaceableObject == null)
        {
            // Update selection
            currentPlaceableObject = ObjectToPlace;
            // Give object unique name
            currentPlaceableObject.name = ObjectToPlace.name + "_" + count;
            // Override to follow terrain angle
            followTerrainAngle = true;

            count++;

            // Disable colliders so raycast doesnt break
            var collidersObj = currentPlaceableObject.GetComponentsInChildren<Collider>();
            for (var index = 0; index < collidersObj.Length; index++)
            {
                var colliderItem = collidersObj[index];
                colliderItem.enabled = false;
            }

        }
        else
        {
            Destroy(currentPlaceableObject);
        }
    }

    public override void ReleaseIfClicked()
    {
        // Check for mouse left click
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
