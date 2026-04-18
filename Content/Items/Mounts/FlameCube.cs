using CalamityMod.Items.Mounts;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Mounts
{
    public class FlameCube : ExoThrone
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<ExoThrone>();
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.mountType = ModContent.MountType<BrimChairMount>();
            //Item.rare = ModContent.RarityType
        }
    }
    public class BrimChairMount : DraedonGamerChairMount
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            MountData.buff = ModContent.BuffType<BrimChairBuff>();
            //MountData.runSpeed = 13f;
            //MountData.dashSpeed = 13f;
            //MountData.acceleration = 13f;
            //MountData.swimSpeed = 13f;
            if (Main.netMode != NetmodeID.Server)
            {
                MountData.frontTextureGlow = ModContent.Request<Texture2D>("Clamity/Content/Items/Mounts/BrimChairMount_Glowmask");
                MountData.textureWidth = MountData.frontTexture.Width();
                MountData.textureHeight = MountData.frontTexture.Height();
            }
        }
    }
    public class BrimChairBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.mount.SetMount(ModContent.MountType<BrimChairMount>(), player);
            player.buffTime[buffIndex] = 10;
            player.Clamity().flyingChair = true;
        }
    }
}
