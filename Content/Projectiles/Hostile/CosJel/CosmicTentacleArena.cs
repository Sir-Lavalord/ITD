using ITD.Content.Dusts;
using ITD.Content.NPCs.Bosses;
using ITD.Utilities;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.UI;

namespace ITD.Content.Projectiles.Hostile.CosJel;

public class CosmicTentacleArena : ModProjectile
{

    private static Asset<Texture2D> segmentA = ModContent.Request<Texture2D>("ITD/Content/Projectiles/Hostile/CosJel/CosmicTentacleSegmentA");
    private static Asset<Texture2D> segmentB = ModContent.Request<Texture2D>("ITD/Content/Projectiles/Hostile/CosJel/CosmicTentacleSegmentB");
    public override string Texture => ITD.BlankTexture;

    public override void Load()
    {
    }

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 3000;
    }
    private enum ActionState
    {
        Extend,
        Spinning,
        Retract,
    }
    private ActionState AI_State;
    public override void SetDefaults()
    {
        Projectile.netImportant = true;
        Projectile.width = 48;
        Projectile.height = 48;
        Projectile.friendly = false; 
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.hide = true;
        Projectile.timeLeft = 99999;
        Projectile.tileCollide = false;
    }
    /*    public bool FinalStar => Projectile.ai[1] == 1;
        public bool RoundTrip => Projectile.ai[2] == 1;*/
    public override void OnSpawn(IEntitySource source)
    {
        NPC CosJel = Main.npc[(int)Projectile.ai[0]];
        if (CosJel.active && CosJel.type == ModContent.NPCType<CosmicJellyfish>())
        {
        }
        else
        {
            Projectile.Kill();
        }
    }
    public override bool? CanDamage()
    {
        return zapDog;
    }
    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        if (Projectile.hide)
            behindNPCs.Add(index);
    }
    public bool zapDog = false;
    float sweepDir = 1;
    public override void AI()
    {
        NPC CosJel = Main.npc[(int)Projectile.ai[0]];
        if (CosJel.active && CosJel.type == ModContent.NPCType<CosmicJellyfish>())
        {
            float distAway = 1500;
            float circleTime = 180;
            switch (AI_State)
            {             
                case ActionState.Extend:
                    if (Projectile.localAI[0]++ < 60)
                    {
                        Projectile.Center = Vector2.Lerp(Projectile.Center, CosJel.Center + new Vector2(0, -distAway), 0.05f);
                    }
                    else if (Projectile.localAI[0] >= 60)
                    {
                        Projectile.Center = CosJel.Center + new Vector2(0, -distAway).RotatedBy(Projectile.localAI[1]);
                        AI_State = ActionState.Spinning;
                        Projectile.localAI[0] = 0;
                        zapGlow = 1;
                    }
                    break;
                case ActionState.Spinning:
                    if (Projectile.localAI[0]++ >= 60)
                    {
                        Projectile.localAI[1] -= ((float)Math.PI / circleTime) * sweepDir;

                        Projectile.Center = CosJel.Center + new Vector2(0, -distAway).RotatedBy(Projectile.localAI[1]);
                        zapDog = true;
                        if (zapDog)
                        {
                            if (Projectile.localAI[0] % 4 == 0)
                            {
                                Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<CosmicLightningOrb>(),
                                    Projectile.damage, 0, 0);
                                proj.timeLeft = 60 * 30;
                            }
                            for (int i = 0; i < 2; i++)
                            {
                                int dust = Dust.NewDust(Projectile.position, Projectile.width / 2, Projectile.height / 2, DustID.Electric, 0, 0, 0, default, 2f);
                                Main.dust[dust].noGravity = true;
                                Main.dust[dust].velocity = Vector2.UnitX.RotatedByRandom(Math.PI) * Main.rand.NextFloat(0.9f, 1.1f) * 3;
                            }
                        }
                        if (Projectile.localAI[2]++ >= circleTime * 2)
                        {
                            AI_State = ActionState.Retract;
                            zapDog = false;
                        }
                    }
                    else
                    {
                        if (CosJel.HasPlayerTarget)
                        {
                            sweepDir = Main.player[CosJel.target].Center.X > CosJel.Center.X ? -1 : 1;
                        }
                    }
                    break;
                case ActionState.Retract:
                    Projectile.Center = Vector2.Lerp(Projectile.Center, CosJel.Center, 0.05f);
                    if (Vector2.Distance(Projectile.Center, CosJel.Center) <= 50)
                    {
                        Projectile.Kill();
                    }
                    break;
            }
            return;
        }
        else
        {
            Projectile.Kill();
        }
        zapGlow -= 0.05f;

    }
    bool expertMode = Main.expertMode;
    bool masterMode = Main.masterMode;

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        float num1 = 0f;
        if (expertMode || masterMode)
        {
            NPC CosJel = Main.npc[(int)Projectile.ai[0]];

            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                Projectile.Center, CosJel.Center,
                18f * Projectile.scale, ref num1))
            {

                return true;
            }
        }
        return base.Colliding(projHitbox, targetHitbox);
    }
    float zapGlow = 1;

    public override bool PreDraw(ref Color lightColor)
    {
        NPC CosJel = Main.npc[(int)Projectile.ai[0]];

        Vector2 cosjelPos = CosJel.Center;
        Rectangle? segmentSourceRectangle = null;
        float segmentHeightAdjustment = 0f;
        Vector2 segmentOrigin = segmentSourceRectangle.HasValue ? (segmentSourceRectangle.Value.Size() / 2f) : (segmentA.Size() / 2f);
        Vector2 segmentDrawPosition = Projectile.Center;
        Vector2 vectorFromProjectileToCosJel = cosjelPos.MoveTowards(segmentDrawPosition, 4f) - segmentDrawPosition;
        Vector2 unitvectorFromProjectileToCosJel = vectorFromProjectileToCosJel.SafeNormalize(Vector2.Zero);
        float segmentSegmentLength = (segmentSourceRectangle.HasValue ? segmentSourceRectangle.Value.Height : segmentA.Height()) + segmentHeightAdjustment;
        if (segmentSegmentLength == 0)
        {
            segmentSegmentLength = 44;
        }
        float segmentRotation = unitvectorFromProjectileToCosJel.ToRotation() + MathHelper.PiOver2;
        int segmentCount = 0;
        float segmentLengthRemainingToDraw = vectorFromProjectileToCosJel.Length() + segmentSegmentLength / 2f;
        float time = Main.GlobalTimeWrappedHourly;
        float timer = (float)Main.time / 240f + time * 0.04f;

        time %= 4f;
        time /= 2f;

        if (time >= 1f)
        {
            time = 2f - time;
        }

        time = time * 2f + 1f;


        while (segmentLengthRemainingToDraw > 0f)
        {
            var segmentTextureToDraw = segmentB;

            if (segmentCount >= 1)
            {
                segmentTextureToDraw = segmentA;
            }
            else
            {
                segmentTextureToDraw = segmentB;
            }
            Color segmentDrawColor = Lighting.GetColor((int)segmentDrawPosition.X / 16, (int)(segmentDrawPosition.Y / 16f));
            if (expertMode || masterMode)
            {
                for (float i = 0f; i < 1f; i += 0.35f)
                {
                    float radians = (i + timer) * MathHelper.TwoPi;

                    Main.spriteBatch.Draw(segmentTextureToDraw.Value, segmentDrawPosition - Main.screenPosition + new Vector2(0f, 4).RotatedBy(radians) * time, segmentSourceRectangle, new Color(90, 70, 255, 50) * Projectile.Opacity, segmentRotation, segmentOrigin, 1f, SpriteEffects.None, 0f);

                }
            }
            if (zapGlow > 0)//fargo eridanus epic
            {
                float scale = 3f * Projectile.scale * (float)Math.Cos(Math.PI / 2 * zapGlow);
                float opacity = Projectile.Opacity * (float)Math.Sqrt(zapGlow);
                Main.spriteBatch.Draw(segmentTextureToDraw.Value, segmentDrawPosition - Main.screenPosition, segmentSourceRectangle, new Color(90, 70, 255, 50) * opacity, segmentRotation, segmentOrigin, 1f * scale, SpriteEffects.None, 0f);
            }
            Main.spriteBatch.Draw(segmentTextureToDraw.Value, segmentDrawPosition - Main.screenPosition, segmentSourceRectangle, segmentDrawColor, segmentRotation, segmentOrigin, 1f,  0, 0f);
            segmentDrawPosition += unitvectorFromProjectileToCosJel * segmentSegmentLength;
            segmentCount++;
            segmentLengthRemainingToDraw -= segmentSegmentLength;
        }
        return true;
    }
}