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
#if HS2
    [BepInProcess("HoneySelect2")]
#elif AI
    [BepInProcess("AI-Syoujyo")]
#endif
    [BepInPlugin("hj." + "aihs2." + nameof(PositionSelector), nameof(PositionSelector), VERSION)]
    public class PositionSelector : BaseUnityPlugin
    {
        public const string VERSION = "2.0.1";
        public static PositionSelector Instance;
        public static ConfigEntry<bool> isInEditMode { get; set; }
        public static ConfigEntry<bool> unlockAll { get; set; }
        public static ConfigEntry<PositionPreset> positionPreset { get; set; }
        private ConfigEntry<KeyboardShortcut> editModeSwitch { get; set; }

        public static Dictionary<string, List<string>> charaFilters;
        public enum PositionPreset
        {
            [System.ComponentModel.DescriptionAttribute("one preset for each card")]
            ByCharacter,
            [System.ComponentModel.DescriptionAttribute("global preset 1")]
            GlobalPreset1,
            [System.ComponentModel.DescriptionAttribute("global preset 2")]
            GlobalPreset2,
            [System.ComponentModel.DescriptionAttribute("global preset 3")]
            GlobalPreset3,
            [System.ComponentModel.DescriptionAttribute("global preset 4")]
            GlobalPreset4,
            [System.ComponentModel.DescriptionAttribute("global preset 5")]
            GlobalPreset5
        }
        public void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(PositionSelector));
            isInEditMode = Config.Bind("Edit Mode", "Edit mode", false, new ConfigDescription("Toggle to switch to edit mode and start hidding/unhidding some animation. You can also use the bellow shortcut to switch more easily."));
            editModeSwitch = Config.Bind("Edit Mode", "Toggle edit mode shortcut", new KeyboardShortcut(KeyCode.R));
            positionPreset = Config.Bind("User Preset", "Save changes in...", PositionPreset.GlobalPreset1, new ConfigDescription("Save the set of hidden position into : one preset for each card, or one of the 5 global presets that will be shared between cards."));

            unlockAll = Config.Bind("Miscs", "Unlock everything", false, new ConfigDescription("Show all animations regardless of personality. NB : Some animation might not work if not available on the current map or with that set of character."));

            Instance = this;
            charaFilters = new Dictionary<string, List<string>>();
            LoadSave();

        }
        public void Update()
        {
            if (editModeSwitch.Value.IsDown())
            {
                isInEditMode.BoxedValue = !isInEditMode.Value;
            }
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
            string destination200 = Path.Combine(Paths.GameRootPath, "UserData", "PositionSelector") + "/PositionFilter.sav";
            FileStream file;

            if (File.Exists(destination200))
            {
                file = File.OpenWrite(destination200);
            }
            else if (File.Exists(destination))
            {
                file = File.OpenWrite(destination);
            }
            else
            {
                Directory.CreateDirectory(Path.Combine(Paths.GameRootPath, "UserData", "PositionSelector"));
                file = File.Create(destination200);
            }


            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, charaFilters);
            file.Close();
        }
        public static string GetPresetName()
        {
            switch (positionPreset.Value)
            {
                case PositionPreset.ByCharacter:
                    return Path.GetFileNameWithoutExtension(HSceneSprite.Instance.chaFemales[0].chaFile.charaFileName);
                case PositionPreset.GlobalPreset1:
                    return "GlobalPreset_1";
                case PositionPreset.GlobalPreset2:
                    return "GlobalPreset_2";
                case PositionPreset.GlobalPreset3:
                    return "GlobalPreset_3";
                case PositionPreset.GlobalPreset4:
                    return "GlobalPreset_4";
                case PositionPreset.GlobalPreset5:
                    return "GlobalPreset_5";
                default:
                    return "GlobalPreset_1";
            }

        }
#if HS2
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
#endif
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
#if HS2
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
#elif AI
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSprite), "CheckMotionLimit")]
        private static void CheckMotionLimitHook(ref HSceneSprite __instance, ref bool __result, HScene.AnimationListInfo lstAnimInfo)
        {
            if (__result == false && unlockAll.Value)
            {
                bool realyFalse = false;
                if (!realyFalse)
                {
                    __result = true;
                }
            }
        }
#endif
#if HS2
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HAnimationInfoComponent), "OnEnable")]
#else
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HAnimationInfoComponent), "Start")]
#endif
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
        private string animeId;

        public void Start()
        {
            animeId = transform.GetComponent<HAnimationInfoComponent>().info.nameAnimation + transform.GetComponent<HAnimationInfoComponent>().info.fileFemale + transform.GetComponent<HAnimationInfoComponent>().info.fileMale;
            PositionSelector.isInEditMode.SettingChanged += HandleEvent;
            PositionSelector.positionPreset.SettingChanged += HandleEvent;
            GetComponent<Toggle>().onValueChanged.AddListener((bool value) =>
            {
                if (value && PositionSelector.isInEditMode.Value)
                {
                    if (state.Equals(AnimFilterInfoState.Show))
                    {
                        //Hide it
                        SetHidden();
                        if (!PositionSelector.charaFilters.ContainsKey(PositionSelector.GetPresetName()))
                        {
                            PositionSelector.charaFilters.Add(PositionSelector.GetPresetName(), new List<string>());
                        }
                        if (!PositionSelector.charaFilters[PositionSelector.GetPresetName()].Contains(animeId))
                        {
                            PositionSelector.charaFilters[PositionSelector.GetPresetName()].Add(animeId);
                        }
                    }
                    else
                    {

                        SetShow();
                        if (PositionSelector.charaFilters.ContainsKey(PositionSelector.GetPresetName()) && PositionSelector.charaFilters[PositionSelector.GetPresetName()].Contains(animeId))
                        {
                            PositionSelector.charaFilters[PositionSelector.GetPresetName()].Remove(animeId);
                        }
                        if (PositionSelector.charaFilters[PositionSelector.GetPresetName()].Count <= 0)
                        {
                            PositionSelector.charaFilters.Remove(PositionSelector.GetPresetName());
                        }
                    }


                }
            });
            UpdateState();
        }

        void HandleEvent(object sender, object a)
        {
            UpdateState();
        }
        public void OnDestroy()
        {
            PositionSelector.isInEditMode.SettingChanged -= HandleEvent;
            PositionSelector.positionPreset.SettingChanged -= HandleEvent;
        }
        public void SetHidden()
        {
            transform.Find("Background").GetComponent<Image>().color = new Color(1f, 0f, 0f);
            state = AnimFilterInfoState.Hide;
            UpdateView();

        }
        public void SetShow()
        {
            transform.Find("Background").GetComponent<Image>().color = new Color(1f, 1f, 1f);
            state = AnimFilterInfoState.Show;
            UpdateView();
        }

        private void UpdateState()
        {
            if (PositionSelector.charaFilters.ContainsKey(PositionSelector.GetPresetName()) && PositionSelector.charaFilters[PositionSelector.GetPresetName()].Contains(animeId))
            {
                SetHidden();
            }
            else
            {
                SetShow();
            }
        }
        private void UpdateView()
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