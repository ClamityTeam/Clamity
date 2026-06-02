using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Clamity.Content.Items.Accessories
{
    public abstract class ToggableAccessory : ModItem, ILocalizedModType, IModType
    {
        private bool accessoryEnabled = true;
        public new string LocalizationCategory => "Items.Accessories";
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.FindAndReplace("[TOGGLE]", accessoryEnabled ? this.GetLocalizedValue("ToggleEffect") : "");
        }

        public override bool CanRightClick()
        {
            return Main.keyState.PressingShift();
        }

        public override void RightClick(Player player)
        {
            accessoryEnabled = !accessoryEnabled;
            base.Item.NetStateChanged();
        }

        public override bool ConsumeItem(Player player)
        {
            return false;
        }

        public override void SaveData(TagCompound tag)
        {
            tag.Add("toggle", accessoryEnabled);
        }

        public override void LoadData(TagCompound tag)
        {
            accessoryEnabled = tag.GetBool("toggle");
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(accessoryEnabled);
        }

        public override void NetReceive(BinaryReader reader)
        {
            accessoryEnabled = reader.ReadBoolean();
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            CalamityUtils.DrawInventoryDot(spriteBatch, position, new Vector2(16f, 16f) * Main.inventoryScale, accessoryEnabled);
        }

        public sealed override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (accessoryEnabled) ToggledUpdateAccessory(player, hideVisual);
            SafeUpdateAccessory(player, hideVisual);
        }
        public virtual void ToggledUpdateAccessory(Player player, bool hideVisual)
        {

        }
        public virtual void SafeUpdateAccessory(Player player, bool hideVisual)
        {

        }
    }
}
