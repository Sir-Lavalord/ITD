using ITD.Content.Buffs.PetBuffs;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using ITD.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace ITD.Content.Projectiles.Friendly.Pets
{
    public class WingedSnowpoffPet : ModProjectile
    {
        // someone pls fix the animation for me idk why it does that (the flashing)
        private readonly Asset<Texture2D> lanternSprite = ModContent.Request<Texture2D>("ITD/Content/Projectiles/Friendly/Pets/WingedSnowpoffLantern");
        private readonly Asset<Texture2D> chainSprite = ModContent.Request<Texture2D>("ITD/Content/Projectiles/Friendly/Pets/WingedSnowpoffChain");
        VerletChain lanternChain;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 3;
            Main.projPet[Type] = true;
            ProjectileID.Sets.CharacterPreviewAnimations[Type] = ProjectileID.Sets.SimpleLoop(0, Main.projFrames[Type], 6)
                .WithOffset(-10, -20f)
                .WithSpriteDirection(-1)
                .WithCode(DelegateMethods.CharacterPreview.Float);
        }
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.ZephyrFish);
            DrawOffsetX = -32;
            DrawOriginOffsetY = -8;
            AIType = ProjectileID.ZephyrFish;
            lanternChain = PhysicsMethods.CreateVerletChain(6, 10, Projectile.Center, Projectile.Center + Vector2.One, false);
        }
        public override bool PreAI()
        {
            Player player = Main.player[Projectile.owner];

            player.zephyrfish = false;

            return true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.dead && player.HasBuff(ModContent.BuffType<SnowyLanternBuff>()))
            {
                Projectile.timeLeft = 2;
            }
            if (lanternChain != null)
            {
                lanternChain.UpdateStart(Projectile.Center + Vector2.UnitY * Projectile.height/2);
                Vector2 lanternCenter = Vector2.Lerp(lanternChain.endStick.pointA.pos, lanternChain.endStick.pointB.pos, 0.5f);
                Lighting.AddLight(lanternCenter, new Vector3(0f, 0.8f, 1.2f));
            }
        }
        public override void PostDraw(Color lightColor)
        {
            lanternChain?.Draw(Main.spriteBatch, Main.screenPosition, chainSprite.Value, Color.White, true, null, null, lanternSprite.Value);
        }
        public override void OnKill(int timeLeft)
        {
            lanternChain.Kill();
        }
    }
}
