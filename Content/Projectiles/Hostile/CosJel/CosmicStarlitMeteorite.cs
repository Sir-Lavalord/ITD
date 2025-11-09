using ITD.Content.NPCs.Bosses;
using ITD.Particles;
using ITD.Particles.CosJel;
using ITD.Particles.Projectiles;
using ITD.Utilities;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;

namespace ITD.Content.Projectiles.Hostile.CosJel;

public class CosmicStarlitMeteorite : ITDProjectile
{
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5; // The length of old position to be recorded
        ProjectileID.Sets.TrailingMode[Projectile.type] = 0; // The recording mode
        Main.projFrames[Projectile.type] = 1;
    }

    public VertexStrip TrailStrip = new();
    readonly int defaultWidthHeight = 8;
    public ParticleEmitter emitter;
    public override void SetDefaults()
    {
        Projectile.width = 64;
        Projectile.height = 64;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 90000;
        Projectile.light = 0.5f;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        DrawOffsetX = -16;
        DrawOriginOffsetY = -16;
        Projectile.hide = false;
        Projectile.alpha = 0;
        emitter = ParticleSystem.NewEmitter<SpaceMist>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
        emitter.tag = Projectile;
    }
    public override void OnSpawn(IEntitySource source)
    {
        Projectile.scale = 0f;
        if (emitter != null)
            emitter.keptAlive = true;
    }

    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White * (1f - Projectile.alpha / 255f);
    }
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        NPC owner = MiscHelpers.NPCExists(OwnerIndex, ModContent.NPCType<CosmicJellyfish>());
        if (owner == null)
        {
            Projectile.timeLeft = 0;
            Projectile.active = false;
            return base.Colliding(projHitbox, targetHitbox);
        }
        return base.Colliding(projHitbox, targetHitbox);

    }
    float innerScale = 1f;

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D outline = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        //this sprite should be remade, i got this from fargo
        Texture2D texture2 = Mod.Assets.Request<Texture2D>("Content/Projectiles/Friendly/Mage/TwilightDemiseHorribleThing").Value;
        Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
        Rectangle frame2 = texture2.Frame(1, Main.projFrames[Type], 0, Projectile.frame);

        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Texture2D deathTex = TextureAssets.Extra[ExtrasID.SharpTears].Value;
        Rectangle deathFrame = deathTex.Frame(1, Main.projFrames[Type], 0, Projectile.frame);

        void DrawAtProj(Texture2D tex)
        {
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, new Vector2(tex.Width * 0.5f, tex.Height / Main.projFrames[Type] * 0.5f), innerScale * Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
        }

        if (Projectile.timeLeft >= 20)
        {
            innerScale = 1f;
        }
        emitter?.InjectDrawAction(ParticleEmitterDrawStep.BeforePreDrawAll, () => 
        Main.EntitySpriteDraw(texture2, Projectile.Center - Main.screenPosition, frame2, new Color(122, 0, 208, 0) * Main.essScale,
        Projectile.rotation, new Vector2(texture2.Width * 0.5f, texture2.Height / Main.projFrames[Type] * 0.5f), innerScale * Projectile.scale * 1.25f *  Main.essScale, SpriteEffects.None, 0f));

        emitter?.InjectDrawAction(ParticleEmitterDrawStep.BeforePreDrawAll, () => DrawAtProj(outline));
        if (Projectile.timeLeft >= 5)
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, new Vector2(texture.Width * 0.5f, texture.Height / Main.projFrames[Type] * 0.5f), innerScale * Projectile.scale, SpriteEffects.None, 0f);

        return false;
    }
    public override int ProjectileShader(int originalShader)
    {
        return GameShaders.Armor.GetShaderIdFromItemId(ItemID.TwilightDye);
    }
    public int OwnerIndex => (int)Projectile.ai[0];
    public override bool? CanDamage()
    {
        return Projectile.localAI[0] >= 120;
    }
    public override void AI()
    {
        NPC owner = MiscHelpers.NPCExists(OwnerIndex, ModContent.NPCType<CosmicJellyfish>());
        if (owner == null)
        {
            Projectile.timeLeft = 0;
            Projectile.active = false;
            return;
        }
        if (++Projectile.localAI[0] < 120) //feed us and we'll grow
        {
            Projectile.Center = Vector2.Lerp(Projectile.Center, owner.Center + new Vector2(Projectile.ai[1], Projectile.ai[2]), 0.05f);
            if (!Main.dedServ)
            {
                if (emitter is null)
                {
                    emitter = ParticleSystem.NewEmitter<TwilightDemiseFlash>(ParticleEmitterDrawCanvas.WorldOverProjectiles);
                    emitter.additive = true;
                }
                emitter.keptAlive = true;
                emitter.timeLeft = 180;

                for (int i = 0; i <= 3; i++)
                {
                    emitter?.Emit(Projectile.Center + Main.rand.NextVector2Circular(Projectile.width, Projectile.height), Vector2.Zero);
                }

            }
        }
        else
        {
            if (!Main.dedServ)
            {
                for (int i = 0; i <= 3; i++)
                {
                    emitter?.Emit(Projectile.Center + Main.rand.NextVector2Circular(Projectile.width, Projectile.height), Vector2.Zero);
                    emitter.scale *= 10;

                }
                if (emitter is null)
                {
                    emitter = ParticleSystem.NewEmitter<TwilightDemiseFlash>(ParticleEmitterDrawCanvas.WorldOverProjectiles);
                    emitter.additive = true;
                }
                emitter.keptAlive = true;
                emitter.timeLeft = 180;
                if (Projectile.localAI[0] == 120)
                {

                    for (int i = 0; i < 18; i++)
                    {
                        emitter?.Emit(Projectile.Center, Vector2.UnitX.RotatedByRandom(Math.PI) * Main.rand.NextFloat(0.9f, 1.1f) * 20);
                    }

                }
            }

            Projectile.scale = MathHelper.Clamp(Projectile.scale + 0.1f, 0, 2);
            Projectile.velocity *= 0.5f;

        }
    }
    public override void OnKill(int timeLeft)
    {
        NPC owner = MiscHelpers.NPCExists(OwnerIndex, ModContent.NPCType<CosmicJellyfish>());
        if (owner == null)
        {
            Projectile.timeLeft = 0;
            Projectile.active = false;
            return;
        }
        int amount = 20;
        for (int i = 0; i < amount; i++)
        {

            double rad = Math.PI / (amount / 2) * i;
            int damage = (int)(Projectile.damage * 0.28f);
            int knockBack = 3;
            float speed = 12f;
            Vector2 vector = Vector2.Normalize(Vector2.UnitY.RotatedBy(rad)) * speed;
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vector, ModContent.ProjectileType<CosmicStar>(), damage, knockBack, Main.myPlayer, 0, 1);
            }
        }
    }
}
