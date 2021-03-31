namespace Studio_AnimationSearch
{
    using BepInEx;
    using HarmonyLib;
    using Studio;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Emit;
    using UnityEngine;
    using UnityEngine.UI;
    using Debug = UnityEngine.Debug;


    [BepInProcess("StudioNEOV2")]
    [BepInPlugin("hj." + "studio." + nameof(Studio_AnimationSearch), nameof(Studio_AnimationSearch), VERSION)]
    public class Studio_AnimationSearch : BaseUnityPlugin
    {
        public const string VERSION = "1.0.0";

        Harmony instance;
        private static GameObject animePanel;

        public void Awake()
        {
            instance = Harmony.CreateAndPatchAll(typeof(Studio_AnimationSearch));

        }
        public void OnDestroy()
        {
            instance.UnpatchAll();
            Destroy(animePanel.GetComponent<AnimationSearch>());
        }



        [HarmonyPostfix]
        [HarmonyPatch(typeof(ManipulatePanelCtrl), "Awake")]
        private static void PannelHook(ref ManipulatePanelCtrl __instance)
        {
            animePanel = __instance.transform.Find("03_Anime").gameObject;
            animePanel.AddComponent<AnimationSearch>();
        }



    }
    public class AnimationSearch : MonoBehaviour
    {

        private static GameObject searchBar;
        private static GameObject groupListPanel;

        private Dictionary<int, Dictionary<int, Dictionary<int, Info.AnimeLoadInfo>>> animeListsBase;

        private static Dictionary<int, Dictionary<int, Dictionary<int, Info.AnimeLoadInfo>>> animeListsFiltered;
        public void Start()
        {

            animeListsBase = Singleton<Info>.Instance.dicAnimeLoadInfo;
            groupListPanel = this.transform.Find("Group Panel").gameObject;
#if AI
            GameObject workspaceSearchbar = this.transform.parent.parent.parent.Find("Canvas Object List").Find("Image Bar").Find("Scroll View").Find("Search").gameObject;
            searchBar = GameObject.Instantiate(workspaceSearchbar, this.transform);
#else
            GameObject workspaceSearchbar = this.transform.parent.parent.parent.Find("Canvas Object List").Find("Image Bar").Find("Scroll View").Find("Search").gameObject;
            searchBar = GameObject.Instantiate(workspaceSearchbar, this.transform);
#endif
            RectTransform rect = searchBar.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.offsetMin = new Vector2(0, -320f);
            rect.offsetMax = new Vector2(250, -290f);

            InputField input = searchBar.GetComponent<InputField>();
            input.onEndEdit.RemoveAllListeners();
            input.onValueChanged.RemoveAllListeners();
            input.onValueChanged.AddListener(delegate { UpdateAnimeList(input.text); });
        }
        public void OnDestroy()
        {
            Singleton<Info>.Instance.dicAnimeLoadInfo = animeListsBase;
        }
        private void UpdateAnimeList(string searchPatern)
        {
            animeListsFiltered = new Dictionary<int, Dictionary<int, Dictionary<int, Info.AnimeLoadInfo>>>();
            foreach (KeyValuePair<int, Dictionary<int, Dictionary<int, Info.AnimeLoadInfo>>> keyValuePairGroup in animeListsBase)
            {
                foreach (KeyValuePair<int, Dictionary<int, Info.AnimeLoadInfo>> keyValuePairCategory in keyValuePairGroup.Value)
                {
                    foreach (KeyValuePair<int, Info.AnimeLoadInfo> keyValuePairAnime in keyValuePairCategory.Value)
                    {
                        if (keyValuePairAnime.Value.name.ToLower().Contains(searchPatern.ToLower()))
                        {

                            if (!animeListsFiltered.ContainsKey(keyValuePairGroup.Key))
                            {
                                animeListsFiltered.Add(keyValuePairGroup.Key, new Dictionary<int, Dictionary<int, Info.AnimeLoadInfo>>());
                            }
                            if (!animeListsFiltered[keyValuePairGroup.Key].ContainsKey(keyValuePairCategory.Key))
                            {
                                animeListsFiltered[keyValuePairGroup.Key].Add(keyValuePairCategory.Key, new Dictionary<int, Info.AnimeLoadInfo>());
                            }
                            animeListsFiltered[keyValuePairGroup.Key][keyValuePairCategory.Key].Add(keyValuePairAnime.Key, keyValuePairAnime.Value);

                        }
                    }
                }
            }

            UpdateNodes();
        }
        private void UpdateNodes()
        {


            Singleton<Info>.Instance.dicAnimeLoadInfo = animeListsFiltered;
            AnimeGroupList a = groupListPanel.GetComponent<AnimeGroupList>();
            Traverse tra = new Traverse(a);
            tra.Field("isInit").SetValue(false);
            a.InitList(a.sex);

        }


    }

}