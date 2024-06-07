using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UNIHper;
using com.rfilkov.kinect;
using UniRx;
using System;
using System.Linq;

using Newtonsoft.Json;
using com.rfilkov.components;

using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;

public class TrackingInfo
{
    public int TrackedBodyCount = 0;
    public bool PrimaryUserTracked = false;
}

public class LimitArea
{
    [XmlAttribute]
    public float minUserDistance = 0;

    [XmlAttribute]
    public float maxUserDistance = 0;

    [XmlAttribute]
    public float maxLeftRightDistance = 0;
}

public class PreviewTexture
{
    [XmlAttribute]
    public int MarginLeft = 0;

    [XmlAttribute]
    public int MarginTop = 0;

    [XmlAttribute]
    public int MarginRight = 0;

    [XmlAttribute]
    public int MarginBottom = 0;

    [XmlAttribute]
    public int Type = 0;
}

public class BaseSettings
{
    [XmlAttribute]
    public bool Enabled = false;
}

public class TrackingDataSettings
{
    [XmlAttribute]
    public int Port = 10010;
}

// 手指选择设置
public class HandSettings : BaseSettings
{
    [XmlAttribute]
    public bool EnableServer = false;

    [XmlAttribute]
    public int Port = 20000;

    [XmlAttribute]
    public bool ShowIcon = true;

    [XmlAttribute]
    public float HorizontalResolution = 1920;

    [XmlAttribute]
    public float VerticalResolution = 1080;

    [XmlAttribute]
    public bool FlipX = false;

    [XmlAttribute]
    public bool FlipY = false;

    public Vector3 CaculateHandPosition(Vector3 screenPos)
    {
        screenPos.x = FlipX ? 1 - screenPos.x : screenPos.x;
        screenPos.y = FlipY ? 1 - screenPos.y : screenPos.y;
        screenPos.x *= HorizontalResolution;
        screenPos.y *= VerticalResolution;
        return screenPos;
    }
}

public class AvatarSettings : BaseSettings
{
    [XmlAttribute]
    public bool EnableServer = false;

    [XmlAttribute]
    public int Port = 20010;

    [XmlAttribute]
    public bool ShowIcon = false;

    [XmlAttribute]
    public int Size = 256;

    [XmlAttribute]
    public bool FlipX = false;

    [XmlAttribute]
    public bool FlipY = false;

    private Texture2D avatarTexture = null;

    [XmlIgnore]
    public Texture2D AvatarTexture
    {
        get
        {
            if (avatarTexture == null)
            {
                avatarTexture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            }
            return avatarTexture;
        }
    }
}

public class PoseSettings : BaseSettings
{
    [XmlAttribute]
    public bool EnableServer = false;

    [XmlAttribute]
    public int Port = 20020;

    [XmlArrayItem("Gesture")]
    public List<GestureType> DetectGestures = new List<GestureType>();
}

public class BodySettings : BaseSettings
{
    [XmlAttribute]
    public bool EnableServer = false;

    [XmlAttribute]
    public int Port = 20030;
}

[SerializedAt(AppPath.StreamingDir)]
public class KinectSettings : UConfig
{
    public string LocalIP = "127.0.0.1";

    public int QueryDataInterval = 50;

    public List<KinectManager.DisplayImageType> DisplayImageTypes =
        new List<KinectManager.DisplayImageType>();

    public float DisplayImageWidthPercent = 0.2f;

    public TrackingDataSettings TrackingDataSettings = new TrackingDataSettings();
    public HandSettings HandSettings = new HandSettings();
    public AvatarSettings AvatarSettings = new AvatarSettings();
    public PoseSettings PoseSettings = new PoseSettings();
    public BodySettings BodySettings = new BodySettings();

    [XmlElement("LimitArea")]
    public LimitArea LimitArea = new LimitArea();
    public PreviewTexture PreviewTexture = new PreviewTexture();

    public Vector2 GetColorImageScale(int sensorIndex)
    {
        return kinectManager.GetColorImageScale(sensorIndex);
    }

