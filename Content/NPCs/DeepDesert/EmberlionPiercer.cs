using ITD.Content.Items.Materials;
using ITD.Content.Items.Weapons.Melee;
using System;
using Terraria.GameContent.ItemDropRules;

namespace ITD.Content.NPCs.DeepDesert;

public class EmberlionPiercer : ModNPC
{
    private readonly Asset<Texture2D> glowmask = ModContent.Request<Texture2D>("ITD/Content/NPCs/DeepDesert/EmberlionPiercer_Glow");
    private enum ActionState
    {
        Asleep,
        Noticed,
        Dashing,
        Chasing
    }
    private ActionState AI_State;
    private float glowmaskOpacity;
    public int dashingTimer;
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[NPC.type] = 4;

        NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
    }
    public override void SetDefaults()
    {
        glowmaskOpacity = 0f;
        AI_State = ActionState.Asleep;
        NPC.width = 72;
        NPC.height = 36;
        NPC.damage = 60;
        NPC.defense = 20;
        NPC.lifeMax = 150;
        NPC.HitSound = SoundID.NPCHit31;
        NPC.DeathSound = SoundID.NPCDeath34;
        NPC.knockBackResist = 0f;
        NPC.value = Item.buyPrice(silver: 4);
        NPC.aiStyle = NPCAIStyleID.Fighter;
        AIType = NPCID.GiantWalkingAntlion;
    }
    public override bool PreAI()
    {
        Player player = Main.player[NPC.target];
        Vector2 toPlayerTotal = player.Center - NPC.Center;
        Vector2 toPlayer = toPlayerTotal.SafeNormalize(Vector2.Zero);
        switch (AI_State)
        {
            case ActionState.Asleep:
                NPC.TargetClosest(false);
                if (Collision.CanHitLine(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height) && toPlayerTotal.Length() < 16f * 60f)
                {
                    AI_State = ActionState.Noticed;
                }
                return false;
            case ActionState.Noticed:
                if (glowmaskOpacity < 1f)
                {
                    glowmaskOpacity += 0.01f;
                }
                else
                {
                    if (Collision.CanHitLine(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height) && toPlayerTotal.Length() < 10f * 60f)
                    {
                        AI_State = ActionState.Dashing;
                    }
                }
                return false;
            case ActionState.Dashing:
                NPC.TargetClosest(true);
                NPC.velocity += toPlayer * 0.8f;
                NPC.velocity.X = Math.Clamp(NPC.velocity.X, -8f, 8f);
                dashingTimer++;
                Dust.NewDust(NPC.Center, NPC.width, NPC.height, DustID.Torch);
                Dust.NewDust(NPC.Center, NPC.width, NPC.height, DustID.Flare);
                Dust.NewDust(NPC.Center, NPC.width, NPC.height, DustID.SolarFlare);
                if (dashingTimer < 3)
                {
                    Gore.NewGoreDirect(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, 62);
                    Gore.NewGoreDirect(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, 61);
                    Gore.NewGoreDirect(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, 63);
                }
                if (dashingTimer > 30)
                {
                    AI_State = ActionState.Chasing;
                }
                return false;
            default: return true;
        }
    }
    public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
    {
        if (AI_State == ActionState.Dashing)
        {
            target.AddBuff(BuffID.OnFire, 60 * 5);
        }
    }
    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        spriteBatch.Draw(glowmask.Value, NPC.position - screenPos + new Vector2(0f, 2f + NPC.gfxOffY), NPC.frame, Color.White * glowmaskOpacity, 0f, Vector2.Zero, 1f, NPC.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, default);
    }
    public override void FindFrame(int frameHeight)
    {
        NPC.spriteDirection = NPC.direction;
        if (NPC.IsOnStandableGround() && AI_State == ActionState.Chasing)
        {
            NPC.frameCounter += 1f;
            if (NPC.frameCounter > 5f)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;

                if (NPC.frame.Y > frameHeight * Main.npcFrameCount[Type] - 1)
                {
                    NPC.frame.Y = frameHeight;
                }
            }
        }
    }
    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        if (spawnInfo.Player.ITD().ZoneDeepDesert)
        {
            return 0.15f;
        }
        return 0f;
    }
    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<EmberlionSclerite>(), 1, 1, 2));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<EmberSlasher>(), 20));
    }
}
