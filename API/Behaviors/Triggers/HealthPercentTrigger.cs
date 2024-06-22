﻿using BloonFactoryMod.API.Serializables;
using BloonFactoryMod.API.Serializables.Behaviors;
using BloonFactoryMod.API.Serializables.Behaviors.Triggers;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using Il2Cpp;
using Il2CppAssets.Scripts.Models.Bloons;
using Il2CppAssets.Scripts.Models.Bloons.Behaviors;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppNinjaKiwi.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BloonFactoryMod.API.Behaviors.Triggers
{
    internal class HealthPercentTrigger : CustomBloonBehavior<HealthPercentTriggerSerializable>
    {
        public override BehaviorType Type => BehaviorType.Trigger;
        public override string Name => "Health Percent Trigger";
        public override void AddToBloon(BloonModel bloon, CustomBloonBehaviorSerializable serializable)
        {
            HealthPercentTriggerSerializable trigger = (HealthPercentTriggerSerializable)serializable;
            bloon.AddBehavior(new HealthPercentTriggerModel("HealthPercentTrigger:" + serializable.GUID, false, new Il2CppStructArray<float>([trigger.Percent / 100]), new Il2CppStringArray(trigger.ActionIDs.ToArray()), true));
        }

        public override ModHelperPanel CreatePanel(CustomBloonBehaviorSerializable serializable, CustomBloonSave save)
        {
            HealthPercentTriggerSerializable time = (HealthPercentTriggerSerializable)serializable;

            var panel = ModHelperPanel.Create(new Info(Name, 0, 0, 950, 700), VanillaSprites.MainBGPanelBlue);
            panel.AddText(new Info("Text", -237.5f, 250, 450, 180), Name, 70, Il2CppTMPro.TextAlignmentOptions.Center).Text.GetComponent<NK_TextMeshProUGUI>().enableAutoSizing = true;
            var scrollpanel = panel.AddScrollPanel(new Info("Actions", 225, 100, 400, 400), UnityEngine.RectTransform.Axis.Vertical, VanillaSprites.BlueInsertPanelRound);
            panel.AddButton(new Info("Button", 225f, -225, 400, 200), VanillaSprites.GreenBtnLong, new System.Action(() =>
            {
                BehaviorHelper.ShowAddActionPopup(save, (CustomBloonBehaviorTriggerSerializable)serializable, new Action<CustomBloonBehaviorSerializable>(behavior =>
                {
                    ((CustomBloonBehaviorTriggerSerializable)serializable).ActionIDs.Add(behavior.GUID);
                    UpdateScrollPanel(scrollpanel, (CustomBloonBehaviorTriggerSerializable)serializable, save);
                }));
            })).AddText(new Info("Text", 0, 0, 400, 150), "Link Action", 60);

            panel.AddText(new Info("PercentText", -237.5f, 50, 400, 150), "Percent:", 60, Il2CppTMPro.TextAlignmentOptions.Center);
            var input = panel.AddInputField(new Info("PercentInput", -237.5f, -75, 250, 100), $"{time.Percent}", VanillaSprites.BlueInsertPanelRound, new Action<string>(value =>
            {
                if (float.TryParse(value, out float result))
                {
                    time.Percent = result;
                }
            }), 50, Il2CppTMPro.TMP_InputField.CharacterValidation.Decimal, Il2CppTMPro.TextAlignmentOptions.Center);
            input.Text.GetComponent<NK_TextMeshProUGUI>().enableAutoSizing = true;
            input.SetActive(false);
            input.SetActive(true);

            panel.AddText(new Info("PercentSign", -50, -75, 100, 100), "%", 80);


            UpdateScrollPanel(scrollpanel, (CustomBloonBehaviorTriggerSerializable)serializable, save);
            return panel;
        }

        public void UpdateScrollPanel(ModHelperScrollPanel panel, CustomBloonBehaviorTriggerSerializable trigger, CustomBloonSave save)
        {
            panel.ScrollContent.transform.DestroyAllChildren();
            List<string> IDsToRemove = new List<string>();
            foreach (var action in trigger.ActionIDs)
            {
                if (!save.BloonBehaviors.Any(bln => bln.GUID == action))
                {
                    IDsToRemove.Add(action);
                    continue;
                }

                panel.AddScrollContent(CreateActionPanel(save.BloonBehaviors.First(bln => bln.GUID == action).Name, new Action(() =>
                {
                    trigger.ActionIDs.Remove(action);
                    UpdateScrollPanel(panel, trigger, save);
                })));
            }
            trigger.ActionIDs.RemoveAll(id => IDsToRemove.Contains(id));
        }
        public ModHelperPanel CreateActionPanel(string name, Action onRemove)
        {
            var panel = ModHelperPanel.Create(new Info("ActionText", 0, 0, 350, 100));
            panel.AddText(new Info("Text", -50, 0, 250, 80), name).Text.GetComponent<NK_TextMeshProUGUI>().enableAutoSizing = true;

            panel.AddButton(new Info("Remove", 150, 0, 80, 80), VanillaSprites.AddRemoveBtn, onRemove);

            return panel;
        }
    }
}
