using Terraria;
using Terraria.ModLoader;

namespace ITD.Content.Dusts
{
    public class BlueshroomSporesDust : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= 0.4f;
            dust.noGravity = true;
            dust.noLight = false;
            dust.scale *= 1.2f;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.rotation += dust.velocity.X * 0.15f;
            dust.scale *= 0.99f;
            Lighting.AddLight(dust.position, 0f, 0.4f, 0.4f);

            if (dust.scale < 0.25f)
            {
                dust.active = false;
            }
            return false;
        }
    }
}