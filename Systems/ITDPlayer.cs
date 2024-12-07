using Microsoft.Xna.Framework;

using System;
using System.Linq;
using System.Collections.Generic;

using ReLogic.Graphics;

using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria;
using Terraria.Localization;
using Terraria.GameContent;
using Terraria.ModLoader.IO;

using ITD.Systems;
using ITD.Content.NPCs;
using ITD.Content.NPCs.Bosses;
using ITD.Content.Items.Weapons.Melee;
using ITD.Content.Dusts;
using ITD.Physics;
using ITD.Systems.Recruitment;
using ITD.Content.Projectiles.Friendly.Misc;
using ITD.Utilities;
using ITD.Networking;
using ITD.Networking.Packets;
using ITD.Content.UI;
using Terraria.GameInput;
using log4net.Core;
using ITD.Content.Buffs.EquipmentBuffs;
using ITD.Content.Projectiles.Friendly.Summoner;
using ITD.Content.Items.Accessories.Gimmicks.IncomprehensiblePolyhedron;

namespace ITD.Players
{
    public class ITDPlayer : ModPlayer
    {
        public float[] itemVar = new float[4];
		int heldItem;

        public int charge = 0;
        public float recoilFront = 0f;
		public float recoilBack = 0f;

        public Vector2 MousePosition { get; set; }
		
        bool prevTime = false;
        bool curTime = false;

        public bool ZoneDeepDesert;
        public bool ZoneBlueshroomsUnderground;
        public bool ZoneBlueshroomsSurface;
		public bool ZoneCatacombs;

		public bool necrosis = false;
		public bool soulRot = false;
        public bool melomycosis = false;

		public float blockChance = 0f;
		public bool dreadBlock = false;

        public bool razedWine = false;
        public int razedCooldown = 0;

        public bool portableLab = false;

        public bool soulTalisman = false;
        public bool soulTalismanEffect = false;
        public int soulTalismanStack = 0;
        public int soulTalismanTally = 0;

        public bool setAlloy_Melee = false;
        public bool setAlloy_Ranged = false;
        public bool setAlloy_Magic = false;
        public bool setAlloy { get { return setAlloy_Melee ||  setAlloy_Ranged || setAlloy_Magic; } }

        public int DebuffCount;
		
        //Drawlayer nonsense
        public int frameCounter = 0;
        public int frameEffect = 0;
        public bool CosJellSuffocated;
        public const int CosJellEscapeMax = 10;
        public int CosJellEscapeCurrent;
        //shakeDuration
        public int shakeDuration;
        public float shakeIntensityX;
        public float shakeIntensityY;
        public bool shakeDecay;
        //Recruitment data
        public RecruitmentData recruitmentData = new();
        public override void ResetEffects()
        {
            //shakeDuration
            if (shakeDuration > 0)
                shakeDuration--;
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
                charge = 0;
                heldItem = Player.inventory[Player.selectedItem].type;
			}
			if (recoilFront > 0f)
				recoilFront -= 0.02f;
			if (recoilBack > 0f)
				recoilBack -= 0.02f;

			necrosis = false;
			soulRot = false;
            melomycosis = false;

			blockChance = 0f;
			dreadBlock = false;

            razedWine = false;

            if (!soulTalisman)
            {
                Player.ClearBuff(ModContent.BuffType<SoulTalismanBuff>());
                soulTalismanStack = 0;
                soulTalismanTally = 0;
            }
            if (!soulTalismanEffect)
            {
                Player.ClearBuff(ModContent.BuffType<SoulTalismanBuff>());
                soulTalismanStack = 0;
            }
            soulTalismanEffect = false;
            soulTalisman = false;
            portableLab = false;
						
