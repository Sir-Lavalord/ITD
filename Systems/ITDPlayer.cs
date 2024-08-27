using ITD.Content.NPCs.Bosses;
using ITD.Content.Items.Weapons.Melee;
using ITD.Content.Dusts;
using ITD.Physics;
using System;
using System.Collections.Generic;
using ITD.Systems;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria;
using Microsoft.Xna.Framework;

namespace ITD.Players
{
    public class ITDPlayer : ModPlayer
    {
        public float[] itemVar = new float[4];  // used for storing info about held items, like wormhole ripper's current charge
		int heldItem;
		
		public float recoilFront = 0f;
		public float recoilBack = 0f;
		
        bool prevTime = false;
        bool curTime = false;

        bool cosJelCounter = false;
        int cosJelTimer = 0;
        private readonly int cosJelTime = 60 * 80;

        public bool ZoneDeepDesert;
        public bool ZoneBlueshroomsUnderground;
		public bool ZoneCatacombs;

		public float blockChance = 0f;
		public bool dreadBlock = false;

        public bool setAlloy = false;
        //Drawlayer nonsense
        public int frameCounter = 0;
        public int frameEffect = 0;
        public bool CosJellSuffocated;
        public const int CosJellEscapeMax = 10;
        public int CosJellEscapeCurrent;

        public override void ResetEffects()
        {
            //Suffocrap
            if (CosJellSuffocated)
            {
                Player.velocity *= 0;
                Player.RemoveAllGrapplingHooks();
                if (CosJellEscapeCurrent <= 0)
                {
                    CosJellSuffocated = false;
                    Player.immune = true;//probably op
                    Player.immuneTime = 180;
                }
            }
            if ((Player.controlJump && Player.releaseJump))
            {
                CosJellEscapeCurrent--;

                if (CosJellEscapeCurrent <= 0)
                {
                    CosJellEscapeCurrent = 0;
                }
            }

            if (heldItem != Player.inventory[Player.selectedItem].type)
			{
				itemVar = new float[4];
				heldItem = Player.inventory[Player.selectedItem].type;
			}
			if (recoilFront > 0f)
				recoilFront -= 0.02f;
			if (recoilBack > 0f)
				recoilBack -= 0.02f;

			blockChance = 0f;
			dreadBlock = false;
			
            setAlloy = false;
        }
		public override void UpdateDead()
        {
			itemVar = new float[4];
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
                //Suffocation Here
                
            }
        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
			if (modifiers.Dodgeable && Main.rand.NextFloat(1f) < blockChance) // Chance to block attacks
			{
				SoundEngine.PlaySound(SoundID.NPCHit4, Player.Center);
				modifiers.DisableSound();
				modifiers.SetMaxDamage(1);
				if (dreadBlock) // Dread Shell block ability
				{
					for (int i = 0; i < Main.maxNPCs; i++)
					{
					NPC target = Main.npc[i];
						if (target.CanBeChasedBy() && target.Distance(Player.Center) < 200)
						{
							int damage = Main.DamageVar(200, Player.luck);
							target.StrikeNPC(new NPC.HitInfo
							{
								Damage = damage,
								Knockback = 2f,
								HitDirection = target.Center.X < Player.Center.X ? -1 : 1,
								Crit = false
							});
							target.AddBuff(BuffID.Confused, 300, false);
						}
					}
					for (int i = 0; i < 60; i++)
					{
						Vector2 offset = new Vector2();
						double angle = Main.rand.NextDouble() * 2d * Math.PI;
						offset.X += (float)(Math.Sin(angle) * 200);
						offset.Y += (float)(Math.Cos(angle) * 200);
						Vector2 spawnPos = Player.Center + offset - new Vector2(4, 0);
						Dust dust = Main.dust[Dust.NewDust(
							spawnPos, 0, 0,
							DustID.LifeDrain, 0, 0, 100, default, 1.5f
							)];
						dust.noGravity = true;
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
		public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
			if (heldItem == ModContent.ItemType<WormholeRipper>())
            {
                if (itemVar[0] > 0)
				{
					int dustType = ModContent.DustType<StarlitDust>();
					if (itemVar[0] == 3)
                    {
                        dustType = 204;
                    }
					int dust = Dust.NewDust(Player.MountedCenter - new Vector2(4f, 40f), 0, 0, dustType, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f, 100, default, 1f + itemVar[0] * 0.5f);
					Main.dust[dust].noGravity = true;
					Main.dust[dust].velocity.X = 0f;
					Main.dust[dust].velocity.Y = -3f;
					drawInfo.DustCache.Add(dust);
				}
            }
		}
    }
}
