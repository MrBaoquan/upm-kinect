using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UNIHper;
using com.rfilkov.kinect;
using UniRx;
using UnityEditor;
using AzureKinect_Util;
using Newtonsoft.Json;
using System.Linq;
using System;

public class AzureKinectScript : SceneScriptBase
{
    void Awake()
    {
        Managements.Resource.AddConfig("AzureKinect_Util_resources");
        Managements.UI.AddConfig("AzureKinect_Util_uis");
    }

    // Called once after scene is loaded
    private void Start()
    {
        // Application.targetFrameRate = 30;
        // Managements.UI.Show<GameUI>();
    }

    // Called per frame after Start
    private void Update()
    {
        // Managements.UI.Get<HelpUI>().SetContent(_gestureText + "\r\n" + _poseText, 60);
    }

    // Called when scene is unloaded
    private void OnDestroy() { }

    // Called when application is quit
    private void OnApplicationQuit() { }
}
