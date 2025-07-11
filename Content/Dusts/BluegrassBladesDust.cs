namespace ITD.Content.Dusts
{
    public class BluegrassBladesDust : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= 0.4f;
            dust.noGravity = false;
            dust.noLight = true;
        }

        public override bool Update(Dust dust)
        {
            return true;
        }
    }
}