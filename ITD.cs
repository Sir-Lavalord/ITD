using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.Server;
using ITD.Physics;
using Terraria.ID;
using Terraria.DataStructures;
using System;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader.IO;
using System.IO;
using Terraria.Audio;
using ITD.Content.NPCs;
using Terraria.Localization;

namespace ITD
{
    public class ITD : Mod
    {
        public class ITDSystem : ModSystem
        {
            public static bool hasMeteorFallen;

            public override void ClearWorld()
            {
                hasMeteorFallen = false;
            }
            public override void SaveWorldData(TagCompound tag)
            {
                if (hasMeteorFallen)
                {
                    tag["hasMeteorFallen"] = true;
                }
            }

            public override void LoadWorldData(TagCompound tag)
            {
                hasMeteorFallen = tag.ContainsKey("hasMeteorFallen");
            }

            public override void NetSend(BinaryWriter writer)
            {
                var flags = new BitsByte();
                flags[0] = hasMeteorFallen;
                writer.Write(flags);
            }

            public override void NetReceive(BinaryReader reader)
            {
                BitsByte flags = reader.ReadByte();
                hasMeteorFallen = flags[0];
            }

            public override void PostUpdateEverything()
            {
                if (WorldGen.spawnMeteor)
                {
                    NPC.SetEventFlagCleared(ref hasMeteorFallen, -1);
                }
            }

            public override void AddRecipeGroups()
            {
                RecipeGroup group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " Iron Ore", new int[]
                {
                ItemID.IronOre,
                ItemID.LeadOre
                });
                RecipeGroup.RegisterGroup("IronOre", group);
            }
        }
        public class ITDPlayer : ModPlayer
        {
            bool prevTime = false;
            bool curTime = false;

            bool cosJelCounter = false;
            int cosJelTimer = 0;
            int cosJelTime = 60 * 80;

            float gravityForPhysics = 0.5f;
            float stiffness = 1f;
            public override void PreUpdate() // I'm using this hook to simulate all of the physics
            {
                List<VerletPoint> pointsList = PhysicsMethods.GetPoints();

                foreach (VerletPoint point in pointsList) // Applies gravity to each point.
                {
                    if (!point.isVerletPinned && point != null)
                    {
                        Vector2 velocity = point.pos - point.oldPos;
                        point.oldPos = point.pos;
                        point.pos = point.pos + velocity;
                        point.pos = point.pos + new Vector2(0, gravityForPhysics);
                    }
                }
                List<VerletStick> sticksList = PhysicsMethods.GetSticks();

                foreach (VerletStick stick in sticksList) // Calculates the distance points should move, then changes their position according to that.
                {
                    for (int i = 0; i < 20; i++)
                    {
                        stick.Update();
                    }
                }

                // Random boss spawns

                prevTime = curTime;
                curTime = Main.dayTime;
                if (prevTime && !curTime)
                {

                    if (NPC.downedBoss1 && ITDSystem.hasMeteorFallen && Player.ZoneOverworldHeight && (!cosJelCounter))
                    {
                        Main.NewText("It's going to be a wiggly night...", Color.Purple);
                        cosJelCounter = true;
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
                PhysicsMethods.ClearAll();
            }
        }
    }
}
