using GISTech.GISTerrainLoader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleWebGLTerrain : MonoBehaviour
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

        TerrainFilePath = Application.streamingAssetsPath + "/GIS Terrains/WebGL/WebGLTerrain.tif";

        RuntimePrefs = GISTerrainLoaderRuntimePrefs.Get;

        RuntimeGenerator = RuntimeTerrainGenerator.Get;

    }

    void Update()
    {
        if (Input.GetKeyDown(GenerateKey))
            GenerateTerrain(TerrainFilePath);
    }
    private void GenerateTerrain(string TerrainPath)
    {
        InitializingRuntimePrefs(TerrainPath);
        StartCoroutine(RuntimeGenerator.StartGenerating());

    }
    private void InitializingRuntimePrefs(string TerrainPath)
    {
        RuntimeGenerator.Error = false;
        RuntimeGenerator.enabled = true;
        RuntimeGenerator.TerrainFilePath = TerrainPath;
        RuntimeGenerator.RemovePrevTerrain = true;

        //Load Real Terrain elevation values
        RuntimePrefs.TerrainElevation = TerrainElevation.RealWorldElevation;
        RuntimePrefs.terrainDimensionMode = TerrainDimensionsMode.AutoDetection;

        RuntimePrefs.heightmapResolution = 1025;
        RuntimePrefs.textureMode = TextureMode.WithTexture;
    }
}
