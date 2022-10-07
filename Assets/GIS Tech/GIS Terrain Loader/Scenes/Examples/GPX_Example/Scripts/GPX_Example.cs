using GISTech.GISTerrainLoader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPX_Example : MonoBehaviour
{
    public KeyCode GenerateKey;

    private string TerrainFilePath;
    private RuntimeTerrainGenerator RuntimeGenerator;
    private GISTerrainLoaderRuntimePrefs RuntimePrefs;

    private Camera3D camera3d;
    // Start is called before the first frame update
    void Start()
    {
        camera3d = Camera.main.GetComponent<Camera3D>();

        TerrainFilePath = Application.streamingAssetsPath + "/GIS Terrains/GPXTerrain/GPXTerrain.tif";

        RuntimePrefs = GISTerrainLoaderRuntimePrefs.Get;

        RuntimeGenerator = RuntimeTerrainGenerator.Get;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(GenerateKey))
            GenerateTerrain(TerrainFilePath);
    }
    private void InitializingRuntimePrefs(string TerrainPath)
    {
        RuntimeGenerator.Error = false;
        RuntimeGenerator.enabled = true;
        RuntimeGenerator.TerrainFilePath = TerrainPath;
        RuntimeGenerator.RemovePrevTerrain = true;
 
        RuntimePrefs.TerrainElevation = TerrainElevation.RealWorldElevation;
        RuntimePrefs.terrainDimensionMode = TerrainDimensionsMode.AutoDetection;
  
        RuntimePrefs.heightmapResolution = 513;
        RuntimePrefs.textureMode = TextureMode.WithTexture;

        //Select GPX as Vector Type
        RuntimePrefs.vectorType = VectorType.GPX;

        //Enable Road Generation
        RuntimePrefs.EnableRoadGeneration = true;
        var GPXPathPrefab = (GISTerrainLoaderSO_Road) Resources.Load("Prefabs/Environment/Roads/GPX", typeof(GISTerrainLoaderSO_Road));
        RuntimePrefs.PathPrefab = GPXPathPrefab;

        //Enable WayPoints Generation
        RuntimePrefs.EnableGeoLocationPointGeneration = true;
        var GeoPointPrefab = (GameObject)Resources.Load("Prefabs/Environment/GeoPoints/PointPrefabs/GeoLocation", typeof(GameObject));
        RuntimePrefs.GeoPointPrefab = GeoPointPrefab;



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
}
