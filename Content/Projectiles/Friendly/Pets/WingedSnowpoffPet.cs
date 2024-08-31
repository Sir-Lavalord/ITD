using ITD.Content.Buffs.PetBuffs;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using ITD.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using rail;
using Terraria.DataStructures;
using System;

namespace ITD.Content.Projectiles.Friendly.Pets
{
    public class WingedSnowpoffPet : ModProjectile
    {
        private readonly Asset<Texture2D> lanternSprite = ModContent.Request<Texture2D>("ITD/Content/Projectiles/Friendly/Pets/WingedSnowpoffLantern");
        private readonly Asset<Texture2D> chainSprite = ModContent.Request<Texture2D>("ITD/Content/Projectiles/Friendly/Pets/WingedSnowpoffChain");
        VerletChain lanternChain;
        float lastDir;
        int targetDir;
        Vector2 randomWander;
        int wanderTimer;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
            Main.projPet[Type] = true;
            ProjectileID.Sets.LightPet[Type] = true;
            ProjectileID.Sets.CharacterPreviewAnimations[Projectile.type] = ProjectileID.Sets.SimpleLoop(0, Main.projFrames[Projectile.type], 6)
                .WithOffset(-10, -20f)
                .WithSpriteDirection(-1)
                .WithCode(DelegateMethods.CharacterPreview.Float);
        }
        public override void SetDefaults()
        {
            Projectile.height = 35;
            Projectile.width = 35;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.netImportant = true;
            DrawOffsetX = -26;
            DrawOriginOffsetY = -2;
            lastDir = 0;
            randomWander = Vector2.Zero;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            int sign = Math.Sign(player.velocity.X);
            if (sign != 0f)
            {
                targetDir = sign;
            }
            lastDir = MathHelper.Lerp(lastDir, targetDir, 0.1f);
            if (!player.dead && player.HasBuff(ModContent.BuffType<SnowyLanternBuff>()))
            {
                Projectile.timeLeft = 2;
            }
            if (++Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
            }
            DoFloating(player);
            if (lanternChain != null)
            {
                lanternChain.UpdateStart(Projectile.Center + (Vector2.UnitY * Projectile.height / 2) + Projectile.velocity);
                Vector2 lanternCenter = Vector2.Lerp(lanternChain.endStick.pointA.pos, lanternChain.endStick.pointB.pos, 0.5f);
                Lighting.AddLight(lanternCenter, new Vector3(0f, 0.8f, 1.2f));
            }
        }
        private void DoFloating(Player player)
        {
            Vector2 targetPoint = player.Center + new Vector2(lastDir * 128f, -64f);
            Vector2 toPlayer = targetPoint - Projectile.Center;
            Vector2 toPlayerNormalized = toPlayer.SafeNormalize(Vector2.Zero);
            float speed = toPlayer.Length();
            Projectile.direction = Projectile.spriteDirection = Math.Sign(lastDir);
            wanderTimer++;
            if (wanderTimer > 32)
            {
                randomWander = Main.rand.NextVector2Circular(2f, 4f);
                wanderTimer = 0;
            }
            Projectile.velocity = toPlayerNormalized * (speed / 8) + randomWander;
        }
        public override void PostDraw(Color lightColor)
        {
            lanternChain?.Draw(Main.spriteBatch, Main.screenPosition, chainSprite.Value, Color.White, true, null, null, lanternSprite.Value);
        }
        public override void OnSpawn(IEntitySource source)
        {
            Vector2 chainStart = Projectile.Center + Vector2.UnitY * Projectile.height / 2;
            lanternChain = PhysicsMethods.CreateVerletChain(4, 10, chainStart, chainStart + Vector2.One, endLength: 44);
        }
        public override void OnKill(int timeLeft)
        {
            lanternChain?.Kill();
        }
    }
}