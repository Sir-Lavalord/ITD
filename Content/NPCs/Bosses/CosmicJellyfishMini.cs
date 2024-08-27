using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
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

            Player player = Main.player[NPC.target];
            Vector2 idlePosition = player.Center + new Vector2(NPC.ai[1], NPC.ai[2]);
            Vector2 vectorToIdlePosition = idlePosition - NPC.Center;
            Vector2 vectorToIdlePositionNorm = vectorToIdlePosition.SafeNormalize(Vector2.UnitY);
            distanceToIdlePosition = vectorToIdlePosition.Length();
            //speed is dependant on attack
            if (distanceToIdlePosition > 10f)
            {
                vectorToIdlePosition.Normalize();
                vectorToIdlePosition *= speed;
            }
            NPC.velocity = (NPC.velocity * (inertia - 2) + vectorToIdlePosition) / inertia;

            NPC.netUpdate = true;
            NPC.rotation = NPC.velocity.X / 50;
            if (Main.player[NPC.target].Distance(NPC.Center) < 250f)
            {
                speed = 6;
            }
            else
            {
                speed = 9;
            }
            Vector2 velo = Vector2.Normalize(new Vector2(player.Center.X, player.Center.Y) - new Vector2(NPC.Center.X, NPC.Center.Y));
            NPC.rotation = (float)Math.Atan2((double)velo.Y, (double)velo.X) + 1.57f;

        }
        int dustcolor;
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)          //this make so when the npc has 0 life(dead) he will spawn this
            {
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