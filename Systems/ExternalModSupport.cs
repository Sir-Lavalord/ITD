using ITD.Content.Items.Placeable;
using ITD.Content.Items.PetSummons;
using ITD.Content.Items.BossSummons;
using ITD.Content.NPCs.Bosses;
using System;
using System.Collections.Generic;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static ITD.Downed;
using Terraria.Achievements;
using ITD.Content.Items.Other;
using Terraria;
using Terraria.ID;
using ITD.Systems.Recruitment;
using System.Linq;
using Terraria.Localization;
using ITD.Content.UI;

namespace ITD
{
    internal class Downed
    {
        public static readonly Func<bool> DownedCosmicJellyfish = () => DownedBossSystem.downedCosJel;
    }
    internal class ExternalModSupport
    {

        private static readonly Dictionary<string, float> BossChecklistProgressionValues = new()
        {
            { "CosmicJellyfish", 3.9f },
        };

        public static void Init()
        {
            BossChecklistSupport();
            MunchiesSupport();
            TMLAchievementsSupport();
            DialogueTweakSupport();
        }

        private static void TMLAchievementsSupport()
        {
            ITD ITDMod = ITD.Instance;
            Mod achievements = ITDMod.achievements;
            if (achievements is null)
                return;
            string[] slayCosJel = {"Kill_" + NPCType<CosmicJellyfish>() };
            string[] parryCosJel = { "Event_ParryCosJelHand" };
            void AddAchievement(string nameNoSpaces, AchievementCategory category, string texturePath, string? customBorderTexturePath, bool showProgressBar, bool showAchievementCard, float achievementCardOrder, string[] conditions)
            {
                achievements.Call("AddAchievement", ITDMod, nameNoSpaces, category, texturePath, customBorderTexturePath, showProgressBar, showAchievementCard, achievementCardOrder, conditions);
            }
            AddAchievement("CosmicConundrum", AchievementCategory.Slayer, "ITD/Achievements/CosmicConundrum", null, false, false, 2.5f, slayCosJel);
            AddAchievement("+PARRY", AchievementCategory.Challenger, "ITD/Achievements/CosJelParry", null, false, false, 2.6f, parryCosJel);
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
            {
                string name = "CosmicJellyfish";
                BossChecklistProgressionValues.TryGetValue(name, out float prog);
                int type = NPCType<CosmicJellyfish>();
                List<int> collectibles =
                [
                    ItemType<CosmicJellyfishRelic>(),
                    ItemType<CosmicJam>(),
                    ItemType<CosmicJellyfishTrophy>(),
                ];
                int spawnItem = ItemType<SpacePrawn>();
                AddBoss(bossChecklist, ITDMod, name, prog, DownedCosmicJellyfish, type, new Dictionary<string, object>()
                {
                    ["spawnItems"] = spawnItem,
                    ["collectibles"] = collectibles,
                });
            }
        }
        private static void MunchiesSupport()
        {
            ITD ITDMod = ITD.Instance;
            Mod munchies = ITDMod.munchies;
            if (munchies is null)
                return;
            object[] driversIncense =
            {
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
            };
            munchies.Call(driversIncense);
        }
        private static void DialogueTweakSupport()
        {
            /// <summary>
            /// Add a button for specific NPCs.
            /// </summary>
            /// <param name="npcType">NPC ID(s) is needed. Use <see cref="ModContent.NPCType"/> to submit your ID. You can also tweak vanilla NPCs by using vanilla NPC ID.</param>
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
            void Behavior()
            {
                if (Main.mouseLeft)
                {
                    RecruitmentButton.DoRecruit();
                }
            }
            AddButton(NPCsThatCanBeRecruited, () => buttonName, () => toTex, Behavior);
        }
    }
}
