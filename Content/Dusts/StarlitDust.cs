namespace ITD.Content.Dusts
{
    public class StarlitDust : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= 1f;
            dust.noGravity = true;
			dust.noLight = true;
            dust.scale *= 1.2f;
        }
		
		public override Color? GetAlpha(Dust dust, Color lightColor) {
			Color color = Color.White;
			color.A = 100;
			return color*0.8f;
		}

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.rotation += dust.velocity.X * 0.15f;
            dust.scale *= 0.95f;
			dust.velocity *= 0.9f;

            if (dust.scale < 0.5f)
            {
                dust.active = false;
            }
            return false;
        }
    }
}