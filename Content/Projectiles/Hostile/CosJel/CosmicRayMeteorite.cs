using ITD.Content.NPCs.Bosses;
using ITD.Particles;
using ITD.Utilities;
using System;
using System.Threading;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;

namespace ITD.Content.Projectiles.Hostile.CosJel;

public class CosmicRayMeteorite : ModProjectile
{
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
    }
    public override void SetDefaults()
    {
        Projectile.width = 40;
        Projectile.height = 40;
        Projectile.aiStyle = 0;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.ignoreWater = true;
        Projectile.light = 1f;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 9900;
        Projectile.penetrate = -1;
    }
    public override void AI()
    {
        Projectile.rotation += 0.05f;
        if (Projectile.timeLeft > 30)
        {
            Projectile.Resize(150, 150);
            Projectile.scale = MathHelper.Clamp(Projectile.scale + 0.05f, 0, 3);
        }
        else
            Projectile.scale = MathHelper.Clamp(Projectile.scale - 0.2f, 0, 3);
        if (Main.essScale >= 1)
        {
            for (int i = 0; i < 20; i++)
            {
                int dust = Dust.NewDust(Projectile.Center, 1, 1, DustID.UltraBrightTorch, 0, 0, 0, default, 1f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity = Vector2.UnitX.RotatedByRandom(Math.PI) * Main.rand.NextFloat(0.9f, 1.1f) * 10;
            }
        }
    }
    public int OwnerIndex => (int)Projectile.ai[0];
    public override bool? CanDamage()
    {
        return Projectile.scale >= 2f;
    }
    public override void OnSpawn(IEntitySource source)
    {
        Projectile.scale = 0;
    }
    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White;
    }
    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 16; i++)
        {
            int dust = Dust.NewDust(Projectile.Center, 1, 1, DustID.Meteorite, 0, 0, 0, default, 1f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity = Vector2.UnitX.RotatedByRandom(Math.PI) * Main.rand.NextFloat(0.9f, 1.1f) * 4;
        }
        for (int i = 0; i < 16; i++)
        {
            int dust = Dust.NewDust(Projectile.Center, 1, 1, DustID.MeteorHead, 0, 0, 0, default, 1f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity = Vector2.UnitX.RotatedByRandom(Math.PI) * Main.rand.NextFloat(0.9f, 1.1f) * 10;
        }
        NPC owner = MiscHelpers.NPCExists(OwnerIndex, ModContent.NPCType<CosmicJellyfish>());
        if (owner == null)
        {
            Projectile.timeLeft = 0;
            Projectile.active = false;
            return;
        }
        else
        {
            Player player = Main.player[owner.target];
            Vector2 velocity = Vector2.Normalize(player.Center - Projectile.Center);
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero,
    ModContent.ProjectileType<CosmicSwordStar2>(), Projectile.damage, 0, -1, 0, 0, 60);
                proj.rotation = velocity.ToRotation();
            }
        }
    }
    public override bool PreDraw(ref Color lightColor)
    {

        Texture2D texture = TextureAssets.Projectile[Type].Value;
        //this sprite should be remade, i got this from fargo
        Texture2D texture2 = Mod.Assets.Request<Texture2D>("Content/Projectiles/Friendly/Mage/TwilightDemiseHorribleThing").Value;
        Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
        Rectangle frame2 = texture2.Frame(1, Main.projFrames[Type], 0, Projectile.frame);

        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Texture2D deathTex = TextureAssets.Extra[ExtrasID.SharpTears].Value;
        Rectangle deathFrame = deathTex.Frame(1, Main.projFrames[Type], 0, Projectile.frame);

        Main.EntitySpriteDraw(texture2, Projectile.Center - Main.screenPosition, frame2, new Color(122, 0, 208, 0) * Main.essScale,
        Projectile.rotation, new Vector2(texture2.Width * 0.5f, texture2.Height / Main.projFrames[Type] * 0.5f), Projectile.scale * Main.essScale * 0.75f, SpriteEffects.None, 0f);

        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, new Vector2(texture.Width * 0.5f, texture.Height / Main.projFrames[Type] * 0.5f), Projectile.scale, SpriteEffects.None, 0f);

        return false;
    }
}
