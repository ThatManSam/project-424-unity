/*     Unity GIS Tech 2020-2021      */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    // In this demo example we will load/Generate terrain from
    // StreamingAsset folder, and we will set some splatmaps to terrain at Runtime 

    public class ProceduralTerrainSplatmap : MonoBehaviour
    {
        public KeyCode GenerateKey;

        private string TerrainFilePath;

        private RuntimeTerrainGenerator RuntimeGenerator;

        private GISTerrainLoaderRuntimePrefs RuntimePrefs;

        private Camera3D camera3d;

        void Start()
        {
            camera3d = Camera.main.GetComponent<Camera3D>();

            TerrainFilePath = Application.streamingAssetsPath + "/GIS Terrains/PNG Demo/ASTER30m.png";

            RuntimePrefs = GISTerrainLoaderRuntimePrefs.Get;

            RuntimeGenerator = RuntimeTerrainGenerator.Get;

        }
        void Update()
        {
            if(Input.GetKeyDown(GenerateKey))
            GenerateTerrain(TerrainFilePath);
        }
        private void GenerateTerrain(string TerrainPath)
        {
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
        }
        private void InitializingRuntimePrefs(string TerrainPath)
        {
            RuntimeGenerator.Error = false;
            RuntimeGenerator.enabled = true;
            RuntimeGenerator.TerrainFilePath = TerrainPath;
            RuntimeGenerator.RemovePrevTerrain = true;

            //Load Real Terrain elevation values
            RuntimePrefs.TerrainElevation = TerrainElevation.RealWorldElevation;

            //Note that GTL Can not Detect Real PNG dimensions so we need to set them manually
            RuntimePrefs.terrainDimensionMode = TerrainDimensionsMode.Manual;
            RuntimePrefs.TerrainDimensions = new Vector2(2, 2);

            RuntimePrefs.heightmapResolution = 513;
            RuntimePrefs.textureMode = TextureMode.Splatmapping;
            //Set the number of terrain tiles
            RuntimePrefs.terrainCount = new Vector2Int(1, 1);


            //Splatmap parameters 
            RuntimePrefs.Slope = 0.1f;
            RuntimePrefs.MergeRaduis = 20;
            RuntimePrefs.MergingFactor = 2;

            AddSplatmapsToTerrain(TerrainPath);

        }
        private void AddSplatmapsToTerrain(string TerrainPath)
        {
            //Directory so Splatmaps folder
            var terrainName = Path.GetFileName(TerrainPath).Split('.')[0];
            //Get SplatMap folder
            var splatmappsFolder = Path.GetDirectoryName(TerrainPath) + "/"+terrainName+"_Splatmap";
 
            var BaseTerrainTexture_Diffuse = GISTerrainLoaderTextureGenerator.LoadedTextureAsync(splatmappsFolder + "/Sand.jpg");
            var BaseTerrainTexture_NormalMap = GISTerrainLoaderTextureGenerator.LoadedTextureAsync(splatmappsFolder + "/SandNormal.png");
            var BaseTerrainTexture_Size = new Vector2Int(100, 100);
            var baseLayer = new GISTerrainLoaderTerrainLayer(BaseTerrainTexture_Diffuse, BaseTerrainTexture_NormalMap, BaseTerrainTexture_Size);


            var GrassTexture_Diffuse = GISTerrainLoaderTextureGenerator.LoadedTextureAsync(splatmappsFolder + "/Grass.png");
            var GrassTexture_NormalMap = GISTerrainLoaderTextureGenerator.LoadedTextureAsync(splatmappsFolder + "/GrassNormal.png");
            var GrassTerrainTexture_Size = new Vector2Int(100, 100);
            var GrassLayer = new GISTerrainLoaderTerrainLayer(GrassTexture_Diffuse, GrassTexture_NormalMap, GrassTerrainTexture_Size);


            var CliffATexture_Diffuse = GISTerrainLoaderTextureGenerator.LoadedTextureAsync(splatmappsFolder + "/CliffA.jpg");
            var CliffATerrainTexture_Size = new Vector2Int(300, 300);
            var CliffALayer = new GISTerrainLoaderTerrainLayer(CliffATexture_Diffuse, null, CliffATerrainTexture_Size);

            var CliffBTexture_Diffuse = GISTerrainLoaderTextureGenerator.LoadedTextureAsync(splatmappsFolder + "/CliffB.jpg");
            var CliffBTerrainTexture_Size = new Vector2Int(300, 300);
            var CliffBLayer = new GISTerrainLoaderTerrainLayer(CliffBTexture_Diffuse, null, CliffBTerrainTexture_Size);


            RuntimePrefs.BaseTerrainLayers = baseLayer;
            RuntimePrefs.TerrainLayers = new List<GISTerrainLoaderTerrainLayer>();
            RuntimePrefs.TerrainLayers.Add(GrassLayer);
            RuntimePrefs.TerrainLayers.Add(CliffALayer);
            RuntimePrefs.TerrainLayers.Add(CliffBLayer);

            GISTerrainLoaderSplatMapping.DistributingHeights(RuntimePrefs.TerrainLayers);
            RuntimePrefs.TerrainLayers[0].X_Height = 0.1f;

        }
    }
}