using BepInEx;
using HarmonyLib;
using HS2;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Debug = UnityEngine.Debug;

namespace AprilFoolSpecial
{
    [BepInProcess("HoneySelect2")]
    [BepInProcess("AI-Syoujyo")]

    [BepInPlugin("hj." + "aihs2." + nameof(AprilFoolSpecial), nameof(AprilFoolSpecial), VERSION)]
    public class AprilFoolSpecial : BaseUnityPlugin
    {
        public const string VERSION = "1.0.0";


        public void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(AprilFoolSpecial));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TitleScene), "Start")]
        public static void youFool(ref TitleScene __instance)
        {
            GameObject pan = __instance.transform.Find("Canvas").Find("Panel").gameObject;
            GameObject supercool = Instantiate(new GameObject(), pan.transform);
            RectTransform rect = supercool.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.offsetMin = new Vector2(0, 0);
            rect.offsetMax = new Vector2(1, 1);
            RawImage ri = supercool.AddComponent<RawImage>();
            RenderTexture rx = new RenderTexture(1920, 1080, 24);
            ri.texture = rx;
            VideoPlayer vp = supercool.AddComponent<VideoPlayer>();
            vp.source = VideoSource.Url;
            vp.url = "https://get-mp4.xyz/videos/rick-astley-never-gonna-give-you-up_362329.mp4";
            vp.targetTexture = rx;



        }

    }

}