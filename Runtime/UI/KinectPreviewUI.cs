using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UNIHper;
using UNIHper.UI;
using com.rfilkov.kinect;
using UniRx;
using UnityEngine.InputSystem;
using MPUIKIT;
using Michsky.MUIP;
using UnityEngine.Events;

[UIPage(Asset = "KinectPreviewUI", Type = UIType.Popup)]
public class KinectPreviewUI : UIBase
{
    public UnityEvent<KinectSettings> OnResizePreviewTexture = new UnityEvent<KinectSettings>();

    public IObservable<KinectSettings> OnResizePreviewTextureAsObservable() =>
        OnResizePreviewTexture.AsObservable();

    KinectManager kinectManager => KinectManager.Instance;

    // Start is called before the first frame update
    private void Start()
    {
        // this.Get<RawImage>("preview_rect/tex_depth").texture = kinectManager.GetDepthImageTex(0); //深度图
        // this.Get<RawImage>("preview_rect/tex_depth").texture = kinectManager.GetColorImageTex(0); //彩色图
        // this.Get<RawImage>("preview_rect/tex_depth").texture = kinectManager.GetInfraredImageTex(0); //红外图
        this.Get<RawImage>("preview_rect/tex_depth").texture = kinectManager.GetUsersImageTex(); //
        this.Get<MPImage>("preview_rect/outer").color = Color.red;

        kinectManager
            .OnUserAddedAsObservable()
            .Subscribe(_ =>
            {
                this.Get<MPImage>("preview_rect/outer").color = Color.green;
            })
            .AddTo(this);

        kinectManager
            .OnUserRemovedAsObservable()
            .Subscribe(_ =>
            {
                if (kinectManager.GetTrackedBodyIndices().Count <= 0)
                    this.Get<MPImage>("preview_rect/outer").color = Color.red;
            })
            .AddTo(this);

        var _kinectSettings = Managements.Config.Get<KinectSettings>();
        // range distance input
        var _rangeSlider = this.Get<RangeSlider>("layout_limit/slider_distance");
        _rangeSlider.minSlider.value = _kinectSettings.LimitArea.minUserDistance;
        _rangeSlider.maxSlider.value = _kinectSettings.LimitArea.maxUserDistance;
        _rangeSlider.minSlider
            .OnValueChangedAsObservable()
            .Subscribe(_ =>
            {
                _kinectSettings.LimitArea.minUserDistance = _;
                reloadSettings();
            });
        _rangeSlider.maxSlider
            .OnValueChangedAsObservable()
            .Subscribe(_ =>
            {
                _kinectSettings.LimitArea.maxUserDistance = 9.9f - _;
                reloadSettings();
            });

        this.Get<SliderManager>("layout_limit/slider_lr_max").mainSlider.value = _kinectSettings
            .LimitArea
            .maxLeftRightDistance;
        this.Get<SliderManager>("layout_limit/slider_lr_max")
            .mainSlider.onValueChanged.AsObservable()
            .Subscribe(_ =>
            {
                _kinectSettings.LimitArea.maxLeftRightDistance = _;
                reloadSettings();
            });

        showPreviewTexture(_kinectSettings.PreviewTexture.Type);
        this.Get<CustomDropdown>("drop_textureTypes")
            .onValueChanged.AsObservable()
            .Subscribe(_ =>
            {
                showPreviewTexture(_);
                _kinectSettings.PreviewTexture.Type = _;
            });

        reloadSettings();
    }

    private void reloadSettings()
    {
        var _kinectSettings = Managements.Config.Get<KinectSettings>();
        kinectManager.minUserDistance = _kinectSettings.LimitArea.minUserDistance;
        kinectManager.maxUserDistance = _kinectSettings.LimitArea.maxUserDistance;
        kinectManager.maxLeftRightDistance = _kinectSettings.LimitArea.maxLeftRightDistance;
    }

    private void showPreviewTexture(int textureID)
    {
        var _rawImage = this.Get<RawImage>("preview_rect/tex_depth");
        switch (textureID)
        {
            case 0:
                _rawImage.texture = kinectManager.GetUsersImageTex(); //
                break;
            case 1:
                _rawImage.texture = kinectManager.GetDepthImageTex(0); //深度图
                break;
            case 2:
                _rawImage.texture = kinectManager.GetColorImageTex(0); //彩色图
                break;
            case 3:
                _rawImage.texture = kinectManager.GetInfraredImageTex(0); //红外图
                break;
            default:
                _rawImage.texture = kinectManager.GetUsersImageTex();
                break;
        }
    }

    Texture2D texDepth2D = null;

