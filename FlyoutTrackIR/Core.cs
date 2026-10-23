using UnityEngine;
using MelonLoader;
using NaturalPoint.TrackIR;
using System.Collections;
using Harmony;
using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using GameObject = UnityEngine.GameObject;

[assembly: MelonInfo(typeof(FlyoutTrackIR.Core), "FlyoutTrackIR", "1.2.2", "HerrTom, Hackerkm", null)]
[assembly: MelonGame("Stonext Games", "Flyout")]

namespace FlyoutTrackIR
{
    public class Core : MelonMod
    {
        public static GameObject mainCamera;
        public static GameObject trackIRgameObject;

        public override void OnInitializeMelon()
        {
            ClassInjector.RegisterTypeInIl2Cpp<TrackIRComponent>();
            MelonLogger.Msg("Initialized FlyoutTrackIR.");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "PlanetScene2")
            {
                MelonCoroutines.Start(WaitForGameObjects());
            }

            base.OnSceneWasLoaded(buildIndex, sceneName);
        }
        
        private IEnumerator WaitForGameObjects()
        {
            Il2Cpp.Craft craft = null;

            // Wait until the object you want is found
            while (craft == null)
            {
                craft = UnityEngine.Object.FindObjectOfType<Il2Cpp.Craft>();
                if (craft == null)
                    // Yield until next frame to check again
                    yield return null;
            }
            // Once the object is found, execute main function

            // just making the track ir rig here
            trackIRgameObject = new GameObject("Track IR");
            trackIRgameObject.transform.parent = craft.transform.Find("_gfx/CamRig");
            
            CreateTrackIRCamera();
        }
        public void CreateTrackIRCamera()
        {
            mainCamera = GameObject.FindWithTag("MainCamera");

            if (trackIRgameObject == null)
            {
                MelonLogger.Error("Main camera not found.");
                return;
            }
            
            // Add TrackIRComponent to the trackir camera rig
            TrackIRComponent trackIRComponent = trackIRgameObject.gameObject.AddComponent<TrackIRComponent>();
            MelonLogger.Msg("Added TrackIRComponent to main camera!");

            // Set properties
            // trackIRComponent.AssignedApplicationId = 1000;
            // trackIRComponent.TrackingLostTimeoutSeconds = 3.0f;
            // trackIRComponent.TrackingLostRecenterDurationSeconds = 1.0f;
        }
        
        // pretty much my only major change; Just a simple presence check & making sure the parent exists 
        [HarmonyLib.HarmonyPatch(typeof(CameraController), nameof(CameraController.SetCockpitView))]
        class ChangeParentCockpitPatch
        {
            public static void Postfix()
            {
                if (!trackIRgameObject || !mainCamera) return;
                mainCamera.transform.SetParent(trackIRgameObject.transform, false);
                mainCamera.transform.localPosition = Vector3.zero;
                mainCamera.transform.localRotation = Quaternion.identity;
            }
        }
    }
}