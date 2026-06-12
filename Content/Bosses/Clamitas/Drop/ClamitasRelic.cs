using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;


namespace Clamity.Content.Bosses.Clamitas.Drop
{
    public class ClamitasRelic : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Placeables.Relics";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<ClamitasRelicTile>());
            Item.width = 30;
            Item.height = 40;
            Item.maxStack = 9999;
            Item.rare = ItemRarityID.Master;
            Item.master = true;
            Item.value = Item.buyPrice(0, 5);
        }
    }
    public class ClamitasRelicTile : ModTile
    {
        public const int FrameWidth = 54;

        public const int FrameHeight = 72;

        public const int HorizontalFrames = 1;

        public const int VerticalFrames = 1;

        //public Asset<Texture2D> RelicTexture;

        //public abstract string RelicTextureName { get; }

        //public abstract int AssociatedItem { get; }

        //public override string Texture => "Clamity/Content/Bosses/Clamitas/Drop/";

        /*public override void Load()
        {
            if (!Main.dedServ)
            {
                RelicTexture = ModContent.Request<Texture2D>(RelicTextureName);
            }
        }

        public override void Unload()
        {
            RelicTexture = null;
        }*/

        public override void SetStaticDefaults()
        {
            RegisterItemDrop(ModContent.ItemType<ClamitasRelic>());
            Main.tileShine[Type] = 400;
            Main.tileFrameImportant[Type] = true;
            TileID.Sets.InteractibleByNPCs[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
            TileObjectData.newTile.StyleHorizontal = false;
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
            TileObjectData.addAlternate(1);
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(233, 207, 94), Language.GetText("MapObject.Relic"));
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            return false;
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            if (drawData.tileFrameX % 54 == 0 && drawData.tileFrameY % 72 == 0)
            {
                Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
            }
        }

        public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Vector2 vector = new Vector2(Main.offScreenRange);
            if (Main.drawToScreen)
            {
                vector = Vector2.Zero;
            }

            Point p = new Point(i, j);
            Tile tile = Main.tile[p.X, p.Y];
            if (!(tile == null) && tile.HasTile)
            {
                Texture2D value = ModContent.Request<Texture2D>(Texture + "_Relic").Value;
                int frameY = tile.TileFrameX / 54;
                Rectangle rectangle = value.Frame(1, 1, 0, frameY);
                Vector2 origin = rectangle.Size() / 2f;
                Vector2 vector2 = p.ToWorldCoordinates(24f, 64f);
                Color color = Lighting.GetColor(p.X, p.Y);
                SpriteEffects effects = tile.TileFrameY / 72 != 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                float num = (float)Math.Sin(Main.GlobalTimeWrappedHourly * (MathF.PI * 2f) / 5f);
                Vector2 vector3 = vector2 + vector - Main.screenPosition + new Vector2(0f, -40f) + new Vector2(0f, num * 4f);
                spriteBatch.Draw(value, vector3, rectangle, color, 0f, origin, 1f, effects, 0f);
                float num2 = (float)Math.Sin(Main.GlobalTimeWrappedHourly * (MathF.PI * 2f) / 2f) * 0.3f + 0.7f;
                Color color2 = color;
                color2.A = 0;
                color2 = color2 * 0.1f * num2;
                float num3 = 6f + num * 2f;
                float num4 = 355f / (678f * MathF.PI);
                for (float num5 = 0f; num5 < 1f; num5 += num4)
                {
                    spriteBatch.Draw(value, vector3 + (MathF.PI * 2f * num5).ToRotationVector2() * num3, rectangle, color2, 0f, origin, 1f, effects, 0f);
                }
            }
        }
    }
}
