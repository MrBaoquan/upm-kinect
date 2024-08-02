// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using UNIHper;
// using DNHper;
// using com.rfilkov.kinect;
// using UniRx;
// using System;

// public class GameUI : UIBase
// {
//     public void UpdateHandPosition(Vector3 _position)
//     {
//         this.hand.position = _position;
//     }

//     KinectSettings kinectSettings => Managements.Config.Get<KinectSettings>();

//     // Start is called before the first frame update
//     private void Start() { }

//     // Update is called once per frame
//     private void Update() { }

//     Transform hand = null;
//     Transform avatar = null;

//     // Called when this ui is loaded
//     protected override void OnLoaded()
//     {
//         this.hand = this.Get("hand");
//         this.avatar = this.Get("avatar");

//         this.hand.SetActive(false);
//         this.avatar.SetActive(false);

//         kinectSettings.PrimaryUserTracked.Subscribe(_tracked =>
//         {
//             this.hand.SetActive(_tracked && kinectSettings.HandSettings.ShowIcon);
//             this.avatar.SetActive(_tracked && kinectSettings.AvatarSettings.ShowIcon);

//             if (_tracked)
//             {
//                 var _avatarImage = this.Get<RawImage>("avatar");
//                 //_avatarImage.texture = kinectSettings.faceTexListener.GetFaceTex();
//                 _avatarImage.material.SetTexture(
//                     "_MainTex",
//                     kinectSettings.AvatarSettings.AvatarTexture
//                 );
//                 // _avatarImage.material.SetTextureScale(
//                 //     "_MainTex",
//                 //     kinectSettings.GetColorImageScale(0)
//                 // );
//             }
//         });
//         kinectSettings.HandPosition.Subscribe(_position =>
//         {
//             this.UpdateHandPosition(_position);
//         });
//     }

//     // Called when this ui is shown
//     protected override void OnShown() { }

//     // Called when this ui is hidden
//     protected override void OnHidden() { }
// }
