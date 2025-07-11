namespace ITD.Content.Items.Accessories.Master
{
    public class EyeofEldritchInsight : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemNoGravity[Type] = true;
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.Size = new Vector2(30);
            Item.master = true;
            Item.accessory = true;
        }
        public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, Color.Turquoise.ToVector3() * 0.5f);
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<InsightedPlayer>().CorporateInsight = true;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
    }
    public class InsightedPlayer : ModPlayer
    {
        public bool CorporateInsight;
        public override void ResetEffects()
        {
            CorporateInsight = false;
        }
        public override void PostUpdate()
        {
            if (CorporateInsight)
            {
            }
        }
    }
    public class InsightedProjectiles : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public bool isGoingToHit = false;
        public override bool? Colliding(Projectile projectile, Rectangle projHitbox, Rectangle targetHitbox)
        {
            Player player = Main.player[Main.myPlayer];
            if (player.GetModPlayer<InsightedPlayer>().CorporateInsight)
            {
                Rectangle playerHitBox = Main.player[Main.myPlayer].Hitbox;
                float num1 = 0f;
                if (projHitbox.Intersects(playerHitBox) ||
                    Collision.CheckAABBvLineCollision(playerHitBox.TopLeft(), playerHitBox.Size(),
                    projectile.Center, projectile.Center + projectile.velocity * 60,
                    projectile.width * projectile.scale, ref num1) ||
                    Collision.CheckAABBvLineCollision(projHitbox.TopLeft(), projHitbox.Size(),
                    player.Center, player.Center + player.velocity * 10,
                    player.width, ref num1) ||
                    Collision.CheckAABBvLineCollision(playerHitBox.TopLeft() + player.velocity * 10, playerHitBox.Size(),
                    projectile.Center, projectile.Center + projectile.velocity * 60,
                    projectile.width * projectile.scale, ref num1))
                {
                    isGoingToHit = true;
                }
                else isGoingToHit = false;

            }
            return base.Colliding(projectile, projHitbox, targetHitbox);
        }
        public override Color? GetAlpha(Projectile projectile, Color lightColor)
        {
            if (Main.player[Main.myPlayer].GetModPlayer<InsightedPlayer>().CorporateInsight && projectile.hostile && projectile.damage > 0 && projectile.alpha < 255)
            {
                if (isGoingToHit)
                {
                    if (projectile.ModProjectile is null || (projectile.ModProjectile != null && projectile.ModProjectile.CanHitPlayer(Main.LocalPlayer) && (projectile.ModProjectile.CanDamage() ?? true)))
                        return Color.Red;
                }
            }
            return null;
        }
/*        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            if (Main.player[Main.myPlayer].GetModPlayer<InsightedPlayer>().CorporateInsight)
            {
                Rectangle hitbox = projectile.getRect();
                ProjectileLoader.ModifyDamageHitbox(projectile, ref hitbox);
                hitbox.Offset((int)-Main.screenPosition.X, (int)-Main.screenPosition.Y);
                hitbox = Main.ReverseGravitySupport(hitbox);
                if (projectile.hostile)
                {
                    if (isGoingToHit)
                    Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, hitbox, Color.DarkRed * 0.4f);
                    else
                        Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, hitbox, Color.Lime * 0.4f);
                }
            }
            return true;
        }*/
    }
    public class InsightedNPCs : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public bool isGoingToHit = false;
        public override bool ModifyCollisionData(NPC npc, Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox)
        {
            Player player = Main.player[Main.myPlayer];
            if (player.GetModPlayer<InsightedPlayer>().CorporateInsight)
            {
                Rectangle playerHitBox = Main.player[Main.myPlayer].Hitbox;
                float num1 = 0f;
                if (npcHitbox.Intersects(playerHitBox) || (Collision.CheckAABBvLineCollision(playerHitBox.TopLeft(), playerHitBox.Size(),
                    npc.Center, npc.Center + npc.velocity * 30,
                    npcHitbox.Width * npc.scale, ref num1)) || (Collision.CheckAABBvLineCollision(npcHitbox.TopLeft(), npcHitbox.Size(),
                    player.Center, player.Center + player.velocity * 10,
                    player.width, ref num1)) ||
                    (Collision.CheckAABBvLineCollision(playerHitBox.TopLeft() * player.velocity * 10, playerHitBox.Size(),
                    npc.Center, npc.Center + npc.velocity * 30,
                    npcHitbox.Width * npc.scale, ref num1)))
                {
                    isGoingToHit = true;
                }
                else isGoingToHit = false;

            }
            return base.ModifyCollisionData(npc, victimHitbox, ref immunityCooldownSlot, ref damageMultiplier, ref npcHitbox);
        }
        /*        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
                {
                    if (Main.myPlayer < 0 || Main.myPlayer > 255)
                        return true;
                    if (Main.player[Main.myPlayer].GetModPlayer<InsightedPlayer>().CorporateInsight)
                    {
                        Rectangle hitbox = npc.getRect();
                        NPCLoader.ModifyHoverBoundingBox(npc, ref hitbox);
                        hitbox.Offset((int)-Main.screenPosition.X, (int)-Main.screenPosition.Y);
                        hitbox = Main.ReverseGravitySupport(hitbox);
                        if (npc.life > 5 || !npc.CountsAsACritter)
                        {
                            if (!npc.friendly)
                            {
                                if (isGoingToHit)
                                Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, hitbox, Color.DarkRed * 0.4f);
                                else
                                    Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, hitbox, Color.LimeGreen * 0.4f);

                            }
                        }
                    }
                        return true;
                }*/
        public override Color? GetAlpha(NPC npc, Color drawColor)
        {
            if (Main.player[Main.myPlayer].GetModPlayer<InsightedPlayer>().CorporateInsight && !npc.friendly && npc.damage > 0 && npc.alpha < 255)
            {
                if (isGoingToHit)
                {
                    if (npc.ModNPC is null || (npc.ModNPC != null && npc.ModNPC.CanHitPlayer(Main.LocalPlayer, ref Main.LocalPlayer.immuneTime)))
                        return Color.Red;
                }
            }
            return base.GetAlpha(npc, drawColor);
        }

    }
}