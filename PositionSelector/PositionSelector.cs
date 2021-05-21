using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

using Debug = UnityEngine.Debug;

namespace PositionSelector
{
    using UnityEngine;

    [BepInProcess("HoneySelect2")]
    [BepInProcess("AI-Syoujyo")]
    [BepInPlugin("hj." + "aihs2." + nameof(PositionSelector), nameof(PositionSelector), VERSION)]
    public class PositionSelector : BaseUnityPlugin
    {
        public const string VERSION = "1.0.0";
        public static PositionSelector Instance;
        public static ConfigEntry<bool> isInEditMode { get; set; }
        public static ConfigEntry<bool> unlockAll { get; set; }
        public static Dictionary<string, List<string>> charaFilters;
        public void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(PositionSelector));
            isInEditMode = Config.Bind("Option", "Edit mode", false, new ConfigDescription("Toggle to switch to edit mode and start hidding some animation."));
            unlockAll = Config.Bind("Option", "Unlock everything", false, new ConfigDescription("Show all animations regardless of personality"));
            Instance = this;
            charaFilters = new Dictionary<string, List<string>>();
            LoadSave();

        }
        public void OnDestroy()
        {
            Save();
        }
        private void LoadSave()
        {
            string destination = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/PositionFilter.sav";
            FileStream file;

            if (File.Exists(destination))
            {
                Debug.Log(destination);
                Debug.Log(File.Exists(destination));
                file = File.OpenRead(destination);
            }
            else
            {
                return;
            }

            BinaryFormatter bf = new BinaryFormatter();
            charaFilters = (Dictionary<string, List<string>>)bf.Deserialize(file);

            file.Close();
        }
        private void Save()
        {
            string destination = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/PositionFilter.sav";
            FileStream file;

            if (File.Exists(destination)) file = File.OpenWrite(destination);
            else file = File.Create(destination);


            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, charaFilters);
            file.Close();
        }
        public static string GetCharaName()
        {
            return Path.GetFileNameWithoutExtension(HSceneSprite.Instance.chaFemales[0].chaFile.charaFileName);
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HSceneSprite), "OnChangePlaySelect", new[] { typeof(Toggle) })]
        private static bool OnButtonClick(Toggle objClick)
        {
            if (PositionSelector.isInEditMode.Value)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HSceneSprite), "OnChangePlaySelect", new[] { typeof(GameObject) })]
        private static bool OnButtonClick2(GameObject objClick)
        {
            if (PositionSelector.isInEditMode.Value)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSprite), "CheckMotionLimit")]
        private static void CheckMotionLimitHook(ref HSceneSprite __instance, ref bool __result, HScene.AnimationListInfo lstAnimInfo)
        {
            if (__result == false && unlockAll.Value)
            {
                bool realyFalse = false;
                if (__instance.chaFemales.Length > 1 && __instance.chaFemales[1] == null && (lstAnimInfo.ActionCtrl.Item1 == 4 || lstAnimInfo.ActionCtrl.Item1 == 5))
                {
                    realyFalse = true;
                }
                if (__instance.chaMales.Length > 1 && __instance.chaMales[1] == null && lstAnimInfo.ActionCtrl.Item1 == 6)
                {
                    realyFalse = true;
                }
                if (__instance.EventNo == 19)
                {
                    if (lstAnimInfo.ActionCtrl.Item1 == 4 || lstAnimInfo.ActionCtrl.Item1 == 5 || lstAnimInfo.ActionCtrl.Item1 == 6)
                    {
                        realyFalse = true;
                    }
                    if (lstAnimInfo.ActionCtrl.Item1 == 3 && lstAnimInfo.id == 0)
                    {
                        realyFalse = true;
                    }
                }
                if (!__instance.CheckEventLimit(lstAnimInfo.Event))
                {
                    realyFalse = true;
                }

                if (!__instance.CheckPlace(lstAnimInfo))
                {
                    realyFalse = true;
                }
                if (!realyFalse)
                {
                    __result = true;
                }
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HAnimationInfoComponent), "OnEnable")]
        private static void ApplyFilter(ref HAnimationInfoComponent __instance)
        {
            //Add the component
            __instance.transform.gameObject.AddComponent<AnimFilterInfo>();





        }
    }


    public enum AnimFilterInfoState
    {
        Show,
        Hide
    }
    public class AnimFilterInfo : MonoBehaviour
    {

        public AnimFilterInfoState state = AnimFilterInfoState.Show;
        private string animeName;

        public void Start()
        {
            animeName = transform.GetComponent<HAnimationInfoComponent>().info.nameAnimation;
            if (PositionSelector.charaFilters.ContainsKey(PositionSelector.GetCharaName()) && PositionSelector.charaFilters[PositionSelector.GetCharaName()].Contains(animeName))
            {
                SetHidden();
            }
            PositionSelector.isInEditMode.SettingChanged += HandleEvent;
            GetComponent<Toggle>().onValueChanged.AddListener((bool value) =>
            {
                if (value && PositionSelector.isInEditMode.Value)
                {
                    if (state.Equals(AnimFilterInfoState.Show))
                    {
                        //Hide it
                        SetHidden();
                        if (!PositionSelector.charaFilters.ContainsKey(PositionSelector.GetCharaName()))
                        {
                            PositionSelector.charaFilters.Add(PositionSelector.GetCharaName(), new List<string>());
                        }
                        if (!PositionSelector.charaFilters[PositionSelector.GetCharaName()].Contains(animeName))
                        {
                            PositionSelector.charaFilters[PositionSelector.GetCharaName()].Add(animeName);
                        }
                    }
                    else
                    {

                        SetShow();
                        if (PositionSelector.charaFilters.ContainsKey(PositionSelector.GetCharaName()) && PositionSelector.charaFilters[PositionSelector.GetCharaName()].Contains(animeName))
                        {
                            PositionSelector.charaFilters[PositionSelector.GetCharaName()].Remove(animeName);
                        }
                        if (PositionSelector.charaFilters[PositionSelector.GetCharaName()].Count <= 0)
                        {
                            PositionSelector.charaFilters.Remove(PositionSelector.GetCharaName());
                        }
                    }


                }
            });
        }
        void HandleEvent(object sender, object a)
        {
            UpdateViewState();
        }
        public void OnDestroy()
        {
            PositionSelector.isInEditMode.SettingChanged -= HandleEvent;
        }
        public void SetHidden()
        {
            transform.Find("Background").GetComponent<Image>().color = new Color(1f, 0f, 0f);
            state = AnimFilterInfoState.Hide;
            UpdateViewState();

        }
        public void SetShow()
        {
            transform.Find("Background").GetComponent<Image>().color = new Color(1f, 1f, 1f);
            state = AnimFilterInfoState.Show;
            UpdateViewState();
        }

        private void UpdateViewState()
        {
            if (PositionSelector.isInEditMode.Value)
            {
                gameObject.SetActive(true);
            }
            else
            {
                if (state.Equals(AnimFilterInfoState.Show))
                {
                    gameObject.SetActive(true);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }

    }



}