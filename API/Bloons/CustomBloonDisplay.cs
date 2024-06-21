﻿using BloonFactoryMod.API.Serializables;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Display;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Models.Bloons;
using Il2CppAssets.Scripts.Models.GenericBehaviors;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Unity.Display;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppNinjaKiwi.Common.ResourceUtils;
using MelonLoader;
using MelonLoader.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using UnityEngine;
using static Il2CppSystem.Linq.Expressions.Interpreter.CastInstruction.CastInstructionNoT;
using static UnityEngine.UIElements.UIR.GradientSettingsAtlas;
using TaskScheduler = BTD_Mod_Helper.Api.TaskScheduler;

namespace BloonFactoryMod.API.Bloons
{
    internal class CustomBloonDisplay : ModDisplay2D
    {

        internal static Dictionary<string, Texture2D> Cache = new Dictionary<string, Texture2D>();
        public override string Name => bloonSave.Name;
        public override DisplayCategory DisplayCategory => DisplayCategory.Bloon;
        protected override string TextureName => "BaseBloonInGame";
        public override string BaseDisplay => "9d3c0064c3ace7448bf8fefa4a97a70f";

        public Sprite sprite = GetSprite<BloonFactoryMod>("BaseBloonInGame");
        public CustomBloonSave bloonSave { get; set; }
        public CustomBloonDisplay(CustomBloonSave save)
        {
            mod = ModHelper.GetMod<BloonFactoryMod>();
            bloonSave = save;

            Register();
        }
        public override void Apply(BloonModel bloonModel)
        {
            base.Apply(bloonModel);
            if (!Cache.ContainsKey(bloonSave.GUID))
            {
                MelonLogger.Msg($"Generating Texture...");
                    
                Il2CppStructArray<Color> decal1TextureColors = null;
                if (bloonSave.Decal1.Type != CustomBloonDecal.DecalType.None)
                {
                    var decal1Texture = GetSprite<BloonFactoryMod>(CustomBloonDecal.GetSpriteNames(bloonSave.Decal1.Type).Item2);
                    decal1TextureColors = decal1Texture.texture.GetPixels(0, 0, 128, 128);
                }

                Il2CppStructArray<Color> decal2TextureColors = null;
                if (bloonSave.Decal2.Type != CustomBloonDecal.DecalType.None)
                {
                    var decal2Texture = GetSprite<BloonFactoryMod>(CustomBloonDecal.GetSpriteNames(bloonSave.Decal2.Type).Item2);
                    decal2TextureColors = decal2Texture.texture.GetPixels(0, 0, 128, 128);
                }

                Il2CppStructArray<Color> baseSpriteColors = sprite.texture.GetPixels(0, 0, 128, 128);


                for (int i = 0; i < baseSpriteColors.Length; i++)
                {
                    if (decal2TextureColors != null && decal2TextureColors[i].a > 0.2)
                    {
                        baseSpriteColors[i] = BlendColors(decal2TextureColors[i], bloonSave.Decal2.Color);
                    }
                    else if (decal1TextureColors != null && decal1TextureColors[i].a > 0.2)
                    {
                        baseSpriteColors[i] = BlendColors(decal1TextureColors[i], bloonSave.Decal1.Color);
                    }
                    else if (baseSpriteColors[i].a > 0.2)
                    {
                        baseSpriteColors[i] = BlendColors(baseSpriteColors[i], bloonSave.Color);
                    }
                }

                //Uses my really scuffed texture fix, will fix later :)
                var texture = GetSprite<BloonFactoryMod>($"EmptyTexture{Cache.Count}");
                texture.texture.SetPixels(baseSpriteColors);
                texture.texture.Apply();

                Cache.Add(bloonSave.GUID, texture.texture);
            }
        }
        public override void ModifyDisplayNode(UnityDisplayNode node)
        {
            if (Cache.TryGetValue(bloonSave.GUID, out Texture2D value))
            {
                node.GetRenderer<SpriteRenderer>().sprite = Sprite.Create(value, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f), 10f);
                node.IsSprite = true;
            }
            else
            {
                throw new Exception("Texture not generated.");
            }
        }

        public static Color BlendColors(Color color1, Color color2)
        {
            return color1 * color2;
        }
        public static void SaveImage(Texture2D texture, string GUID)
        {
            texture.SaveToPNG(Path.Combine(MelonEnvironment.ModsDirectory, "BloonFactoryBloons", $"{GUID}.png"));
        }
    }
}

