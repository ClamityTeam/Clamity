using CalamityMod;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Ores;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Biomes.EntropicSpace.Tiles
{
    public class CosmiliteOre : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
            ItemID.Sets.SortingPriorityMaterials[Type] = 119;
        }

        public override void SetDefaults()
        {
            Item.width = 10;
            Item.height = 10;
            Item.createTile = ModContent.TileType<CosmiliteOreTile>();
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.consumable = true;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ModContent.RarityType<CosmicPurple>();
        }
        public override void AddRecipes()
        {
            Recipe.Create(ModContent.ItemType<CosmiliteBar>())
                .AddIngredient(Item.type, 6)
                .AddTile(TileID.AdamantiteForge)
                .Register();
        }
    }
    public class CosmiliteOreTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileOreFinderPriority[Type] = 950;

            CalamityUtils.MergeWithGeneral(Type);
            ClamityUtils.MergeWithDistortion(Type);

            TileID.Sets.Ore[Type] = true;
            //TileID.Sets.OreMergesWithMud[Type] = true;

            AddMapEntry(Color.Magenta, CreateMapEntryName());
            MineResist = 3f;
            MinPick = 275;
            HitSound = AuricOre.MineSound;
            Main.tileSpelunker[Type] = true;


            //this.RegisterUniversalMerge(TileID.Dirt, "CalamityMod/Tiles/Merges/DirtMerge");
            //this.RegisterUniversalMerge(TileID.Stone, "CalamityMod/Tiles/Merges/StoneMerge");
            //this.RegisterUniversalMerge(TileID.Mud, "CalamityMod/Tiles/Merges/MudMerge");
        }

        public override bool CanExplode(int i, int j)
        {
            return false;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
    }
}
