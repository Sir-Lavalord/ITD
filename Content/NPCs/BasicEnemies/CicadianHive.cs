using ITD.Content.Biomes;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ITD.Content.NPCs.BasicEnemies
{
    public class CicadianHive : ModNPC
    {
        public static LocalizedText BestiaryEntry { get; private set; }
        private enum ActionState
        {
            Hovering,
            Escaping
        }
        private ActionState AI_State;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 7;
            BestiaryEntry = Language.GetOrRegister(Mod.GetLocalizationKey($"NPCs.{nameof(CicadianHive)}.Bestiary"));
        }
        public override void SetDefaults()
        {
            AI_State = ActionState.Hovering;
            NPC.width = 66;
            NPC.height = 82;
            NPC.damage = 30;
            NPC.defense = 10;
            NPC.lifeMax = 200;
            NPC.HitSound = SoundID.NPCHit31;
            NPC.DeathSound = SoundID.NPCDeath34;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(silver: 4);
            NPC.noGravity = true;
            NPC.aiStyle = -1;
            SpawnModBiomes = [ModContent.GetInstance<BlueshroomGrovesBiome>().Type];
        }
        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 1f;
            if (NPC.frameCounter > 4f)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;

                if (NPC.frame.Y > frameHeight * (Main.npcFrameCount[Type] - 1))
                {
                    NPC.frame.Y = frameHeight;
                }
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (AI_State == ActionState.Hovering)
            {
                AI_State = ActionState.Escaping;
            }
        }
        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest();
            }
            Player player = Main.player[NPC.target];

            int tileRange = 16;
            int hoverDistance = 16;

            float maxRotation = MathHelper.Pi / 6;
            float rotationFactor = MathHelper.Clamp(NPC.velocity.X / 8f, -1f, 1f);
            float rotation = rotationFactor * maxRotation;
            NPC.rotation = rotation;

            switch (AI_State)
            {
                case ActionState.Hovering:
                    Vector2 ground = Helpers.QuickRaycast(NPC.Center, Vector2.UnitY, 32);
                    Vector2 hoverPoint = ground - new Vector2(0f, hoverDistance * 16);
                    Vector2 toHoverPoint = (hoverPoint - NPC.Center) / 32f;
                    NPC.velocity = Vector2.Lerp(NPC.velocity, toHoverPoint, 0.05f);
                    if (Vector2.Distance(NPC.Center, player.Center) < tileRange * 16)
                    {
                        AI_State = ActionState.Escaping;
                    }
                    break;
                case ActionState.Escaping:
                    float speed = 800f;
                    Vector2 toPlayer = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * speed;
                    Vector2 escapeVector = -toPlayer / (Vector2.Distance(NPC.Center, player.Center) * 1f);
                    escapeVector.Y /= 4f;
                    NPC.velocity = Vector2.Lerp(NPC.velocity, escapeVector, 0.1f);
                    if (Vector2.Distance(NPC.Center, player.Center) > tileRange * 16)
                    {
                        AI_State = ActionState.Hovering;
                    }
                    break;
            }
        }
    }
}
