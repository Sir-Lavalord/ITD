using ITD.Content.NPCs.Bosses;
using ITD.Physics;
using System.Collections.Generic;
using static ITD.ITD;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;

namespace ITD.Players
{
    public class ITDPlayer : ModPlayer
    {
        bool prevTime = false;
        bool curTime = false;

        bool cosJelCounter = false;
        int cosJelTimer = 0;
        private readonly int cosJelTime = 60 * 80;

        public bool ZoneDeepDesert;
        public bool ZoneBlueshroomsUnderground;

        public bool setAlloy = false;

        readonly float gravityForPhysics = 0.5f;
        public override void ResetEffects()
        {
            setAlloy = false;
        }
        public override void PostUpdateEquips()
        {
            if (setAlloy)
            {
                Player.GetDamage(DamageClass.Melee) += 0.1f;
                Player.GetDamage(DamageClass.Ranged) += 0.08f;
                Player.GetDamage(DamageClass.Magic) += 0.06f;
                Player.GetDamage(DamageClass.Summon) += 0.06f;
                Player.endurance += 0.02f;
            }
        }
        public override void PreUpdate()
        {
            ZoneBlueshroomsUnderground = ModContent.GetInstance<ITDSystem>().bluegrassCount > 50 && (Player.ZoneDirtLayerHeight || Player.ZoneRockLayerHeight);
            ZoneDeepDesert = ModContent.GetInstance<ITDSystem>().deepdesertTileCount > 50 && Player.ZoneRockLayerHeight;

            // Random boss spawns

            prevTime = curTime;
            curTime = Main.dayTime;
            if (prevTime && !curTime) // It has just turned into nighttime
            {
                if (!ITDSystem.hasMeteorFallen) // If the hasMeteorFallen flag is false, it checks for a meteor
                {
                    bool found = false;
                    for (int i = 0; i < Main.maxTilesX && !found; i++) // Loop through every horizontal tile
                    {
                        for (int j = 0; j < Main.maxTilesY; j++) // For each horizontal tile, loop through every column of that tile
                        {
                            Tile tile = Main.tile[i, j];
                            if (tile.TileType == TileID.Meteorite)
                            {
                                ITDSystem.hasMeteorFallen = true;
                                found = true;
                                break;
                            }
                        }
                    }
                }
                if (NPC.downedBoss1 && ITDSystem.hasMeteorFallen && (Player.ZoneOverworldHeight || Player.ZoneSkyHeight) && !cosJelCounter && !DownedBossSystem.downedCosJel)
                {
                    if (Main.rand.NextBool(3))
                    {
                        Main.NewText("It's going to be a wiggly night...", Color.Purple);
                        cosJelCounter = true;
                    }
                }
            }
            if (cosJelCounter)
            {
                cosJelTimer++;
                if (cosJelTimer > cosJelTime)
                {
                    cosJelTimer = 0;
                    cosJelCounter = false;
                    SoundEngine.PlaySound(SoundID.Roar, Player.position);

                    int type = ModContent.NPCType<CosmicJellyfish>();

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        NPC.SpawnOnPlayer(Player.whoAmI, type);
                    }
                    else
                    {
                        NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: Player.whoAmI, number2: type);
                    }
                }
            }
        }
        public override void OnEnterWorld()
        {
            cosJelCounter = false;
            cosJelTimer = 0;
            PhysicsMethods.ClearAll();
        }
    }
}
