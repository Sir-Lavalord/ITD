using ITD.Content.Buffs.Debuffs;

namespace ITD.Systems;

public class WeaponEnchantmentPlayer : ModPlayer
{
    public bool melomycosisImbue = false;
    public bool flamingRazor = false;
    public bool poisonFang = false;
    public override void ResetEffects()
    {
        flamingRazor = false;
        poisonFang = false;
        melomycosisImbue = false;
    }
    public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (melomycosisImbue && item.DamageType.CountsAsClass<MeleeDamageClass>())
        {
            target.AddBuff(ModContent.BuffType<MelomycosisBuff>(), 60 * Main.rand.Next(3, 7));
        }
        if (flamingRazor)
        {
            //significantly weaker than magma stone ofc
            //be consistent with others
            //since debuff is ass, add more time
            if (Main.rand.NextBool(4))
            {
                target.AddBuff(BuffID.OnFire, 420, false);
            }
            else if (Main.rand.NextBool())
            {
                target.AddBuff(BuffID.OnFire, 300, false);
            }
            else
            {
                target.AddBuff(BuffID.OnFire, 180, false);
            }
        }
        if (poisonFang)
        {
            if (Main.rand.NextBool(4))
            {
                target.AddBuff(BuffID.Poisoned, 360, false);
            }
            else if (Main.rand.NextBool())
            {
                target.AddBuff(BuffID.Poisoned, 240, false);
            }
            else
            {
                target.AddBuff(BuffID.Poisoned, 120, false);
            }
        }
    }

    public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (melomycosisImbue && (proj.DamageType.CountsAsClass<MeleeDamageClass>() || ProjectileID.Sets.IsAWhip[proj.type]))
        {
            target.AddBuff(ModContent.BuffType<MelomycosisBuff>(), 60 * Main.rand.Next(3, 7));
        }
        if (flamingRazor)
        {
            if (Main.rand.NextBool(4))
            {
                target.AddBuff(BuffID.OnFire, 360, false);
            }
            else if (Main.rand.NextBool())
            {
                target.AddBuff(BuffID.OnFire, 240, false);
            }
            else
            {
                target.AddBuff(BuffID.OnFire, 120, false);
            }
        }

        if (poisonFang)
        {
            if (Main.rand.NextBool(4))
            {
                target.AddBuff(BuffID.Poisoned, 360, false);
            }
            else if (Main.rand.NextBool())
            {
                target.AddBuff(BuffID.Poisoned, 240, false);
            }
            else
            {
                target.AddBuff(BuffID.Poisoned, 120, false);
            }
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
        if (flamingRazor && item.DamageType.CountsAsClass<MeleeDamageClass>() && !item.noMelee && !item.noUseGraphic)
        {
            if (Main.rand.NextBool(6))
            {
                Dust dust = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.Torch);
                dust.velocity *= 0.5f;
            }
        }
        if (poisonFang && item.DamageType.CountsAsClass<MeleeDamageClass>() && !item.noMelee && !item.noUseGraphic)
        {
            if (Main.rand.NextBool(6))
            {
                Dust dust = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.Poisoned);
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
        if (flamingRazor && !projectile.noEnchantments && projectile.damage > 0 && projectile.Opacity != 0 && projectile.alpha < 255)
        {
            if (Main.rand.NextBool(6))
            {
                Dust dust = Dust.NewDustDirect(boxPosition, boxWidth, boxHeight, DustID.Torch);
                dust.velocity *= 0.5f;
            }
        }
        if (poisonFang && !projectile.noEnchantments && projectile.damage > 0 && projectile.Opacity != 0 && projectile.alpha < 255)
        {
            if (Main.rand.NextBool(6))
            {
                Dust dust = Dust.NewDustDirect(boxPosition, boxWidth, boxHeight, DustID.Poisoned);
                dust.velocity *= 0.5f;
            }
        }
    }
}