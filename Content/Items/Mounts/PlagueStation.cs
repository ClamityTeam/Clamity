using CalamityMod.Items.Mounts;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Mounts
{
    public class PlagueStation : ExoThrone
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.mountType = ModContent.MountType<PlagueChairMount>();
            Item.rare = ItemRarityID.Yellow;
        }
    }
    public class PlagueChairMount : DraedonGamerChairMount
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            MountData.buff = ModContent.BuffType<PlagueChairBuff>();
            MountData.runSpeed = 5f;
            MountData.dashSpeed = 5f;
            MountData.acceleration = 5f;
            MountData.swimSpeed = 5f;
            if (Main.netMode != NetmodeID.Server)
            {
                MountData.frontTextureGlow = ModContent.Request<Texture2D>("Clamity/Content/Items/Mounts/PlagueChairMount_Glowmask");
                MountData.textureWidth = MountData.frontTexture.Width();
                MountData.textureHeight = MountData.frontTexture.Height();
            }
        }
    }
    public class PlagueChairBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.mount.SetMount(ModContent.MountType<PlagueChairMount>(), player);
            player.buffTime[buffIndex] = 10;
            player.Clamity().FlyingChair = true;
            player.Clamity().FlyingChairPower = 5;
        }
    }
}
