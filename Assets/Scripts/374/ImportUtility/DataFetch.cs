using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using UnityEngine;

/**
 * FOR DEVELOPING
 * Files saved from this script are in:
 * Assets/GIS Tech/GIS Terrain Loader/Resources/GIS Terrains/linz_data
 * 
 * Using the GIS Terrain Loader you can open the .tif file there to load the downloaded area
 */
public class LINZExportPostClass
{
    public class formatsClass
    {
        public string grid;
    }

    public class itemsClass
    {
        public string item;
    }

    public class extentClass
    {
        public string type;
        public double[,,] coordinates;
    }

    public string crs;
    public formatsClass formats = new formatsClass();
    public itemsClass[] items = new itemsClass[] { new itemsClass() };
    public extentClass extent = new extentClass();                   
}

public class LINZexportsGetClass {
    public int id;
    public string name;
    public string created_at;
    public string created_via;
    public string state;
    public string url;
    public string download_url;
}


public class DataFetchException : Exception
{
    public DataFetchException() { }
    public DataFetchException(string message) : base(message) { }
}

public class DataFetch : MonoBehaviour
{
    public string FileLocation = "GIS Tech/GIS Terrain Loader/Resources/GIS Terrains/linz_data";
    public string LinzTifFilename = "linz_data.tif";
    public static DataFetch Instance;
    
    // Singleton Pattern
    private void Awake()
    {
        Instance = this;
    }

    const string OSM_URL_PREFIX = "https://www.openstreetmap.org/api/0.6/map?bbox=";

    // For validating the POST request body
    JSchema LINZExportPostSchema = JSchema.Parse(@"{
	    '$schema': 'https://www.jsonschemavalidator.net/s/PERCJ3lW',
        'type': 'object',
        'properties': {
            'crs': {'type': 'string'},
            'formats': {
                'type': 'object',
                'properties': {
                    'grid': {'type': 'string'} 
                }
            },
            'items': {
                'type': 'array',
                'items': {
                    'type': 'object',
                    'properties': {
                        'item': {'type': 'string'}
                    }
                }
            },
            'extent': {
                'type': 'object',
                'properties': {
                    'type': {'type': 'string'},
                    'coordinates': {
                        'type': 'array',
                        'items': {
                            'type': 'array',
                            'items': {
                                'type': 'array',
                                'items': {
                                    'type': 'number'
                                }
                            }
                        }
                    }
                }
            }
        }
    }");

    // For validating the returned data from the GET request
    JSchema LINZexportsGetSchema = JSchema.Parse(@"{
    	'$schema': 'https://www.jsonschemavalidator.net/s/gyaPWgfN',
        'type': 'array',
        'items': {
          'type': 'object',
            'properties': {
                'id': {'type': 'integer'},
                'name': { 'type': 'string'},
                'created_at': { 'type': 'string'},
                'created_via': { 'type': 'string'},
                'state': { 'type': 'string'},
                'url': { 'type': 'string'},
                'download_url': { 'type': ['string', 'null']}
            }
        }
    }");


    // Test location used for development
    public void downloadOSMAndLINZ()
    {
        string tempDir = UnityEngine.Windows.Directory.temporaryFolder;

        Debug.Log("Adding temp dir: " + tempDir);


        double co_north = -37.7800;
        double co_south = -37.7910;
        double co_east = 175.2760;
        double co_west = 175.2600;

        downloadOSMAndLINZ(co_north, co_south, co_east, co_west);
    }


    // Test location used for development
    public void downloadOSMAndLINZFromCSV(string csv)
    {
        string[] ordinates = csv.Split(',');
        if (ordinates.Length < 4)
        {
            Debug.Log(string.Format("CSV input too short: {0}", ordinates.Length));
            throw new DataFetchException("Please input like '175.27, -37.79, 175.28, -37.78'");
        }

        // CSV input is West, South, East, North

        double co_north = Double.Parse(ordinates[3]);
        double co_south = Double.Parse(ordinates[1]);
        double co_east = Double.Parse(ordinates[2]);
        double co_west = Double.Parse(ordinates[0]);

        downloadOSMAndLINZ(co_north, co_south, co_east, co_west);
    }