    // Write your comments here
    protected override string Comment()
    {
        return @"

        HandDataServerPort: 手势选择数据服务器端口
        PoseDataServerPort: 姿势数据服务器端口
        BodyDataServerPort: 骨骼结点数据服务器端口
        SendBodyDataInterval: 发送骨骼结点数据的间隔，单位毫秒

        DisplayImageWidthPercent: 显示图像宽度占屏幕宽度的百分比
        DisplayImageTypes: 显示图像类型
            None = 0,
            Sensor0ColorImage = 0x01,
            Sensor0DepthImage = 0x02,
            Sensor0InfraredImage = 0x03,
            Sensor1ColorImage = 0x11,
            Sensor1DepthImage = 0x12,
            Sensor1InfraredImage = 0x13,
            Sensor2ColorImage = 0x21,
            Sensor2DepthImage = 0x22,
            Sensor2InfraredImage = 0x23,
            UserBodyImageS0 = 0x101,
            UserBodyImageS1 = 0x102,
            UserBodyImageS2 = 0x103

        HandSettings: 手势选择设置
            HorizontalResolution: 水平分辨率
            VerticalResolution: 垂直分辨率
            FlipX: 是否水平翻转
            FlipY: 是否垂直翻转

        PoseSettings: 姿势检测设置
            DetectGestures: 需要检测的手势列表
                RaiseRightHand,
                RaiseLeftHand,
                Psi,
                Tpose,
                Stop,
                Wave,
                SwipeLeft,
                SwipeRight,
                SwipeUp,
                SwipeDown,
                ZoomIn,
                ZoomOut,
                Wheel,
                Jump,
                Squat,
                Push,
                Pull,
                ShoulderLeftFront,
                ShoulderRightFront,
                LeanLeft,
                LeanRight,
                LeanForward,
                LeanBack,
                KickLeft,
                KickRight,
                Run,

                RaisedRightHorizontalLeftHand,   // by Andrzej W
                RaisedLeftHorizontalRightHand,

                TouchRightElbow,   // suggested by Nayden N.
                TouchLeftElbow,

                MoveLeft,          // suggested by Indra Adi D. C
                MoveRight,

                Apose,             // suggested by avrxpert

        PreviewTexture:
            @type: int
            0: Body Texture
            1: Depth Texture
            2: Color Texture
            3: Infrared Texture
        ";
    }

    [XmlIgnore]
    public UTcpServer TrackingServer;

    [XmlIgnore]
    public UTcpServer HandDataServer;

    [XmlIgnore]
    public UTcpServer PoseDataServer;

    [XmlIgnore]
    public UTcpServer AvatarDataServer;

    [XmlIgnore]
    public UTcpServer BodyDataServer;

    ReactiveProperty<bool> primaryUserTracked = new ReactiveProperty<bool>(false);

    [XmlIgnore]
    public IObservable<bool> PrimaryUserTracked => primaryUserTracked;

    private KinectManager kinectManager => KinectManager.Instance;
    ulong primaryUserID => kinectManager.GetPrimaryUserID();

    private string logText = "";

    private Camera screenCamera;

    [Tooltip(
        "Interaction manager instance, used to detect hand interactions. If left empty, the component will try to find a proper interaction manager in the scene."
    )]
    [XmlIgnore]
    public InteractionManager interactionManager { get; private set; }

    [XmlIgnore]
    public ShowFaceImage faceTexListener { get; private set; }

