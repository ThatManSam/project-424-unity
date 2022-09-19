/*     Unity GIS Tech 2020-2021      */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GISTech.GISTerrainLoader
{
    public class GeoRef : MonoBehaviour
    {
        // Use this Option to georeference a terrain in generated/Modfied in the edit mode
        public TerrainOriginMode OriginProjectionMode = TerrainOriginMode.FromPlayMode;

        public Dropdown Projections;

        public Text LatLonText;
        public Text ElevationText;

        //Use Mask after adding Terrain to layers list 
        public LayerMask TerrainLayer;

        private Terrain m_terrain;
        private GISTerrainLoaderRuntimePrefs prefs;

        public Terrain terrain
        {
            get { return m_terrain; }
            set
            {
                if (m_terrain != value)
                {
                    m_terrain = value;

                }
            }
        }

        private RaycastHit hitInfo;
        private Ray ray;


        private RuntimeTerrainGenerator runtimeGenerator;
        void Start()
        {
            prefs = GISTerrainLoaderRuntimePrefs.Get;
            runtimeGenerator = RuntimeTerrainGenerator.Get;
            RuntimeTerrainGenerator.SendTerrainOrigin += UpdateOrigin;

            if (Projections)
            Projections.onValueChanged.AddListener(OnProjectionChanged);

            if(OriginProjectionMode == TerrainOriginMode.FromEditor)
            {
                var terrain = GameObject.FindObjectOfType<TerrainContainerObject>();

                if (terrain)
                {
                    RuntimeTerrainGenerator.Get.SetGeneratedTerrain(terrain);
                }
                else
                {
                    OriginProjectionMode = TerrainOriginMode.FromPlayMode;
                    Debug.LogError("Terrain not found in the curret scene, generate a terrain in the editor mode or set the'Origin Projection Mode' to PlayMode ");

                }

            }

        }
        
        /// <summary>
        /// Update terrain Origin for GeoRefrence 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="minelevation"></param>
        /// <param name="maxelevation"></param>
        private void UpdateOrigin(DVector2 origin, float minelevation, float maxelevation)
        {
            GeoRefConversion.SetLocalOrigin(origin);
        }

        void Update()
        {
            RayCastMousePosition();
        }

        private void RayCastMousePosition()
        {
            hitInfo = new RaycastHit();

            if (Camera.main)
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, Mathf.Infinity, TerrainLayer))
                {
                    if (terrain == null)
                    {
                        terrain = hitInfo.collider.transform.gameObject.GetComponent<Terrain>();
                        ElevationText.text = GetHeight(terrain, hitInfo.point).ToString() + " m ";
                    }


                    if (terrain != null)
                    {
                        if (!string.Equals(hitInfo.collider.transform.name, terrain.name))
                        {
                            terrain = hitInfo.collider.transform.gameObject.GetComponent<Terrain>();
                            ElevationText.text = GetHeight(terrain, hitInfo.point).ToString() + " m ";
                        }
                    }


                    var mousePos = new Vector3(hitInfo.point.x, hitInfo.point.y, hitInfo.point.z);

                    var RWP = GetObjectLatLonElevation(mousePos, runtimeGenerator.GeneratedContainer, RealWorldElevation.Elevation);

                    if (terrain != null)
                    {


                        if (ElevationText)
                            ElevationText.text = Math.Round(RWP.z,2) + " m ";
                    }

                    if (LatLonText)
                        LatLonText.text = GetPosition(new DVector2(RWP.x, RWP.y), prefs.Projection);

                }
            }
 
        }

        /// <summary>
        /// Get the real world Elevation of an object
        /// </summary>
        /// <returns></returns>
        public DVector3 GetObjectLatLonElevation(Vector3 SpacePosition,TerrainContainerObject container,RealWorldElevation ElevationMode)
        {
            var LatLonPos = GeoRefConversion.UnityWorldSpaceToLatLog(SpacePosition, container);

            return new DVector3(LatLonPos.x, LatLonPos.y, Math.Round(GeoRefConversion.GetRealWorldElevation(container, SpacePosition, ElevationMode), 2));
        }

        public float GetHeight(Terrain terrain, Vector3 position)
        {
            float height = 0;

            if(terrain)
            {
                TerrainData t = terrain.terrainData;
                height = terrain.SampleHeight(position);
               
            }
            return height;
        }
        private void OnProjectionChanged(int value)
        {
            var prj = (Projections)value;

            prefs.Projection = prj;
        }
        private string GetPosition(DVector2 LatLon,Projections proj)
        {
            return GeoRefConversion.ConvertLatLonTO(LatLon, proj); ;
        }

        void OnDisable()
        {
            RuntimeTerrainGenerator.SendTerrainOrigin -= UpdateOrigin;
        }

    }
}