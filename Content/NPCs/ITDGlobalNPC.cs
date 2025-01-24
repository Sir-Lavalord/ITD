using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

using ITD.Content.Buffs.Debuffs;
using ITD.Content.Buffs.FavorBuffs;
using ITD.Content.Items.Accessories.Defensive;
using ITD.Content.Items.Accessories.Master;
using ITD.Content.Items.BossSummons;
using ITD.Content.Items.Other;
using ITD.Content.Items.Favors.Prehardmode;
using ITD.Content.Items.Weapons.Melee.Snaptraps;
using ITD.Content.NPCs.BlueshroomGroves.Critters;
using ITD.Content.Projectiles.Friendly.Melee.Snaptraps.Extra;
using ITD.Content.Projectiles.Friendly.Ranger;
using ITD.Utilities;
using ITD.Content.Items.Accessories.Combat.All;
using ITD.Content.Buffs.EquipmentBuffs;
using ITD.Content.Items.Weapons.Mage;
using ITD.Content.Events;


namespace ITD.Content.NPCs
{
    public class ITDGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool zapped;
        public bool necrosis;
        public bool soulRot;
        public bool toasted;
        public bool melomycosis;
        public bool chilled;
        public bool bleedingII;
        public bool toppled;
		
		public bool haunted;
		public bool haunting;
		public int hauntingProgress = 0;
		
        public float originalKB;
        private int chilledTimer = 0;
        private const int MAX_CHILLED_DURATION = 60;
		
		
        //shit
        public static int cosjelBoss = -1;

