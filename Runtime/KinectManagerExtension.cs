using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.rfilkov.kinect;
using UniRx;
using System.Linq;

public static class KinectManagerExtension
{
    public static IObservable<Tuple<ulong, int>> OnUserAddedAsObservable(this KinectManager manager)
    {
        return manager.userManager.OnUserAdded.AsObservable();
    }

    public static IObservable<Tuple<ulong, int>> OnUserRemovedAsObservable(
        this KinectManager manager
    )
    {
        return manager.userManager.OnUserRemoved.AsObservable();
    }

    /// <summary>
    /// Get user fade alpha by user position
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="maxLeftRightDistance"></param>
    /// <param name="fadePercent"></param>
    /// <returns></returns>
    public static Color GetUserFadeColor(
        this KinectManager manager,
        float maxLeftRightDistance,
        float fadePercent = 0.4f
    )
    {
        if (maxLeftRightDistance <= 0)
            return Color.white;

        Color _color = Color.white;
        var _primaryID = manager.GetPrimaryUserID();
        var _userPos = manager.GetUserPosition(_primaryID);

        var _posX = Mathf.Abs(_userPos.x);
        var _alpha = 0f;

        var _fadeStartDistance = maxLeftRightDistance * (1 - fadePercent);
        if (_posX <= _fadeStartDistance)
        {
            _alpha = 1;
        }
        else
        {
            _alpha = 1 - (_posX - _fadeStartDistance) / (maxLeftRightDistance * fadePercent);
        }
        _color.a = _alpha;
        return _color;
    }

    public static int GetPrimaryUserIndex(this KinectManager manager)
    {
        var _userID = manager.GetPrimaryUserID();
        return manager.GetUserIndexById(_userID);
    }

    public static List<KinectInterop.JointData> GetPrimaryUserJointDatas(this KinectManager manager)
    {
        var _userIndex = manager.GetPrimaryUserIndex();
        return manager.GetSensorData(0).alTrackedBodies[_userIndex].joint.ToList();
    }

    public static KinectInterop.BodyData GetPrimaryUserBodyData(this KinectManager manager)
    {
        return manager.GetUserBodyData(manager.GetPrimaryUserID());
    }
}
