using UnityEngine;
using System.IO;
using GISTech.GISTerrainLoader;

public class LoadShapeFileData : MonoBehaviour
{    
    string ShapeFilePath = @"E:\Terrains\Zip\Example_Terrain_SHP_VectorData\Example_Terrain_SHP_VectorData\Cuenca_VectorData\SHP Vector data\Areas\Areas.shp";
   

    void Start()
    {
        if(File.Exists(ShapeFilePath))
        {

            GISTerrainLoaderShpFile shpfile = GISTerrainLoaderShapeReader.LoadFiles(ShapeFilePath) as GISTerrainLoaderShpFile;

            GISTerrainLoaderShapeFileLoader shapeloader = new GISTerrainLoaderShapeFileLoader(shpfile);

            GISTerrainLoaderGeoVectorData GeoData = shapeloader.GetGeoShapeFileData();

            Debug.Log("ID: " + GeoData.GeoPolygons.Count + " Lat-Lon: " + GeoData.GeoPoints.Count);

            //Debug Geo-Point Data

            if (GeoData.GeoPoints.Count > 0)
            {
                foreach (var point in GeoData.GeoPoints)
                {
                    Debug.Log("ID: " + point.ID + " Lat-Lon: " + point.GeoPoint);
                }
            }


            //We Can Also Debug Shape Elevation Geo-Poly(Line - gone ) Z 

            if (GeoData.GeoLines.Count > 0)
            {
                foreach (var Poly in GeoData.GeoLines)
                {
                    Debug.Log("PolyLineZ ID: " + Poly.ID + " Points Count : " + Poly.GeoPoints.Count);

                    //Debug Lat-Lon Points
                    foreach (var point in Poly.GeoPoints)
                    {
                        Debug.Log("PolyLineZ ID: " + Poly.ID + " Point N " + Poly.GeoPoints.IndexOf(point) + " Lat-Lon : " + point.x + " - " + point.y + " Elevation " + point.z);
                    }

                    //Debug PolyLine DataBase
                    foreach (var data in Poly.DataBase)
                    {
                        Debug.Log("PolyLineZ ID: " + Poly.ID + " Attribute: " + data.Key + " Value: " + data.Value);
                    }
                }
            }

            //Debug Geo-Polygons Geo-Points Data

            if (GeoData.GeoPolygons.Count > 0)
            {
                foreach (var Poly in GeoData.GeoPolygons)
                {
                    Debug.Log("Poly ID: " + Poly .ID + " Points Count : " + Poly.GeoPoints.Count);

                    //Debug Lat-Lon Points
                    foreach (var point in Poly.GeoPoints)
                    {
                        Debug.Log("Poly ID: " + Poly.ID + " Point N " + Poly.GeoPoints.IndexOf(point)+" Lat-Lon : " + point);
                    }

                    //Debug Polygon DataBase
                    foreach (var data in Poly.DataBase)
                    {
                        Debug.Log("Poly ID: " + Poly.ID + " Attribute: " + data.Key + " Value: " + data.Value);
                    }
                }
            }


        }

    }

}
