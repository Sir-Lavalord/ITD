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
    }
}
