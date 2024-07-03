using ITD.Content.Items.Placeable;
using ITD.Content.Items.PetSummons;
using ITD.Content.Items.BossSummons;
using ITD.Content.NPCs.Bosses;
using System;
using System.Collections.Generic;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static ITD.Downed;

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
    }
}
