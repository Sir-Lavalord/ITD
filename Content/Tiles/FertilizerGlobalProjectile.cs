namespace ITD.Content.Tiles;

public class FertilizerGlobalProjectile : GlobalProjectile
{
    public override void PostAI(Projectile projectile)
    {
        Point tileCoords = projectile.position.ToTileCoordinates();
        int xSize = projectile.width / 16;
        int ySize = projectile.height / 16;
        for (int i = tileCoords.X; i < tileCoords.X + xSize; i++)
        {
            for (int j = tileCoords.Y; j < tileCoords.Y + ySize; j++)
            {
                if (TileLoader.GetTile(Framing.GetTileSafely(i, j).TileType) is ITDSapling sap && sap.FertilizerType == projectile.type)
                {
                    sap.Grow(i, j);
                }
            }
        }
    }
}
