using ITD.Content.Dusts;
using ITD.Content.Projectiles.Hostile;
using ITD.Content.Tiles.BlueshroomGroves;
using System;
using Terraria.Audio;
using Terraria.GameContent;

namespace ITD.Content.NPCs.BlueshroomGroves;

public class ShroomishSlime : ModNPC
{
    public bool canTriggerLanding = false;
    public ref float AITimer => ref NPC.ai[0];
    public ref float AIIsActiveOrOppositeDirTimer => ref NPC.ai[1];
    public ref float AIBigJumpXPosition => ref NPC.ai[2];
    public Vector2 stretchScale = Vector2.One;

    private readonly Asset<Texture2D> glow = ModContent.Request<Texture2D>("ITD/Content/NPCs/BlueshroomGroves/ShroomishSlime_Glow");
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[NPC.type] = 2;
    }
    public override void SetDefaults()
    {
        NPC.damage = 26;
        //NPC.aiStyle = NPCAIStyleID.Slime;
        AnimationType = NPCID.BlueSlime;
        NPC.width = 28;
        NPC.height = 36;
        NPC.defense = 8;
        NPC.lifeMax = 80;
        NPC.value = Item.buyPrice(silver: 4);
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
    }

    public override void AI() // slime ai but evil
    {
        bool aggro = false;
        if (!Main.dayTime || NPC.life != NPC.lifeMax || NPC.position.Y > Main.worldSurface * 16.0 || Main.slimeRain) // this part makes the slime chase you
        {
            aggro = true;
        }

        if (AIBigJumpXPosition > 1f)
        {
            AIBigJumpXPosition -= 1f;
        }
        if (NPC.wet)
        {
            if (NPC.collideY)
            {
                NPC.velocity.Y = -2f;
            }
            if (NPC.velocity.Y < 0f && NPC.ai[3] == NPC.position.X)
            {
                NPC.direction *= -1;
                AIBigJumpXPosition = 200f;
            }
            if (NPC.velocity.Y > 0f)
            {
                NPC.ai[3] = NPC.position.X;
            }
            if (NPC.velocity.Y > 2f)
            {
                NPC.velocity.Y = NPC.velocity.Y * 0.9f;
            }
            NPC.velocity.Y = NPC.velocity.Y - 0.5f;
            if (NPC.velocity.Y < -4f)
            {
                NPC.velocity.Y = -4f;
            }
            if (AIBigJumpXPosition == 1f & aggro)
            {
                NPC.TargetClosest(true);
            }
            canTriggerLanding = false; // ow my canTriggerLanding
        }

        NPC.aiAction = 0;
        if (AIBigJumpXPosition == 0f)
        {
            AITimer = -100f;
            AIBigJumpXPosition = 1f;
            NPC.TargetClosest(true);
        }
        if (NPC.velocity.Y == 0f)
        {
            if (canTriggerLanding) // the slime has landed, commence the canTriggerLanding
            {
                if (Main.expertMode && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + NPC.velocity * 4, new Vector2(6f, 0), ModContent.ProjectileType<Sporeflake>(), 15, 0, -1);
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + NPC.velocity * 4, new Vector2(-6f, 0), ModContent.ProjectileType<Sporeflake>(), 15, 0, -1);
                }
                SoundEngine.PlaySound(SoundID.NPCHit1, NPC.Center);
                stretchScale.X = 2f;
                stretchScale.Y = 0.5f;
                canTriggerLanding = false;
            }
            if (NPC.collideY && NPC.oldVelocity.Y != 0f && Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
            {
                NPC.position.X = NPC.position.X - (NPC.velocity.X + NPC.direction);
            }
            if (NPC.ai[3] == NPC.position.X)
            {
                NPC.direction *= -1;
                AIBigJumpXPosition = 200f;
            }
            NPC.ai[3] = 0f;
            NPC.velocity.X = NPC.velocity.X * 0.8f;
            if (NPC.velocity.X > -0.1 && NPC.velocity.X < 0.1)
            {
                NPC.velocity.X = 0f;
            }
            if (aggro)
            {
                AITimer += 1f;
            }
            AITimer += 1f;
            float num31 = -1000f;
            int num32 = 0;
            if (AITimer >= 0f)
            {
                num32 = 1;
            }
            if (AITimer >= num31 && AITimer <= num31 * 0.5f)
            {
                num32 = 2;
            }
            if (AITimer >= num31 * 2f && AITimer <= num31 * 1.5f)
            {
                num32 = 3;
            }
            if (num32 > 0)
            {
                NPC.netUpdate = true;
                if (aggro && AIBigJumpXPosition == 1f)
                {
                    NPC.TargetClosest(true);
                }
                if (num32 == 3)
                {
                    stretchScale.Y = 2f;
                    stretchScale.X = 0.5f;
                    NPC.velocity.Y = -8f;
                    NPC.velocity.X = NPC.velocity.X + 3 * NPC.direction;
                    AITimer = -200f;
                    NPC.ai[3] = NPC.position.X;
                }
                else
                {
                    stretchScale.Y = 2f;
                    stretchScale.X = 0.5f;
                    NPC.velocity.Y = -6f;
                    NPC.velocity.X = NPC.velocity.X + 2 * NPC.direction;
                    AITimer = -120f;
                    if (num32 == 1)
                    {
                        AITimer += num31;
                    }
                    else
                    {
                        AITimer += num31 * 2f;
                    }
                }
                canTriggerLanding = true; // prepare canTriggerLanding
            }
            else
            {
                if (AITimer >= -30f)
                {
                    NPC.aiAction = 1;
                    return;
                }
            }
        }
        else
        {
            if (NPC.target < 255 && ((NPC.direction == 1 && NPC.velocity.X < 3f) || (NPC.direction == -1 && NPC.velocity.X > -3f)))
            {
                if (NPC.collideX && Math.Abs(NPC.velocity.X) == 0.2f)
                {
                    NPC.position.X = NPC.position.X - 1.4f * NPC.direction;
                }
                if (NPC.collideY && NPC.oldVelocity.Y != 0f && Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                {
                    NPC.position.X = NPC.position.X - (NPC.velocity.X + NPC.direction);
                }
                if ((NPC.direction == -1 && NPC.velocity.X < 0.01) || (NPC.direction == 1 && NPC.velocity.X > -0.01))
                {
                    NPC.velocity.X = NPC.velocity.X + 0.2f * NPC.direction;
                    return;
                }
                NPC.velocity.X = NPC.velocity.X * 0.93f;
            }
        }
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        if (NPC.life > 0)
        {
            int i = 0;
            while (i < hit.Damage / (double)NPC.lifeMax * 50.0)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<BlueshroomSporesDust>(), hit.HitDirection, -1f, 0, default, 1f);
                i++;
            }
            return;
        }
        for (int j = 0; j < 20; ++j)
        {
            Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<BlueshroomSporesDust>(), 0f, 0f, 0, default, 1f);
        }
    }

    public override void FindFrame(int frameHeight)
    {
        stretchScale = Vector2.SmoothStep(stretchScale, Vector2.One, 0.25f);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D tex = TextureAssets.Npc[Type].Value;
        float texHeight = tex.Height / Main.npcFrameCount[Type];
        Vector2 origin = new(tex.Width / 2, texHeight);
        Vector2 offset = new(0f, NPC.gfxOffY + 1f + texHeight / 2f);
        spriteBatch.Draw(tex, NPC.Center - screenPos + offset, NPC.frame, drawColor, 0f, origin, stretchScale, SpriteEffects.None, 0f);
        spriteBatch.Draw(glow.Value, NPC.Center - screenPos + offset, NPC.frame, Color.White * BlueshroomTree.opac, 0f, origin, stretchScale, SpriteEffects.None, 0f);
        return false;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        if (spawnInfo.Player.ITD().ZoneBlueshroomsUnderground)
        {
            return 0.45f;
        }
        return 0f;
    }
}
