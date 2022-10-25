using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/* 
/*  This script is adapted from MainUI.cs
 *  which is a script of GIS Terrain Loader
 *  which has been paid for and allows use
 *  of modification
 */

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

        // Terrain Exaggeration
        public Dropdown exaggerationDropdown;


        private DataFetch DataFetchInstance;
        public EnableDisable EnableDisableScript;
        public InputField CoordinatesCSVInput;

        public GameObject downloadingTerrainLabel;

        void Start()
        {
            // Update terrain generation progress
            RuntimeTerrainGenerator.OnProgress += OnGeneratingTerrainProg;
            // Get runtime preferences 
            RuntimePrefs = GISTerrainLoaderRuntimePrefs.Get;
            // Enable and set layer of terrain
            RuntimePrefs.TerrainLayerSet = OptionEnabDisab.Enable;
            RuntimePrefs.TerrainLayer = 10;
            // Get runtime terrain generator
            RuntimeGenerator = RuntimeTerrainGenerator.Get;
            // Add a onClick listener for terrain generation button
            GenerateTerrainBtn.onClick.AddListener(OnGenerateTerrainbtnClicked);
            // Get data fetch singleton
            DataFetchInstance = DataFetch.Instance;

            downloadingTerrainLabel.SetActive(false);
        }

        void Update()
        {
            // Update selected quality of terrain
            SetQuality((int)detailSlider.value);
            // Update terrain height exaggeration
            terrainExaggeration = exaggerationDropdown.value + 1;
        }

        public async void OnGenerateTerrainbtnClicked()
        {
            // Show downloading status text
            downloadingTerrainLabel.SetActive(true);
            string tempFolder = Application.dataPath + "/StreamingAssets";

            Debug.Log("Setting temp folder: " + tempFolder);

            // Get data 
            //string testCSV = "175.27000253,-37.7897077301,175.28376692,-37.7830590926";
            DataFetchInstance.FileLocation = tempFolder;
            terrainPathText = tempFolder + "/" + DataFetchInstance.LinzTifFilename;
            Debug.Log(string.Format("Getting area: {0}", CoordinatesCSVInput.text));
            try
            {
                await DataFetchInstance.downloadOSMAndLINZFromCSV(CoordinatesCSVInput.text);
            }
            catch (DataFetchException ex)
            {
                downloadingTerrainLabel.SetActive(false);
                AlertPopUp.Instance.SetMessage(ex.Message).Show();
                Debug.Log(string.Format("Error downloading data: {0}", ex.Message));
                return;
            }

            RuntimeGenerator.Error = false;

            RuntimeGenerator.enabled = true;

            // Give data folder location to GTL
            var TerrainPath = terrainPathText;
            if (!string.IsNullOrEmpty(TerrainPath) && System.IO.File.Exists(TerrainPath))
            {
                // Set terrain preferences for generation

                RuntimeGenerator.TerrainFilePath = TerrainPath;

                RuntimePrefs.TerrainElevation = elevationMode;

                RuntimePrefs.TerrainExaggeration = terrainExaggeration;

                RuntimePrefs.terrainDimensionMode = dimensionMode;

                RuntimePrefs.UnderWater = underwaterMode;

                RuntimePrefs.TerrainFixOption = autofixMode;

                RuntimePrefs.terrainScale = new Vector3(1, terrainExaggeration, 1);

                RuntimePrefs.heightmapResolution_index = heightmapResolution;

                RuntimePrefs.heightmapResolution = RuntimePrefs.heightmapResolutions[RuntimePrefs.heightmapResolution_index];

                RuntimeGenerator.RemovePrevTerrain = true;

                RuntimePrefs.EnableRoadGeneration = enableRoads;

                RuntimePrefs.EnableBuildingGeneration = enableBuildings;

                RuntimePrefs.EnableTreeGeneration = enableTrees;

                StartCoroutine(RuntimeGenerator.StartGenerating());
            }
            else
            {
                Debug.LogError("Terrain file null or not supported.. Try again");
                return;
            }

            EnableDisableScript.ToggleGameObjects();
        }

        private void OnGeneratingTerrainProg(string phase, float progress)
        {
            // Update terrain generation progress
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

        private void SetQuality(int i)
        {
            // Set terrain generation quality
            switch (i)
            {
                case 0:
                    heightmapResolution = 3;
                    enableTrees = false;
                    enableRoads = false;
                    enableBuildings = false;
                    break;
                case 1:
                    heightmapResolution = 4;
                    enableTrees = false;
                    enableRoads = true;
                    enableBuildings = false;
                    break;

                case 2:
                    heightmapResolution = 5;
                    enableTrees = true;
                    enableRoads = true;
                    enableBuildings = true;
                    break;

                case 3:
                    heightmapResolution = 6;
                    enableTrees = true;
                    enableRoads = true;
                    enableBuildings = true;
                    break;

                case 4:
                    heightmapResolution = 7;
                    enableTrees = true;
                    enableRoads = true;
                    enableBuildings = true;
                    break;
            }

        }

    }
}