    // Called once after the config data is loaded
    protected override void OnLoaded()
    {
        Debug.LogWarning("OnLoaded...");

        if (this.PoseSettings.DetectGestures.Count <= 0)
        {
            this.PoseSettings.DetectGestures.Add(GestureType.Wave);
            this.Serialize();
        }

        TrackingInfo trackingInfo = new TrackingInfo();
        Observable
            .EveryUpdate()
            .Where(_ => kinectManager && kinectManager.IsInitialized())
            .First()
            .Subscribe(_ =>
            {
                // 姿势检测设置
                var _simpleGestureListener =
                    GameObject.FindObjectOfType<AzureKinect_Util.SimpleGestureListener>();
                if (_simpleGestureListener != null)
                    _simpleGestureListener.detectGestures = PoseSettings.DetectGestures;
                else
                    Debug.LogWarning("SimpleGestureListener Not Found");

                faceTexListener = GameObject.FindObjectOfType<ShowFaceImage>();

                // 创建数据服务器
                createServers();

                kinectManager.displayImages = DisplayImageTypes;
                kinectManager.displayImageWidthPercent = DisplayImageWidthPercent;

                if (screenCamera == null)
                {
                    screenCamera = Camera.main;
                }

                // get the interaction manager instance
                if (interactionManager == null)
                {
                    //interactionManager = InteractionManager.Instance;
                    interactionManager = InteractionManager.GetInstance(0, true, true);
                }

                listenPoseEvent();
                Observable
                    .Interval(TimeSpan.FromMilliseconds(QueryDataInterval))
                    //.EveryUpdate()
                    .Subscribe(_ =>
                    {
                        var _primaryUserID = primaryUserID;
                        var _primaryUserIndex = kinectManager.GetUserIndexById(_primaryUserID);
                        primaryUserTracked.Value = kinectManager.IsUserTracked(_primaryUserID);

                        logText =
                            $"Primary User Tracked: {primaryUserTracked.Value}, ID: {_primaryUserID},  Index:{_primaryUserIndex} \n";
                        trackingInfo.PrimaryUserTracked = primaryUserTracked.Value;
                        TrackingServer.Send2Clients(
                            JsonConvert.SerializeObject(trackingInfo).ToUTF8Bytes()
                        );

                        if (!primaryUserTracked.Value)
                        {
                            Managements.UI.Get<HelpUI>()?.SetContent("No Primary User", 60);
                            return;
                        }

                        queryHandFrame();
                        logText += poseText + "\n";
                        queryAvatarFrame();
                        // queryAvatarFrame_cus();
                        queryBodyDataFrame();
                        Managements.UI.Get<HelpUI>()?.SetContent(logText, 60);
                    });
            });
    }

    private void createServers()
    {
        TrackingServer = Managements.Network
            .BuildTcpListener(LocalIP, TrackingDataSettings.Port, new StringMsgReceiver())
            .Listen();

        if (HandSettings.EnableServer)
            HandDataServer = Managements.Network
                .BuildTcpListener(LocalIP, HandSettings.Port, new StringMsgReceiver())
                .Listen();

        if (PoseSettings.EnableServer)
            PoseDataServer = Managements.Network
                .BuildTcpListener(LocalIP, PoseSettings.Port, new StringMsgReceiver())
                .Listen();

        if (AvatarSettings.EnableServer)
            AvatarDataServer = Managements.Network
                .BuildTcpListener(LocalIP, AvatarSettings.Port, new StringMsgReceiver())
                .Listen();

        if (BodySettings.EnableServer)
            BodyDataServer = Managements.Network
                .BuildTcpListener(LocalIP, BodySettings.Port, new StringMsgReceiver())
                .Listen();
    }

    protected override void OnUnloaded()
    {
        TrackingServer?.Dispose();
        HandDataServer?.Dispose();
        PoseDataServer?.Dispose();
        AvatarDataServer?.Dispose();
        BodyDataServer?.Dispose();
    }

    private string poseText = "";

    private void listenPoseEvent()
    {
        if (PoseSettings.Enabled == false)
            return;
        AzureKinect_Util.SimpleGestureListener simpleGestureListener =
            GameObject.FindObjectOfType<AzureKinect_Util.SimpleGestureListener>();
        simpleGestureListener
            .OnGestureUpdateAsObservable()
            .Subscribe(_gesture =>
            {
                if (PoseSettings.EnableServer)
                {
                    poseText = $"Gesture: {_gesture.ToString()}\n";
                    try
                    {
                        var _gestureJson = _gesture.ToJson();
                        PoseDataServer.Send2Clients(_gestureJson.ToUTF8Bytes());
                    }
                    catch (System.Exception)
                    {
                        Debug.LogWarning("Send PoseData Failed");
                    }
                }
            });
    }

    private int avatarSize => AvatarSettings.Size;

