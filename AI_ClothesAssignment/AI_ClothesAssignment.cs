using AIProject;
using BepInEx;
using HarmonyLib;
using Manager;
using System.Collections.Generic;
using UnityEngine;


namespace AI_ClothesAssignment
{
    using Debug = UnityEngine.Debug;
    [BepInProcess("AI-Syoujyo")]
    [BepInPlugin("hj." + "ai." + nameof(AI_ClothesAssignment), nameof(AI_ClothesAssignment), VERSION)]
    public class AI_ClothesAssignment : BaseUnityPlugin
    {
        public const string VERSION = "1.0.0";


        public void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(AI_ClothesAssignment));
        }

        private static List<string> closetCoordinateList;
        private static List<string> dressCoordinateList;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ClothChange), "OnStart")]
        static void ClothChangePrefix(ref ClothChange __instance)
        {
            closetCoordinateList = Singleton<Game>.Instance.Environment.ClosetCoordinateList;
            List<string> filteredList = new List<string>();
            Traverse tra = new Traverse(__instance);
            string name = tra.Property<AgentActor>("Agent").Value.CharaName.ToLower();

            foreach (string coord in Singleton<Game>.Instance.Environment.ClosetCoordinateList)
            {

                if (coord.ToLower().Contains(name))
                {

                    filteredList.Add(coord);
                }

            }
            if (filteredList.Count > 0)
            {
                Singleton<Game>.Instance.Environment.ClosetCoordinateList = filteredList;
            }


        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ClothChange), "OnStart")]
        static void ClothChangePostfix()
        {
            Singleton<Game>.Instance.Environment.ClosetCoordinateList = closetCoordinateList;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(DressIn), "OnStart")]
        static void DessInPrefix(ref DressIn __instance)
        {
            dressCoordinateList = Singleton<Game>.Instance.Environment.DressCoordinateList;
            List<string> filteredList = new List<string>();
            Traverse tra = new Traverse(__instance);
            string name = tra.Property<AgentActor>("Agent").Value.CharaName.ToLower();

            foreach (string coord in Singleton<Game>.Instance.Environment.DressCoordinateList)
            {

                if (coord.ToLower().Contains(name))
                {

                    filteredList.Add(coord);
                }

            }
            if (filteredList.Count > 0)
            {
                Singleton<Game>.Instance.Environment.DressCoordinateList = filteredList;
            }


        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(DressIn), "OnStart")]
        static void DressInPostfix()
        {
            Singleton<Game>.Instance.Environment.DressCoordinateList = dressCoordinateList;
        }
    }

}
