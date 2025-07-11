namespace ITD.Content.Dusts
{
    public class CosmisumaruIndicator : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= 0f;
            dust.noGravity = false;
            dust.noLight = true;
            dust.scale *= 0.5f;
        }

        public override bool Update(Dust dust)
        {
            dust.velocity.Y += 0.2f;
            dust.scale *= 1.01f;
            dust.alpha += 5;
            Lighting.AddLight(dust.position, 0f, 0.3f, 0.4f);

            if (dust.scale > 2f)
            {
                dust.active = false;
            }

            return false;
        }
    }
}