    private void queryAvatarFrame_cus()
    {
        if (AvatarSettings.Enabled == false)
            return;
        var _headPos = kinectManager.GetJointKinectPosition(
            kinectManager.GetPrimaryUserID(),
            (int)KinectInterop.JointType.Nose,
            true
        );
        var _headColorPos = kinectManager.MapSpacePointToColorCoords(0, _headPos);

        var _colorTex = kinectManager.GetColorImageTex(0) as Texture2D;

        _headColorPos.y = _colorTex.height - _headColorPos.y;
        _headColorPos.x = _colorTex.width - _headColorPos.x;
        _headColorPos.x -= avatarSize / 2;
        //_headColorPos.y -= avatarSize / 4;
        _headColorPos.x = Mathf.Clamp(_headColorPos.x, 0, _colorTex.width - avatarSize - 1);
        _headColorPos.y = Mathf.Clamp(_headColorPos.y, 0, _colorTex.height - avatarSize - 1);

        // crop _colorTex to _headPosColor square
        var _headPosColorX = (int)_headColorPos.x;
        var _headPosColorY = (int)_headColorPos.y;

        var _pixels = _colorTex.GetPixels(_headPosColorX, _headPosColorY, avatarSize, avatarSize);

        // flip y
        var _flippedPixels = new Color[avatarSize * avatarSize];
        for (int i = 0; i < avatarSize; i++)
        {
            for (int j = 0; j < avatarSize; j++)
            {
                _flippedPixels[i * avatarSize + j] = _pixels[(avatarSize - i - 1) * avatarSize + j];
            }
        }
        AvatarSettings.AvatarTexture.SetPixels(_flippedPixels);
        AvatarSettings.AvatarTexture.Apply();

        if (AvatarSettings.EnableServer)
        {
            try
            {
                var _bytes = AvatarSettings.AvatarTexture.GetRawTextureData();
                AvatarDataServer.Send2Clients(_bytes);
                Debug.LogWarning($"Avatar Texture Size: {_bytes.Count()}");
            }
            catch (System.Exception)
            {
                Debug.LogWarning("Send AvatarData Failed");
            }
        }
    }

    private Mat _avatarMat = null;
    private Texture2D _avatarTex = null;

    byte[] _avatarBytes = null;

    private void queryAvatarFrame()
    {
        if (AvatarSettings.Enabled == false)
            return;
        if (_avatarTex == null)
            _avatarTex = new Texture2D(avatarSize, avatarSize, TextureFormat.RGBA32, false);
        if (_avatarBytes == null)
            _avatarBytes = new byte[avatarSize * avatarSize * 4];

        var _faceTex = faceTexListener.GetFaceTex() as Texture2D;
        _avatarMat = new Mat(new Size(_faceTex.width, _faceTex.height), CvType.CV_8UC4);

        Utils.texture2DToMat(_faceTex, _avatarMat);
        Imgproc.resize(_avatarMat, _avatarMat, new Size(avatarSize, avatarSize));

        if (AvatarSettings.FlipX)
            Core.flip(_avatarMat, _avatarMat, 1);
        if (AvatarSettings.FlipY)
            Core.flip(_avatarMat, _avatarMat, 0);

        Utils.matToTexture2D(_avatarMat, AvatarSettings.AvatarTexture, false, 0, true);
        if (AvatarSettings.EnableServer)
        {
            try
            {
                _avatarMat.get(0, 0, _avatarBytes);
                // _avatarBytes = AvatarSettings.AvatarTexture.GetRawTextureData();
                // Debug.LogWarning($"length: {_avatarBytes.Length}");
                AvatarDataServer.Send2Clients(_avatarBytes);
            }
            catch (System.Exception)
            {
                Debug.LogWarning("Send AvatarData Failed");
            }
        }
        _avatarMat.release();
        _avatarMat.Dispose();
    }

    [XmlIgnore]
    public ReactiveProperty<Vector2> HandPosition = new ReactiveProperty<Vector2>(Vector2.one * -1);

    private void queryHandFrame()
    {
        if (HandSettings.Enabled == false)
            return;

        var _uiPosition = HandSettings.CaculateHandPosition(interactionManager.GetCursorPosition());

        logText += $"Hand Position: {_uiPosition}\n";

        HandPosition.Value = _uiPosition;

        if (HandSettings.EnableServer)
        {
            try
            {
                var _uiPositionJson = JsonConvert.SerializeObject(_uiPosition);
                HandDataServer.Send2Clients(_uiPositionJson.ToUTF8Bytes());
            }
            catch (System.Exception)
            {
                Debug.LogWarning("Send HandData Failed");
            }
        }
    }

    private void queryBodyDataFrame()
    {
        if (!BodySettings.EnableServer)
            return;

        var _bodyData = kinectManager.GetUserBodyData(primaryUserID);

        var _jointData = _bodyData.joint.ToList();
        var _jointDict = _jointData.ToDictionary(_ => _.jointType, _ => _);

        try
        {
            var _bodyJsonData = JsonConvert.SerializeObject(_jointDict);
            BodyDataServer.Send2Clients(_bodyJsonData.ToUTF8Bytes());
        }
        catch (System.Exception)
        {
            Debug.LogWarning("Send BodyData Failed");
        }
    }
}
