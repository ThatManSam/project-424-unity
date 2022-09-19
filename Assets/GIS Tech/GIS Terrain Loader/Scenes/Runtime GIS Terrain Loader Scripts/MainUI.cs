/*     Unity GIS Tech 2020-2021      */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;

namespace GISTech.GISTerrainLoader
{
    public class MainUI : MonoBehaviour
    {
        public Button LoadTerrain;

        public FileBrowser fileBrowserDiag;


        public Text TerrainPathText;

        public Scrollbar Terrain_Exaggeration;

        public Dropdown ElevationMode;
        public Text Terrain_Exaggeration_value;
        public Dropdown DimensionMode;
        public InputField TerrainLenght;
        public InputField TerrainWidth;

        public Dropdown UnderWaterMode;
        public Dropdown AutoFixMode;
        public InputField MinElevation;
        public InputField MaxElevation;

        public InputField TerrainScale_x;
        public InputField TerrainScale_y;
        public InputField TerrainScale_z;

        public Dropdown HeightMapResolution;

        public Dropdown TexturingMode;
        public Dropdown ShaderType;

        public Dropdown VectorType;
        public Dropdown GenerateTrees;
        public Dropdown GenerateGrass;
        public Dropdown GenerateRoads;
        public Dropdown GenerateBuildings;


        public Toggle ClearLastTerrain;

        public Button GenerateTerrainBtn;

        private RuntimeTerrainGenerator RuntimeGenerator;

        public const string version = "2.5";

        private GISTerrainLoaderRuntimePrefs RuntimePrefs;

        private Camera3D camera3d;

        public Scrollbar GenerationProgress;

        public Text Phasename;

        public Text progressValue;




        private DVector2 TerrainDimensions = new DVector2(0, 0);

        public KeyCode ResetCameraPosKey;
        void Start()
        {
            RuntimeTerrainGenerator.OnProgress += OnGeneratingTerrainProg;

            FileBrowser.SetFilters(true, new FileBrowser.Filter("DEM File", ".tif", ".flt",".hgt",".bil", ".asc", ".bin", ".las", ".ter" ,".png", ".raw"));
            FileBrowser.SetDefaultFilter(".tif");
            FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");
            FileBrowser.AddQuickLink("Data Path", Application.dataPath, null);

            if (camera3d)
            {
                camera3d = Camera.main.GetComponent<Camera3D>();

                camera3d.enabled = false;
            }



            LoadTerrain.onClick.AddListener(OnLoadBtnClicked);

            GenerateTerrainBtn.onClick.AddListener(OnGenerateTerrainbtnClicked);

            RuntimePrefs = GISTerrainLoaderRuntimePrefs.Get;

            RuntimeGenerator = RuntimeTerrainGenerator.Get;

            ElevationMode.onValueChanged.AddListener(OnElevationModeChanged);
            DimensionMode.onValueChanged.AddListener(OnDimensionModeChanged);
            Terrain_Exaggeration.onValueChanged.AddListener(OnTerrainExaggerationChanged);
            AutoFixMode.onValueChanged.AddListener(OnFixModeChanged);
            TexturingMode.onValueChanged.AddListener(OnTexturingModeChanged);
            ShaderType.onValueChanged.AddListener(OnShaderTypeChanged);
            VectorType.onValueChanged.AddListener(OnVectorTypeChanged);
            GenerateTrees.onValueChanged.AddListener(OnGenerateTreesChanged);
            GenerateGrass.onValueChanged.AddListener(OnGenerateGrassChanged);
            GenerateRoads.onValueChanged.AddListener(OnGenerateRoadsChanged);
            GenerateBuildings.onValueChanged.AddListener(OnGenerateBuildingsChanged);

        }
        void Update()
        {
            if (Input.GetKeyDown(ResetCameraPosKey))
                RuntimeGenerator.ResetCameraPosition();
        }
        private void OnGenerateTerrainbtnClicked()
        {
            if(camera3d)
            camera3d.enabled = false;

            RuntimeGenerator.Error = false;

            RuntimeGenerator.enabled = true;

            var TerrainPath = TerrainPathText.text;


            if (!string.IsNullOrEmpty(TerrainPath) && System.IO.File.Exists(TerrainPath))
            {
                RuntimeGenerator.TerrainFilePath = TerrainPath;

                RuntimePrefs.TerrainElevation = (TerrainElevation)ElevationMode.value;
                RuntimePrefs.TerrainExaggeration = Terrain_Exaggeration.value;
                RuntimePrefs.terrainDimensionMode = (TerrainDimensionsMode)DimensionMode.value;
                RuntimePrefs.UnderWater = (OptionEnabDisab)UnderWaterMode.value;
                RuntimePrefs.TerrainFixOption = (FixOption)AutoFixMode.value;

                if (RuntimePrefs.TerrainFixOption == FixOption.ManualFix)
                {
                    var min = float.Parse(MinElevation.text.Replace(".", ","));
                    var max = float.Parse(MinElevation.text.Replace(".", ","));
                    RuntimePrefs.TerrainMaxMinElevation = new Vector2(min, max);
                }
                var scale_x = float.Parse(TerrainScale_x.text.Replace(".", ","));
                var scale_y = float.Parse(TerrainScale_y.text.Replace(".", ","));
                var scale_z = float.Parse(TerrainScale_z.text.Replace(".", ","));
                RuntimePrefs.terrainScale = new Vector3(scale_x, scale_y, scale_z);

                RuntimePrefs.heightmapResolution_index = HeightMapResolution.value;

                RuntimePrefs.heightmapResolution = RuntimePrefs.heightmapResolutions[RuntimePrefs.heightmapResolution_index];
 
                RuntimeGenerator.RemovePrevTerrain = true;

                if (!RuntimePrefs.TerrainHasDimensions || RuntimePrefs.terrainDimensionMode == TerrainDimensionsMode.Manual)
                {
                    if (!string.IsNullOrEmpty(TerrainLenght.text) && !string.IsNullOrEmpty(TerrainWidth.text))
                    {

                        var terrainWidth = float.Parse(TerrainLenght.text.Replace(".", ","));
                        var terrainLenght = float.Parse(TerrainWidth.text.Replace(".", ","));

                        RuntimePrefs.TerrainDimensions = new Vector2(terrainWidth, terrainLenght);
                    }
                    else
                    {
                        Debug.LogError("Reset terrain Dimensions...");
                        return;
                    }
                }



                StartCoroutine(RuntimeGenerator.StartGenerating());
            }
            else
            {
                Debug.LogError("Terrain file null or not supported.. Try againe");
                return;
            }



        }
        private void OnTerrainExaggerationChanged(float value)
        {
            Terrain_Exaggeration_value.text = value.ToString();
        }
        private void OnElevationModeChanged(int value)
        {

            switch (value)
            {
                case (int)TerrainElevation.RealWorldElevation:
                    Terrain_Exaggeration.transform.parent.gameObject.SetActive(false);
                    Terrain_Exaggeration.transform.parent.GetComponent<Element>().ShowElement = false;

                    break;
                case (int)TerrainElevation.ExaggerationTerrain:
                    Terrain_Exaggeration.transform.parent.gameObject.SetActive(true);
                    Terrain_Exaggeration.transform.parent.GetComponent<Element>().ShowElement = true;
                    break;
            }

        }
        private void OnDimensionModeChanged(int value)
        {

            switch (value)
            {
                case (int)TerrainDimensionsMode.AutoDetection:
                    TerrainLenght.transform.parent.gameObject.SetActive(false);
                    TerrainLenght.transform.parent.GetComponent<Element>().ShowElement = false;

                    break;
                case (int)TerrainDimensionsMode.Manual:
                    TerrainLenght.transform.parent.gameObject.SetActive(true);
                    TerrainLenght.transform.parent.GetComponent<Element>().ShowElement = true;

                    break;
            }

        }
        private void OnFixModeChanged(int value)
        {

            if(value == (int)FixOption.ManualFix)
            {
               MinElevation.transform.parent.gameObject.SetActive(true);
                MinElevation.transform.parent.GetComponent<Element>().ShowElement = true;
            }
            else
            {
                MinElevation.transform.parent.gameObject.SetActive(false);
                MinElevation.transform.parent.GetComponent<Element>().ShowElement = false;
            }



        }
        private void OnTexturingModeChanged(int value)
        {
            RuntimePrefs.textureMode = (TextureMode)value;
 
            if ((GISTerrainLoader.TextureMode)value == TextureMode.ShadedRelief)
            {
                ShaderType.transform.parent.gameObject.SetActive(true);
                ShaderType.transform.parent.GetComponent<Element>().ShowElement = true;
            }
            else
            {
                ShaderType.transform.parent.gameObject.SetActive(false);
                ShaderType.transform.parent.GetComponent<Element>().ShowElement = false;
            }
        }
        private void OnVectorTypeChanged(int value)
        {
            RuntimePrefs.vectorType = (VectorType)value;
        }
        
