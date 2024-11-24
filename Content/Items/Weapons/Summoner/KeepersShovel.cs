using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;

using ITD.Content.Projectiles.Friendly.Summoner;
using ITD.Content.Buffs.Debuffs;

namespace ITD.Content.Items.Weapons.Summoner
{
    public class KeepersShovel : ModItem
    {
		public int attackCycle = 0;
        public override void SetDefaults()
        {
            Item.damage = 20;
            Item.DamageType = DamageClass.SummonMeleeSpeed;
            Item.width = 66;
            Item.height = 66;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;
            Item.value = 10000;
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
        }
		
		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(ModContent.BuffType<KeepersShovelTagDebuff>(), 300);
			player.MinionAttackTargetNPC = target.whoAmI;
		}
		
		public override void MeleeEffects (Player player, Rectangle hitbox)
        {
			if (player.itemAnimation == 1f)
			{
				Vector2 position = player.Center + new Vector2(60f*player.direction, player.height * 0.5f);
				Point point = position.ToTileCoordinates();
				
				int j = 0;
				while (j < 8 && point.Y >= 10 && WorldGen.SolidTile(point.X, point.Y, false))
				{
					point.Y--;
					j++;
				}
				int k = 0;
				while (k < 8 && point.Y <= Main.maxTilesY - 10 && !WorldGen.ActiveAndWalkableTile(point.X, point.Y-1))
				{
					point.Y++;
					k++;
				}
				
				if (WorldGen.ActiveAndWalkableTile(point.X, point.Y-1) && !WorldGen.SolidTile(point.X, point.Y-2, false))
				{
					position = new Vector2((float)(point.X * 16 + 8), (float)(point.Y * 16 - 8));
					
					attackCycle = ++attackCycle % 3;
					if (attackCycle == 0)
					{
						Projectile.NewProjectileDirect(player.GetSource_FromThis(), position - new Vector2(0f, 34f), new Vector2((Main.rand.NextFloat()*3f+3f)*player.direction, -6f + 2f * Main.rand.NextFloat()), ModContent.ProjectileType<TheUnburied>(), 20, 5f, player.whoAmI);
						SoundEngine.PlaySound(SoundID.NPCDeath17, position);
					}
					SoundEngine.PlaySound(SoundID.Dig, position);
					
					for (int i = 0; i < 12; i++)
					{
						Dust.NewDustPerfect(position, DustID.Dirt, new Vector2(Main.rand.NextFloat()*6f*player.direction, -8f + 8f * Main.rand.NextFloat()), 0, default(Color), 1.5f).noGravity = true;
					}
				}
			}
        }
		
		public override bool MeleePrefix() {
			return true;
		}
    }
}