using ITD.Content.Buffs.Debuffs;
using ITD.Content.Buffs.GeneralBuffs;
using ITD.Content.Projectiles;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Systems
{
    public class WeaponEnchantmentPlayer : ModPlayer
    {
		public bool melomycosisImbue = false;

        public override void ResetEffects()
        {
            melomycosisImbue = false;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (melomycosisImbue && item.DamageType.CountsAsClass<MeleeDamageClass>())
            {
                target.AddBuff(ModContent.BuffType<MelomycosisBuff>(), 60 * Main.rand.Next(3, 7));
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (melomycosisImbue && (proj.DamageType.CountsAsClass<MeleeDamageClass>() || ProjectileID.Sets.IsAWhip[proj.type]) && !proj.noEnchantments)
            {
                target.AddBuff(ModContent.BuffType<MelomycosisBuff>(), 60 * Main.rand.Next(3, 7));
            }
        }

        public override void MeleeEffects(Item item, Rectangle hitbox)
        {
            if (melomycosisImbue && item.DamageType.CountsAsClass<MeleeDamageClass>() && !item.noMelee && !item.noUseGraphic)
            {
                if (Main.rand.NextBool(5))
                {
                    Dust dust = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.t_Slime);
                    dust.velocity *= 0.5f;
                }
            }
        }

        public override void EmitEnchantmentVisualsAt(Projectile projectile, Vector2 boxPosition, int boxWidth, int boxHeight)
        {
            if (melomycosisImbue && (projectile.DamageType.CountsAsClass<MeleeDamageClass>() || ProjectileID.Sets.IsAWhip[projectile.type]) && !projectile.noEnchantments)
            {
                if (Main.rand.NextBool(5))
                {
                    Dust dust = Dust.NewDustDirect(boxPosition, boxWidth, boxHeight, DustID.t_Slime);
                    dust.velocity *= 0.5f;
                }
            }
        }
    }
}