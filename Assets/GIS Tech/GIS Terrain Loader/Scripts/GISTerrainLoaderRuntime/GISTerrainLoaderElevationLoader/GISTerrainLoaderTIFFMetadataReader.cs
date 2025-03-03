﻿/*     Unity GIS Tech 2020-2021      */

using System;
using UnityEngine;
using BitMiracle.LibTiff.Classic;
using System.Text.RegularExpressions;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderTIFFMetadataReader 
    {
        public GTLGeographicCoordinateSystem CoordinateReferenceSystem;

        public ProjectionSystem ProjectionSystem =  ProjectionSystem.Undefined;
        public ModelType GTModelTypeGeoKey { get; private set; }
        public RasterType GTRasterTypeGeoKey { get; private set; } = RasterType.PixelIsArea;
        public string GTCitationGeoKey { get; private set; }
        public GeographicCoordinateSystem GeographicTypeGeoKey { get; private set; }
        public string GeogCitationGeoKey { get; private set; }
        public GeodeticDatum GeogGeodeticDatumGeoKey { get; private set; }
        public PrimeMeridian GeogPrimeMeridianGeoKey { get; private set; }
        public LinearUnit LinearUnit { get; private set; }
        public double GeogLinearUnitSize { get; private set; }
        public AngularUnit AngularUnit { get; private set; }
        public double AngularUnitSize { get; private set; }
        public Ellipsoid Ellipsoid { get; private set; }
        public double SemiMajorAxis { get; private set; }
        public double SemiMinorAxis { get; private set; }
        public double InvFlattening { get; private set; }
        public AngularUnit AzimuthUnit { get; private set; }
        public double PrimeMeridianLongitude { get; private set; }

        // Projected Coordinate System Parameters
        public ProjectedCoordinateSystem ProjectedCoordinatesystem { get; private set; }
        public string ProjectedCitation { get; private set; }
        public double ProjLinearUnitSize { get; private set; }
        public string GeoProjectionData { get; private set; }



        public GISTerrainLoaderTIFFMetadataReader(Tiff tiff)
        {
            ReadGeoKeyDirectory(tiff);
            SetProjection();
        }
        private void ReadGeoKeyDirectory(Tiff tiff)
        {
            var GeoAsciiParamsTag = tiff.GetField((TiffTag)ExtraTiffTag.GeoAsciiParamsTag);

            if (GeoAsciiParamsTag != null && GeoAsciiParamsTag.Length == 2)
                GeoProjectionData = GeoAsciiParamsTag[1].ToString();

            FieldValue[] field = tiff.GetField((TiffTag)ExtraTiffTag.GeoKeyDirectoryTag);

            if (field != null && field.Length == 2 && field[0].ToInt() >= 4)
            {
                int count = field[0].ToInt();
                var bytes = field[1].ToByteArray();
                ushort[] shorts = bytes.ConvertToArray(count, sizeof(ushort), BitConverter.ToUInt16);

                ushort KeyDirectoryVersion = shorts[0];
                ushort KeyRevision = shorts[1];
                ushort MinorRevision = shorts[2];
                ushort NumberOfKeys = shorts[3];

                if (NumberOfKeys > 0)
                {
                    var info = new GeoKeyInfo
                    {
                        doubleParams = ReadGeoDoubleParamsTag(tiff),
                        asciiParams = ReadGeoAsciiParamsTag(tiff)
                    };

                    for (ushort i = 0; i < NumberOfKeys; i++)
                    {
                        ushort keyOffset = (ushort)(4 + i * 4);

                        var keyID = (KeyID)shorts[keyOffset];
                        info.TIFFTagLocation = shorts[keyOffset + 1];
                        info.Count = shorts[keyOffset + 2];
                        info.ValueOffset = shorts[keyOffset + 3];

                        switch (keyID)
                        {   
                             // GeoTIFF Configuration Keys
                            case KeyID.GTModelTypeGeoKey:
                                GTModelTypeGeoKey = (ModelType)info.GetValue();
                                break;
                            case KeyID.GTRasterTypeGeoKey:
                                GTRasterTypeGeoKey = (RasterType)info.GetValue();
                                break;
                            case KeyID.GTCitationGeoKey:
                                GTCitationGeoKey = info.GetString();
                                break;
                            // Geographic Coordinate System Parameter Keys
                            case KeyID.GeographicTypeGeoKey:
                                GeographicTypeGeoKey = (GeographicCoordinateSystem)info.GetValue();
                                break;
                            case KeyID.GeogCitationGeoKey:
                                GeogCitationGeoKey = info.GetString();
                                break;
                            case KeyID.GeogGeodeticDatumGeoKey:
                                GeogGeodeticDatumGeoKey = (GeodeticDatum)info.GetValue();
                                break;
                            case KeyID.GeogPrimeMeridianGeoKey:
                                GeogPrimeMeridianGeoKey = (PrimeMeridian)info.GetValue();
                                break;
                            case KeyID.GeogLinearUnitsGeoKey:
                                LinearUnit = (LinearUnit)info.GetValue();
                                break;
                            case KeyID.GeogLinearUnitSizeGeoKey:
                                GeogLinearUnitSize = info.GetDouble();
                                break;
                            case KeyID.GeogAngularUnitsGeoKey:
                                AngularUnit = (AngularUnit)info.GetValue();
                                break;
                            case KeyID.GeogAngularUnitSizeGeoKey:
                                AngularUnitSize = info.GetDouble();
                                break;
                            case KeyID.GeogEllipsoidGeoKey:
                                Ellipsoid = (Ellipsoid)info.GetValue();
                                break;
                            case KeyID.GeogSemiMajorAxisGeoKey:
                                SemiMajorAxis = info.GetDouble();
                                break;
                            case KeyID.GeogSemiMinorAxisGeoKey:
                                SemiMinorAxis = info.GetDouble();
                                break;
                            case KeyID.GeogInvFlatteningGeoKey:
                                InvFlattening = info.GetDouble();
                                break;
                            case KeyID.GeogAzimuthUnitsGeoKey:
                                AzimuthUnit = (AngularUnit)info.GetValue();
                                break;
                            case KeyID.GeogPrimeMeridianLongGeoKey:
                                PrimeMeridianLongitude = info.GetDouble();
                                break;
                            // Projected Coordinate System Parameter Keys
                            case KeyID.ProjectedCSTypeGeoKey:
                                ProjectedCoordinatesystem = (ProjectedCoordinateSystem)info.GetValue();

                                break;
                            case KeyID.PCSCitationGeoKey:
                                GTCitationGeoKey = info.GetString();
                                break;
                            case KeyID.ProjLinearUnitsGeoKey:
                                LinearUnit = (LinearUnit)info.GetValue();
                                break;
                            case KeyID.ProjLinearUnitSizeGeoKey:
                                ProjLinearUnitSize = info.GetDouble();
                                break;

                            case KeyID.ProjectionGeoKey:

                                break;
                            case KeyID.ProjCoordTransGeoKey:

                                break;
                            case KeyID.ProjStdParallel1GeoKey:

                                break;
                            case KeyID.ProjStdParallel2GeoKey:

                                break;
                            case KeyID.ProjNatOriginLongGeoKey:

                                break;
                            case KeyID.ProjNatOriginLatGeoKey:

                                break;
                            case KeyID.ProjFalseEastingGeoKey:

                                break;
                            case KeyID.ProjFalseNorthingGeoKey:

                                break;
 
                            default:
                                break;
                        }
                    }
                }
            }

            if (GTModelTypeGeoKey == ModelType.Projected)
            {
                if (LinearUnit == LinearUnit.Meter)
                {
                    if (ProjectedCoordinatesystem >= ProjectedCoordinateSystem.WGS84_UTM_zone_1N &&
                        ProjectedCoordinatesystem <= ProjectedCoordinateSystem.WGS84_UTM_zone_60N)
                    {
                        int zone = ProjectedCoordinatesystem - ProjectedCoordinateSystem.WGS84_UTM_zone_1N;
                    }
                    else if (ProjectedCoordinatesystem >= ProjectedCoordinateSystem.WGS84_UTM_zone_1S &&
                             ProjectedCoordinatesystem <= ProjectedCoordinateSystem.WGS84_UTM_zone_60S)
                    {
                 
                        int zone = ProjectedCoordinatesystem - ProjectedCoordinateSystem.WGS84_UTM_zone_1S;
                    }
                }
                else
                {
                    Debug.LogError("LinearUnit '" + LinearUnit + "' not supported");
                }
            }
        }
        private double[] ReadGeoDoubleParamsTag(Tiff tiff)
        {
            double[] doubles = null;
            FieldValue[] field = tiff.GetField((TiffTag)ExtraTiffTag.GeoDoubleParamsTag);
            if (field != null && field.Length == 2)
            {
                int count = field[0].ToInt();
                var bytes = field[1].ToByteArray();
                doubles = bytes.ConvertToArray(count, sizeof(double), BitConverter.ToDouble);
            }
            return doubles;
        }
        private string ReadGeoAsciiParamsTag(Tiff tiff)
        {
            string asciiParams = null;
            FieldValue[] field = tiff.GetField((TiffTag)ExtraTiffTag.GeoAsciiParamsTag);
            if (field != null && field.Length == 2)
            {
                var bytes = field[1].ToByteArray();
                asciiParams = System.Text.Encoding.Default.GetString(bytes);
            }
            return asciiParams;
        }
        private void SetProjection()
        {
            if (GTModelTypeGeoKey == ModelType.Geographic)
            {
                CoordinateReferenceSystem = new GTLGeographicCoordinateSystem("GCS_WGS_1984");
                ProjectionSystem = ProjectionSystem.defined;
            }
           

            else if (GTModelTypeGeoKey == ModelType.Projected)
            {
                int epsg = (int)ProjectedCoordinatesystem;
                if (ProjectedCoordinatesystem == ProjectedCoordinateSystem.UserDefined && GeographicTypeGeoKey != GeographicCoordinateSystem.Undefined)
                    epsg = (int)GeographicTypeGeoKey;

                var projName = GISTerrainLoaderEPSG.GetEPSGName(epsg);
 
                if (!string.IsNullOrEmpty(projName))
                {
                    if (projName.Contains("NAD83"))
                    {
                        bool UTM = projName.Contains("UTM");
                        NAD83Case(projName, epsg, UTM);
                        ProjectionSystem = ProjectionSystem.defined;
                        return;
                    }else
                    if (projName.Contains("UTM"))
                    {
                        UTMCase(projName);
                        ProjectionSystem = ProjectionSystem.defined;
                        return;
                    }else
                    if (projName.Contains("Lambert"))
                    {
                        LambertCase();
                        ProjectionSystem = ProjectionSystem.defined;
                        return;
                    }else
                    {
                        CoordinateReferenceSystem = new GTLGeographicCoordinateSystem("Undefined");
                        ProjectionSystem = ProjectionSystem.Undefined;
                    }

                }
            }
            
        }
        private void UTMCase(string ProjName)
        {
            CoordinateReferenceSystem = new GTLGeographicCoordinateSystem("UTM",true);
            var Sub = ProjName.Split('/');

            if (Sub.Length > 0)
            {
                var fullZone = Sub[1].TrimStart().Split(' ');
                var zoneLet = Regex.Replace(fullZone[2], @"[^A-Z]+", string.Empty);
                CoordinateReferenceSystem.UTMData.ZoneLet =  zoneLet;
                var zonenum = Regex.Match(fullZone[2], @"\d+").Value;
                CoordinateReferenceSystem.UTMData.ZoneNum = int.Parse(zonenum);
                return;
            }
        }

        private void LambertCase()
        {
            CoordinateReferenceSystem = new GTLGeographicCoordinateSystem("GCS_RESEAU_GEODESIQUE_FRANCAIS_1993");
            var Sub = GTCitationGeoKey.Split('/');
            if (Sub.Length > 0)
            {
                var fullZone = Sub[1].Trim();
                if (fullZone == "Lambert-93")
                    CoordinateReferenceSystem.LambertData.Lambertzone = LambertZone.Lambert93;
                return;
            }
        }
        private void NAD83Case(string ProjName, int epsg, bool UTM)
        {
            CoordinateReferenceSystem = new GTLGeographicCoordinateSystem("NAD83",UTM);
            CoordinateReferenceSystem.EPSG_Code = epsg;
            if (UTM)
            {
                var Sub = ProjName.Split('/');
                if (Sub.Length > 0)
                {
                    var fullZone = Sub[1].TrimStart().Split(' ');
                    var zoneLet = Regex.Replace(fullZone[2], @"[^A-Z]+", string.Empty);
                    CoordinateReferenceSystem.UTMData.ZoneLet = zoneLet;
                    var zonenum = Regex.Match(fullZone[2], @"\d+").Value;
                    CoordinateReferenceSystem.UTMData.ZoneNum = int.Parse(zonenum);
                    return;
                }
            }

        }
    }
    public static class ExtraTiffTag
    {
        public const ushort GeoKeyDirectoryTag = 34735;         // (SPOT)
        public const ushort GeoDoubleParamsTag = 34736;         // (SPOT)
        public const ushort GeoAsciiParamsTag = 34737;          // (SPOT)
        public const ushort GDAL_METADATA = 42112;              // (GDAL)
        public const ushort GDAL_NODATA = 42113;                // (GDAL)
    }
    public static class GeoTiffExtensions
    {
        public delegate T ConvertFunc<T>(byte[] bytes, int offset);
        public static T[] ConvertToArray<T>(this byte[] bytes, int count, int stride, ConvertFunc<T> func)
        {
            T[] values = new T[count];
            int offset = 0;
            for (int i = 0; i < count; i++)
            {
                values[i] = func(bytes, offset);
                offset += stride;
            }
            return values;
        }

    }
    public class GeoKeyInfo
    {
        public double[] doubleParams;
        public string asciiParams;

        public ushort TIFFTagLocation;
        public ushort Count;
        public ushort ValueOffset;

        public ushort GetValue()
        {
            if (TIFFTagLocation == 0)
            {
                return ValueOffset;
            }
            return 0;
        }

        public double GetDouble()
        {
            if (TIFFTagLocation == ExtraTiffTag.GeoDoubleParamsTag && Count == 1)
            {
                return doubleParams[ValueOffset];
            }
            return 0;
        }

        public string GetString()
        {
            if (TIFFTagLocation == ExtraTiffTag.GeoAsciiParamsTag)
            {
                return asciiParams.Substring(ValueOffset, Count - 1);
            }
            return string.Empty;
        }

    }
    public enum ProjectionSystem
    {
        Undefined = 0,
        defined = 1,
    }
    public enum KeyID : ushort
    {
        //
        // GeoTIFF Configuration GeoKeys
        //

        GTModelTypeGeoKey = 1024,
        GTRasterTypeGeoKey = 1025,
        GTCitationGeoKey = 1026,

        //
        // Geographic Coordinate System (CS) Parameter GeoKeys
        // 

        GeographicTypeGeoKey = 2048,
        GeogCitationGeoKey = 2049,
        GeogGeodeticDatumGeoKey = 2050,
        GeogPrimeMeridianGeoKey = 2051,
        GeogLinearUnitsGeoKey = 2052,
        GeogLinearUnitSizeGeoKey = 2053,
        GeogAngularUnitsGeoKey = 2054,
        GeogAngularUnitSizeGeoKey = 2055,
        GeogEllipsoidGeoKey = 2056,
        GeogSemiMajorAxisGeoKey = 2057,
        GeogSemiMinorAxisGeoKey = 2058,
        GeogInvFlatteningGeoKey = 2059,
        GeogAzimuthUnitsGeoKey = 2060,
        GeogPrimeMeridianLongGeoKey = 2061,

        //
        // Projected Coordinate System (CS) Parameter GeoKeys
        //

        ProjectedCSTypeGeoKey = 3072,
        PCSCitationGeoKey = 3073,

        //
        // Projection Definition GeoKeys
        //

        ProjectionGeoKey = 3074,
        ProjCoordTransGeoKey = 3075,
        ProjLinearUnitsGeoKey = 3076,
        ProjLinearUnitSizeGeoKey = 3077,
        ProjStdParallel1GeoKey = 3078,
        ProjStdParallel2GeoKey = 3079,
        ProjNatOriginLongGeoKey = 3080,
        ProjNatOriginLatGeoKey = 3081,
        ProjFalseEastingGeoKey = 3082,
        ProjFalseNorthingGeoKey = 3083,
        ProjFalseOriginLongGeoKey = 3084,
        ProjFalseOriginLatGeoKey = 3085,
        ProjFalseOriginEastingGeoKey = 3086,
        ProjFalseOriginNorthingGeoKey = 3087,
        ProjCenterLongGeoKey = 3088,
        ProjCenterLatGeoKey = 3089,
        ProjCenterEastingGeoKey = 3090,
        ProjCenterNorthingGeoKey = 3091,
        ProjScaleAtNatOriginGeoKey = 3092,
        ProjScaleAtCenterGeoKey = 3093,
        ProjAzimuthAngleGeoKey = 3094,
        ProjStraightVertPoleLongGeoKey = 3095,

        //
        // Vertical CS Parameter Keys
        //

        VerticalCSTypeGeoKey = 4096,
        VerticalCitationGeoKey = 4097,
        VerticalDatumGeoKey = 4098,
        VerticalUnitsGeoKey = 4099,

    }
    public enum ModelType : ushort
    {
        Undefined = 0,

        Projected = 1,  // Projection Coordinate System
        Geographic = 2,    // Geographic latitude-longitude System
        Geocentric = 3,    // Geocentric (X,Y,Z) Coordinate System

        UserDefined = 32767
    }
    public enum RasterType : ushort
    {
        Undefined = 0,

        PixelIsArea = 1,
        PixelIsPoint = 2,

        UserDefined = 32767
    }
    public enum GeographicCoordinateSystem
    {
        Undefined = 0,

        // EPSG GCS Based on Ellipsoid only
        // Note: the numeric code is equal to the code of the correspoding EPSG ellipsoid minus 3000
        Airy1830 = 4001,
        AiryModified1849 = 4002,
        AustralianNationalSpheroid = 4003,
        Bessel1841 = 4004,
        BesselModified = 4005,
        BesselNamibia = 4006,
        Clarke1858 = 4007,
        Clarke1866 = 4008,
        Clarke1866Michigan = 4009,
        Clarke1880_Benoit = 4010,
        Clarke1880_IGN = 4011,
        Clarke1880_RGS = 4012,
        Clarke1880_Arc = 4013,
        Clarke1880_SGA1922 = 4014,
        Everest1830_1937Adjustment = 4015,
        Everest1830_1967Definition = 4016,
        Everest1830_1975Definition = 4017,
        Everest1830Modified = 4018,
        GRS1980 = 4019,
        Helmert1906 = 4020,
        IndonesianNationalSpheroid = 4021,
        International1924 = 4022,
        International1967 = 4023,
        Krassowsky1940 = 4024,
        NWL9D = 4025,
        NWL10D = 4026,
        Plessis1817 = 4027,
        Struve1860 = 4028,
        WarOffice = 4029,
        WGS84 = 4030,
        GEM10C = 4031,
        OSU86F = 4032,
        OSU91A = 4033,
        Clarke1880 = 4034,
        Sphere = 4035,

        // EPSG GCS Based on EPSG Datum
        // Note: Geodetic datum using Greenwich PM have codes equal to the corresponding Datum code minus 2000
        Adindan = 4201,
        AGD66 = 4202,
        AGD84 = 4203,
        Ain_el_Abd = 4204,
        Afgooye = 4205,
        Agadez = 4206,
        Lisbon = 4207,
        Aratu = 4208,
        Arc_1950 = 4209,
        Arc_1960 = 4210,
        Batavia = 4211,
        Barbados = 4212,
        Beduaram = 4213,
        Beijing_1954 = 4214,
        Belge_1950 = 4215,
        Bermuda_1957 = 4216,
        Bern_1898 = 4217,
        Bogota = 4218,
        Bukit_Rimpah = 4219,
        Camacupa = 4220,
        Campo_Inchauspe = 4221,
        Cape = 4222,
        Carthage = 4223,
        Chua = 4224,
        Corrego_Alegre = 4225,
        Cote_d_Ivoire = 4226,
        Deir_ez_Zor = 4227,
        Douala = 4228,
        Egypt_1907 = 4229,
        ED50 = 4230,
        ED87 = 4231,
        Fahud = 4232,
        Gandajika_1970 = 4233,
        Garoua = 4234,
        Guyane_Francaise = 4235,
        Hu_Tzu_Shan = 4236,
        HD72 = 4237,
        ID74 = 4238,
        Indian_1954 = 4239,
        Indian_1975 = 4240,
        Jamaica_1875 = 4241,
        JAD69 = 4242,
        Kalianpur = 4243,
        Kandawala = 4244,
        Kertau = 4245,
        KOC = 4246,
        La_Canoa = 4247,
        PSAD56 = 4248,
        Lake = 4249,
        Leigon = 4250,
        Liberia_1964 = 4251,
        Lome = 4252,
        Luzon_1911 = 4253,
        Hito_XVIII_1963 = 4254,
        Herat_North = 4255,
        Mahe_1971 = 4256,
        Makassar = 4257,
        EUREF89 = 4258,
        Malongo_1987 = 4259,
        Manoca = 4260,
        Merchich = 4261,
        Massawa = 4262,
        Minna = 4263,
        Mhast = 4264,
        Monte_Mario = 4265,
        M_poraloko = 4266,
        NAD27 = 4267,
        NAD_Michigan = 4268,
        NAD83 = 4269,
        Nahrwan_1967 = 4270,
        Naparima_1972 = 4271,
        GD49 = 4272,
        NGO_1948 = 4273,
        Datum_73 = 4274,
        NTF = 4275,
        NSWC_9Z_2 = 4276,
        OSGB_1936 = 4277,
        OSGB70 = 4278,
        OS_SN80 = 4279,
        Padang = 4280,
        Palestine_1923 = 4281,
        Pointe_Noire = 4282,
        GDA94 = 4283,
        Pulkovo_1942 = 4284,
        Qatar = 4285,
        Qatar_1948 = 4286,
        Qornoq = 4287,
        Loma_Quintana = 4288,
        Amersfoort = 4289,
        RT38 = 4290,
        SAD69 = 4291,
        Sapper_Hill_1943 = 4292,
        Schwarzeck = 4293,
        Segora = 4294,
        Serindung = 4295,
        Sudan = 4296,
        Tananarive = 4297,
        Timbalai_1948 = 4298,
        TM65 = 4299,
        TM75 = 4300,
        Tokyo = 4301,
        Trinidad_1903 = 4302,
        TC_1948 = 4303,
        Voirol_1875 = 4304,
        Voirol_Unifie = 4305,
        Bern_1938 = 4306,
        Nord_Sahara_1959 = 4307,
        Stockholm_1938 = 4308,
        Yacare = 4309,
        Yoff = 4310,
        Zanderij = 4311,
        MGI = 4312,
        Belge_1972 = 4313,
        DHDN = 4314,
        Conakry_1905 = 4315,
        WGS_72 = 4322,
        WGS_72BE = 4324,
        WGS_84 = 4326,
        Bern_1898_Bern = 4801,
        Bogota_Bogota = 4802,
        Lisbon_Lisbon = 4803,
        Makassar_Jakarta = 4804,
        MGI_Ferro = 4805,
        Monte_Mario_Rome = 4806,
        NTF_Paris = 4807,
        Padang_Jakarta = 4808,
        Belge_1950_Brussels = 4809,
        Tananarive_Paris = 4810,
        Voirol_1875_Paris = 4811,
        Voirol_Unifie_Paris = 4812,
        Batavia_Jakarta = 4813,
        ATF_Paris = 4901,
        NDG_Paris = 4902,

        UserDefined = 32767,
    }
    public enum GeodeticDatum
    {
        Undefined = 0,

        // EPSG Datum Based on Ellipsoid only
        // The codes are the same as Ellipsoid's codes minus 1000.
        E_Airy1830 = 6001,
        E_AiryModified1849 = 6002,
        E_AustralianNationalSpheroid = 6003,
        E_Bessel1841 = 6004,
        E_BesselModified = 6005,
        E_BesselNamibia = 6006,
        E_Clarke1858 = 6007,
        E_Clarke1866 = 6008,
        E_Clarke1866Michigan = 6009,
        E_Clarke1880_Benoit = 6010,
        E_Clarke1880_IGN = 6011,
        E_Clarke1880_RGS = 6012,
        E_Clarke1880_Arc = 6013,
        E_Clarke1880_SGA1922 = 6014,
        E_Everest1830_1937Adjustment = 6015,
        E_Everest1830_1967Definition = 6016,
        E_Everest1830_1975Definition = 6017,
        E_Everest1830Modified = 6018,
        E_GRS1980 = 6019,
        E_Helmert1906 = 6020,
        E_IndonesianNationalSpheroid = 6021,
        E_International1924 = 6022,
        E_International1967 = 6023,
        E_Krassowsky1960 = 6024,
        E_NWL9D = 6025,
        E_NWL10D = 6026,
        E_Plessis1817 = 6027,
        E_Struve1860 = 6028,
        E_WarOffice = 6029,
        E_WGS84 = 6030,
        E_GEM10C = 6031,
        E_OSU86F = 6032,
        E_OSU91A = 6033,
        E_Clarke1880 = 6034,
        E_Sphere = 6035,

        // EPSG Datum Based on EPSG Datum
        Adindan = 6201,
        Australian_Geodetic_Datum_1966 = 6202,
        Australian_Geodetic_Datum_1984 = 6203,
        Ain_el_Abd_1970 = 6204,
        Afgooye = 6205,
        Agadez = 6206,
        Lisbon = 6207,
        Aratu = 6208,
        Arc_1950 = 6209,
        Arc_1960 = 6210,
        Batavia = 6211,
        Barbados = 6212,
        Beduaram = 6213,
        Beijing_1954 = 6214,
        Reseau_National_Belge_1950 = 6215,
        Bermuda_1957 = 6216,
        Bern_1898 = 6217,
        Bogota = 6218,
        Bukit_Rimpah = 6219,
        Camacupa = 6220,
        Campo_Inchauspe = 6221,
        Cape = 6222,
        Carthage = 6223,
        Chua = 6224,
        Corrego_Alegre = 6225,
        Cote_d_Ivoire = 6226,
        Deir_ez_Zor = 6227,
        Douala = 6228,
        Egypt_1907 = 6229,
        European_Datum_1950 = 6230,
        European_Datum_1987 = 6231,
        Fahud = 6232,
        Gandajika_1970 = 6233,
        Garoua = 6234,
        Guyane_Francaise = 6235,
        Hu_Tzu_Shan = 6236,
        Hungarian_Datum_1972 = 6237,
        Indonesian_Datum_1974 = 6238,
        Indian_1954 = 6239,
        Indian_1975 = 6240,
        Jamaica_1875 = 6241,
        Jamaica_1969 = 6242,
        Kalianpur = 6243,
        Kandawala = 6244,
        Kertau = 6245,
        Kuwait_Oil_Company = 6246,
        La_Canoa = 6247,
        Provisional_S_American_Datum_1956 = 6248,
        Lake = 6249,
        Leigon = 6250,
        Liberia_1964 = 6251,
        Lome = 6252,
        Luzon_1911 = 6253,
        Hito_XVIII_1963 = 6254,
        Herat_North = 6255,
        Mahe_1971 = 6256,
        Makassar = 6257,
        European_Reference_System_1989 = 6258,
        Malongo_1987 = 6259,
        Manoca = 6260,
        Merchich = 6261,
        Massawa = 6262,
        Minna = 6263,
        Mhast = 6264,
        Monte_Mario = 6265,
        M_poraloko = 6266,
        North_American_Datum_1927 = 6267,
        NAD_Michigan = 6268,
        North_American_Datum_1983 = 6269,
        Nahrwan_1967 = 6270,
        Naparima_1972 = 6271,
        New_Zealand_Geodetic_Datum_1949 = 6272,
        NGO_1948 = 6273,
        Datum_73 = 6274,
        Nouvelle_Triangulation_Francaise = 6275,
        NSWC_9Z_2 = 6276,
        OSGB_1936 = 6277,
        OSGB_1970_SN = 6278,
        OS_SN_1980 = 6279,
        Padang_1884 = 6280,
        Palestine_1923 = 6281,
        Pointe_Noire = 6282,
        Geocentric_Datum_of_Australia_1994 = 6283,
        Pulkovo_1942 = 6284,
        Qatar = 6285,
        Qatar_1948 = 6286,
        Qornoq = 6287,
        Loma_Quintana = 6288,
        Amersfoort = 6289,
        RT38 = 6290,
        South_American_Datum_1969 = 6291,
        Sapper_Hill_1943 = 6292,
        Schwarzeck = 6293,
        Segora = 6294,
        Serindung = 6295,
        Sudan = 6296,
        Tananarive_1925 = 6297,
        Timbalai_1948 = 6298,
        TM65 = 6299,
        TM75 = 6300,
        Tokyo = 6301,
        Trinidad_1903 = 6302,
        Trucial_Coast_1948 = 6303,
        Voirol_1875 = 6304,
        Voirol_Unifie_1960 = 6305,
        Bern_1938 = 6306,
        Nord_Sahara_1959 = 6307,
        Stockholm_1938 = 6308,
        Yacare = 6309,
        Yoff = 6310,
        Zanderij = 6311,
        Militar_Geographische_Institut = 6312,
        Reseau_National_Belge_1972 = 6313,
        Deutsche_Hauptdreiecksnetz = 6314,
        Conakry_1905 = 6315,

        // WGS Datum
        WGS72 = 6322,
        WGS72_Transit_Broadcast_Ephemeris = 6324,
        WGS84 = 6326,

        // Archaic Datum
        Ancienne_Triangulation_Francaise = 6901,
        Nord_de_Guerre = 6902,

        UserDefined = 32767,
    }
    public enum PrimeMeridian
    {
        Undefined = 0,

        Greenwich = 8901,
        Lisbon = 8902,
        Paris = 8903,
        Bogota = 8904,
        Madrid = 8905,
        Rome = 8906,
        Bern = 8907,
        Jakarta = 8908,
        Ferro = 8909,
        Brussels = 8910,
        Stockholm = 8911,

        UserDefined = 32767,
    }
    public enum LinearUnit : ushort
    {
        Undefined = 0,

        Meter = 9001,
        Foot = 9002,
        Foot_US_Survey = 9003,
        Foot_Modified_American = 9004,
        Foot_Clarke = 9005,
        Foot_Indian = 9006,
        Link = 9007,
        Link_Benoit = 9008,
        Link_Sears = 9009,
        Chain_Benoit = 9010,
        Chain_Sears = 9011,
        Yard_Sears = 9012,
        Yard_Indian = 9013,
        Fathom = 9014,
        Mile_International_Nautical = 9015,

        UserDefined = 32767
    }
    public enum AngularUnit : ushort
    {
        Undefined = 0,

        Radian = 9101,
        Degree = 9102,
        Arc_Minute = 9103,
        Arc_Second = 9104,
        Grad = 9105,
        Gon = 9106,
        DMS = 9107,
        DMS_Hemisphere = 9108,

        UserDefined = 32767
    }
    public enum Ellipsoid
    {
        Undefined = 0,

        Airy_1830 = 7001,
        Airy_Modified_1849 = 7002,
        Australian_National_Spheroid = 7003,
        Bessel_1841 = 7004,
        Bessel_Modified = 7005,
        Bessel_Namibia = 7006,
        Clarke_1858 = 7007,
        Clarke_1866 = 7008,
        Clarke_1866_Michigan = 7009,
        Clarke_1880_Benoit = 7010,
        Clarke_1880_IGN = 7011,
        Clarke_1880_RGS = 7012,
        Clarke_1880_Arc = 7013,
        Clarke_1880_SGA_1922 = 7014,
        Everest_1830_1937_Adjustment = 7015,
        Everest_1830_1967_Definition = 7016,
        Everest_1830_1975_Definition = 7017,
        Everest_1830_Modified = 7018,
        GRS_1980 = 7019,
        Helmert_1906 = 7020,
        Indonesian_National_Spheroid = 7021,
        International_1924 = 7022,
        International_1967 = 7023,
        Krassowsky_1940 = 7024,
        NWL_9D = 7025,
        NWL_10D = 7026,
        Plessis_1817 = 7027,
        Struve_1860 = 7028,
        War_Office = 7029,
        WGS_84 = 7030,
        GEM_10C = 7031,
        OSU86F = 7032,
        OSU91A = 7033,
        Clarke_1880 = 7034,
        Sphere = 7035,

        UserDefined = 32767,
    }
    public enum ProjectedCoordinateSystem
    {
        Undefined = 0,
        UserDefined = 32767,

        Adindan_UTM_zone_37N = 20137,
        Adindan_UTM_zone_38N = 20138,
        AGD66_AMG_zone_48 = 20248,
        AGD66_AMG_zone_49 = 20249,
        AGD66_AMG_zone_50 = 20250,
        AGD66_AMG_zone_51 = 20251,
        AGD66_AMG_zone_52 = 20252,
        AGD66_AMG_zone_53 = 20253,
        AGD66_AMG_zone_54 = 20254,
        AGD66_AMG_zone_55 = 20255,
        AGD66_AMG_zone_56 = 20256,
        AGD66_AMG_zone_57 = 20257,
        AGD66_AMG_zone_58 = 20258,
        AGD84_AMG_zone_48 = 20348,
        AGD84_AMG_zone_49 = 20349,
        AGD84_AMG_zone_50 = 20350,
        AGD84_AMG_zone_51 = 20351,
        AGD84_AMG_zone_52 = 20352,
        AGD84_AMG_zone_53 = 20353,
        AGD84_AMG_zone_54 = 20354,
        AGD84_AMG_zone_55 = 20355,
        AGD84_AMG_zone_56 = 20356,
        AGD84_AMG_zone_57 = 20357,
        AGD84_AMG_zone_58 = 20358,
        Ain_el_Abd_UTM_zone_37N = 20437,
        Ain_el_Abd_UTM_zone_38N = 20438,
        Ain_el_Abd_UTM_zone_39N = 20439,
        Ain_el_Abd_Bahrain_Grid = 20499,
        Afgooye_UTM_zone_38N = 20538,
        Afgooye_UTM_zone_39N = 20539,
        Lisbon_Portugese_Grid = 20700,
        Aratu_UTM_zone_22S = 20822,
        Aratu_UTM_zone_23S = 20823,
        Aratu_UTM_zone_24S = 20824,
        Arc_1950_Lo13 = 20973,
        Arc_1950_Lo15 = 20975,
        Arc_1950_Lo17 = 20977,
        Arc_1950_Lo19 = 20979,
        Arc_1950_Lo21 = 20981,
        Arc_1950_Lo23 = 20983,
        Arc_1950_Lo25 = 20985,
        Arc_1950_Lo27 = 20987,
        Arc_1950_Lo29 = 20989,
        Arc_1950_Lo31 = 20991,
        Arc_1950_Lo33 = 20993,
        Arc_1950_Lo35 = 20995,
        Batavia_NEIEZ = 21100,
        Batavia_UTM_zone_48S = 21148,
        Batavia_UTM_zone_49S = 21149,
        Batavia_UTM_zone_50S = 21150,
        Beijing_Gauss_zone_13 = 21413,
        Beijing_Gauss_zone_14 = 21414,
        Beijing_Gauss_zone_15 = 21415,
        Beijing_Gauss_zone_16 = 21416,
        Beijing_Gauss_zone_17 = 21417,
        Beijing_Gauss_zone_18 = 21418,
        Beijing_Gauss_zone_19 = 21419,
        Beijing_Gauss_zone_20 = 21420,
        Beijing_Gauss_zone_21 = 21421,
        Beijing_Gauss_zone_22 = 21422,
        Beijing_Gauss_zone_23 = 21423,
        Beijing_Gauss_13N = 21473,
        Beijing_Gauss_14N = 21474,
        Beijing_Gauss_15N = 21475,
        Beijing_Gauss_16N = 21476,
        Beijing_Gauss_17N = 21477,
        Beijing_Gauss_18N = 21478,
        Beijing_Gauss_19N = 21479,
        Beijing_Gauss_20N = 21480,
        Beijing_Gauss_21N = 21481,
        Beijing_Gauss_22N = 21482,
        Beijing_Gauss_23N = 21483,
        Belge_Lambert_50 = 21500,
        Bern_1898_Swiss_Old = 21790,
        Bogota_UTM_zone_17N = 21817,
        Bogota_UTM_zone_18N = 21818,
        Bogota_Colombia_3W = 21891,
        Bogota_Colombia_Bogota = 21892,
        Bogota_Colombia_3E = 21893,
        Bogota_Colombia_6E = 21894,
        Camacupa_UTM_32S = 22032,
        Camacupa_UTM_33S = 22033,
        C_Inchauspe_Argentina_1 = 22191,
        C_Inchauspe_Argentina_2 = 22192,
        C_Inchauspe_Argentina_3 = 22193,
        C_Inchauspe_Argentina_4 = 22194,
        C_Inchauspe_Argentina_5 = 22195,
        C_Inchauspe_Argentina_6 = 22196,
        C_Inchauspe_Argentina_7 = 22197,
        Carthage_UTM_zone_32N = 22332,
        Carthage_Nord_Tunisie = 22391,
        Carthage_Sud_Tunisie = 22392,
        Corrego_Alegre_UTM_23S = 22523,
        Corrego_Alegre_UTM_24S = 22524,
        Douala_UTM_zone_32N = 22832,
        Egypt_1907_Red_Belt = 22992,
        Egypt_1907_Purple_Belt = 22993,
        Egypt_1907_Ext_Purple = 22994,
        ED50_UTM_zone_28N = 23028,
        ED50_UTM_zone_29N = 23029,
        ED50_UTM_zone_30N = 23030,
        ED50_UTM_zone_31N = 23031,
        ED50_UTM_zone_32N = 23032,
        ED50_UTM_zone_33N = 23033,
        ED50_UTM_zone_34N = 23034,
        ED50_UTM_zone_35N = 23035,
        ED50_UTM_zone_36N = 23036,
        ED50_UTM_zone_37N = 23037,
        ED50_UTM_zone_38N = 23038,
        Fahud_UTM_zone_39N = 23239,
        Fahud_UTM_zone_40N = 23240,
        Garoua_UTM_zone_33N = 23433,
        ID74_UTM_zone_46N = 23846,
        ID74_UTM_zone_47N = 23847,
        ID74_UTM_zone_48N = 23848,
        ID74_UTM_zone_49N = 23849,
        ID74_UTM_zone_50N = 23850,
        ID74_UTM_zone_51N = 23851,
        ID74_UTM_zone_52N = 23852,
        ID74_UTM_zone_53N = 23853,
        ID74_UTM_zone_46S = 23886,
        ID74_UTM_zone_47S = 23887,
        ID74_UTM_zone_48S = 23888,
        ID74_UTM_zone_49S = 23889,
        ID74_UTM_zone_50S = 23890,
        ID74_UTM_zone_51S = 23891,
        ID74_UTM_zone_52S = 23892,
        ID74_UTM_zone_53S = 23893,
        ID74_UTM_zone_54S = 23894,
        Indian_1954_UTM_47N = 23947,
        Indian_1954_UTM_48N = 23948,
        Indian_1975_UTM_47N = 24047,
        Indian_1975_UTM_48N = 24048,
        Jamaica_1875_Old_Grid = 24100,
        JAD69_Jamaica_Grid = 24200,
        Kalianpur_India_0 = 24370,
        Kalianpur_India_I = 24371,
        Kalianpur_India_IIa = 24372,
        Kalianpur_India_IIIa = 24373,
        Kalianpur_India_IVa = 24374,
        Kalianpur_India_IIb = 24382,
        Kalianpur_India_IIIb = 24383,
        Kalianpur_India_IVb = 24384,
        Kertau_Singapore_Grid = 24500,
        Kertau_UTM_zone_47N = 24547,
        Kertau_UTM_zone_48N = 24548,
        La_Canoa_UTM_zone_20N = 24720,
        La_Canoa_UTM_zone_21N = 24721,
        PSAD56_UTM_zone_18N = 24818,
        PSAD56_UTM_zone_19N = 24819,
        PSAD56_UTM_zone_20N = 24820,
        PSAD56_UTM_zone_21N = 24821,
        PSAD56_UTM_zone_17S = 24877,
        PSAD56_UTM_zone_18S = 24878,
        PSAD56_UTM_zone_19S = 24879,
        PSAD56_UTM_zone_20S = 24880,
        PSAD56_Peru_west_zone = 24891,
        PSAD56_Peru_central = 24892,
        PSAD56_Peru_east_zone = 24893,
        Leigon_Ghana_Grid = 25000,
        Lome_UTM_zone_31N = 25231,
        Luzon_Philippines_I = 25391,
        Luzon_Philippines_II = 25392,
        Luzon_Philippines_III = 25393,
        Luzon_Philippines_IV = 25394,
        Luzon_Philippines_V = 25395,
        Makassar_NEIEZ = 25700,
        Malongo_1987_UTM_32S = 25932,
        Merchich_Nord_Maroc = 26191,
        Merchich_Sud_Maroc = 26192,
        Merchich_Sahara = 26193,
        Massawa_UTM_zone_37N = 26237,
        Minna_UTM_zone_31N = 26331,
        Minna_UTM_zone_32N = 26332,
        Minna_Nigeria_West = 26391,
        Minna_Nigeria_Mid_Belt = 26392,
        Minna_Nigeria_East = 26393,
        Mhast_UTM_zone_32S = 26432,
        Monte_Mario_Italy_1 = 26591,
        Monte_Mario_Italy_2 = 26592,
        M_poraloko_UTM_32N = 26632,
        M_poraloko_UTM_32S = 26692,
        NAD27_UTM_zone_3N = 26703,
        NAD27_UTM_zone_4N = 26704,
        NAD27_UTM_zone_5N = 26705,
        NAD27_UTM_zone_6N = 26706,
        NAD27_UTM_zone_7N = 26707,
        NAD27_UTM_zone_8N = 26708,
        NAD27_UTM_zone_9N = 26709,
        NAD27_UTM_zone_10N = 26710,
        NAD27_UTM_zone_11N = 26711,
        NAD27_UTM_zone_12N = 26712,
        NAD27_UTM_zone_13N = 26713,
        NAD27_UTM_zone_14N = 26714,
        NAD27_UTM_zone_15N = 26715,
        NAD27_UTM_zone_16N = 26716,
        NAD27_UTM_zone_17N = 26717,
        NAD27_UTM_zone_18N = 26718,
        NAD27_UTM_zone_19N = 26719,
        NAD27_UTM_zone_20N = 26720,
        NAD27_UTM_zone_21N = 26721,
        NAD27_UTM_zone_22N = 26722,
        NAD27_Alabama_East = 26729,
        NAD27_Alabama_West = 26730,
        NAD27_Alaska_zone_1 = 26731,
        NAD27_Alaska_zone_2 = 26732,
        NAD27_Alaska_zone_3 = 26733,
        NAD27_Alaska_zone_4 = 26734,
        NAD27_Alaska_zone_5 = 26735,
        NAD27_Alaska_zone_6 = 26736,
        NAD27_Alaska_zone_7 = 26737,
        NAD27_Alaska_zone_8 = 26738,
        NAD27_Alaska_zone_9 = 26739,
        NAD27_Alaska_zone_10 = 26740,
        NAD27_California_I = 26741,
        NAD27_California_II = 26742,
        NAD27_California_III = 26743,
        NAD27_California_IV = 26744,
        NAD27_California_V = 26745,
        NAD27_California_VI = 26746,
        NAD27_California_VII = 26747,
        NAD27_Arizona_East = 26748,
        NAD27_Arizona_Central = 26749,
        NAD27_Arizona_West = 26750,
        NAD27_Arkansas_North = 26751,
        NAD27_Arkansas_South = 26752,
        NAD27_Colorado_North = 26753,
        NAD27_Colorado_Central = 26754,
        NAD27_Colorado_South = 26755,
        NAD27_Connecticut = 26756,
        NAD27_Delaware = 26757,
        NAD27_Florida_East = 26758,
        NAD27_Florida_West = 26759,
        NAD27_Florida_North = 26760,
        NAD27_Hawaii_zone_1 = 26761,
        NAD27_Hawaii_zone_2 = 26762,
        NAD27_Hawaii_zone_3 = 26763,
        NAD27_Hawaii_zone_4 = 26764,
        NAD27_Hawaii_zone_5 = 26765,
        NAD27_Georgia_East = 26766,
        NAD27_Georgia_West = 26767,
        NAD27_Idaho_East = 26768,
        NAD27_Idaho_Central = 26769,
        NAD27_Idaho_West = 26770,
        NAD27_Illinois_East = 26771,
        NAD27_Illinois_West = 26772,
        NAD27_Indiana_East = 26773,
        NAD27_BLM_14N_feet = 26774,
        NAD27_Indiana_West = 26774,
        NAD27_BLM_15N_feet = 26775,
        NAD27_Iowa_North = 26775,
        NAD27_BLM_16N_feet = 26776,
        NAD27_Iowa_South = 26776,
        NAD27_BLM_17N_feet = 26777,
        NAD27_Kansas_North = 26777,
        NAD27_Kansas_South = 26778,
        NAD27_Kentucky_North = 26779,
        NAD27_Kentucky_South = 26780,
        NAD27_Louisiana_North = 26781,
        NAD27_Louisiana_South = 26782,
        NAD27_Maine_East = 26783,
        NAD27_Maine_West = 26784,
        NAD27_Maryland = 26785,
        NAD27_Massachusetts = 26786,
        NAD27_Massachusetts_Is = 26787,
        NAD27_Michigan_North = 26788,
        NAD27_Michigan_Central = 26789,
        NAD27_Michigan_South = 26790,
        NAD27_Minnesota_North = 26791,
        NAD27_Minnesota_Cent = 26792,
        NAD27_Minnesota_South = 26793,
        NAD27_Mississippi_East = 26794,
        NAD27_Mississippi_West = 26795,
        NAD27_Missouri_East = 26796,
        NAD27_Missouri_Central = 26797,
        NAD27_Missouri_West = 26798,
        NAD_Michigan_Michigan_East = 26801,
        NAD_Michigan_Michigan_Old_Central = 26802,
        NAD_Michigan_Michigan_West = 26803,
        NAD83_UTM_zone_3N = 26903,
        NAD83_UTM_zone_4N = 26904,
        NAD83_UTM_zone_5N = 26905,
        NAD83_UTM_zone_6N = 26906,
        NAD83_UTM_zone_7N = 26907,
        NAD83_UTM_zone_8N = 26908,
        NAD83_UTM_zone_9N = 26909,
        NAD83_UTM_zone_10N = 26910,
        NAD83_UTM_zone_11N = 26911,
        NAD83_UTM_zone_12N = 26912,
        NAD83_UTM_zone_13N = 26913,
        NAD83_UTM_zone_14N = 26914,
        NAD83_UTM_zone_15N = 26915,
        NAD83_UTM_zone_16N = 26916,
        NAD83_UTM_zone_17N = 26917,
        NAD83_UTM_zone_18N = 26918,
        NAD83_UTM_zone_19N = 26919,
        NAD83_UTM_zone_20N = 26920,
        NAD83_UTM_zone_21N = 26921,
        NAD83_UTM_zone_22N = 26922,
        NAD83_UTM_zone_23N = 26923,
        NAD83_Alabama_East = 26929,
        NAD83_Alabama_West = 26930,
        NAD83_Alaska_zone_1 = 26931,
        NAD83_Alaska_zone_2 = 26932,
        NAD83_Alaska_zone_3 = 26933,
        NAD83_Alaska_zone_4 = 26934,
        NAD83_Alaska_zone_5 = 26935,
        NAD83_Alaska_zone_6 = 26936,
        NAD83_Alaska_zone_7 = 26937,
        NAD83_Alaska_zone_8 = 26938,
        NAD83_Alaska_zone_9 = 26939,
        NAD83_Alaska_zone_10 = 26940,
        NAD83_California_1 = 26941,
        NAD83_California_2 = 26942,
        NAD83_California_3 = 26943,
        NAD83_California_4 = 26944,
        NAD83_California_5 = 26945,
        NAD83_California_6 = 26946,
        NAD83_Arizona_East = 26948,
        NAD83_Arizona_Central = 26949,
        NAD83_Arizona_West = 26950,
        NAD83_Arkansas_North = 26951,
        NAD83_Arkansas_South = 26952,
        NAD83_Colorado_North = 26953,
        NAD83_Colorado_Central = 26954,
        NAD83_Colorado_South = 26955,
        NAD83_Connecticut = 26956,
        NAD83_Delaware = 26957,
        NAD83_Florida_East = 26958,
        NAD83_Florida_West = 26959,
        NAD83_Florida_North = 26960,
        NAD83_Hawaii_zone_1 = 26961,
        NAD83_Hawaii_zone_2 = 26962,
        NAD83_Hawaii_zone_3 = 26963,
        NAD83_Hawaii_zone_4 = 26964,
        NAD83_Hawaii_zone_5 = 26965,
        NAD83_Georgia_East = 26966,
        NAD83_Georgia_West = 26967,
        NAD83_Idaho_East = 26968,
        NAD83_Idaho_Central = 26969,
        NAD83_Idaho_West = 26970,
        NAD83_Illinois_East = 26971,
        NAD83_Illinois_West = 26972,
        NAD83_Indiana_East = 26973,
        NAD83_Indiana_West = 26974,
        NAD83_Iowa_North = 26975,
        NAD83_Iowa_South = 26976,
        NAD83_Kansas_North = 26977,
        NAD83_Kansas_South = 26978,
        NAD83_Kentucky_North = 26979,
        NAD83_Kentucky_South = 26980,
        NAD83_Louisiana_North = 26981,
        NAD83_Louisiana_South = 26982,
        NAD83_Maine_East = 26983,
        NAD83_Maine_West = 26984,
        NAD83_Maryland = 26985,
        NAD83_Massachusetts = 26986,
        NAD83_Massachusetts_Is = 26987,
        NAD83_Michigan_North = 26988,
        NAD83_Michigan_Central = 26989,
        NAD83_Michigan_South = 26990,
        NAD83_Minnesota_North = 26991,
        NAD83_Minnesota_Cent = 26992,
        NAD83_Minnesota_South = 26993,
        NAD83_Mississippi_East = 26994,
        NAD83_Mississippi_West = 26995,
        NAD83_Missouri_East = 26996,
        NAD83_Missouri_Central = 26997,
        NAD83_Missouri_West = 26998,
        Nahrwan_1967_UTM_38N = 27038,
        Nahrwan_1967_UTM_39N = 27039,
        Nahrwan_1967_UTM_40N = 27040,
        Naparima_UTM_20N = 27120,
        GD49_NZ_Map_Grid = 27200,
        GD49_North_Island_Grid = 27291,
        GD49_South_Island_Grid = 27292,
        Datum_73_UTM_zone_29N = 27429,
        ATF_Nord_de_Guerre = 27500,
        NTF_France_I = 27581,
        NTF_France_II = 27582,
        NTF_France_III = 27583,
        NTF_Nord_France = 27591,
        NTF_Centre_France = 27592,
        NTF_Sud_France = 27593,
        British_National_Grid = 27700,
        Point_Noire_UTM_32S = 28232,
        GDA94_MGA_zone_48 = 28348,
        GDA94_MGA_zone_49 = 28349,
        GDA94_MGA_zone_50 = 28350,
        GDA94_MGA_zone_51 = 28351,
        GDA94_MGA_zone_52 = 28352,
        GDA94_MGA_zone_53 = 28353,
        GDA94_MGA_zone_54 = 28354,
        GDA94_MGA_zone_55 = 28355,
        GDA94_MGA_zone_56 = 28356,
        GDA94_MGA_zone_57 = 28357,
        GDA94_MGA_zone_58 = 28358,
        Pulkovo_Gauss_zone_4 = 28404,
        Pulkovo_Gauss_zone_5 = 28405,
        Pulkovo_Gauss_zone_6 = 28406,
        Pulkovo_Gauss_zone_7 = 28407,
        Pulkovo_Gauss_zone_8 = 28408,
        Pulkovo_Gauss_zone_9 = 28409,
        Pulkovo_Gauss_zone_10 = 28410,
        Pulkovo_Gauss_zone_11 = 28411,
        Pulkovo_Gauss_zone_12 = 28412,
        Pulkovo_Gauss_zone_13 = 28413,
        Pulkovo_Gauss_zone_14 = 28414,
        Pulkovo_Gauss_zone_15 = 28415,
        Pulkovo_Gauss_zone_16 = 28416,
        Pulkovo_Gauss_zone_17 = 28417,
        Pulkovo_Gauss_zone_18 = 28418,
        Pulkovo_Gauss_zone_19 = 28419,
        Pulkovo_Gauss_zone_20 = 28420,
        Pulkovo_Gauss_zone_21 = 28421,
        Pulkovo_Gauss_zone_22 = 28422,
        Pulkovo_Gauss_zone_23 = 28423,
        Pulkovo_Gauss_zone_24 = 28424,
        Pulkovo_Gauss_zone_25 = 28425,
        Pulkovo_Gauss_zone_26 = 28426,
        Pulkovo_Gauss_zone_27 = 28427,
        Pulkovo_Gauss_zone_28 = 28428,
        Pulkovo_Gauss_zone_29 = 28429,
        Pulkovo_Gauss_zone_30 = 28430,
        Pulkovo_Gauss_zone_31 = 28431,
        Pulkovo_Gauss_zone_32 = 28432,
        Pulkovo_Gauss_4N = 28464,
        Pulkovo_Gauss_5N = 28465,
        Pulkovo_Gauss_6N = 28466,
        Pulkovo_Gauss_7N = 28467,
        Pulkovo_Gauss_8N = 28468,
        Pulkovo_Gauss_9N = 28469,
        Pulkovo_Gauss_10N = 28470,
        Pulkovo_Gauss_11N = 28471,
        Pulkovo_Gauss_12N = 28472,
        Pulkovo_Gauss_13N = 28473,
        Pulkovo_Gauss_14N = 28474,
        Pulkovo_Gauss_15N = 28475,
        Pulkovo_Gauss_16N = 28476,
        Pulkovo_Gauss_17N = 28477,
        Pulkovo_Gauss_18N = 28478,
        Pulkovo_Gauss_19N = 28479,
        Pulkovo_Gauss_20N = 28480,
        Pulkovo_Gauss_21N = 28481,
        Pulkovo_Gauss_22N = 28482,
        Pulkovo_Gauss_23N = 28483,
        Pulkovo_Gauss_24N = 28484,
        Pulkovo_Gauss_25N = 28485,
        Pulkovo_Gauss_26N = 28486,
        Pulkovo_Gauss_27N = 28487,
        Pulkovo_Gauss_28N = 28488,
        Pulkovo_Gauss_29N = 28489,
        Pulkovo_Gauss_30N = 28490,
        Pulkovo_Gauss_31N = 28491,
        Pulkovo_Gauss_32N = 28492,
        Qatar_National_Grid = 28600,
        RD_Netherlands_Old = 28991,
        RD_Netherlands_New = 28992,
        SAD69_UTM_zone_18N = 29118,
        SAD69_UTM_zone_19N = 29119,
        SAD69_UTM_zone_20N = 29120,
        SAD69_UTM_zone_21N = 29121,
        SAD69_UTM_zone_22N = 29122,
        SAD69_UTM_zone_17S = 29177,
        SAD69_UTM_zone_18S = 29178,
        SAD69_UTM_zone_19S = 29179,
        SAD69_UTM_zone_20S = 29180,
        SAD69_UTM_zone_21S = 29181,
        SAD69_UTM_zone_22S = 29182,
        SAD69_UTM_zone_23S = 29183,
        SAD69_UTM_zone_24S = 29184,
        SAD69_UTM_zone_25S = 29185,
        Sapper_Hill_UTM_20S = 29220,
        Sapper_Hill_UTM_21S = 29221,
        Schwarzeck_UTM_33S = 29333,
        Sudan_UTM_zone_35N = 29635,
        Sudan_UTM_zone_36N = 29636,
        Tananarive_Laborde = 29700,
        Tananarive_UTM_38S = 29738,
        Tananarive_UTM_39S = 29739,
        Timbalai_1948_Borneo = 29800,
        Timbalai_1948_UTM_49N = 29849,
        Timbalai_1948_UTM_50N = 29850,
        TM65_Irish_Nat_Grid = 29900,
        Trinidad_1903_Trinidad = 30200,
        TC_1948_UTM_zone_39N = 30339,
        TC_1948_UTM_zone_40N = 30340,
        Voirol_N_Algerie_ancien = 30491,
        Voirol_S_Algerie_ancien = 30492,
        Voirol_Unifie_N_Algerie = 30591,
        Voirol_Unifie_S_Algerie = 30592,
        Bern_1938_Swiss_New = 30600,
        Nord_Sahara_UTM_29N = 30729,
        Nord_Sahara_UTM_30N = 30730,
        Nord_Sahara_UTM_31N = 30731,
        Nord_Sahara_UTM_32N = 30732,
        Yoff_UTM_zone_28N = 31028,
        Zanderij_UTM_zone_21N = 31121,
        MGI_Austria_West = 31291,
        MGI_Austria_Central = 31292,
        MGI_Austria_East = 31293,
        Belge_Lambert_72 = 31300,
        DHDN_Germany_zone_1 = 31491,
        DHDN_Germany_zone_2 = 31492,
        DHDN_Germany_zone_3 = 31493,
        DHDN_Germany_zone_4 = 31494,
        DHDN_Germany_zone_5 = 31495,
        NAD27_Montana_North = 32001,
        NAD27_Montana_Central = 32002,
        NAD27_Montana_South = 32003,
        NAD27_Nebraska_North = 32005,
        NAD27_Nebraska_South = 32006,
        NAD27_Nevada_East = 32007,
        NAD27_Nevada_Central = 32008,
        NAD27_Nevada_West = 32009,
        NAD27_New_Hampshire = 32010,
        NAD27_New_Jersey = 32011,
        NAD27_New_Mexico_East = 32012,
        NAD27_New_Mexico_Cent = 32013,
        NAD27_New_Mexico_West = 32014,
        NAD27_New_York_East = 32015,
        NAD27_New_York_Central = 32016,
        NAD27_New_York_West = 32017,
        NAD27_New_York_Long_Is = 32018,
        NAD27_North_Carolina = 32019,
        NAD27_North_Dakota_N = 32020,
        NAD27_North_Dakota_S = 32021,
        NAD27_Ohio_North = 32022,
        NAD27_Ohio_South = 32023,
        NAD27_Oklahoma_North = 32024,
        NAD27_Oklahoma_South = 32025,
        NAD27_Oregon_North = 32026,
        NAD27_Oregon_South = 32027,
        NAD27_Pennsylvania_N = 32028,
        NAD27_Pennsylvania_S = 32029,
        NAD27_Rhode_Island = 32030,
        NAD27_South_Carolina_N = 32031,
        NAD27_South_Carolina_S = 32033,
        NAD27_South_Dakota_N = 32034,
        NAD27_South_Dakota_S = 32035,
        NAD27_Tennessee = 32036,
        NAD27_Texas_North = 32037,
        NAD27_Texas_North_Cen = 32038,
        NAD27_Texas_Central = 32039,
        NAD27_Texas_South_Cen = 32040,
        NAD27_Texas_South = 32041,
        NAD27_Utah_North = 32042,
        NAD27_Utah_Central = 32043,
        NAD27_Utah_South = 32044,
        NAD27_Vermont = 32045,
        NAD27_Virginia_North = 32046,
        NAD27_Virginia_South = 32047,
        NAD27_Washington_North = 32048,
        NAD27_Washington_South = 32049,
        NAD27_West_Virginia_N = 32050,
        NAD27_West_Virginia_S = 32051,
        NAD27_Wisconsin_North = 32052,
        NAD27_Wisconsin_Cen = 32053,
        NAD27_Wisconsin_South = 32054,
        NAD27_Wyoming_East = 32055,
        NAD27_Wyoming_E_Cen = 32056,
        NAD27_Wyoming_W_Cen = 32057,
        NAD27_Wyoming_West = 32058,
        NAD27_Puerto_Rico = 32059,
        NAD27_St_Croix = 32060,
        NAD83_Montana = 32100,
        NAD83_Nebraska = 32104,
        NAD83_Nevada_East = 32107,
        NAD83_Nevada_Central = 32108,
        NAD83_Nevada_West = 32109,
        NAD83_New_Hampshire = 32110,
        NAD83_New_Jersey = 32111,
        NAD83_New_Mexico_East = 32112,
        NAD83_New_Mexico_Cent = 32113,
        NAD83_New_Mexico_West = 32114,
        NAD83_New_York_East = 32115,
        NAD83_New_York_Central = 32116,
        NAD83_New_York_West = 32117,
        NAD83_New_York_Long_Is = 32118,
        NAD83_North_Carolina = 32119,
        NAD83_North_Dakota_N = 32120,
        NAD83_North_Dakota_S = 32121,
        NAD83_Ohio_North = 32122,
        NAD83_Ohio_South = 32123,
        NAD83_Oklahoma_North = 32124,
        NAD83_Oklahoma_South = 32125,
        NAD83_Oregon_North = 32126,
        NAD83_Oregon_South = 32127,
        NAD83_Pennsylvania_N = 32128,
        NAD83_Pennsylvania_S = 32129,
        NAD83_Rhode_Island = 32130,
        NAD83_South_Carolina = 32133,
        NAD83_South_Dakota_N = 32134,
        NAD83_South_Dakota_S = 32135,
        NAD83_Tennessee = 32136,
        NAD83_Texas_North = 32137,
        NAD83_Texas_North_Cen = 32138,
        NAD83_Texas_Central = 32139,
        NAD83_Texas_South_Cen = 32140,
        NAD83_Texas_South = 32141,
        NAD83_Utah_North = 32142,
        NAD83_Utah_Central = 32143,
        NAD83_Utah_South = 32144,
        NAD83_Vermont = 32145,
        NAD83_Virginia_North = 32146,
        NAD83_Virginia_South = 32147,
        NAD83_Washington_North = 32148,
        NAD83_Washington_South = 32149,
        NAD83_West_Virginia_N = 32150,
        NAD83_West_Virginia_S = 32151,
        NAD83_Wisconsin_North = 32152,
        NAD83_Wisconsin_Cen = 32153,
        NAD83_Wisconsin_South = 32154,
        NAD83_Wyoming_East = 32155,
        NAD83_Wyoming_E_Cen = 32156,
        NAD83_Wyoming_W_Cen = 32157,
        NAD83_Wyoming_West = 32158,
        NAD83_Puerto_Rico_Virgin_Is = 32161,
        WGS72_UTM_zone_1N = 32201,
        WGS72_UTM_zone_2N = 32202,
        WGS72_UTM_zone_3N = 32203,
        WGS72_UTM_zone_4N = 32204,
        WGS72_UTM_zone_5N = 32205,
        WGS72_UTM_zone_6N = 32206,
        WGS72_UTM_zone_7N = 32207,
        WGS72_UTM_zone_8N = 32208,
        WGS72_UTM_zone_9N = 32209,
        WGS72_UTM_zone_10N = 32210,
        WGS72_UTM_zone_11N = 32211,
        WGS72_UTM_zone_12N = 32212,
        WGS72_UTM_zone_13N = 32213,
        WGS72_UTM_zone_14N = 32214,
        WGS72_UTM_zone_15N = 32215,
        WGS72_UTM_zone_16N = 32216,
        WGS72_UTM_zone_17N = 32217,
        WGS72_UTM_zone_18N = 32218,
        WGS72_UTM_zone_19N = 32219,
        WGS72_UTM_zone_20N = 32220,
        WGS72_UTM_zone_21N = 32221,
        WGS72_UTM_zone_22N = 32222,
        WGS72_UTM_zone_23N = 32223,
        WGS72_UTM_zone_24N = 32224,
        WGS72_UTM_zone_25N = 32225,
        WGS72_UTM_zone_26N = 32226,
        WGS72_UTM_zone_27N = 32227,
        WGS72_UTM_zone_28N = 32228,
        WGS72_UTM_zone_29N = 32229,
        WGS72_UTM_zone_30N = 32230,
        WGS72_UTM_zone_31N = 32231,
        WGS72_UTM_zone_32N = 32232,
        WGS72_UTM_zone_33N = 32233,
        WGS72_UTM_zone_34N = 32234,
        WGS72_UTM_zone_35N = 32235,
        WGS72_UTM_zone_36N = 32236,
        WGS72_UTM_zone_37N = 32237,
        WGS72_UTM_zone_38N = 32238,
        WGS72_UTM_zone_39N = 32239,
        WGS72_UTM_zone_40N = 32240,
        WGS72_UTM_zone_41N = 32241,
        WGS72_UTM_zone_42N = 32242,
        WGS72_UTM_zone_43N = 32243,
        WGS72_UTM_zone_44N = 32244,
        WGS72_UTM_zone_45N = 32245,
        WGS72_UTM_zone_46N = 32246,
        WGS72_UTM_zone_47N = 32247,
        WGS72_UTM_zone_48N = 32248,
        WGS72_UTM_zone_49N = 32249,
        WGS72_UTM_zone_50N = 32250,
        WGS72_UTM_zone_51N = 32251,
        WGS72_UTM_zone_52N = 32252,
        WGS72_UTM_zone_53N = 32253,
        WGS72_UTM_zone_54N = 32254,
        WGS72_UTM_zone_55N = 32255,
        WGS72_UTM_zone_56N = 32256,
        WGS72_UTM_zone_57N = 32257,
        WGS72_UTM_zone_58N = 32258,
        WGS72_UTM_zone_59N = 32259,
        WGS72_UTM_zone_60N = 32260,
        WGS72_UTM_zone_1S = 32301,
        WGS72_UTM_zone_2S = 32302,
        WGS72_UTM_zone_3S = 32303,
        WGS72_UTM_zone_4S = 32304,
        WGS72_UTM_zone_5S = 32305,
        WGS72_UTM_zone_6S = 32306,
        WGS72_UTM_zone_7S = 32307,
        WGS72_UTM_zone_8S = 32308,
        WGS72_UTM_zone_9S = 32309,
        WGS72_UTM_zone_10S = 32310,
        WGS72_UTM_zone_11S = 32311,
        WGS72_UTM_zone_12S = 32312,
        WGS72_UTM_zone_13S = 32313,
        WGS72_UTM_zone_14S = 32314,
        WGS72_UTM_zone_15S = 32315,
        WGS72_UTM_zone_16S = 32316,
        WGS72_UTM_zone_17S = 32317,
        WGS72_UTM_zone_18S = 32318,
        WGS72_UTM_zone_19S = 32319,
        WGS72_UTM_zone_20S = 32320,
        WGS72_UTM_zone_21S = 32321,
        WGS72_UTM_zone_22S = 32322,
        WGS72_UTM_zone_23S = 32323,
        WGS72_UTM_zone_24S = 32324,
        WGS72_UTM_zone_25S = 32325,
        WGS72_UTM_zone_26S = 32326,
        WGS72_UTM_zone_27S = 32327,
        WGS72_UTM_zone_28S = 32328,
        WGS72_UTM_zone_29S = 32329,
        WGS72_UTM_zone_30S = 32330,
        WGS72_UTM_zone_31S = 32331,
        WGS72_UTM_zone_32S = 32332,
        WGS72_UTM_zone_33S = 32333,
        WGS72_UTM_zone_34S = 32334,
        WGS72_UTM_zone_35S = 32335,
        WGS72_UTM_zone_36S = 32336,
        WGS72_UTM_zone_37S = 32337,
        WGS72_UTM_zone_38S = 32338,
        WGS72_UTM_zone_39S = 32339,
        WGS72_UTM_zone_40S = 32340,
        WGS72_UTM_zone_41S = 32341,
        WGS72_UTM_zone_42S = 32342,
        WGS72_UTM_zone_43S = 32343,
        WGS72_UTM_zone_44S = 32344,
        WGS72_UTM_zone_45S = 32345,
        WGS72_UTM_zone_46S = 32346,
        WGS72_UTM_zone_47S = 32347,
        WGS72_UTM_zone_48S = 32348,
        WGS72_UTM_zone_49S = 32349,
        WGS72_UTM_zone_50S = 32350,
        WGS72_UTM_zone_51S = 32351,
        WGS72_UTM_zone_52S = 32352,
        WGS72_UTM_zone_53S = 32353,
        WGS72_UTM_zone_54S = 32354,
        WGS72_UTM_zone_55S = 32355,
        WGS72_UTM_zone_56S = 32356,
        WGS72_UTM_zone_57S = 32357,
        WGS72_UTM_zone_58S = 32358,
        WGS72_UTM_zone_59S = 32359,
        WGS72_UTM_zone_60S = 32360,
        WGS72BE_UTM_zone_1N = 32401,
        WGS72BE_UTM_zone_2N = 32402,
        WGS72BE_UTM_zone_3N = 32403,
        WGS72BE_UTM_zone_4N = 32404,
        WGS72BE_UTM_zone_5N = 32405,
        WGS72BE_UTM_zone_6N = 32406,
        WGS72BE_UTM_zone_7N = 32407,
        WGS72BE_UTM_zone_8N = 32408,
        WGS72BE_UTM_zone_9N = 32409,
        WGS72BE_UTM_zone_10N = 32410,
        WGS72BE_UTM_zone_11N = 32411,
        WGS72BE_UTM_zone_12N = 32412,
        WGS72BE_UTM_zone_13N = 32413,
        WGS72BE_UTM_zone_14N = 32414,
        WGS72BE_UTM_zone_15N = 32415,
        WGS72BE_UTM_zone_16N = 32416,
        WGS72BE_UTM_zone_17N = 32417,
        WGS72BE_UTM_zone_18N = 32418,
        WGS72BE_UTM_zone_19N = 32419,
        WGS72BE_UTM_zone_20N = 32420,
        WGS72BE_UTM_zone_21N = 32421,
        WGS72BE_UTM_zone_22N = 32422,
        WGS72BE_UTM_zone_23N = 32423,
        WGS72BE_UTM_zone_24N = 32424,
        WGS72BE_UTM_zone_25N = 32425,
        WGS72BE_UTM_zone_26N = 32426,
        WGS72BE_UTM_zone_27N = 32427,
        WGS72BE_UTM_zone_28N = 32428,
        WGS72BE_UTM_zone_29N = 32429,
        WGS72BE_UTM_zone_30N = 32430,
        WGS72BE_UTM_zone_31N = 32431,
        WGS72BE_UTM_zone_32N = 32432,
        WGS72BE_UTM_zone_33N = 32433,
        WGS72BE_UTM_zone_34N = 32434,
        WGS72BE_UTM_zone_35N = 32435,
        WGS72BE_UTM_zone_36N = 32436,
        WGS72BE_UTM_zone_37N = 32437,
        WGS72BE_UTM_zone_38N = 32438,
        WGS72BE_UTM_zone_39N = 32439,
        WGS72BE_UTM_zone_40N = 32440,
        WGS72BE_UTM_zone_41N = 32441,
        WGS72BE_UTM_zone_42N = 32442,
        WGS72BE_UTM_zone_43N = 32443,
        WGS72BE_UTM_zone_44N = 32444,
        WGS72BE_UTM_zone_45N = 32445,
        WGS72BE_UTM_zone_46N = 32446,
        WGS72BE_UTM_zone_47N = 32447,
        WGS72BE_UTM_zone_48N = 32448,
        WGS72BE_UTM_zone_49N = 32449,
        WGS72BE_UTM_zone_50N = 32450,
        WGS72BE_UTM_zone_51N = 32451,
        WGS72BE_UTM_zone_52N = 32452,
        WGS72BE_UTM_zone_53N = 32453,
        WGS72BE_UTM_zone_54N = 32454,
        WGS72BE_UTM_zone_55N = 32455,
        WGS72BE_UTM_zone_56N = 32456,
        WGS72BE_UTM_zone_57N = 32457,
        WGS72BE_UTM_zone_58N = 32458,
        WGS72BE_UTM_zone_59N = 32459,
        WGS72BE_UTM_zone_60N = 32460,
        WGS72BE_UTM_zone_1S = 32501,
        WGS72BE_UTM_zone_2S = 32502,
        WGS72BE_UTM_zone_3S = 32503,
        WGS72BE_UTM_zone_4S = 32504,
        WGS72BE_UTM_zone_5S = 32505,
        WGS72BE_UTM_zone_6S = 32506,
        WGS72BE_UTM_zone_7S = 32507,
        WGS72BE_UTM_zone_8S = 32508,
        WGS72BE_UTM_zone_9S = 32509,
        WGS72BE_UTM_zone_10S = 32510,
        WGS72BE_UTM_zone_11S = 32511,
        WGS72BE_UTM_zone_12S = 32512,
        WGS72BE_UTM_zone_13S = 32513,
        WGS72BE_UTM_zone_14S = 32514,
        WGS72BE_UTM_zone_15S = 32515,
        WGS72BE_UTM_zone_16S = 32516,
        WGS72BE_UTM_zone_17S = 32517,
        WGS72BE_UTM_zone_18S = 32518,
        WGS72BE_UTM_zone_19S = 32519,
        WGS72BE_UTM_zone_20S = 32520,
        WGS72BE_UTM_zone_21S = 32521,
        WGS72BE_UTM_zone_22S = 32522,
        WGS72BE_UTM_zone_23S = 32523,
        WGS72BE_UTM_zone_24S = 32524,
        WGS72BE_UTM_zone_25S = 32525,
        WGS72BE_UTM_zone_26S = 32526,
        WGS72BE_UTM_zone_27S = 32527,
        WGS72BE_UTM_zone_28S = 32528,
        WGS72BE_UTM_zone_29S = 32529,
        WGS72BE_UTM_zone_30S = 32530,
        WGS72BE_UTM_zone_31S = 32531,
        WGS72BE_UTM_zone_32S = 32532,
        WGS72BE_UTM_zone_33S = 32533,
        WGS72BE_UTM_zone_34S = 32534,
        WGS72BE_UTM_zone_35S = 32535,
        WGS72BE_UTM_zone_36S = 32536,
        WGS72BE_UTM_zone_37S = 32537,
        WGS72BE_UTM_zone_38S = 32538,
        WGS72BE_UTM_zone_39S = 32539,
        WGS72BE_UTM_zone_40S = 32540,
        WGS72BE_UTM_zone_41S = 32541,
        WGS72BE_UTM_zone_42S = 32542,
        WGS72BE_UTM_zone_43S = 32543,
        WGS72BE_UTM_zone_44S = 32544,
        WGS72BE_UTM_zone_45S = 32545,
        WGS72BE_UTM_zone_46S = 32546,
        WGS72BE_UTM_zone_47S = 32547,
        WGS72BE_UTM_zone_48S = 32548,
        WGS72BE_UTM_zone_49S = 32549,
        WGS72BE_UTM_zone_50S = 32550,
        WGS72BE_UTM_zone_51S = 32551,
        WGS72BE_UTM_zone_52S = 32552,
        WGS72BE_UTM_zone_53S = 32553,
        WGS72BE_UTM_zone_54S = 32554,
        WGS72BE_UTM_zone_55S = 32555,
        WGS72BE_UTM_zone_56S = 32556,
        WGS72BE_UTM_zone_57S = 32557,
        WGS72BE_UTM_zone_58S = 32558,
        WGS72BE_UTM_zone_59S = 32559,
        WGS72BE_UTM_zone_60S = 32560,
        WGS84_UTM_zone_1N = 32601,
        WGS84_UTM_zone_2N = 32602,
        WGS84_UTM_zone_3N = 32603,
        WGS84_UTM_zone_4N = 32604,
        WGS84_UTM_zone_5N = 32605,
        WGS84_UTM_zone_6N = 32606,
        WGS84_UTM_zone_7N = 32607,
        WGS84_UTM_zone_8N = 32608,
        WGS84_UTM_zone_9N = 32609,
        WGS84_UTM_zone_10N = 32610,
        WGS84_UTM_zone_11N = 32611,
        WGS84_UTM_zone_12N = 32612,
        WGS84_UTM_zone_13N = 32613,
        WGS84_UTM_zone_14N = 32614,
        WGS84_UTM_zone_15N = 32615,
        WGS84_UTM_zone_16N = 32616,
        WGS84_UTM_zone_17N = 32617,
        WGS84_UTM_zone_18N = 32618,
        WGS84_UTM_zone_19N = 32619,
        WGS84_UTM_zone_20N = 32620,
        WGS84_UTM_zone_21N = 32621,
        WGS84_UTM_zone_22N = 32622,
        WGS84_UTM_zone_23N = 32623,
        WGS84_UTM_zone_24N = 32624,
        WGS84_UTM_zone_25N = 32625,
        WGS84_UTM_zone_26N = 32626,
        WGS84_UTM_zone_27N = 32627,
        WGS84_UTM_zone_28N = 32628,
        WGS84_UTM_zone_29N = 32629,
        WGS84_UTM_zone_30N = 32630,
        WGS84_UTM_zone_31N = 32631,
        WGS84_UTM_zone_32N = 32632,
        WGS84_UTM_zone_33N = 32633,
        WGS84_UTM_zone_34N = 32634,
        WGS84_UTM_zone_35N = 32635,
        WGS84_UTM_zone_36N = 32636,
        WGS84_UTM_zone_37N = 32637,
        WGS84_UTM_zone_38N = 32638,
        WGS84_UTM_zone_39N = 32639,
        WGS84_UTM_zone_40N = 32640,
        WGS84_UTM_zone_41N = 32641,
        WGS84_UTM_zone_42N = 32642,
        WGS84_UTM_zone_43N = 32643,
        WGS84_UTM_zone_44N = 32644,
        WGS84_UTM_zone_45N = 32645,
        WGS84_UTM_zone_46N = 32646,
        WGS84_UTM_zone_47N = 32647,
        WGS84_UTM_zone_48N = 32648,
        WGS84_UTM_zone_49N = 32649,
        WGS84_UTM_zone_50N = 32650,
        WGS84_UTM_zone_51N = 32651,
        WGS84_UTM_zone_52N = 32652,
        WGS84_UTM_zone_53N = 32653,
        WGS84_UTM_zone_54N = 32654,
        WGS84_UTM_zone_55N = 32655,
        WGS84_UTM_zone_56N = 32656,
        WGS84_UTM_zone_57N = 32657,
        WGS84_UTM_zone_58N = 32658,
        WGS84_UTM_zone_59N = 32659,
        WGS84_UTM_zone_60N = 32660,
        WGS84_UTM_zone_1S = 32701,
        WGS84_UTM_zone_2S = 32702,
        WGS84_UTM_zone_3S = 32703,
        WGS84_UTM_zone_4S = 32704,
        WGS84_UTM_zone_5S = 32705,
        WGS84_UTM_zone_6S = 32706,
        WGS84_UTM_zone_7S = 32707,
        WGS84_UTM_zone_8S = 32708,
        WGS84_UTM_zone_9S = 32709,
        WGS84_UTM_zone_10S = 32710,
        WGS84_UTM_zone_11S = 32711,
        WGS84_UTM_zone_12S = 32712,
        WGS84_UTM_zone_13S = 32713,
        WGS84_UTM_zone_14S = 32714,
        WGS84_UTM_zone_15S = 32715,
        WGS84_UTM_zone_16S = 32716,
        WGS84_UTM_zone_17S = 32717,
        WGS84_UTM_zone_18S = 32718,
        WGS84_UTM_zone_19S = 32719,
        WGS84_UTM_zone_20S = 32720,
        WGS84_UTM_zone_21S = 32721,
        WGS84_UTM_zone_22S = 32722,
        WGS84_UTM_zone_23S = 32723,
        WGS84_UTM_zone_24S = 32724,
        WGS84_UTM_zone_25S = 32725,
        WGS84_UTM_zone_26S = 32726,
        WGS84_UTM_zone_27S = 32727,
        WGS84_UTM_zone_28S = 32728,
        WGS84_UTM_zone_29S = 32729,
        WGS84_UTM_zone_30S = 32730,
        WGS84_UTM_zone_31S = 32731,
        WGS84_UTM_zone_32S = 32732,
        WGS84_UTM_zone_33S = 32733,
        WGS84_UTM_zone_34S = 32734,
        WGS84_UTM_zone_35S = 32735,
        WGS84_UTM_zone_36S = 32736,
        WGS84_UTM_zone_37S = 32737,
        WGS84_UTM_zone_38S = 32738,
        WGS84_UTM_zone_39S = 32739,
        WGS84_UTM_zone_40S = 32740,
        WGS84_UTM_zone_41S = 32741,
        WGS84_UTM_zone_42S = 32742,
        WGS84_UTM_zone_43S = 32743,
        WGS84_UTM_zone_44S = 32744,
        WGS84_UTM_zone_45S = 32745,
        WGS84_UTM_zone_46S = 32746,
        WGS84_UTM_zone_47S = 32747,
        WGS84_UTM_zone_48S = 32748,
        WGS84_UTM_zone_49S = 32749,
        WGS84_UTM_zone_50S = 32750,
        WGS84_UTM_zone_51S = 32751,
        WGS84_UTM_zone_52S = 32752,
        WGS84_UTM_zone_53S = 32753,
        WGS84_UTM_zone_54S = 32754,
        WGS84_UTM_zone_55S = 32755,
        WGS84_UTM_zone_56S = 32756,
        WGS84_UTM_zone_57S = 32757,
        WGS84_UTM_zone_58S = 32758,
        WGS84_UTM_zone_59S = 32759,
        WGS84_UTM_zone_60S = 32760
    }
}