using CalamityMod;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Biomes.FrozenHell.Items
{
    public class FrozenHellstone : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Placeables";

        public override void SetStaticDefaults()
        {
            ItemID.Sets.SortingPriorityMaterials[Type] = 99;
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(0, 1, 0, 0);
            Item.rare = ModContent.RarityType<BurnishedAuric>();

            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;

            Item.consumable = true;
            Item.createTile = ModContent.TileType<FrozenHellstoneTile>();
        }
    }
    public class FrozenHellstoneTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileOreFinderPriority[Type] = 950;
            Main.tileSpelunker[Type] = true;
            Main.tileShine2[Type] = true;
            Main.tileShine[Type] = 975;
            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeWithHell(Type);
            //CalamityUtils.SetMerge(Type, ModContent.TileType<FrozenAshTile>());
            TileID.Sets.Ore[Type] = true;
            TileID.Sets.OreMergesWithMud[Type] = true;
            AddMapEntry(new Color(208, byte.MaxValue, byte.MaxValue), CreateMapEntryName());
            MineResist = 3f;
            MinPick = 280;
            HitSound = new SoundStyle?(SoundID.Tink);
            Main.tileSpelunker[Type] = true;
        }

        public override bool CanExplode(int i, int j) => false;

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
}
