using CalamityMod.Events;
using CalamityMod.Rarities;
using Clamity.Content.Bosses.WoB.NPCs;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.WoB
{
    public class WoBSummonItem : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.SummonBoss";
        public override void SetDefaults()
        {
            Item.width = Item.height = 32;
            Item.value = Terraria.Item.sellPrice(0, 10, 0);
            Item.rare = ModContent.RarityType<BurnishedAuric>();

            Item.useTime = Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.UseSound = SoundID.Item1;
            Item.consumable = false;
        }
        public override bool? UseItem(Player player)
        {
            //player.Teleport(new Vector2(100, Main.UnderworldLayer + 40) * 16);
            //NPC.NewNPC(player.GetSource_ItemUse(Item), (int)player.Center.X - 100, (int)player.Center.Y, ModContent.NPCType<WallOfBronze>());
            //NPC.SpawnWOF

            int center = Main.maxTilesX * 16 / 2;
            NPC.NewNPC(player.GetSource_ItemUse(Item), (int)player.Center.X - 1000 * (player.Center.X > center ? -1 : 1), (int)player.Center.Y, ModContent.NPCType<WallOfBronze>());

            SoundEngine.PlaySound(AncientConsoleTile.SummonSound, player.Center);

            /*if (Main.netMode != NetmodeID.MultiplayerClient)
                NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<WallOfBronze>());
            else
                NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, player.whoAmI, (int)ModContent.NPCType<WallOfBronze>());*/

            return true;
        }
        public override bool CanUseItem(Player player)
        {
            return player.ZoneUnderworldHeight && !NPC.AnyNPCs(ModContent.NPCType<WallOfBronze>()) && !BossRushEvent.BossRushActive;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<AncientConsole>()
                .Register();
        }
    }
}
