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
            public static bool downedCosJel;

            public override void ClearWorld()
            {
                hasMeteorFallen = false;
                downedCosJel = false;
            }
            public override void SaveWorldData(TagCompound tag)
            {
                if (hasMeteorFallen)
                {
                    tag["hasMeteorFallen"] = true;
                }
                if (downedCosJel)
                {
                    tag["downedCosJel"] = true;
                }
            }

            public override void LoadWorldData(TagCompound tag)
            {
                hasMeteorFallen = tag.ContainsKey("hasMeteorFallen");
                downedCosJel = tag.ContainsKey("downedCosJel");
            }

            public override void NetSend(BinaryWriter writer)
            {
                var flags = new BitsByte();
                flags[0] = hasMeteorFallen;
                flags[1] = downedCosJel;
                writer.Write(flags);
            }

            public override void NetReceive(BinaryReader reader)
            {
                BitsByte flags = reader.ReadByte();
                hasMeteorFallen = flags[0];
                downedCosJel = flags[1];
            }

            /*public override void PreUpdateTime()
            {
                if (WorldGen.spawnMeteor)
                {
                    hasMeteorFallen = true;
                }
            }*/

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
                if (prevTime && !curTime) // It has just turned into nighttime
                { 
                    if (!ITDSystem.hasMeteorFallen) // If the hasMeteorFallen flag is false, it checks for a meteor
                    {
                        bool found = false;
                        for (int i = 0;i < Main.maxTilesX && !found; i++) // Loop through every horizontal tile
                        {
                            for (int j = 0;j < Main.maxTilesY; j++) // For each horizontal tile, loop through every column of that tile
                            {
                                Tile tile = Main.tile[i, j];
                                if (tile.TileType == TileID.Meteorite)
                                {
                                    ITDSystem.hasMeteorFallen = true; // Set the flag to true
                                    found = true; // Break the outer loop
                                    break; // Break the inner loop
                                }
                            }
                        }
                    }
                    if (NPC.downedBoss1 && ITDSystem.hasMeteorFallen && (Player.ZoneOverworldHeight||Player.ZoneSkyHeight) && (!cosJelCounter) && (!ITDSystem.downedCosJel))
                    {
                        Random rand = new();
                        if (rand.Next(3) == 0)
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

    public static class Helpers
    {
        public static Color ColorFromHSV(float hue, float saturation, float value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            float f = hue / 60 - (float)Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            switch (hi)
            {
                case 0:
                    return new Color(v, t, p);
                case 1:
                    return new Color(q, v, p);
                case 2:
                    return new Color(p, v, t);
                case 3:
                    return new Color(p, q, v);
                case 4:
                    return new Color(t, p, v);
                default:
                    return new Color(v, p, q);
            }
        }
        public static Vector2? RecursiveRaycast(Vector2 startWorldPos, float approxLengthTiles, float currentLengthTiles)
        {
            if (!(currentLengthTiles > approxLengthTiles))
            {
                currentLengthTiles++;
                if (Collision.SolidCollision(startWorldPos, 1, 1))
                {
                    return startWorldPos;
                }
                else
                {
                    return RecursiveRaycast(startWorldPos + new Vector2(0f, 8f), approxLengthTiles, currentLengthTiles);
                }
            }
            return null;
        }
    }
}
