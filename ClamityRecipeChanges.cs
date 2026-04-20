using CalamityMod;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Items.Weapons.Rogue;
using Clamity.Content.Biomes.FrozenHell.Items;
using Clamity.Content.Bosses.Clamitas.Drop;
using Clamity.Content.Bosses.Pyrogen.Drop.Weapons;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity
{
    public class ClamityRecipeChanges : ModSystem
    {
        public override void AddRecipes()
        {
            Recipe.Create(ModContent.ItemType<PearlGod>())
                .AddIngredient<Arietes41>()
                .AddIngredient<LifeAlloy>(5)
                .AddIngredient<RuinousSoul>(2)
                .AddIngredient<ClamitousPearl>()
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
        public override void PostAddRecipes()
        {
            ChangeVanillaRecipes();
        }
        private void ChangeVanillaRecipes()
        {
            foreach (Recipe recipe in Main.recipe)
            {

                //Weapon Changes
                if (ClamityConfig.Instance.UsesClamityWeaponsInCalamityRecipes)
                {
                    if (recipe.HasResult<Seadragon>())
                        recipe.requiredItem.Insert(1, new Item(ModContent.ItemType<Obsidigun>(), 1));
                    if (recipe.HasResult<ShatteredDawn>())
                        recipe.requiredItem.Insert(1, new Item(ModContent.ItemType<MoltenPiercer>(), 1));
                    if (recipe.HasResult<NuclearFury>())
                        recipe.requiredItem.Insert(1, new Item(ModContent.ItemType<TheGenerator>(), 1));
                    /*if (recipe.HasResult<ElementalShiv>())
                    {
                        recipe.requiredItem.Insert(0, ModContent.GetInstance<TerraShiv>().Item);
                    }*/
                }

                //Changes in Calamity with using Clamity Materials
                if (ClamityConfig.Instance.ChangesCalamityRecipes)
                {
                    //Husk of Calamity
                    if (recipe.HasResult(ModContent.ItemType<TheAbsorber>()))
                    {
                        recipe.ReplaceIngredient(ModContent.ItemType<MolluskHusk>(), ModContent.ItemType<HuskOfCalamity>(), 5);
                    }
                    if (recipe.HasResult(ModContent.ItemType<TheAmalgam>()))
                    {
                        recipe.ReplaceIngredient(ModContent.ItemType<MolluskHusk>(), ModContent.ItemType<HuskOfCalamity>(), 10);
                    }
                    if (recipe.HasResult(ModContent.ItemType<AbyssalDivingSuit>()))
                    {
                        recipe.ReplaceIngredient(ModContent.ItemType<MolluskHusk>(), ModContent.ItemType<HuskOfCalamity>(), 15);
                    }
                }

                //Other changes
                if (recipe.HasResult<ShadowspecBar>() && ClamityConfig.Instance.UsesEnchantedMetalInShadowspecBarRecipe)
                {
                    //item3.stack = 10;
                    recipe.requiredItem.Insert(1, new Item(ModContent.ItemType<EndobsidianBar>()));
                }
                if (recipe.HasResult<ManaRose>() && recipe.IngredientIndex(ItemID.JungleRose) != -1)
                {
                    recipe.DisableRecipe();
                }
            }
        }
        /*private void ChangeVanillaRecipes()
        {
            Dictionary<Func<Recipe, bool>, Action<Recipe>> changes = new Dictionary<Func<Recipe, bool>, Action<Recipe>>(128);

            //changes.Add(Vanilla(ItemID.Magiluminescence), RemoveIngredient(ItemID.DemoniteBar));
            //changes.Add(Vanilla(ItemID.Magiluminescence), RemoveIngredient(ItemID.CrimtaneBar));
            //changes.Add(Vanilla(ItemID.Magiluminescence), AddIngredient(Mod.ItemType("Thulecite"), 12));
            //changes.Add(Vanilla(ItemID.Magiluminescence), ReplaceTile(TileID.Anvils, Mod.TileType("AncientPseudoscienceStationTile")));


            Dictionary<Func<Recipe, bool>, Action<Recipe>> changes2 = changes;
            IEnumerator<Recipe> recipes = (IEnumerator<Recipe>)((IEnumerable<Recipe>)Main.recipe).ToList<Recipe>().GetEnumerator();
            while (recipes.MoveNext())
            {
                Recipe current_recipe = recipes.Current;
                using (Dictionary<Func<Recipe, bool>, Action<Recipe>>.Enumerator changes_enumerator = changes2.GetEnumerator())
                {
                    while (changes_enumerator.MoveNext())
                    {
                        KeyValuePair<Func<Recipe, bool>, Action<Recipe>> change = changes_enumerator.Current;
                        if (change.Key(current_recipe))
                            change.Value(current_recipe);
                    }
                }
            }
        }
        public Func<Recipe, bool> Vanilla(int itemID) => (Func<Recipe, bool>)(r => r.Mod == null && r.HasResult(itemID));
        public Func<Recipe, bool> Modded(int itemID) => (Func<Recipe, bool>)(r => r.Mod != null && r.HasResult(itemID));
        public Action<Recipe> RemoveIngredient(int itemID) => (Action<Recipe>)(r => r.RemoveIngredient(itemID));
        public Action<Recipe> AddIngredient(int itemID, int stack = 1) => (Action<Recipe>)(r => r.AddIngredient(itemID, stack));
        public Action<Recipe> ReplaceTile(int oldTileID, int newTileID) => (Action<Recipe>)(r =>
        {
            int index = ((List<int>)r.requiredTile).IndexOf(oldTileID);
            if (index == -1)
                return;
            ((List<int>)r.requiredTile)[index] = newTileID;
        });*/

    }
}
