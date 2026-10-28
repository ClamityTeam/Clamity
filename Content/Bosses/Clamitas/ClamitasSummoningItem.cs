using CalamityMod;
using CalamityMod.Events;
using CalamityMod.Items.Materials;
using Clamity.Content.Bosses.Clamitas.Drop;
using Clamity.Content.Bosses.Clamitas.NPCs;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Clamitas
{
    public class ClamitasSummoningItem : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.SummonBoss";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 36;
            Item.rare = ItemRarityID.Lime;
            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.HoldUp;
        }
        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossItem;
        }
        public override bool CanUseItem(Player player)
        {
            if (player.Calamity().ZoneCalamity && !NPC.AnyNPCs(ModContent.NPCType<ClamitasBoss>()))
            {
                return !BossRushEvent.BossRushActive;
            }

            return false;
        }
        public override bool? UseItem(Player player)
        {
            SoundEngine.PlaySound(in SoundID.Roar, player.Center);
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<ClamitasBoss>());
            }
            else
            {
                NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, -1, -1, null, player.whoAmI, ModContent.NPCType<ClamitasBoss>());
            }

            return true;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<AshesofCalamity>(10)
                .AddIngredient<MolluskHusk>(5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
