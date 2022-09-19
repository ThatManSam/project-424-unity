using GISTech.GISTerrainLoader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiBandsLoader : MonoBehaviour
{
    string MultibandTiffPath = @"E:\Terrains\GeoReferenced_DEM_Files\Example_TIFF\Example_Tiff_MultiBands\MultiBands_Tiff.tif";
    DVector2 LatLonPosition = new DVector2(-119.9581587375, 38.9406188940); 


    void Start()
    {
        //Read Multiple Values
        var RasterBands = GISTerrainLoaderTIFFLoader.LoadTiffBands(MultibandTiffPath);

        //Load all data in the different bands 
        var Values = RasterBands.GetValues(RasterBands.BandsNumber, LatLonPosition);
        for(int band = 0; band< Values.Length; band++)
        {
            var value = Values[band];
            Debug.Log("Value For Band N : " + band +"  " + value);
        }

        //Read data from one bands By Lat/Lon Position
        int BandNumber = 7;
        var BValue = RasterBands.GetValue(BandNumber, LatLonPosition);
        Debug.Log("Value For Band N : " + BandNumber + "  " + BValue);

        //Read Data By Row and Col Number and Band Number
        var Row = 194; var Col = 305; 
        var Value = RasterBands.GetValue(BandNumber, Row, Col);
        Debug.Log("Band : " + BandNumber + "  " + Value);

    }


}
