using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UNIHper;
using com.rfilkov.kinect;
using UniRx;

public class AzureKinectSampleScript : SceneScriptBase
{
    private void Awake()
    {
        Managements.Resource.AddConfig("AzureKinect_Util_resources");
        Managements.UI.AddConfig("AzureKinect_Util_uis");
    }

    // Called once after scene is loaded
    private void Start()
    {
        Managements.UI.Show<KinectPreviewUI>();
        var _kinectManger = KinectManager.Instance;
        _kinectManger.userManager.OnUserAdded
            .AsObservable()
            .Subscribe(_ =>
            {
                Debug.LogWarning("User Added " + _.Item1);
            });

        _kinectManger.userManager.OnUserRemoved
            .AsObservable()
            .Subscribe(_ =>
            {
                Debug.LogWarning("User Removed " + _.Item1);
            });
    }

    // Called per frame after Start
    private void Update()
    {
        //Debug.Log(KinectManager.Instance.IsTrackedUsersLimited());
    }

    // Called when scene is unloaded
    private void OnDestroy() { }

    // Called when application is quit
    private void OnApplicationQuit() { }
}