        private void OnShaderTypeChanged(int value)
        {
            RuntimePrefs.TerrainShaderType = (GISTerrainLoader.ShaderType)value;
        }
        private void OnGenerateTreesChanged(int value)
        {
            if (value == 0)
                RuntimePrefs.EnableTreeGeneration = false;
            else RuntimePrefs.EnableTreeGeneration = true;
         }
        private void OnGenerateGrassChanged(int value)
        {
            if (value == 0)
                RuntimePrefs.EnableGrassGeneration = false;
            else RuntimePrefs.EnableGrassGeneration = true;
        }
        private void OnGenerateRoadsChanged(int value)
        {
            if (value == 0)
                RuntimePrefs.EnableRoadGeneration = false;
            else RuntimePrefs.EnableRoadGeneration = true;
        }
        private void OnGenerateBuildingsChanged(int value)
        {
            if (value == 0)
                RuntimePrefs.EnableBuildingGeneration = false;
            else RuntimePrefs.EnableBuildingGeneration = true;
        }
  

        
        private void OnLoadBtnClicked()
        {
            StartCoroutine(ShowLoadDialogCoroutine());       
        }
        IEnumerator ShowLoadDialogCoroutine()
        {
            yield return FileBrowser.WaitForLoadDialog(false, null, "Load Terrain File", "Load");
            TerrainPathText.text = FileBrowser.Result;

            OnTerrainFileChanged(TerrainPathText.text);
        }
        private void OnGeneratingTerrainProg(string phase, float progress)
        {
            if (!phase.Equals("Finalization"))
            {
                GenerationProgress.transform.parent.gameObject.SetActive(true);

                Phasename.text = phase.ToString();

                GenerationProgress.value = progress/100;

                progressValue.text = (progress).ToString() + "%";
            }
            else
            {
                if (camera3d)
                    camera3d.enabled = true;
                GenerationProgress.transform.parent.gameObject.SetActive(false);
            }
        }
        private void OnTerrainFileChanged(string TerrainFilePath)
        {
            var HasAutoDim = GISTerrainLoaderSupport.IsGeoFile(Path.GetExtension(TerrainFilePath));
            RuntimePrefs.TerrainHasDimensions = HasAutoDim;
            if (!HasAutoDim)
            {
                TerrainLenght.transform.parent.gameObject.SetActive(true);
                TerrainWidth.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                TerrainLenght.transform.parent.gameObject.SetActive(false);
                TerrainWidth.transform.parent.gameObject.SetActive(false);
            }
        }

    }
}