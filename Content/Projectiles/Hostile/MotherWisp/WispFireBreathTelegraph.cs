using ITD.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Hostile.CosJel;

public class WispFireBreathTelegraph : ModProjectile
{
    public override string Texture => ITD.BlankTexture;

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
    }

    public override void SetDefaults()
    {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.aiStyle = -1;
        Projectile.scale = 0.5f;
        Projectile.alpha = 0;
        CooldownSlot = 1;
    }

    Vector2 spawnPoint;

    public override void AI()
    {
        Projectile.rotation = Projectile.ai[0];

        if (spawnPoint == Vector2.Zero)
            spawnPoint = Projectile.Center;

        // FIXED: Look for an NPC instead of a Projectile, since the Candle is an NPC
        NPC parent = Main.npc[(int)Projectile.ai[1]];
        if (parent != null && parent.active)
            spawnPoint = parent.Center;

        Projectile.Center = spawnPoint + Vector2.UnitX.RotatedBy(Projectile.ai[0]) * 96 * Projectile.scale;

        int maxScale = 2;
        if (Projectile.scale < maxScale)
        {
            Projectile.scale += 0.125f;
        }
        else
        {
            Projectile.scale = maxScale;
            Projectile.alpha += 10;
        }
        if (Projectile.alpha > 255)
        {
            Projectile.Kill();
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D glow = TextureAssets.Extra[ExtrasID.MartianProbeScanWave].Value;
        int rect1 = glow.Height;
        int rect2 = 0;
        Rectangle glowrectangle = new(0, rect2, glow.Width, rect1);
        Vector2 gloworigin2 = glowrectangle.Size() / 2f;
        Color glowcolor = Color.PaleTurquoise;

        float scale = Projectile.scale;
        Main.EntitySpriteDraw(glow, Projectile.Center + Projectile.Size / 2f - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(glowrectangle), Projectile.GetAlpha(glowcolor),
            Projectile.rotation, gloworigin2, scale * 2, SpriteEffects.None, 0);

        return false;
    }
}