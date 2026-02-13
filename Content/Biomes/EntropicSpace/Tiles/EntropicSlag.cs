using CalamityMod;
using CalamityMod.Systems;
using CalamityMod.Tiles.FurnitureCosmilite;
using CalamityMod.Tiles.FurnitureProfaned;
using CalamityMod.Tiles.FurnitureSilva;
using Microsoft.Xna.Framework;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Biomes.EntropicSpace.Tiles
{
    public class EntropicSlag : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.width = 13;
            Item.height = 10;
            Item.createTile = ModContent.TileType<EntropicSlagTile>();
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.consumable = true;
            Item.maxStack = 9999;
            Item.value = 1000;
        }
    }
    public class EntropicSlagTile : ModTile
    {
        private const short subsheetWidth = 450;
        private const short subsheetHeight = 198;

        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);
            ClamityUtils.MergeWithDistortion(Type);

            CalamityUtils.SetMerge(Type, ModContent.TileType<CosmiliteBrick>());
            CalamityUtils.SetMerge(Type, ModContent.TileType<SilvaCrystal>());
            CalamityUtils.SetMerge(Type, ModContent.TileType<RunicProfanedBrick>());
            CalamityUtils.SetMerge(Type, TileID.LunarOre);
            CalamityUtils.SetMerge(Type, 409); //Luminite Brick

            HitSound = SoundID.Tink;
            MineResist = 2f;
            //MinPick = 225;
            AddMapEntry(Color.Purple);
            //this.RegisterUniversalMerge(TileID.Ash, "CalamityMod/Tiles/Merges/AshMerge");
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.PurpleTorch, 0f, 0f, 1, new Color(255, 255, 255), 1f);
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.Stone, 0f, 0f, 1, new Color(100, 100, 100), 1f);
            return false;
        }

        public override bool CanExplode(int i, int j)
        {
            return false;
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            frameXOffset = i % 2 * subsheetWidth;
            frameYOffset = j % 2 * subsheetHeight;
        }
        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            //return TileFraming.BrimstoneFraming(i, j, resetFrame);
            return typeof(TileFramingSystem).GetMethod("BrimstoneFraming", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { i, j, resetFrame }) as bool? ?? true;
        }
    }
}
