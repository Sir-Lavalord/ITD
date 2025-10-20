namespace ITD.Content.Dusts;

public class WickedHeartBar : ModDust
{
    public override void OnSpawn(Dust dust)
    {
        dust.noGravity = true;
        dust.noLight = false;
        dust.scale *= 1.0f;
        dust.frame = new Rectangle(0, 10, 10, 24);
    }
}