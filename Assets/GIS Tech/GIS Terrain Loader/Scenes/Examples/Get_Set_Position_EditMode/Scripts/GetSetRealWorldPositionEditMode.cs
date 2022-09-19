using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using GISTech.GISTerrainLoader;

// This Example show you how to use GTL to get and set a real world position in edit mode
#if UNITY_EDITOR
[CustomEditor(typeof(GetSetRealWorldPosition))]
public class GetSetRealWorldPositionEditMode : Editor
{
    public override void OnInspectorGUI()
    {
        GetSetRealWorldPosition editorGetSetPosition = (GetSetRealWorldPosition)target;

        if (GUILayout.Button("Get Terrain Data"))
        {
            editorGetSetPosition.Player = GameObject.Find("Player");
            editorGetSetPosition.GetTerrainData();

        }

        if (GUILayout.Button("Set Position"))
        {
            editorGetSetPosition.LatLonPos = new DVector2(2.72289517819244, 33.8634254169107);

            if(editorGetSetPosition.Player)
            {
                editorGetSetPosition.SetPosition();
            }
 

        }
        if (GUILayout.Button("Get Position"))
        {
            if (editorGetSetPosition.Player)
            {
                editorGetSetPosition.GetPosition();
            }
        }
    }
}
#endif