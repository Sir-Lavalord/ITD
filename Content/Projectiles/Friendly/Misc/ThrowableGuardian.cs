using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.Audio;
using System;

using ITD.Players;
using ITD.Utilities;

namespace ITD.Content.Projectiles.Friendly.Misc
{
    public class ThrowableGuardian : ModProjectile
    {
		public override string Texture => "Terraria/Images/NPC_68";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 800;
        }
        public override void OnKill(int timeLeft)
        {
			Main.player[Main.myPlayer].GetITDPlayer().BetterScreenshake(20, 4, 4, false);
			SoundEngine.PlaySound(SoundID.Item62, Projectile.Center);
            Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f)), 54);
            Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f)), 55);
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation()+MathHelper.PiOver2;
        }
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			modifiers.SourceDamage.Base += target.lifeMax/20;
		}
    }
}
