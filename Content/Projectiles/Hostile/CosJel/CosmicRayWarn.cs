using ITD.Content.NPCs.Bosses;
using ITD.PrimitiveDrawing;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;

namespace ITD.Content.Projectiles.Hostile.CosJel;


public class CosmicRayWarn : ModProjectile
{
    public override string Texture => ITD.BlankTexture;


    public override bool? CanDamage()
    {
        return false;
    }
    private bool LockIn => Projectile.ai[2] != 0;
    private ref float Timer => ref Projectile.ai[0];
    private ref float NPCWhoAmI => ref Projectile.ai[1];
    private float maxTime = 240;
    private float rayWidthMax = 150;
    private float rayWidth = 0;

    public override void OnSpawn(IEntitySource source)
    {
        maxTime = Timer;
    }
    bool spawnAnim;
    
    public override void AI()
    {
        NPC CosJel = Main.npc[(int)NPCWhoAmI];
        if (CosJel == null)
        {
            Projectile.Kill();
            Projectile.timeLeft = 0;
            Projectile.active = false;
            return;
        }
        Projectile.Center = CosJel.Center - new Vector2(0, 12);
        if (!spawnAnim)
        {                //from clamtea
            float dustLoopCheck = 24f;
            int dustIncr = 0;
            while (dustIncr < dustLoopCheck)
            {
                Vector2 dustRotate = Vector2.UnitX * 0f;
                dustRotate += -Vector2.UnitY.RotatedBy((double)(dustIncr * (MathHelper.TwoPi / dustLoopCheck)), default) * new Vector2(2f, 8f);
                dustRotate = dustRotate.RotatedBy((double)Projectile.rotation, default);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.WhiteTorch, 0f, 0f, 0, default, 2f);
                dust.noGravity = true;
                dust.scale = 2f;
                dust.position = Projectile.Center + dustRotate;
                dust.velocity = Projectile.velocity * 0f + dustRotate.SafeNormalize(Vector2.UnitY) * 2f;
                Dust dust2 = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.WhiteTorch, 0f, 0f, 0, default, 2f);
                dust2.noGravity = true;
                dust2.scale = 3f;
                dust2.position = Projectile.Center + dustRotate * 2;
                dust2.velocity = Projectile.velocity * 0f + dustRotate.SafeNormalize(Vector2.UnitY) * 3f;
                dustIncr++;
            }
            spawnAnim = true;
        }
        rayWidth = MathHelper.Lerp(rayWidth, rayWidthMax, 0.025f);
        Projectile.rotation = MathHelper.Pi;

        if (CosJel.HasPlayerTarget)
        {
            sweepDir = Main.player[CosJel.target].Center.X > CosJel.Center.X ? -1 : 1;
        }
        if (--Timer <= 0)
        {
            Projectile.Kill();
        }
    }
    float sweepDir = 1;

    public override bool PreDraw(ref Color lightColor)
    {
        CosmicTelegraphVertex.Draw(Projectile.Center - Main.screenPosition, new Vector2(Projectile.velocity.Length() * CosmicRay.MAXLASERLENGTH, rayWidth), Projectile.rotation + MathHelper.PiOver2);
        return false;
    }

    public override void OnKill(int timeLeft)
    {
        CosmicJellyfish CosJel = (CosmicJellyfish)Main.npc[(int)Projectile.ai[1]].ModNPC;
        if (CosJel.NPC.active && CosJel.NPC.type == ModContent.NPCType<CosmicJellyfish>())
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile ray = Projectile.NewProjectileDirect(Terraria.Entity.InheritSource(Projectile), Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.UnitY), ModContent.ProjectileType<CosmicRay>(), Projectile.damage, Projectile.knockBack, -1, Projectile.ai[1], Projectile.rotation);
                if (CosJel.AttackID != 7)
                {
                    ray.localAI[0] = sweepDir;//mog
                    ray.localAI[1] = 0;// 0 = no collision, 1 = tile collision only, 2 = tile and platform collisions.
                    ray.localAI[2] = 1;
                    ray.timeLeft = 800;
                }
            }
        }
    }
    public readonly struct CosmicTelegraphVertex
    {
        public static void Draw(Vector2 position, Vector2 size, float rotation)
        {
            GameShaders.Misc["Telegraph"]
                .UseColor(new Color(248, 160, 121, 50))
                .UseSecondaryColor(new Color(248, 160, 121, 50))
                .Apply();
            SimpleSquare.Draw(position + rotation.ToRotationVector2() * (size.X * 0.5f), Color.White, size * new Vector2(1, 1f),
                rotation, position + rotation.ToRotationVector2() * size.X / 2f);
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

        }
    }
}