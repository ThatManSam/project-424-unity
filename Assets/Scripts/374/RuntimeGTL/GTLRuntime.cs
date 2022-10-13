/*     Unity GIS Tech 2020-2021      */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/* +-+-+-+-+-+-+-+ Adapted from MainUI.cs +-+-+-+-+-+-+-+ */

namespace GISTech.GISTerrainLoader
{
    public class GTLRuntime : MonoBehaviour
    {

        // path of elevation file
        public string terrainPathText;

        // Always set
        public TerrainElevation elevationMode = TerrainElevation.RealWorldElevation;
        public TerrainDimensionsMode dimensionMode = TerrainDimensionsMode.AutoDetection;
        public OptionEnabDisab underwaterMode = OptionEnabDisab.Disable;
        public FixOption autofixMode = FixOption.AutoFix;

        // Generation options
        public int heightmapResolution; // Index of vals -> 33, 65, 129, 257, 513, 1025, 2049, 4097
        public float terrainExaggeration;
        public bool enableTrees;
        public bool enableRoads;
        public bool enableBuildings;

        // Button to generate file
        public Button GenerateTerrainBtn;

        // Terrain generation scripts
        private RuntimeTerrainGenerator RuntimeGenerator;

        private GISTerrainLoaderRuntimePrefs RuntimePrefs;

        // Progress bar
        public Scrollbar GenerationProgress;
        public Text Phasename;
        public Text progressValue;

        // Buttons
        public GameObject nextButton;
        public GameObject backButton;

        // Detail slider
        public Slider detailSlider;


        void Start()
        {

            RuntimeTerrainGenerator.OnProgress += OnGeneratingTerrainProg;

            terrainPathText = "E:\\Unity\\project-424-unity\\Assets\\GIS Tech\\GIS Terrain Loader\\Resources\\GIS Terrains\\newHamV8Track\\v8.tif";
            //terrainPathText = "";

            Debug.Log("Path: " + terrainPathText);

            //GenerateTerrainBtn.onClick.AddListener(OnGenerateTerrainbtnClicked);

            RuntimePrefs = GISTerrainLoaderRuntimePrefs.Get;
            RuntimePrefs.TerrainLayerSet = OptionEnabDisab.Enable;
            RuntimePrefs.TerrainLayer = 10;

            RuntimeGenerator = RuntimeTerrainGenerator.Get;

            GenerateTerrainBtn.onClick.AddListener(OnGenerateTerrainbtnClicked);

            // Set preferences

            heightmapResolution = 5;
            terrainExaggeration = 1;
            enableTrees = true;
            enableRoads = true;
            enableBuildings = true;


        }

        void Update()
        {

        }

        public void OnGenerateTerrainbtnClicked()
        {

            RuntimeGenerator.Error = false;

            RuntimeGenerator.enabled = true;

            var TerrainPath = terrainPathText;


            if (!string.IsNullOrEmpty(TerrainPath) && System.IO.File.Exists(TerrainPath))
            {
                RuntimeGenerator.TerrainFilePath = TerrainPath;

                RuntimePrefs.TerrainElevation = elevationMode;
                RuntimePrefs.TerrainExaggeration = terrainExaggeration;
                RuntimePrefs.terrainDimensionMode = dimensionMode;
                RuntimePrefs.UnderWater = underwaterMode;
                RuntimePrefs.TerrainFixOption = autofixMode;

                //var scale_x = float.Parse(TerrainScale_x.text.Replace(".", ","));
                //var scale_y = float.Parse(TerrainScale_y.text.Replace(".", ","));
                //var scale_z = float.Parse(TerrainScale_z.text.Replace(".", ","));
                RuntimePrefs.terrainScale = new Vector3(1, 1, 1);

                RuntimePrefs.heightmapResolution_index = heightmapResolution;

                RuntimePrefs.heightmapResolution = RuntimePrefs.heightmapResolutions[RuntimePrefs.heightmapResolution_index];

                RuntimeGenerator.RemovePrevTerrain = true;

                RuntimePrefs.EnableRoadGeneration = true;
                RuntimePrefs.EnableBuildingGeneration = true;
                RuntimePrefs.EnableTreeGeneration = true;


                StartCoroutine(RuntimeGenerator.StartGenerating());
            }
            else
            {
                Debug.LogError("Terrain file null or not supported.. Try againe");
                return;
            }



        }

        private void OnGeneratingTerrainProg(string phase, float progress)
        {

            if (!phase.Equals("Finalization"))
            {
                GenerationProgress.transform.parent.gameObject.SetActive(true);

                Phasename.text = phase.ToString();

                GenerationProgress.value = progress / 100;

                progressValue.text = (progress).ToString() + "%";
            }
            else
            {
                GenerationProgress.transform.parent.gameObject.SetActive(false);
                // Only show buttons once terrain has loaded
                nextButton.SetActive(true);
                backButton.SetActive(true);
            }
        }

    }
}