    // Update is called once per frame
    private void Update()
    {
        if (!kinectManager.IsInitialized())
            return;

        this.Get<RawImage>("preview_rect/tex_depth").color = kinectManager.GetUserFadeColor(
            Managements.Config.Get<KinectSettings>().LimitArea.maxLeftRightDistance
        );

        // if (
        //     kinectManager && kinectManager.IsInitialized()
        // /**&& depthImage.sprite == null*/)
        // {
        //     Texture texDepth = kinectManager.GetUsersImageTex();

        //     if (texDepth != null)
        //     {
        //         if (texDepth2D == null && texDepth != null && sensorData != null)
        //         {
        //             texDepth2D = new Texture2D(
        //                 texDepth.width,
        //                 texDepth.height,
        //                 TextureFormat.ARGB32,
        //                 false
        //             );
        //         }

        //         if (texDepth2D != null)
        //         {
        //             Graphics.CopyTexture(texDepth, texDepth2D);
        //             this.Get<RawImage>("texture_depth").texture = texDepth2D;
        //         }
        //     }
        // }
    }

    RectTransform previewRect;

    // Called when this ui is loaded
    protected override void OnLoaded()
    {
        previewRect = this.Get<RectTransform>("preview_rect");
        enablePreviewResize();

        Observable
            .EveryUpdate()
            .Subscribe(_ =>
            {
                if (keyboard.f5Key.wasPressedThisFrame)
                {
                    this.Toggle();
                }
            })
            .AddTo(this);
        reloadSettings();
    }

    Keyboard keyboard => Keyboard.current;

    private void enablePreviewResize()
    {
        var _kinectSettings = Managements.Config.Get<KinectSettings>();

        previewRect.offsetMin = new Vector2(
            _kinectSettings.PreviewTexture.MarginLeft,
            _kinectSettings.PreviewTexture.MarginBottom
        );
        previewRect.offsetMax = new Vector2(
            -_kinectSettings.PreviewTexture.MarginRight,
            -_kinectSettings.PreviewTexture.MarginTop
        );

        Observable
            .EveryUpdate()
            .Where(_ => previewRect.gameObject.activeInHierarchy)
            .Subscribe(_ =>
            {
                bool _isDirty = false;
                if (keyboard.numpadPlusKey.isPressed)
                {
                    if (keyboard.leftArrowKey.isPressed)
                    {
                        previewRect.offsetMin += new Vector2(1, 0);
                        _isDirty = true;
                    }
                    else if (keyboard.rightArrowKey.isPressed)
                    {
                        previewRect.offsetMax += new Vector2(-1, 0);
                        _isDirty = true;
                    }
                    else if (keyboard.upArrowKey.isPressed)
                    {
                        previewRect.offsetMax += new Vector2(0, -1);
                        _isDirty = true;
                    }
                    else if (keyboard.downArrowKey.isPressed)
                    {
                        previewRect.offsetMin += new Vector2(0, 1);
                        _isDirty = true;
                    }
                }
                if (keyboard.numpadMinusKey.isPressed)
                {
                    if (keyboard.leftArrowKey.isPressed)
                    {
                        previewRect.offsetMin += new Vector2(-1, 0);
                        _isDirty = true;
                    }
                    else if (keyboard.rightArrowKey.isPressed)
                    {
                        previewRect.offsetMax += new Vector2(1, 0);
                        _isDirty = true;
                    }
                    else if (keyboard.upArrowKey.isPressed)
                    {
                        previewRect.offsetMax += new Vector2(0, 1);
                        _isDirty = true;
                    }
                    else if (keyboard.downArrowKey.isPressed)
                    {
                        previewRect.offsetMin += new Vector2(0, -1);
                        _isDirty = true;
                    }
                }

                if (!_isDirty)
                    return;

                // Clamp preview rect
                var _offsetMin = previewRect.offsetMin;
                var _offsetMax = previewRect.offsetMax;
                _offsetMin.x = Mathf.Clamp(_offsetMin.x, 0, Screen.width - _offsetMax.x - 100);
                _offsetMin.y = Mathf.Clamp(_offsetMin.y, 0, Screen.height - _offsetMax.y - 100);

                _offsetMax.x = Mathf.Clamp(_offsetMax.x, -Screen.width + _offsetMin.x + 100, 0);
                _offsetMax.y = Mathf.Clamp(_offsetMax.y, -Screen.height + _offsetMin.y + 100, 0);

                previewRect.offsetMin = _offsetMin;
                previewRect.offsetMax = _offsetMax;

                _kinectSettings.PreviewTexture.MarginLeft = (int)previewRect.offsetMin.x;
                _kinectSettings.PreviewTexture.MarginBottom = (int)previewRect.offsetMin.y;
                _kinectSettings.PreviewTexture.MarginRight = -(int)previewRect.offsetMax.x;
                _kinectSettings.PreviewTexture.MarginTop = -(int)previewRect.offsetMax.y;

                OnResizePreviewTexture?.Invoke(_kinectSettings);
            })
            .AddTo(this);
    }

    // Called when this ui is shown
    protected override void OnShown() { }

    // Called when this ui is hidden
    protected override void OnHidden() { }

    void OnDestroy() { }
}