        private static int[] shouldDropSandberusSummon =
            [
            NPCID.WalkingAntlion,
            NPCID.FlyingAntlion,
            NPCID.GiantWalkingAntlion,
            NPCID.GiantFlyingAntlion,
            NPCID.TombCrawlerHead,
            NPCID.LarvaeAntlion
            ];
        public override void ResetEffects(NPC npc)
        {
            zapped = false;
            chilled = false;
            melomycosis = false;
            necrosis = false;
            soulRot = false;
            toasted = false;
            bleedingII = false;
			haunted = false;
			if (!haunting && hauntingProgress > 0)
                hauntingProgress--;
			haunting = false;
            if (!toppled)
                npc.knockBackResist = originalKB;
			toppled = false;
        }

        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            originalKB = npc.knockBackResist;
        }
        
        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (toasted)
            {
                modifiers.Defense *= ToastedBuff.DefenseMultiplier;
            }
			if (haunted)
            {
                modifiers.SourceDamage += HauntedBuff.DamageTakenMultiplier;
            }
        }
        public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
        {
            if (toasted)
            {
                modifiers.SourceDamage += ToastedBuff.DamageMultiplier;
            }
        }
        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (toppled)
            {
                if (npc.type != NPCID.TargetDummy)//awful fix but it's fine since target dummy is the only one like this
                {
                    if (projectile.type == ModContent.ProjectileType<HunterrGreatarrow>())
                        npc.knockBackResist += 0.05f;
                }
            }
        }
        public override void OnKill(NPC npc)
        {
            if (npc.AnyInteractions())
            {
                Player player = Main.player[npc.lastInteraction];//the man behind the slaughter
                var modPlayer = player.GetITDPlayer();
                if (npc.CanBeChasedBy())//no critter
                {
                    if (modPlayer.soulTalisman)
                    {
                        modPlayer.soulTalismanEffect = true;
                        player.AddBuff(ModContent.BuffType<SoulTalismanBuff>(), 400);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = 0; i <= 2; i++)
                            {
                                Vector2 vel = Main.rand.NextVector2Circular(5, 5);
                                Projectile.NewProjectile(npc.GetSource_Death(), npc.Center, vel, ProjectileID.SpectreWrath,
                                    (int)player.GetDamage(DamageClass.Generic).ApplyTo(30 * (modPlayer.soulTalismanStack + 1)), 0, Main.myPlayer);
                            }
                        }
                    }
                }
            }
            EventsSystem.OnKill(npc);
        }
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (necrosis)
            {
                if (npc.lifeRegen > 0)
                {
                    npc.lifeRegen = 0;
                }
                npc.lifeRegen -= 20;

                if (damage < 5)
                    damage = 5;
            }

            if (soulRot)
            {
                if (npc.lifeRegen > 0)
                {
                    npc.lifeRegen = 0;
                }
                npc.lifeRegen -= 60;

                if (damage < 10)
                    damage = 10;
            }

            if (toasted)
            {
                if (npc.lifeRegen > 0)
                {
                    npc.lifeRegen = 0;
                }
                npc.lifeRegen -= 60;

                if (damage < 10)
                    damage = 10;
            }

            if (melomycosis)
            {
                int defenseCalc = npc.defense;
                if (npc.lifeRegen > 0)
                {
                    npc.lifeRegen = 0;
                }
                npc.lifeRegen -= 60 + defenseCalc * 2;

                if (damage < defenseCalc / 2)
                    damage = defenseCalc / 2;
            }

            if (bleedingII)
            {
                if (npc.lifeRegen > 0)
                {
                    npc.lifeRegen = 0;
                }
                npc.lifeRegen -= 30;

                if (damage < 15)
                    damage = 15;
            }
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 ScreenPosition, Color drawColor)
        {
            if (chilled)
            {
                int MAX_CHILLED_DURATION = npc.width * npc.height;
                chilledTimer += 2;

                float scale = MathHelper.Lerp(0.5f, 1.5f, (float)chilledTimer / MAX_CHILLED_DURATION);
                Texture2D iceTexture = ModContent.Request<Texture2D>("ITD/Content/Projectiles/Friendly/Melee/Snaptraps/Extra/FrostgripIceCube").Value;

                Vector2 drawOrigin = new Vector2(iceTexture.Width / 2, iceTexture.Height / 2);
                Vector2 drawPosition = npc.position - Main.screenPosition + new Vector2(npc.width / 2, npc.height / 2);
                spriteBatch.Draw(iceTexture, drawPosition, null, drawColor * ((float)chilledTimer / MAX_CHILLED_DURATION), 0f, drawOrigin, scale, SpriteEffects.None, 0f);

                if (chilledTimer >= MAX_CHILLED_DURATION)
                {
                    chilledTimer = 0;

                    int projType = ModContent.ProjectileType<FrostgripIceCube>();
                    int projectileWidth = (int)(iceTexture.Width * scale);
                    int projectileHeight = (int)(iceTexture.Height * scale);

                    npc.SimpleStrikeNPC(3000, 0, false, 0f, null, true, 0, false);

                    Projectile iceProjectile = Projectile.NewProjectileDirect
                    (
                        npc.GetSource_FromAI(),
                        npc.Center,
                        Vector2.Zero,
                        projType,
                        0,
                        0f,
                        Main.myPlayer
                    );

                    iceProjectile.width = projectileWidth;
                    iceProjectile.height = projectileHeight;
                    iceProjectile.scale = scale;
                }
            }
        }

    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            if (npc.type == NPCID.EaterofSouls || npc.type == NPCID.BigEater || npc.type == NPCID.LittleEater)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Soulsnapper>(), 50));
            }
            if (npc.type == NPCID.Crimera || npc.type == NPCID.LittleCrimera || npc.type == NPCID.BigCrimera)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Crimeratrap>(), 50));
            }
            if (shouldDropSandberusSummon.Contains(npc.type))
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DuneSkull>(), 50));
            }
            if (npc.type == NPCID.BloodNautilus)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DreadShell>(), 10));
            }
            if (npc.type == NPCID.KingSlime)
            {
                npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<SlimeSecretionGland>()));
            }
            if (npc.type == NPCID.EyeofCthulhu)
            {
                npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<EyeofEldritchInsight>()));
            }
            if (npc.type == NPCID.EaterofWorldsHead || npc.type == NPCID.EaterofWorldsBody || npc.type == NPCID.EaterofWorldsTail)
            {
                LeadingConditionRule IsABoss = new LeadingConditionRule(new Conditions.LegacyHack_IsABoss());
                IsABoss.OnSuccess(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<EatersTail>()));
                npcLoot.Add(IsABoss);
            }
            if (npc.type == NPCID.BrainofCthulhu)
            {
                npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Prophylaxis>()));
            }
            if (npc.type == NPCID.QueenBee)
            {
                npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<RoyalJellyScepter>()));
            }
            if (npc.type == NPCID.SkeletronHead)
            {
                npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Cursebreaker>()));
            }
        }
        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            if (player.HasBuff(ModContent.BuffType<SqueakyClean>()))
            {
                float factor = SqueakyClean.SpawnrateMultiplier;
                spawnRate = (int)(spawnRate / factor);
                maxSpawns = (int)(maxSpawns * factor);
            }
            sbyte activeEvent = EventsSystem.ActiveEvent;
            if (activeEvent < 0)
                return;
            ITDEvent e = EventsSystem.EventsByID[activeEvent];
            e.ModifySpawnRate(player, ref spawnRate, ref maxSpawns);
        }
        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            sbyte activeEvent = EventsSystem.ActiveEvent;
            if (activeEvent < 0)
                return;
            ITDEvent e = EventsSystem.EventsByID[activeEvent];
            if (e.OverrideVanillaSpawns)
                pool.Clear();
            foreach (var item in e.GetPool(spawnInfo))
                pool[item.Item1] = item.Item2;
        }
        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (necrosis)
            {
                drawColor.R = 200;
                drawColor.G = 100;
                drawColor.B = 255;
            }
            if (soulRot)
            {
                drawColor.R = 100;
                drawColor.G = 200;
                drawColor.B = 255;
            }
            if (toasted)
            {
                drawColor.R = 255;
                drawColor.G = 100;
                drawColor.B = 255;
            }
            if (chilled)
            {
                drawColor.R = 173;
                drawColor.G = 216;
                drawColor.B = 230;
            }
			if (haunted)
            {
                drawColor.R /= 2;
                drawColor.G /= 2;
                drawColor.B /= 2;
            }
        }
    }
}
