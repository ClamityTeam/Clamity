using CalamityMod.Items.Materials;
using Clamity.Content.Bosses.Clamitas.Drop;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace Clamity.Content.Bosses.Clamitas.Crafted
{
    public class SupremeLuckPotion : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Potions";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
        }
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 30;
            Item.maxStack = 9999;
            Item.value = 1000;
            Item.rare = ItemRarityID.Pink;

            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useTime = Item.useAnimation = 17;
            Item.UseSound = SoundID.Item3;

            Item.consumable = true;
            Item.buffType = ModContent.BuffType<SupremeLucky>();
            Item.buffTime = 54000;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater)
                .AddIngredient(ItemID.Fireblossom)
                .AddIngredient<ClamitousPearl>()
                .AddIngredient(ItemID.LadyBug)
                .AddTile(TileID.Bottles)
                .Register();
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater)
                .AddIngredient<BloodOrb>(10)
                .AddIngredient<ClamitousPearl>()
                .AddTile(TileID.AlchemyTable)
                .Register();
        }
    }
    public class SupremeLucky : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.buffImmune[BuffID.Lucky] = true;
            player.Clamity().supremeLuck = true;
        }
    }
}
