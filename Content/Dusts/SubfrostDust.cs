namespace ITD.Content.Dusts;

public class SubfrostDust : ModDust
{
    public override void OnSpawn(Dust dust)
    {
        dust.velocity *= 0.4f;
        dust.noGravity = false;
        dust.noLight = true;
        dust.scale *= 1.5f;
    }

    public override bool Update(Dust dust)
    {
        dust.position += dust.velocity;
        dust.velocity.Y += 0.1f;
        dust.rotation += dust.velocity.X * 0.15f;
        dust.scale *= 0.99f;
        Lighting.AddLight(dust.position, 0f, 0.3f, 0.4f);

        if (dust.scale < 0.5f)
        {
            dust.active = false;
        }

        return false; // Return false to prevent vanilla behavior.
    }
}