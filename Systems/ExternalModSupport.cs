using ITD.Content.Items.BossSummons;
using ITD.Content.Items.Other;
using ITD.Content.NPCs;
using ITD.Content.UI;
using ITD.Systems.Recruitment;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;
using static Terraria.ModLoader.ModContent;

namespace ITD.Systems;

internal class ExternalModSupport
{
    public static void Init()
    {
        BossChecklistSupport();
        MunchiesSupport();
        DialogueTweakSupport();
    }
    private static void AddBoss(Mod bossChecklist, Mod hostMod, string name, float difficulty, Func<bool> downed, object npcTypes, Dictionary<string, object> extraInfo)
        => bossChecklist.Call("LogBoss", hostMod, name, difficulty, downed, npcTypes, extraInfo);
    private static void BossChecklistSupport()
    {
        ITD ITDMod = ITD.Instance;
        Mod bossChecklist = ITDMod.bossChecklist;
        if (bossChecklist is null)
            return;
        AddBosses(bossChecklist, ITDMod);
    }
    private static void AddBosses(Mod bossChecklist, Mod ITDMod)
    {

        foreach (var boss in ITDMod.GetContent<ModNPC>().Where(m => m is ITDNPC itdNPC && itdNPC.BossWeight >= 0f))
        {
            ITDNPC iNPC = (ITDNPC)boss;

            var downed = () => false;// iNPC.DownedMe ?? throw new Exception("Override the DownedMe hook in ITDNPC and provide a valid value.");
            string name = boss.Name;
            float prog = iNPC.BossWeight;
            int type = boss.Type;

            IItemDropRule[] collectibles = iNPC.CollectibleRules;
            List<int> finalCollectibles = null;

            if (collectibles != null)
            {
                finalCollectibles = [];
                for (int i = 0; i < collectibles.Length; i++)
                {
                    IItemDropRule possibleCollectible = collectibles[i];
                    if (possibleCollectible is CommonDrop drop0)
                    {
                        finalCollectibles.Add(drop0.itemId);
                    }
                    else if (possibleCollectible is DropBasedOnExpertMode dropExpert)
                    {
                        if (dropExpert.ruleForExpertMode is CommonDrop drop1)
                        {
                            finalCollectibles.Add(drop1.itemId);
                        }
                    }
                    else if (possibleCollectible is DropBasedOnMasterMode dropMaster)
                    {
                        if (dropMaster.ruleForMasterMode is CommonDrop drop2)
                        {
                            finalCollectibles.Add(drop2.itemId);
                        }
                    }
                }
            }

            int spawnItem = -1;

            foreach (var summoner in ITDMod.GetContent<ModItem>().Where(i => i is BossSummoner))
            {
                if (((BossSummoner)summoner).NPCType == type)
                    spawnItem = summoner.Type;
            }

            Dictionary<string, object> specialInfo = [];

            if (iNPC.HowToSummon != null)
            {
                Func<LocalizedText> dynamicText = () => iNPC.HowToSummon;
                specialInfo["spawnInfo"] = dynamicText;
            }

            if (spawnItem > -1)
                specialInfo["spawnItems"] = spawnItem;

            if (finalCollectibles != null)
                specialInfo["collectibles"] = finalCollectibles;

            var portraitAction = iNPC.DrawBossChecklistPortrait;
            if (portraitAction != null)
                specialInfo["customPortrait"] = portraitAction;

            AddBoss(bossChecklist, ITDMod, name, prog, downed, type, specialInfo);

        }
    }
    private static void MunchiesSupport()
    {
        ITD ITDMod = ITD.Instance;
        Mod munchies = ITDMod.munchies;
        if (munchies is null)
            return;
        object[] driversIncense =
        [
            "AddSingleConsumable", // ModCall type
            ITDMod, // Mod ref
            "1.4", // Munchies version
            GetInstance<DriversIncense>(), // ModItem
            "player", // Consumable type
            () => Main.LocalPlayer.GetModPlayer<DriversIncensePlayer>().DriversIncenseConsumed, // Func<bool>
            null, // Color
            null, // Difficulty
            null, // Extra tooltip
            null, // Availability (Func<bool>)
            DriversIncense.MunchiesExplanation // Explanation LocalizedText
        ];
        munchies.Call(driversIncense);
    }
    private static void DialogueTweakSupport()
    {
        /// <summary>
        /// Add a button for specific NPCs.
        /// </summary>
        /// <param name="npcType">NPC ID(s) is needed. Use <see cref="NPCType"/> to submit your ID. You can also tweak vanilla NPCs by using vanilla NPC ID.</param>
        /// <param name="buttonText">This is the text which will be shown in the button. It is <see cref="Func{TResult}"/> so you can use <see cref="Language.GetTextValue"/> or something else.</param>
        /// <param name="iconTexturePath">You have to specify the icon texture of the button. Use your texture's path. If you use "" or <see langword="null"/>, no icon will be shown.</param>
        /// <param name="hoverCallback">The action that will be called when the client hovers over the button. Use this to define the behavior when the button is pressed.</param>
        /// <param name="availability">You can decide if your button should be shown.</param>
        /// <param name="frame">You can customize the frame of the texture. It is useful to display different parts of the texture in different situations.</param>
        /// <param name="customTextOffset">You can customize the distance from the left side of the box containing the text to the left side of the button. The invisible box is the right part of the entire button excluding the icon, and the text will be drawn in its center. Check "how_offset_works.png" for image explanation</param>
        static void AddButton(List<int> npcType, Func<string> buttonText, Func<string> iconTexturePath,
            Action hoverCallback, Func<bool> availability = null, Func<Rectangle> frame = null,
            Func<float> customTextOffset = null)
        {
            Mod dialogue = ITD.Instance.dialogueTweak;
            if (dialogue is null)
                return;

            availability ??= () => true;
            dialogue.Call("AddButton", npcType, buttonText, iconTexturePath, hoverCallback, availability, frame, customTextOffset);
        }
        List<int> NPCsThatCanBeRecruited = [.. TownNPCRecruitmentLoader.NPCsThatCanBeRecruited, .. TownNPCRecruitmentLoader.recruitmentDataRegistry.Keys];
        string buttonName = ITD.Instance.GetLocalization($"UI.{nameof(RecruitmentButton)}.MouseHoverName").Value;
        string toTex = "ITD/Content/UI/RecruitmentButton";
        static void Behavior()
        {
            if (Main.mouseLeft)
            {
                RecruitmentButton.DoRecruit();
            }
        }
        AddButton(NPCsThatCanBeRecruited, () => buttonName, () => toTex, Behavior);
    }
}
