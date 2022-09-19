/*     Unity GIS Tech 2020-2021      */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace GISTech.GISTerrainLoader
{

    public class SimpleTerrainGenerator : MonoBehaviour
    {
        public KeyCode GenerateKey;
        public WayPoints GeoWaypoints;
        public AirplaneDemo AirPlane;
        public Text UIText;
        //Enable it to place empty gameobjects 
        public bool InstantiateGameObjects;

        private string TerrainFilePath;

        private RuntimeTerrainGenerator RuntimeGenerator;

        private GISTerrainLoaderRuntimePrefs RuntimePrefs;

        [HideInInspector]
        public bool TerrainGenerated;
        void Start()
        {
            TerrainFilePath = Application.streamingAssetsPath + "/GIS Terrains/Example_SRTM30/Desert.tif";

            RuntimePrefs = GISTerrainLoaderRuntimePrefs.Get;

            RuntimeGenerator = RuntimeTerrainGenerator.Get;

            RuntimeTerrainGenerator.OnFinish += OnTerrainGeneratingCompleted;
        }
        void Update()
        {
            if (Input.GetKeyDown(GenerateKey))
                GenerateTerrain(TerrainFilePath);
  
            if (TerrainGenerated)
                UIText.text = "Latitude: " + AirPlane.GetAirPlaneLatLonElevation().y + " \n" + "Longitude: " + AirPlane.GetAirPlaneLatLonElevation().x + " \n" + "Elevation:" + AirPlane.GetAirPlaneLatLonElevation().z + " m";
        }
        private void GenerateTerrain(string TerrainPath)
        {

#if UNITY_WEBGL
            InitializingRuntimePrefs(TerrainPath);
            StartCoroutine(RuntimeGenerator.StartGenerating());
#else
            if (!string.IsNullOrEmpty(TerrainPath) && System.IO.File.Exists(TerrainPath))
            {
                InitializingRuntimePrefs(TerrainPath);

               StartCoroutine(RuntimeGenerator.StartGenerating());
            }
            else
            {
                Debug.LogError("Terrain file null or not supported.. Try againe");
                return;
            }
#endif
        }
        private void InitializingRuntimePrefs(string TerrainPath)
        {
            RuntimeGenerator.Error = false;
            RuntimeGenerator.enabled = true;
            RuntimeGenerator.TerrainFilePath = TerrainPath;
            RuntimeGenerator.RemovePrevTerrain = true;

            //Load Real Terrain elevation values
            RuntimePrefs.TerrainElevation = TerrainElevation.RealWorldElevation;
            //RuntimePrefs.terrainScale = new Vector3(1, 1, 1);

            RuntimePrefs.terrainDimensionMode = TerrainDimensionsMode.AutoDetection;

            RuntimePrefs.heightmapResolution = 513;
            RuntimePrefs.textureloadingMode = TexturesLoadingMode.AutoDetection;

        }

        private void OnTerrainGeneratingCompleted()
        {
            //Convert Geo Lat/Lon Way Points to Unity World Space

            GeoWaypoints.ConvertLatLonToSpacePosition(RuntimeGenerator.GeneratedContainer, InstantiateGameObjects);

            AirPlane.OnTerrainGeneratingCompleted();

            TerrainGenerated = true;
        }
    }
}
 