            setAlloy_Melee = false;
            setAlloy_Ranged = false;
            setAlloy_Magic = false;
        }
		public override void UpdateDead()
        {
            soulTalismanEffect = false;
            soulTalismanTally = 0;
            soulTalismanStack = 0;
            CosJellSuffocated = false;
            CosJellEscapeCurrent = 0;
            if (shakeDuration > 0)
                shakeDuration--; 
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
            if (melomycosis)
            {
                int defenseCalc = Player.statDefense / 5;
                if (Player.lifeRegen > 0)
                    Player.lifeRegen = 0;

                Player.lifeRegenTime = 0;

                Player.lifeRegen -= 5 + defenseCalc;
            }
		}
        public override void PostUpdateEquips()
        {
            if (setAlloy)
            {
                Player.endurance += 0.02f;
                if (setAlloy_Melee)
                    Player.GetDamage(DamageClass.Melee) += 0.1f;
                if (setAlloy_Ranged)
                    Player.GetDamage(DamageClass.Ranged) += 0.08f;
                if (setAlloy_Magic)
                    Player.GetDamage(DamageClass.Magic) += 0.06f;
            }
            if (razedWine)
            {
                if (razedCooldown > 0)
                {
                    razedCooldown--;
                }
            }
            if (soulTalisman)
            {
                if (soulTalismanStack >= 4)
                {
                    soulTalismanStack = 4;
                }
            }
            if (soulTalismanEffect)
            {
                Player.GetDamage<GenericDamageClass>() += 0.04f * (1 + soulTalismanStack);
                Player.GetAttackSpeed<GenericDamageClass>() += 0.03f * (1 + soulTalismanStack);
                Player.endurance += 0.01f * (1 + soulTalismanStack);
            }
        }
        public void BetterScreenshake(int dur, float powerX, float powerY, bool Decay)
        {
            shakeDuration = dur;
            shakeIntensityX = powerX; shakeIntensityY = powerX;
            shakeDecay = Decay;
        }
        public override void PreUpdate()
        {
            ITDSystem system = ModContent.GetInstance<ITDSystem>();
            ZoneBlueshroomsSurface = system.bluegrassCount > 50 && Player.ZoneOverworldHeight;
            ZoneBlueshroomsUnderground = system.bluegrassCount > 50 && (Player.ZoneDirtLayerHeight || Player.ZoneRockLayerHeight);
            ZoneDeepDesert = system.deepdesertTileCount > 50 && Player.ZoneRockLayerHeight;

            Player.ManageSpecialBiomeVisuals("BlackMold", melomycosis);

            UpdateMouse();

            prevTime = curTime;
            curTime = Main.dayTime;

            NaturalSpawns.CosmicJellyfishSpawn(curTime, prevTime, Player);
                //Suffocation Here
        }
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            // see if player just right clicked on an ITDNPC to call OnRightClick
            if (Main.mouseRight && Main.mouseRightRelease)
            {
                foreach (var npc in Main.ActiveNPCs)
                {
                    if (npc.Hitbox.Contains(MousePosition.ToPoint()) && npc.ModNPC is ITDNPC itdNPC)
                    {
                        itdNPC.OnRightClick(Player);
                    }
                }
            }
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
            if (shakeDuration > 0)
            {
                if (shakeDecay)
                {
                    shakeIntensityY = float.Lerp(shakeIntensityY, 0, 0.025f);

                    shakeIntensityX = float.Lerp(shakeIntensityX, 0, 0.025f);
                }
                Main.screenPosition += Main.rand.NextVector2Circular(shakeIntensityX, shakeIntensityY);
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (soulTalisman)
            {
                soulTalismanTally++;
                if (soulTalismanTally >= 20)
                {
                    soulTalismanTally = 0;
                    soulTalismanEffect = true;
                    Player.AddBuff(ModContent.BuffType<SoulTalismanBuff>(), 600);
                }
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
            if (soulTalismanEffect)
            {
                BetterScreenshake(10, 5, 5, true);
                Projectile Blast = Projectile.NewProjectileDirect(Player.GetSource_FromThis(), Player.Center, Vector2.Zero,
        ModContent.ProjectileType<SoulTalismanBlast>(), (int)Player.GetDamage(DamageClass.Generic).ApplyTo(80 * (soulTalismanStack + 1)), 20f, Player.whoAmI);
                Blast.ai[1] = 80 * (soulTalismanStack + 1);
                Blast.localAI[1] = Main.rand.NextFloat(0.1f, 0.3f);
                Blast.netUpdate = true;
                soulTalismanEffect = false;
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
                if (melomycosis)
                    damageSource = DeathByLocalization("Melomycosis");
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
                Player player = Main.CurrentPlayer;
                NaturalSpawns.LeaveWorld();
                PhysicsMethods.ClearAll();

                // transform recruited npc back to original type
                RecruitmentData recruitmentData = player.GetITDPlayer().recruitmentData;
                if (recruitmentData.WhoAmI > -1)
                    TownNPCRecruitmentLoader.Unrecruit(recruitmentData.WhoAmI, player);
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
