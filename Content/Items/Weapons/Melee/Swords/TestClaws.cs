using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Clamity.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Weapons.Melee.Swords
{
    public class TestClaws : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override bool IsLoadingEnabled(Mod mod)
        {
            return false;
        }
        public override void SetDefaults()
        {
            Item.width = 26; Item.height = 18;
            Item.rare = ItemRarityID.Blue;
            Item.value = CalamityGlobalItem.RarityBlueBuyPrice;

            Item.useAnimation = Item.useTime = 6;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;

            Item.damage = 1000;
            Item.DamageType = DamageClass.Melee;
            Item.knockBack = 3.5f;
        }
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!Main.rand.NextBool(4))
                Projectile.NewProjectile(player.GetSource_OnHit(target), target.Center, Vector2.Zero, ModContent.ProjectileType<ExoNewExplosion>(), Item.damage, Item.knockBack, player.whoAmI);
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.FetidBaghnakhs)
                .AddIngredient<MycelialClaws>()
                .AddIngredient<MantisClaws>()
                //.AddIngredient<MandibleClaws>()
                .AddIngredient<MiracleMatter>()
                .AddTile<DraedonsForge>()
                .Register();
        }
    }
}
