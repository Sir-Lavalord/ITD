using ITD.Content.Dusts;
using ITD.Content.Items.Weapons.Ranger;
using ITD.PrimitiveDrawing;
using ITD.Utilities;
using System;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;

namespace ITD.Content.Projectiles.Friendly.Ranger;


public class TheEpicenterBlackhole : ModProjectile
{
    public MiscShaderData Shader = new MiscShaderData(Main.VertexPixelShaderRef, "MagicMissile")
        .UseProjectionMatrix(true)
        .UseImage0("Images/Extra_" + 192)
        .UseImage1("Images/Extra_" + 194)
        .UseImage2("Images/Extra_" + 190)
        .UseSaturation(-4f)
        .UseOpacity(2f);

    public VertexStrip TrailStrip = new();
    public override void SetDefaults()
    {
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.width = 120; Projectile.height = 120;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.light = 1f;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 1800;
    }
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

    private NPC HomingTarget
    {
        get => Projectile.ai[0] == 0 ? null : Main.npc[(int)Projectile.ai[0] - 1];
        set
        {
            Projectile.ai[0] = value == null ? 0 : value.whoAmI + 1;
        }
    }
    Vector2 spawnMousePos;
    int CurrentBulletCount = 0;
    int totalDamage = 0;
    // readonly int MaxBulletCount = 100;
    // readonly int Time = 60;
    int TimeBeforeRetract;
    int TimeWithoutWeapon;
    bool Retracting;
    public override void OnSpawn(IEntitySource source)
    {
        if (Main.myPlayer == Projectile.owner)
        {
            SoundStyle blackholeSpawn = new SoundStyle("ITD/Content/Sounds/NPCSounds/Bosses/CosjelBlackholeStart") with
            {
                Volume = 1.25f,
                PitchVariance = 0.1f,
                MaxInstances = 1,
                SoundLimitBehavior = SoundLimitBehavior.IgnoreNew
            };
            SoundEngine.PlaySound(blackholeSpawn, Player.Center);
            spawnMousePos = Main.MouseWorld;
        }
    }
    public override bool? CanDamage()
    {
        return false;
    }
    Player Player => Main.player[Projectile.owner];
    public override void AI()
    {
        Projectile.spriteDirection = (Projectile.velocity.X < 0).ToDirectionInt();

        if (!Retracting)
        {
            int dustRings = 2;
            for (int h = 0; h < dustRings; h++)
            {
                float distanceDivisor = h + 1.5f;
                float dustDistance = 300 / distanceDivisor;
                int numDust = (int)(0.1f * MathHelper.TwoPi * dustDistance);
                float angleIncrement = MathHelper.TwoPi / numDust;
                Vector2 dustOffset = new(dustDistance, 0f);
                dustOffset = dustOffset.RotatedByRandom(MathHelper.TwoPi);

                int var = (int)dustDistance;
                float dustVelocity = 20f / distanceDivisor;
                for (int i = 0; i < numDust; i++)
                {
                    if (Main.rand.NextBool(var))
                    {
                        dustOffset = dustOffset.RotatedBy(angleIncrement);
                        int dust = Dust.NewDust(Projectile.Center, 1, 1, ModContent.DustType<CosJelDust>());
                        Main.dust[dust].position = Projectile.Center + dustOffset;
                        Main.dust[dust].fadeIn = 1f;
                        Main.dust[dust].velocity = Vector2.Normalize(Projectile.Center - Main.dust[dust].position) * dustVelocity;
                        Main.dust[dust].scale = 1.5f - h;
                    }
                }
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 * 2;
        }
        else
        {
            Projectile.rotation = Projectile.DirectionTo(Player.Center).SafeNormalize(Vector2.Zero).ToRotation();

        }

        Projectile.timeLeft = 5;

        if (CurrentBulletCount < 100)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];
                if (i != Projectile.whoAmI &&
                    other.active &&
                    other.aiStyle == 1
                    &&
                    other.owner == Player.whoAmI
                    && Math.Abs(Projectile.Center.X - other.position.X)
                    + Math.Abs(Projectile.Center.Y - other.position.Y) < 120)
                {
                    ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.Excalibur, new ParticleOrchestraSettings
                    {
                        PositionInWorld = Projectile.Center,
                    }, Projectile.whoAmI);
                    other.active = false;
                    other.netUpdate = true;
                    Projectile.localAI[1] += 20f;
                    CurrentBulletCount++;
                    totalDamage += (int)(other.damage * (other.GetGlobalProjectile<ITDInstancedGlobalProjectile>().ProjectileSource == ITDInstancedGlobalProjectile.ProjectileItemSource.TheEpicenter ? 1.25f : 1f));

                }
            }
        }
        if (Projectile.localAI[1] > 0f)
            Projectile.localAI[1] -= 5f;
        if (Projectile.localAI[1] < 0f)
            Projectile.localAI[1] += 5f;
        if (Projectile.localAI[1] > 100f)
            Projectile.localAI[1] = 100f;
        Projectile.rotation += 0.05f;
        if (!Retracting)
        {

            Projectile.Center = Vector2.Lerp(Projectile.Center, spawnMousePos, 0.2f);
        }
        else
        {
            if (Projectile.Distance(Player.Center) <= 50)
            {
                Projectile.Kill();
            }
            /*                Projectile.rotation = Projectile.DirectionTo(player.Center).SafeNormalize(Vector2.Zero).ToRotation() + MathHelper.PiOver2;
            */
            Projectile.Center = Vector2.Lerp(Projectile.Center, Player.Center, 0.2f);
        }
        if (totalDamage <= 0)
        {
            totalDamage = 0;
        }
        if (Projectile.Distance(spawnMousePos) >= 40)
        {
        }
        else
        {

            if (Player.HeldItem.ModItem is not TheEpicenter)
            {
                TimeWithoutWeapon++;
            }
            else TimeWithoutWeapon = 0;
            if (TimeBeforeRetract++ >= 120)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    if (Main.mouseRight && Player.HeldItem.ModItem is TheEpicenter ||
/*                            TimeWithoutWeapon >= 600 ||
*/                            TimeBeforeRetract >= 2500 && (HomingTarget == null || CurrentBulletCount == 0)
                        )
                    {
                        Retracting = true;
                    }
                }
            }
            Projectile.velocity *= 0.95f;
            HomingTarget ??= Projectile.FindClosestNPC(1500);

            if (HomingTarget == null)
            {
                if (CurrentBulletCount > 0)
                {
                    if (Projectile.localAI[2]++ % 180 == 0)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            int dust = Dust.NewDust(Projectile.Center, 1, 1, ModContent.DustType<StarlitDust>(), 0f, 0f, 0, default, 1.5f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].velocity *= 2f;
                        }
                        CurrentBulletCount--;
                        Projectile.localAI[1] -= 30f;
                    }
                }
                return;
            }
            if (!HomingTarget.active || HomingTarget.life <= 0 || !HomingTarget.CanBeChasedBy())
            {
                HomingTarget = null;
                return;
            }
            if (CurrentBulletCount > 0)
            {
                if (Projectile.localAI[0]++ % 12 == 0)
                {
                    float dmg = totalDamage / CurrentBulletCount + 1;
                    totalDamage -= totalDamage / CurrentBulletCount + 1;

                    CurrentBulletCount--;
                    if (Main.myPlayer == Projectile.owner)
                    {
                        Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center,
                            (HomingTarget.Center - Projectile.Center).SafeNormalize(Vector2.Zero).RotatedByRandom(MathHelper.ToRadians(3)) * 22f,
                            ModContent.ProjectileType<TheEpicenterSpark>(), (int)(dmg * 1.1f), Projectile.knockBack, Projectile.owner, 1);
                        proj.tileCollide = false;
                        proj.CritChance = (int)Player.GetTotalCritChance<RangedDamageClass>();
                    }
                    Projectile.localAI[1] += 25f;
                    for (int i = 0; i < 10; i++)
                    {
                        int dust = Dust.NewDust(Projectile.Center, 1, 1, ModContent.DustType<StarlitDust>(), 0f, 0f, 0, default, 1.5f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].velocity *= 2f;
                    }
                }
            }
        }
    }
    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White;
    }
    private Color StripColors(float progressOnStrip)
    {
        Color result = Color.Lerp(Color.Black, Color.White, Utils.GetLerpValue(0f, 0.7f, progressOnStrip, true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, false));
        result.A /= 2;
        return result * Projectile.Opacity;
    }
    private float StripWidth(float progressOnStrip)
    {
        return 120f;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D effectTexture = TextureAssets.Extra[ExtrasID.SharpTears].Value;
        Vector2 effectOrigin = effectTexture.Size() / 2f;
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        float scaleX = 1f;
        float scaleY = 2.5f;
        Rectangle rectangle = texture.Frame(1, 1);
        Player player = Main.player[Projectile.owner];
        lightColor = Lighting.GetColor((int)player.Center.X / 16, (int)player.Center.Y / 16);
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        if (Projectile.Distance(spawnMousePos) >= 40 || Retracting)
        {
            Shader.Apply(null);
            TrailStrip.PrepareStrip(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, Projectile.Size * 0.5f - Main.screenPosition, Projectile.oldPos.Length, true);
            TrailStrip.DrawTrail();
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            Main.EntitySpriteDraw(effectTexture, drawPosition, null, Color.White, Projectile.rotation + MathHelper.PiOver2, effectTexture.Size() / 2f, new Vector2(scaleX, scaleY), SpriteEffects.None, 0);
        }
        else
        {
            GameShaders.Misc["Blackhole"]
                .UseImage0(TextureAssets.Extra[ExtrasID.MagicMissileTrailErosion])
                .UseColor(new Color(133, 50, 88))
                .UseSecondaryColor(Color.Beige)
                .Apply();
            float scaleUp = 200 + Projectile.localAI[1];
            SimpleSquare.Draw(Projectile.Center - Main.screenPosition, size: new
                Vector2(scaleUp, scaleUp));
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }
        return false;
    }
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        modifiers.HitDirectionOverride = (Projectile.Center.X < target.Center.X).ToDirectionInt();
    }
}