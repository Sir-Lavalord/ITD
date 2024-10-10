﻿using ITD.Content.NPCs.Bosses;
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
using Terraria.Localization;
using Microsoft.Xna.Framework;
using System.Linq;
using ITD.Content.Projectiles.Friendly.Misc;
using ITD.Utilities;
using ITD.Networking;
using ITD.Networking.Packets;
using ReLogic.Graphics;
using Terraria.GameContent;
using ITD.Systems.Recruitment;
using Terraria.ModLoader.IO;

namespace ITD.Players
{
    public class ITDPlayer : ModPlayer
    {
        public float[] itemVar = new float[4];  // used for storing info about held items, like wormhole ripper's current charge
		int heldItem;
		
		public float recoilFront = 0f;
		public float recoilBack = 0f;

        public Vector2 MousePosition { get; set; }
		
        bool prevTime = false;
        bool curTime = false;

        public bool ZoneDeepDesert;
        public bool ZoneBlueshroomsUnderground;
		public bool ZoneCatacombs;

		public bool necrosis = false;
		public bool soulRot = false;

		public float blockChance = 0f;
		public bool dreadBlock = false;

        public bool razedWine = false;
        public int razedCooldown = 0;

        public bool setAlloy = false;
        //Drawlayer nonsense
        public int frameCounter = 0;
        public int frameEffect = 0;
        public bool CosJellSuffocated;
        public const int CosJellEscapeMax = 10;
        public int CosJellEscapeCurrent;
        //Screenshake
        public int Screenshake;
        //Recruitment data
        public RecruitmentData recruitmentData = new();
        public override void ResetEffects()
        {
            //Screenshake
            if (Screenshake > 0)
                Screenshake--;
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

			necrosis = false;
			soulRot = false;

			blockChance = 0f;
			dreadBlock = false;

            razedWine = false;
			
            setAlloy = false;
        }
		
		public override void UpdateDead()
        {
            CosJellSuffocated = false;
            CosJellEscapeCurrent = 0;
            if (Screenshake > 0)
                Screenshake--; 
            itemVar = new float[4];
		}
		
		public override void UpdateBadLifeRegen()
        {
			if (necrosis)
			{
				if (Player.lifeRegen > 0)
                    Player.lifeRegen = 0;
				
				Player.lifeRegenTime = 0;
				 
				Player.lifeRegen -= 10;
			}
			if (soulRot)
			{
				if (Player.lifeRegen > 0)
                    Player.lifeRegen = 0;
				
				Player.lifeRegenTime = 0;
				 
				Player.lifeRegen -= 20;
			}
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
            if (razedWine)
            {
                if (razedCooldown > 0)
                {
                    razedCooldown--;
                }
            }
        }
		
        public override void PreUpdate()
        {
            ZoneBlueshroomsUnderground = ModContent.GetInstance<ITDSystem>().bluegrassCount > 50 && (Player.ZoneDirtLayerHeight || Player.ZoneRockLayerHeight);
            ZoneDeepDesert = ModContent.GetInstance<ITDSystem>().deepdesertTileCount > 50 && Player.ZoneRockLayerHeight;

            UpdateMouse();

            prevTime = curTime;
            curTime = Main.dayTime;

            NaturalSpawns.CosmicJellyfishSpawn(curTime, prevTime, Player);
                //Suffocation Here
        }
        public void UpdateMouse()
        {
            if (Player.IsLocalPlayer())
            {
                MousePosition = Main.MouseWorld;
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    NetSystem.SendPacket(new MousePositionPacket(Player));
                }
            }
        }
        public override void ModifyScreenPosition()
        {
            if (Screenshake > 0)
            {
                Main.screenPosition += Main.rand.NextVector2Circular(4, 4);
            }
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
		public PlayerDeathReason DeathByLocalization(string key)
        {
            string death = Language.GetTextValue($"Mods.ITD.DeathMessage.{key}");
            return PlayerDeathReason.ByCustomReason($"{Player.name} {death}");
        }
		public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
			if (damage == 10.0 && hitDirection == 0 && damageSource.SourceOtherIndex == 8)
            {
                if (necrosis)
                    damageSource = DeathByLocalization("Necrosis");
				if (soulRot)
                    damageSource = DeathByLocalization("SoulRot");
            }
			return true;
		}
        public NPC[] GetNearbyNPCs(float distance, bool ignoreFriendly = true)
        {
            List<NPC> npcs = [];
            foreach (var npc in Main.ActiveNPCs)
            {
                if (npc.WithinRange(Player.Center, distance))
                {
                    if (!npc.CanBeChasedBy() && ignoreFriendly)
                        continue;
                    npcs.Add(npc);
                }
            }
            return [.. npcs];
        }
        public override void OnEnterWorld()
        {

        }
        private sealed class ITDPlayerSystem : ModSystem // there's no OnExitWorld hook :death:
        {
            public override void PreSaveAndQuit()
            {
                base.PreSaveAndQuit();
                Player player = Main.CurrentPlayer;
                NaturalSpawns.LeaveWorld();
                PhysicsMethods.ClearAll();

                // transform recruited npc back to original type
                RecruitmentData recruitmentData = player.GetITDPlayer().recruitmentData;
                if (recruitmentData.WhoAmI > -1)
                    Main.npc[recruitmentData.WhoAmI].Transform(recruitmentData.OriginalType);
            }
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
			
			if (necrosis)
			{
				r = 0.7f;
				g = 0.4f;
				b = 0.9f;
			}
			
			if (soulRot)
			{
				r = 0.4f;
				g = 0.7f;
				b = 0.9f;
			}
		}
    }
}
