using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.NPCs.Bosses
{
    public class CosmicJellyfishMini : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;

            NPCID.Sets.DontDoHardmodeScaling[Type] = true;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.ImmuneToAllBuffs[Type] = true;
            //
            NPCID.Sets.NPCBestiaryDrawModifiers bestiaryData = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Hide = true
            };
        }
        public bool forcedkill;
        public override void SetDefaults()
        {
            NPC.width = 36;
            NPC.height = 46;
            NPC.damage = 10;
            NPC.defense = 0;
            NPC.lifeMax = 100;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0.1f;
            NPC.netAlways = true;
            NPC.aiStyle = -1;
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = ImmunityCooldownID.Bosses; // use the boss immunity cooldown counter, to prevent ignoring boss attacks by taking damage from other sources
            return true;
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            forcedkill = true;
            NPC.checkDead();
            NPC.life = 0;
        }
        float speed = 6;
        float inertia = 40;
        float distanceToIdlePosition;
        float distance;
        public override void AI()
        {

            // WARNING: DO NOT FORGET THIS LINE(obvious but yes)
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest();
            }
            NPC.TargetClosest();
            if (!IsDashing)
            {
                if (NPC.localAI[2]++ >= 150)
                {
                    NPC.velocity *= 0.9f;
                    if (NPC.localAI[2]++ >= 200)//Have to stop first
                    {

                        IsDashing = true;
                    }
                }
                else
                {
                }
            }
            else
            {
                Dash(1, 10, 20, 30, 1);
            }
        }
        Vector2 dashvel;
        public bool IsDashing;
        public void Dash(int time1, int time2, int time3, int reset, int attackID)
        {
            IsDashing = true;
            Player player = Main.player[NPC.target];
            NPC.localAI[1]++;
            //very hard coded
            if (NPC.localAI[1] == time1)//set where to dash
            {
                dashvel = Vector2.Normalize(new Vector2(player.Center.X, player.Center.Y) - new Vector2(NPC.Center.X, NPC.Center.Y));
                NPC.velocity = dashvel;
                NPC.netUpdate = true;
            }

            if (NPC.localAI[1] > time1 && NPC.localAI[1] < time2)//xcel
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.netUpdate = true;

                    NPC.velocity *= 1.18f;
                    NPC.netUpdate = true;
                }
            }
            if (NPC.localAI[1] > time3) //Decelerate 
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.netUpdate = true;

                    NPC.velocity *= 0.96f;
                    NPC.netUpdate = true;

                }
            }
            if (NPC.localAI[1] >= reset)//
            {
                NPC.localAI[0] = 0;
                NPC.localAI[1] = 0;
                NPC.localAI[2] = 0;
                IsDashing = false;
            }

        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)          //this make so when the npc has 0 life(dead) he will spawn this
            {
            }
        }
        public override void OnSpawn(IEntitySource source)
        {
            for (int i = 0; i < 12; i++)
            {
                Dust dust = Dust.NewDustDirect(NPC.Center, NPC.width, NPC.height, DustID.ShimmerTorch, 0, 0f, 40, default, Main.rand.NextFloat(2f, 3f));
                dust.velocity *= 2f;
                Dust dust2 = Dust.NewDustDirect(NPC.Center, NPC.width, NPC.height, DustID.ShimmerTorch, 0, 0f, 40, default, Main.rand.NextFloat(2f, 3f));
                dust2.velocity *= 1f;
                dust2.noGravity = true;
            }
        }
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            forcedkill = true;
            NPC.checkDead();
            NPC.life = 0;
        }
        public override void OnKill()
        {
            Player closestPlayer = Main.player[Player.FindClosest(NPC.position, NPC.width, NPC.height)];
        }
        public override void DrawBehind(int index)
        {
            // The 6 available positions are as follows: // Behind tiles and walls
            Main.instance.DrawCacheProjsBehindNPCs.Add(index);
        }
    }
}