using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class WayPoints : MonoBehaviour
    {
         // (Lat-Lon-Elevation(m))
        public List<DVector3> RealWorldPoints = new List<DVector3>();

        // (Space Positions)
        [HideInInspector]
        public List<Vector3> UnityWorldSpacePoints = new List<Vector3>();

        public void ConvertLatLonToSpacePosition (TerrainContainerObject terrainContainer,bool InstantiateGameObjects)
        {
            UnityWorldSpacePoints = new List<Vector3>();

            this.transform.DestroyChildren();

            foreach (var point in RealWorldPoints)
            {
                var spaceP = GeoRefConversion.SetRealWorldPosition(terrainContainer, new DVector2(point.x, point.y), (float)point.z, SetElevationMode.RelativeToSeaLevel);

                UnityWorldSpacePoints.Add(spaceP);

                if(InstantiateGameObjects)
                {
                    var p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    p.name = "Point_"+ RealWorldPoints.IndexOf(point).ToString();
                    p.transform.position = spaceP;
                    p.transform.parent = this.transform;
                }
            }

        }

    }
}