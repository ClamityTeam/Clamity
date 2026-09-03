using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Weapons.Ranged.Bows
{
    public class RoseBow : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";
        public override void SetDefaults()
        {
            Item.width = 54;
            Item.height = 114;
            Item.rare = ModContent.RarityType<PureGreen>();
            Item.value = CalamityGlobalItem.RarityPureGreenBuyPrice;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = Item.useAnimation = 30;

            Item.useAmmo = AmmoID.Arrow;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.shootSpeed = 10;

            Item.damage = 400;
            Item.DamageType = DamageClass.Ranged;
            Item.knockBack = 5;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 5; i++)
            {
                Projectile.NewProjectile(source, position, (velocity + Main.rand.NextVector2Circular(2.5f, 2.5f)).RotatedByRandom(0.33f), type, (int)(damage * 0.5f), knockback);
            }
            for (int i = 0; i < 5; i++)
            {
                int num2 = ModContent.ProjectileType<BeamingBolt2>();
                int index = Projectile.NewProjectile(source, position, (velocity + Main.rand.NextVector2Circular(2.5f, 2.5f)).RotatedByRandom(0.33f), num2, (int)(damage * 0.5f), knockback);
                Main.projectile[index].DamageType = DamageClass.Ranged;
            }
            return false;
        }
        public override Vector2? HoldoutOffset() => new Vector2?(new Vector2(-5f, 0.0f));
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.PearlwoodBow)
                .AddIngredient<ArchAmaryllis>()
                .AddIngredient<RuinousSoul>(5)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
