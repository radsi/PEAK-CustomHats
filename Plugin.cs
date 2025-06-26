using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BepInEx;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Windows;
using Zorro.Core;

namespace MoreHats
{
    [BepInPlugin("radsi.hats", "custom hats", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static AssetBundle assetBundle;
        public static List<(GameObject, HatType)> customHats;
        public static List<Texture2D> customHatsIcons;

        public enum HatType
        {
            Hat, Mask
        };

        private class Patcher
        {
            public static bool dummyPatched = false;

            public static bool CreateHatOption(Customization customization, string name, Texture2D icon)
            {
                if (Array.Exists(customization.hats, hat => hat.name == name)) return false;
                var hatOption = ScriptableObject.CreateInstance<CustomizationOption>();
                hatOption.color = Color.white;
                hatOption.name = name;
                hatOption.texture = icon;
                hatOption.type = Customization.Type.Hat;
                hatOption.requiredAchievement = ACHIEVEMENTTYPE.NONE;
                customization.hats = customization.hats.AddToArray(hatOption);

                return true;
            }

            [HarmonyPatch(typeof(PassportManager), "Awake")]
            [HarmonyPostfix]
            public static void PassportManagerAwakePostfix(PassportManager __instance)
            {
                CreateHatOption(__instance.GetComponent<Customization>(), "Hat_GhostMask", customHatsIcons[0]);
                CreateHatOption(__instance.GetComponent<Customization>(), "Hat_CreeperHat", customHatsIcons[1]);
            }
            
            [HarmonyPatch(typeof(CharacterCustomization), "Awake")]
            [HarmonyPostfix]
            public static void CharacterCustomizationAwakePostfix(CharacterCustomization __instance)
            {
                Transform hatsObject = __instance.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(1).GetChild(1);

                for(int i = 0; i < customHats.Count; i++)
                {
                    GameObject newHat = Instantiate(customHats[i].Item1);
                    newHat.GetComponent<MeshRenderer>().material.shader = Shader.Find("W/Character");
                    newHat.transform.SetParent(hatsObject);
                    newHat.transform.localPosition = (customHats[i].Item2 == HatType.Mask ? new Vector3(0f, -1f, 5.6f) : new Vector3(0f, 0.5f, 8f));
                    newHat.transform.localScale = (customHats[i].Item2 == HatType.Mask ? new Vector3(50f, 50f, 50f) : new Vector3(3f, 3f, 3f));
                    newHat.transform.localRotation = (customHats[i].Item2 == HatType.Mask ? Quaternion.Euler(0f, 0f, 90f) : Quaternion.Euler(0f, 0f, 0f));
                    newHat.SetActive(false);

                    __instance.refs.playerHats = __instance.refs.playerHats.AddToArray(newHat.GetComponent<MeshRenderer>());
                }
            }
        }

        public void Awake()
        {
            new Harmony("radsi.hats").PatchAll(typeof(Patcher));

            assetBundle = AssetBundle.LoadFromMemory(Resource1.hats);

            customHats = new List<(GameObject, HatType)>();
            customHatsIcons = new List<Texture2D>();

            customHats.Add((assetBundle.LoadAsset<GameObject>("assets/ghost.prefab"), HatType.Mask));
            customHats.Add((assetBundle.LoadAsset<GameObject>("assets/creeper.prefab"), HatType.Hat));
            customHatsIcons.Add(assetBundle.LoadAsset<Texture2D>("assets/ghosticon.png"));
            customHatsIcons.Add(assetBundle.LoadAsset<Texture2D>("assets/creepericon.png"));
        }
    }
}