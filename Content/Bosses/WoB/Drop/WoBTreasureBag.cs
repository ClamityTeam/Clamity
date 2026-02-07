using CalamityMod;
using CalamityMod.Items.Placeables.Furniture.Paintings;
using Clamity.Content.Bosses.WoB.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.WoB.Drop
{
    public class WoBTreasureBag : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.TreasureBags";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;
            ItemID.Sets.BossBag[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.maxStack = 9999;
            Item.consumable = true;
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Expert;
            Item.expert = true;
        }
        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossBags;
        }
        public override bool CanRightClick()
        {
            return true;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.Lerp(lightColor, Color.White, 0.4f);
        }
        public override void PostUpdate()
        {
            Item.TreasureBagLightAndDust();
        }
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            return CalamityUtils.DrawTreasureBagInWorld(Item, spriteBatch, ref rotation, ref scale, whoAmI);
        }
        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            itemLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(ModContent.NPCType<WallOfBronze>()));
            itemLoot.Add(ModContent.ItemType<HeavyMetalTrashCan>());
            itemLoot.Add(ItemDropRule.FewFromOptionsNotScalingWithLuck(2, 1, ModContent.ItemType<AMS>(), ModContent.ItemType<TheWOBbler>(), ModContent.ItemType<LargeFather>()));
            itemLoot.Add(ModContent.ItemType<WoBMask>(), 7);
            itemLoot.Add(ModContent.ItemType<ThankYouPainting>(), 100);
        }
    }
}
