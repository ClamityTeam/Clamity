using Clamity.Content.Biomes.FrozenHell.Items;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Commons
{
    public class SolutionConversion : GlobalProjectile
    {
        public override void PostAI(Projectile projectile)
        {
            /*if ((projectile.type == 145 || projectile.type == 147 || projectile.type == 149 || projectile.type == 146 || projectile.type == ProjectileID.HolyWater || projectile.type == ProjectileID.UnholyWater || projectile.type == ProjectileID.BloodWater) && projectile.owner == Main.myPlayer)
            {
                if (projectile.owner == Main.myPlayer)
                {
                    ConvertNormal((int)(projectile.position.X + projectile.width / 2) / 16, (int)(projectile.position.Y + projectile.height / 2) / 16, 2);
                }
            }*/
            if ((/*projectile.type == 10 || */projectile.type == ProjectileID.PureSpray) && projectile.owner == Main.myPlayer)
            {
                if (projectile.owner == Main.myPlayer)
                {
                    ConvertPure((int)(projectile.position.X + projectile.width / 2) / 16, (int)(projectile.position.Y + projectile.height / 2) / 16, 2);
                }
            }
        }
        public void ConvertNormal(int i, int j, int size = 4)
        {
            for (int k = i - size; k <= i + size; k++)
            {
                for (int l = j - size; l <= j + size; l++)
                {
                    if (WorldGen.InWorld(k, l, 1) && Math.Abs(k - i) + Math.Abs(l - j) < Math.Sqrt(size * size + size * size) && Main.tile[k, l] != null)
                    {
                        int type = Main.tile[k, l].TileType;
                        int wall = Main.tile[k, l].WallType;

                        if (type == ModContent.TileType<FrozenAshTile>())
                        {
                            Main.tile[k, l].TileType = TileID.Ash;
                            WorldGen.SquareTileFrame(k, l, true);
                            NetMessage.SendTileSquare(-1, k, l, 1);
                        }
                        if (type == ModContent.TileType<FrozenHellstoneTile>())
                        {
                            Main.tile[k, l].TileType = TileID.Hellstone;
                            WorldGen.SquareTileFrame(k, l, true);
                            NetMessage.SendTileSquare(-1, k, l, 1);
                        }
                    }
                }
            }
        }

        public void ConvertPure(int i, int j, int size = 4)
        {
            for (int k = i - size; k <= i + size; k++)
            {
                for (int l = j - size; l <= j + size; l++)
                {
                    if (WorldGen.InWorld(k, l, 1) && Math.Abs(k - i) + Math.Abs(l - j) < Math.Sqrt(size * size + size * size) && Main.tile[k, l] != null)
                    {
                        int type = Main.tile[k, l].TileType;
                        int wall = Main.tile[k, l].WallType;

                        if (type == ModContent.TileType<FrozenAshTile>())
                        {
                            Main.tile[k, l].TileType = TileID.Ash;
                            WorldGen.SquareTileFrame(k, l, true);
                            NetMessage.SendTileSquare(-1, k, l, 1);
                        }
                        if (type == ModContent.TileType<FrozenHellstoneTile>())
                        {
                            Main.tile[k, l].TileType = TileID.Hellstone;
                            WorldGen.SquareTileFrame(k, l, true);
                            NetMessage.SendTileSquare(-1, k, l, 1);
                        }
                    }
                }
            }
        }
    }
}
