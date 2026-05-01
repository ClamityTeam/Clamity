using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Accessories
{
    public class SeaShell : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 46;
            Item.accessory = true;
            Item.value = Item.sellPrice(0, 0, 40);
            Item.rare = ItemRarityID.Green;
            Item.defense = 3;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.Clamity().seaShell = true;
        }
        public static int parryTime = 30;
        public static void HandleParryCountdown(Player player)
        {

            player.Clamity().seaShellParryingTime--;

            if (player.Clamity().seaShellParryingTime > 0)
            {
                player.controlJump = false;
                player.controlDown = false;
                player.controlLeft = false;
                player.controlRight = false;
                player.controlUp = false;
                player.controlUseItem = false;
                player.controlUseTile = false;
                player.controlThrow = false;
                player.gravDir = 1f;
                player.velocity = Vector2.Zero;
                player.velocity.Y = -0.1f; //if player velocity is 0, the flight meter gets reset
                player.RemoveAllGrapplingHooks();
            }
            else
            {
                /*for (int i = 0; i < 8; i++)
                {
                    int theDust = Dust.NewDust(player.position, player.width, player.height, (int)CalamityDusts.Brimstone, 0f, 0f, 100, new Color(255, 255, 255), 2f);
                    Main.dust[theDust].noGravity = true;
                }*/
            }
        }
    }
}
