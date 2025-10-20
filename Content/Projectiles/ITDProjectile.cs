namespace ITD.Content.Projectiles;

/// <summary>
/// Have you ever wished that <see cref="ModProjectile.OnHitPlayer(Player, Player.HurtInfo)"/> ran for PVP players? Well, worry no more, as this does it!
/// </summary>
public abstract class ITDProjectile : ModProjectile
{
    /// <summary>
    /// Return an armor shader ID to have that shader applied to the projectile.<br/>
    /// This shader will take precedence over the dye this projectile has assigned (if it's a pet, light pet or grapple).<para/>
    /// This method is called in <see cref="ITDInstancedGlobalProjectile.ITDProjectileShaderHook(On_Main.orig_GetProjectileDesiredShader, Projectile)"/><para/>
    /// <b>Note:</b> If relying on this method to Apply a shader to a projectile, you also need to make sure, if the projectile uses custom drawing, that the textures drawn are drawn using <see cref="Main.EntitySpriteDraw(Terraria.DataStructures.DrawData)"/> or its overloads.
    /// </summary>
    /// <returns></returns>
    public virtual int ProjectileShader(int originalShader)
    {
        return originalShader;
    }
}