    public void downloadOSMAndLINZ(double north, double south, double east, double west)
    {
        //throw new DataFetchException("This is a test Exception!");
        System.IO.DirectoryInfo di = new DirectoryInfo(FileLocation);

        foreach (FileInfo file in di.GetFiles())
        {
            file.Delete();
        }
        foreach (DirectoryInfo dir in di.GetDirectories())
        {
            dir.Delete(true);
        }

        double co_north = north;
        double co_south = south;
        double co_east = east;
        double co_west = west;

        string OsmFilename = "linz_data_VectorData.osm";
        string OsmSubDirectory= "linz_data_VectorData";
        string LinzZipFilename = "linz_data.zip";
        string LinzTifFilename = "linz_data.tif";
        
        using (var client = new WebClient())
        {
            
            // GETTING OSM DATA
            // Coordinate order: West -> South -> East -> North
            string url = string.Format("{0}{1:F4}%2C{2:F4}%2C{3:F4}%2C{4:F4}", OSM_URL_PREFIX, co_west, co_south, co_east, co_north);
            Debug.Log(string.Format("Sending request to {0}", url));
            try
            {
                Directory.CreateDirectory(string.Format("{0}/{1}", FileLocation, OsmSubDirectory));
                client.DownloadFile(url, string.Format("{0}/{1}/{2}", FileLocation, OsmSubDirectory, OsmFilename));
            }
            catch (DirectoryNotFoundException ex)
            {
                Debug.Log(string.Format("Directory not found: {0}", ex.Message));
                throw new DataFetchException(string.Format("Couldn't fetch data from OSM: {0}", ex.Message));
            }
            catch (WebException ex)
            {
                Debug.Log(string.Format("The request was too big: {0}", ex.Message));
                throw new DataFetchException(string.Format("Couldn't fetch data from OSM: {0}", ex.Message));
            }

            // GETTING LINZ DATA
            // First create the download with a POST request
            string linzDataURL = "https://data.linz.govt.nz/services/api/v1/exports/";
            string lidarLayerURL = "https://data.linz.govt.nz/services/api/v1/layers/104772/";

            // This is the API key linked to a LINZ account
            string APIKey = "766cd6ac344b4d2cbf0a0952c94dc3fa";

            var httpPostWebRequest = (HttpWebRequest)WebRequest.Create(linzDataURL);
            
            // Web request setup
            httpPostWebRequest.ContentType = "application/json";
            httpPostWebRequest.Method = "POST";
            httpPostWebRequest.Headers["Authorization"] = "Key " + APIKey;

            using (var streamWriter = new StreamWriter(httpPostWebRequest.GetRequestStream()))
            {
                LINZExportPostClass postData = new LINZExportPostClass();

                // Building the data object to send in the request
                postData.crs = "EPSG:4326";
                postData.formats.grid = "image/tiff;subtype=geotiff";
                postData.items[0].item = lidarLayerURL;

                postData.extent.type = "Polygon";
                postData.extent.coordinates = new double[1,5,2] {
                    { 
                        { co_west, co_south },
                        { co_west, co_north },
                        { co_east, co_north },
                        { co_east, co_south },
                        { co_west, co_south }
                    }
                };

                string json = JsonConvert.SerializeObject(postData);

                // Check that the post request data is valid
                if (!JObject.Parse(json).IsValid(LINZExportPostSchema))
                {
                    Debug.Log(string.Format("Is it valid? {0}", JObject.Parse(json).IsValid(LINZExportPostSchema)));
                    throw new DataFetchException("The data for the LINZ post request was not formed correctly");
                }

                // Sending the request with data
                streamWriter.Write(json);
                Debug.Log(string.Format("Sending request to {0} with json {1}", linzDataURL, json));
            }

            var result = "";
            try
            {
                var httpResponse = (HttpWebResponse)httpPostWebRequest.GetResponse();
                
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                using (var stream = ex.Response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    string errorMessage = reader.ReadToEnd();
                    Debug.Log(errorMessage);
                    throw new DataFetchException(errorMessage);
                }
            }

            // Setting up the GET request
            var httpGetWebRequest = (HttpWebRequest)WebRequest.Create(linzDataURL);
            httpGetWebRequest.ContentType = "application/json";
            httpGetWebRequest.Method = "GET";
            httpGetWebRequest.Headers["Authorization"] = "Key " + APIKey;

            List<LINZexportsGetClass> returnedClass;

            while (true)
            {
                result = "";
                try
                {
                    var httpResponse = (HttpWebResponse)httpGetWebRequest.GetResponse();

                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        // Get the returned data
                        result = streamReader.ReadToEnd();
                    }
                }
                catch (WebException ex)
                {
                    using (var stream = ex.Response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        string errorMessage = reader.ReadToEnd();
                        Debug.Log(errorMessage);
                        throw new DataFetchException(errorMessage);
                    }
                }

                Debug.Log(result);

                // TODO: Fix this
                //// Check that the get response data is valid
                //if (!JObject.Parse(result).IsValid(LINZexportsGetSchema))
                //{
                //    Debug.Log(string.Format("Is it valid? {0}", JObject.Parse(result).IsValid(LINZexportsGetSchema)));
                //    throw new DataFetchException("The data returned from the LINZ get request was not formed correctly");
                //}

                // Parse the returned data into an object

                //TODO: Make this and all queries async 
                returnedClass = JsonConvert.DeserializeObject<List<LINZexportsGetClass>>(result);
                //TODO: Check the time created instead of if the download is null (this could still download old exports)
                if (returnedClass[0].download_url != null) break;
                Thread.Sleep(500);
            }

            Debug.Log(string.Format("First item url: {0} from {1}", returnedClass[0].download_url, result));
            
            string LINZFullFilename = string.Format("{0}/{1}", FileLocation, LinzZipFilename);
            string LINZFullFileDirectory = FileLocation;

            // Sending Get request to download the LINZ lidar files
            try
            {
                Debug.Log(string.Format("Sending request to {0}", returnedClass[0].download_url));
                client.Headers["Authorization"] = "Key " + APIKey;
                client.DownloadFile(returnedClass[0].download_url, LINZFullFilename);
            }
            catch (WebException ex)
            {
                Debug.Log(string.Format("Error downloading LINZ file: {0}", ex));
                throw new DataFetchException(string.Format("Error downloading LINZ file: {0}", ex));
            }

            // Extracting the returned file (it is a .zip)
            if (File.Exists(LINZFullFilename))
            {
                Debug.Log(string.Format("Extracting file {0} to {1}", LINZFullFilename, LINZFullFileDirectory));
                ZipFile.ExtractToDirectory(LINZFullFilename, LINZFullFileDirectory);
                Debug.Log("Extracted Successfully");
            }

            // Get the tif file from the extracted files
            Debug.Log("Checking for tif files");
            string[] tifFiles = Directory.GetFiles(LINZFullFileDirectory, "*.tif");

            foreach (string file in tifFiles)
            {
                Debug.Log(string.Format("Tif files in dir {0}", file));
            }

            // Rename the file so that its name links with the OSM filename
            if (tifFiles.Length > 0)
            {
                File.Move(string.Format(tifFiles[0].Replace('\\', '/')), string.Format("{0}/{1}", LINZFullFileDirectory, LinzTifFilename));
            }

        }
    }
}
