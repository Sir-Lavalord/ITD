namespace ITD.Content.Dusts;

public class IceChunkDust : ModDust
{
    public override void OnSpawn(Dust dust)
    {
        dust.velocity *= 0.4f;
        dust.noGravity = false;
        dust.noLight = true;
        dust.frame = new Rectangle(0, Main.rand.Next(5) * 30, 30, 30);
    }

    public override bool Update(Dust dust)
    {
        return true;
    }
}