using CalamityMod;
using CalamityMod.Items.DraedonMisc;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Furniture.CraftingStations;
using CalamityMod.Items.SummonItems;
using CalamityMod.Items.TreasureBags;
using CalamityMod.Items.TreasureBags.MiscGrabBags;
using Clamity.Content.Items.Accessories;
using Clamity.Content.Items.Mounts;
using Clamity.Content.Items.Potions.Food;
using Clamity.Content.Items.Weapons.Classless;
using Clamity.Content.Items.Weapons.Melee.Shortswords;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;

namespace Clamity
{
    public class ClamityGlobalItem : GlobalItem
    {
        /*public readonly int geode;
        public override void Load()
        {
            geode = ModContent.ItemType<NecromanticGeode>();
        }
        public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
        {
            switch (item.type)
            {
                case geode:

                    break;
            }
        }*/
        public override bool InstancePerEntity => true;
        public bool keyItem = false;
        public bool referenceItem = false;
        public override void SetDefaults(Item entity)
        {
            if (entity.ModItem is CodebreakerBase or DecryptionComputer or LongRangedSensorArray or AdvancedDisplay or VoltageRegulationSystem or AuricQuantumCoolingCell
                or AltarOfTheAccursedItem or AshesofCalamity
                or EyeofDesolation or CosmicWorm or YharonEgg or MarkofProvidence or ProfanedCore or ProfanedShard)
            {
                //keyItem = true;
            }
        }
        public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (item.DamageType == ModContent.GetInstance<RogueDamageClass>())
            {
                if (player.Clamity().vampireEX && player.Calamity().StealthStrikeAvailable())
                {
                    //NPC npc = NPC
                    for (int i = 0; i < 10; i++)
                    {
                        Projectile.NewProjectile(player.GetSource_Accessory(item), player.Center, Main.rand.NextVector2CircularEdge(5, 5), ModContent.ProjectileType<DraculasCharmProj>(), 25, 0.1f, player.whoAmI, -1);
                    }
                }
            }

        }
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (keyItem)
            {
                TooltipLine line = new TooltipLine(Mod, "ClamityKey", CalamityUtils.ColorMessage("- Key Item -", Color.Gold));
                tooltips.Add(line);
            }
            if (referenceItem)
            {
                TooltipLine line = new TooltipLine(Mod, "ClamityReference", CalamityUtils.ColorMessage("- Reference Item -", Color.Lime));
                tooltips.Add(line);
            }
        }
        public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
        {
            if (item.type == ModContent.ItemType<StarterBag>())
            {
                LeadingConditionRule leadingConditionRule = itemLoot.DefineConditionalDropSet((Func<bool>)(() => WorldGen.SavedOreTiers.Copper == 166));


                //Mod clicker = ModLoader.GetMod("ClickerClass");

                if (ModLoader.TryGetMod("ClickerClass", out Mod clicker))
                {
                    leadingConditionRule.Add(new CommonDrop(clicker.Find<ModItem>("CopperClicker").Item.type, 1));
                    leadingConditionRule.OnFailedConditions(new CommonDrop(clicker.Find<ModItem>("TinClicker").Item.type, 1));
                }
            }
            if (item.type == ModContent.ItemType<PlaguebringerGoliathBag>())
            {
                itemLoot.Add(ModContent.ItemType<Disease>(), 4);
                itemLoot.Add(ModContent.ItemType<PlagueStation>());
                itemLoot.Add(ItemDropRule.NormalvsExpert(ModContent.ItemType<TrashOfMagnus>(), 4, 3));
            }
            if (item.type == ModContent.ItemType<CalamitasCoffer>())
            {
                itemLoot.Add(ModContent.ItemType<Calamitea>(), 1, 10, 10);
            }
        }
    }
}
