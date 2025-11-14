namespace Clamity.Content.Items.Placeable.Blocks
{
    /*public class Pyroplate : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
            //ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<Onyxplate>();
        }

        public override void SetDefaults()
        {
            Item.createTile = ModContent.TileType<PyroplatePlaced>();
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.consumable = true;
            Item.width = 13;
            Item.height = 10;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 3);
            Item.rare = ItemRarityID.Orange;
        }

        public override void AddRecipes()
        {
            CreateRecipe(25).
                AddIngredient(ItemID.Obsidian, 25).
                AddIngredient<EssenceOfFlame>().
                AddTile(TileID.Hellforge).
                Register();
            CreateRecipe().
                AddIngredient<PyroplateWall>(4).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
    public class PyroplatePlaced : ModTile
    {
        public static readonly SoundStyle MinePlatingSound = new("CalamityMod/Sounds/Custom/PlatingMine", 3);
        internal static Texture2D GlowTexture;
        internal static Texture2D PulseTexture;
        internal static Color[] PulseColors;
        public override void SetStaticDefaults()
        {
            if (!Main.dedServ)
            {
                PulseTexture = ModContent.Request<Texture2D>("CalamityMod/Tiles/Plates/ElumplatePulse", AssetRequestMode.ImmediateLoad).Value;
                PulseColors = new Color[PulseTexture.Width];
                Main.QueueMainThreadAction(() => PulseTexture.GetData(PulseColors));
                GlowTexture = ModContent.Request<Texture2D>("Clamity/Content/Items/Placeable/Blocks/PyroplatePlacedGlow", AssetRequestMode.ImmediateLoad).Value;
            }
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);

            HitSound = MinePlatingSound;
            MineResist = 1f;
            AddMapEntry(new Color(121, 180, 212));
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.PlatinumCoin, 0f, 0f, 1, new Color(255, 255, 255), 1f);
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.Stone, 0f, 0f, 1, new Color(100, 100, 100), 1f);
            return false;
        }

        public override void RandomUpdate(int i, int j)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.PlatinumCoin, 0f, 0f, 1, new Color(255, 255, 255), 1f);
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            // If the cached textures don't exist for some reason, don't bother using them.
            if (GlowTexture is null || PulseTexture is null)
                return;

            int xPos = Main.tile[i, j].TileFrameX;
            int yPos = Main.tile[i, j].TileFrameY;
            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Vector2 drawOffset = new Vector2(i * 16 - Main.screenPosition.X, j * 16 - Main.screenPosition.Y) + zero;

            // Glowmask 'pulse' effect
            int factor = (int)Main.GameUpdateCount % PulseTexture.Width;
            float brightness = PulseColors[factor].R / 255f;
            int drawBrightness = (int)(40 * brightness) + 10;
            Color drawColour = GetDrawColour(i, j, new Color(drawBrightness, drawBrightness, drawBrightness, drawBrightness));

            // If these tiles cause lag, comment out the pulse effect code and uncomment this:
            //Color drawColour = GetDrawColour(i, j, new Color(50, 50, 50, 50));

            Tile trackTile = Main.tile[i, j];
            SlopedGlowmask(i, j, 0, GlowTexture, drawOffset, null, GetDrawColour(i, j, drawColour), default);
        }

        private Color GetDrawColour(int i, int j, Color colour)
        {
            int colType = Main.tile[i, j].TileColor;
            Color paintCol = WorldGen.paintColor(colType);
            if (colType >= 13 && colType <= 24)
            {
                colour.R = (byte)(paintCol.R / 255f * colour.R);
                colour.G = (byte)(paintCol.G / 255f * colour.G);
                colour.B = (byte)(paintCol.B / 255f * colour.B);
            }
            return colour;
        }
        private void SlopedGlowmask(int i, int j, int type, Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color drawColor, Vector2 positionOffset, bool overrideTileFrame = false)
        {
            Tile tile = Main.tile[i, j];

            int TileFrameX = tile.TileFrameX;
            int TileFrameY = tile.TileFrameY;

            if (overrideTileFrame)
            {
                TileFrameX = 0;
                TileFrameY = 0;
            }

            int width = 16;
            int height = 16;

            if (sourceRectangle != null)
            {
                TileFrameX = ((Rectangle)sourceRectangle).X;
                TileFrameY = ((Rectangle)sourceRectangle).Y;
            }

            int iX16 = i * 16;
            int jX16 = j * 16;
            Vector2 location = new Vector2(iX16, jX16);
            Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);
            if (Main.drawToScreen)
                zero = Vector2.Zero;

            Vector2 offsets = -Main.screenPosition + zero + positionOffset;
            Vector2 drawCoordinates = location + offsets;
            if (tile.Slope == 0 && !tile.IsHalfBlock || Main.tileSolid[tile.TileType] && Main.tileSolidTop[tile.TileType]) //second one should be for platforms
            {
                Main.spriteBatch.Draw(texture, drawCoordinates, new Rectangle(TileFrameX, TileFrameY, width, height), drawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }
            else if (tile.IsHalfBlock)
            {
                Main.spriteBatch.Draw(texture, new Vector2(drawCoordinates.X, drawCoordinates.Y + 8), new Rectangle(TileFrameX, TileFrameY, width, 8), drawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }
            else
            {
                byte b = (byte)tile.Slope;
                Rectangle TileFrame;
                Vector2 drawPos;
                if (b == 1 || b == 2)
                {
                    int length;
                    int height2;
                    for (int a = 0; a < 8; ++a)
                    {
                        int aX2 = a * 2;
                        if (b == 2)
                        {
                            length = 16 - aX2 - 2;
                            height2 = 14 - aX2;
                        }
                        else
                        {
                            length = aX2;
                            height2 = 14 - length;
                        }

                        TileFrame = new Rectangle(TileFrameX + length, TileFrameY, 2, height2);
                        drawPos = new Vector2(iX16 + length, jX16 + aX2) + offsets;
                        Main.spriteBatch.Draw(texture, drawPos, TileFrame, drawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
                    }

                    TileFrame = new Rectangle(TileFrameX, TileFrameY + 14, 16, 2);
                    drawPos = new Vector2(iX16, jX16 + 14) + offsets;
                    Main.spriteBatch.Draw(texture, drawPos, TileFrame, drawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
                }
                else
                {
                    int length;
                    int height2;
                    for (int a = 0; a < 8; ++a)
                    {
                        int aX2 = a * 2;
                        if (b == 3)
                        {
                            length = aX2;
                            height2 = 16 - length;
                        }
                        else
                        {
                            length = 16 - aX2 - 2;
                            height2 = 16 - aX2;
                        }

                        TileFrame = new Rectangle(TileFrameX + length, TileFrameY + 16 - height2, 2, height2);
                        drawPos = new Vector2(iX16 + length, jX16) + offsets;
                        Main.spriteBatch.Draw(texture, drawPos, TileFrame, drawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
                    }

                    drawPos = new Vector2(iX16, jX16) + offsets;
                    if (tile.TileType != ModContent.TileType<EutrophicGlass>())
                    {
                        TileFrame = new Rectangle(TileFrameX, TileFrameY, 16, 2);
                        Main.spriteBatch.Draw(texture, drawPos, TileFrame, drawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
                    }
                }
            }
            // Contribuited by Vortex
        }
    }*/
}
