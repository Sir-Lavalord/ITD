using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.DataStructures;
using ITD.Content.Tiles.Misc;
using ITD.Content.Dusts;
using Terraria.ID;
using Terraria.GameContent.Drawing;
using Terraria.Audio;
using Mono.Cecil;

namespace ITD.Content.Projectiles.Friendly.Melee
{
    public class CosmisumaruPheonix : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 8;
        }

        public override void SetDefaults()
        {
            Projectile.width = 68;
            Projectile.height = 72;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.alpha = 0;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.damage = 360;

            Projectile.damage = 540;
            Projectile.CritChance = 4;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Main.rand.NextBool(2))
            {
                int choice = Main.rand.Next(2);
                if (choice == 0)
                {
                    for (int d = 0; d < 2; d++)
                    {
                        Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<CosJelDust>(), Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f, 150, default(Color), 0.7f);
                    }
                }
                else if (choice == 1)
                {
                    for (int d = 0; d < 4; d++)
                    {
                        Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<CosJelDust>(), Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f, 150, default(Color), 0.7f);
                    }
                }
                else
                {
                    for (int d = 0; d < 6; d++)
                    {
                        Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<CosJelDust>(), Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f, 150, default(Color), 0.7f);
                    }
                }
            }

            if (++Projectile.frameCounter >= 3)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= 8)
                {
                    Projectile.frame = 0;
                }
            }
        }

        public override void OnKill(int timeLeft)
        {

            Player player = Main.LocalPlayer;
            if (Projectile.owner == Main.myPlayer)
            {
                for (int i = 0; i < 5; i++)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(player.direction, 0f), ModContent.ProjectileType<CosmisumaruPheonixImpact>(), 90, 0f);
                }
            }

            for (int d = 0; d < 9; d++)
            {
                float randomAngle = Main.rand.NextFloat(-12, 12) * (float)(Math.PI / 180);
                Vector2 direction = new Vector2(0, -1).RotatedBy(randomAngle);
                float speed = Main.rand.NextFloat(1f, 2f);
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<CosJelDust>(), direction.X * speed, direction.Y * speed, 150, default(Color), Projectile.scale);
            }
        }
    }
}