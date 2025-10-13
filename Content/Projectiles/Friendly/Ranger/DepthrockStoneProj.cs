using ITD.Systems;
using ITD.Utilities;
using Terraria.Audio;

namespace ITD.Content.Projectiles.Friendly.Ranger;

public class DepthrockStoneProj : ModProjectile
{
    private NPC HomingTarget
    {
        get => Projectile.ai[1] == 0 ? null : Main.npc[(int)Projectile.ai[1] - 1];
        set
        {
            Projectile.ai[1] = value == null ? 0 : value.whoAmI + 1;
        }
    }
    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 1;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }
    public override void SetDefaults()
    {
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.width = 40; Projectile.height = 40;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = 1;
        Projectile.light = 0.5f;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = true;
        Projectile.timeLeft = 300;
    }

    public override void AI()
    {
        if (Main.rand.NextBool(5))
        {
            int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Stone, 0, 0, 0, default, 2f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity *= 2f;
            int dust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Stone, 0, 0, 0, default, 1f);
            Main.dust[dust2].noGravity = false;
            Main.dust[dust2].velocity *= 1f;
        }
        Projectile.rotation += 0.05f;
        Projectile.velocity.X *= 0.96f;
        Projectile.velocity.Y += 0.4f;
    }
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        return base.OnTileCollide(oldVelocity);
    }
    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 20; i++)
        {
            int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Stone, 0, 0, 0, default, 2f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity *= 2f;
        }
        SoundEngine.PlaySound(SoundID.NPCDeath43, Projectile.Center);

        if (Main.myPlayer == Projectile.owner)
        {
            Player player = Main.player[Projectile.owner];
            ITDPlayer modPlayer = player.GetModPlayer<ITDPlayer>();
            modPlayer.BetterScreenshake(5, 3, 3, true);
            HomingTarget ??= Projectile.FindClosestNPC(600);

            if (HomingTarget == null)
            {
            }
            else
            {
                SoundEngine.PlaySound(SoundID.Item5, Projectile.Center);
                Projectile arrow = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, (int)Projectile.ai[0], (int)(Projectile.damage / 1.75f), Projectile.knockBack, Projectile.owner);
                arrow.velocity = (HomingTarget.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 14f;
                arrow.tileCollide = false;
            }
        }
    }
    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
    {
        width = 25;
        height = 25;
        fallThrough = false;
        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        modifiers.HitDirectionOverride = (Projectile.Center.X < target.Center.X).ToDirectionInt();
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
        int num156 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]; //ypos of lower right corner of sprite to draw
        int y3 = num156 * Projectile.frame; //ypos of upper left corner of sprite to draw
        Rectangle rectangle = new(0, y3, texture2D13.Width, num156);
        Vector2 origin2 = rectangle.Size() / 2f;

        Color color26 = lightColor;
        color26 = Projectile.GetAlpha(color26);

        SpriteEffects effects = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i++)
        {
            Color color27 = Color.White * Projectile.Opacity * 0.75f * 0.5f;
            color27 *= (float)(ProjectileID.Sets.TrailCacheLength[Projectile.type] - i) / ProjectileID.Sets.TrailCacheLength[Projectile.type];
            Vector2 value4 = Projectile.oldPos[i];
            float num165 = Projectile.oldRot[i];
            Main.EntitySpriteDraw(texture2D13, value4 + Projectile.Size / 2f - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), color27, num165, origin2, Projectile.scale, SpriteEffects.FlipVertically, 0);
        }
        Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), Projectile.GetAlpha(lightColor), Projectile.rotation, origin2, Projectile.scale, SpriteEffects.FlipVertically, 0);
        return false;
    }
}