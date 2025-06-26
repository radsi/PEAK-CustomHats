using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace MoreHats
{
    [BepInPlugin("radsi.hats", "custom hats", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static AssetBundle assetBundle;
        public static List<GameObject> customHats;
        public static List<Texture2D> customHatsIcons;

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
                CreateHatOption(__instance.GetComponent<Customization>(), "Hat_Wat", customHatsIcons[2]);
            }
            
            [HarmonyPatch(typeof(CharacterCustomization), "Awake")]
            [HarmonyPostfix]
            public static void CharacterCustomizationAwakePostfix(CharacterCustomization __instance)
            {
                Transform hatsObject = __instance.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(1).GetChild(1);

                for(int i = 0; i < customHats.Count; i++)
                {
                    GameObject newHat = Instantiate(customHats[i]);
                    newHat.GetComponent<MeshRenderer>().material.shader = Shader.Find("W/Character");
                    newHat.transform.SetParent(hatsObject);

                    if (newHat.name.Contains("creeper"))
                    {
                        newHat.transform.localPosition = new Vector3(0f, 0.5f, 8f);
                        newHat.transform.localScale = new Vector3(3f, 3f, 3f);
                        newHat.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    }
                    else if (newHat.name.Contains("ghost"))
                    {
                        newHat.transform.localPosition = new Vector3(0f, -1f, 5.6f);
                        newHat.transform.localScale = new Vector3(50f, 50f, 50f);
                        newHat.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    }
                    else
                    {
                        newHat.transform.localPosition = new Vector3(0f, 0f, 6.5f);
                        newHat.transform.localScale = new Vector3(3f, 3f, 3f);
                        newHat.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    }

                    newHat.SetActive(false);

                    __instance.refs.playerHats = __instance.refs.playerHats.AddToArray(newHat.GetComponent<MeshRenderer>());
                }
            }
        }

        public void Awake()
        {
            new Harmony("radsi.hats").PatchAll(typeof(Patcher));

            assetBundle = AssetBundle.LoadFromMemory(Resource1.hats);

            customHats = new List<GameObject>();
            customHatsIcons = new List<Texture2D>();

            customHats.Add((assetBundle.LoadAsset<GameObject>("assets/ghost.prefab")));
            customHats.Add((assetBundle.LoadAsset<GameObject>("assets/creeper.prefab")));
            customHats.Add((assetBundle.LoadAsset<GameObject>("assets/wat.prefab")));
            customHatsIcons.Add(assetBundle.LoadAsset<Texture2D>("assets/ghosticon.png"));
            customHatsIcons.Add(assetBundle.LoadAsset<Texture2D>("assets/creepericon.png"));
            customHatsIcons.Add(assetBundle.LoadAsset<Texture2D>("assets/nagawat.png"));
        }
    }
}
