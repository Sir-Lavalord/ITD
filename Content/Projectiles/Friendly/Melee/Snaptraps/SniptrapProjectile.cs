namespace ITD.Content.Projectiles.Friendly.Melee.Snaptraps
{
    public class SniptrapProjectile : ITDSnaptrap
    {
        public override void SetSnaptrapDefaults()
        {
            ShootRange = 16f * 8f;
            RetractAccel = 1.5f;
            ExtraFlexibility = 16f * 2f;
            MinDamage = 1;
            FullPowerHitsAmount = 10;
            WarningFrames = 60;
            ChompDust = DustID.Iron;
            ToChainTexture = "ITD/Content/Projectiles/Friendly/Melee/Snaptraps/SniptrapChain";
            toSnaptrapChain = "ITD/Content/Sounds/SniptrapClose";
        }
    }
}