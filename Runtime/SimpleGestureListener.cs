using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using com.rfilkov.kinect;
using UniRx;
using UNIHper;
using Newtonsoft.Json;

namespace AzureKinect_Util
{
    public enum GestureEventType
    {
        GestureDetected = 0,
        GestureProgress,
        GestureCancelled
    }

    public class Gesture
    {
        public string GestureName => GestureType.ToString();
        public GestureType GestureType;
        public string GestureEventName => EventType.ToString();
        public GestureEventType EventType;
        public float Progress = 0;

        public override string ToString()
        {
            return $"GestureType: {GestureType}, EventType: {EventType}, Progress: {Progress}";
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    /// <summary>
    /// Simple gesture listener that only displays the status and progress of the given gestures.
    /// </summary>
    public class SimpleGestureListener : MonoBehaviour, GestureListenerInterface
    {
        private ReactiveCommand<Gesture> gestureUpdate = new ReactiveCommand<Gesture>();

        public IObservable<Gesture> OnGestureUpdateAsObservable()
        {
            return gestureUpdate.AsObservable();
        }

        [SerializeField, Tooltip("List of the gestures to detect.")]
        public List<GestureType> detectGestures = new List<GestureType>();

        // private bool to track if progress message has been displayed
        private bool progressDisplayed;
        private float progressGestureTime;

        private ulong primaryUserID => KinectManager.Instance.GetPrimaryUserID();

        // invoked when a new user is detected
        public void UserDetected(ulong userId, int userIndex)
        {
            if (userId == primaryUserID)
            {
                // as an example - detect these user specific gestures
                KinectGestureManager gestureManager = KinectManager.Instance.gestureManager;

                foreach (GestureType gesture in detectGestures)
                {
                    gestureManager.DetectGesture(userId, gesture);
                }
            }
        }

        // invoked when the user is lost
        public void UserLost(ulong userId, int userIndex)
        {
            if (userId != primaryUserID)
                return;
        }

        // invoked to report gesture progress. important for the continuous gestures, because they never complete.
        public void GestureInProgress(
            ulong userId,
            int userIndex,
            GestureType gesture,
            float progress,
            KinectInterop.JointType joint,
            Vector3 screenPos
        )
        {
            if (userId != primaryUserID)
                return;
            Debug.LogWarning($"Gesture {gesture} progress: {progress}");

            if (progress > 0.4f)
            {
                var _gesture = new Gesture()
                {
                    GestureType = gesture,
                    Progress = progress,
                    EventType = GestureEventType.GestureProgress
                };

                gestureUpdate.Execute(_gesture);
            }
        }

        // invoked when a (discrete) gesture is complete.
        public bool GestureCompleted(
            ulong userId,
            int userIndex,
            GestureType gesture,
            KinectInterop.JointType joint,
            Vector3 screenPos
        )
        {
            if (userId != primaryUserID)
                return false;

            var _gesture = new Gesture()
            {
                GestureType = gesture,
                EventType = GestureEventType.GestureDetected
            };
            gestureUpdate.Execute(_gesture);

            // UpdateGesture(gesture).Detected = true;
            // NotifyGestureUpdate(gesture);
            Debug.LogWarning($"Gesture {gesture} completed.");
            return true;
        }

        // invoked when a gesture gets cancelled by the user
        public bool GestureCancelled(
            ulong userId,
            int userIndex,
            GestureType gesture,
            KinectInterop.JointType joint
        )
        {
            if (userId != primaryUserID)
                return false;

            var _gesture = new Gesture()
            {
                GestureType = gesture,
                EventType = GestureEventType.GestureCancelled
            };
            gestureUpdate.Execute(_gesture);

            Debug.LogWarning($"Gesture {gesture} cancelled.");

            return true;
        }

        public void Update() { }
    }
}
