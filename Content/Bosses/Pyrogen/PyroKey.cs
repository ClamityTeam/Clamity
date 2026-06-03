using CalamityMod.CalPlayer;
using CalamityMod;
using CalamityMod.Events;
using CalamityMod.Items.Materials;
using Clamity.Content.Bosses.Pyrogen.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Items.Placeables.Crags;
using Microsoft.Xna.Framework;
using CalamityMod.Particles;
using CalamityMod.UI.DialogueDisplay;
using CalamityMod.UI.DialogueDisplay.DisplayEffects;
using Terraria.Audio;


namespace Clamity.Content.Bosses.Pyrogen
{
    public class PyroKey : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.SummonBoss";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 7;
        }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 48;
            Item.rare = ItemRarityID.Pink;
            //Item.useAnimation = 10;
            //Item.useTime = 10;
            //Item.useStyle = ItemUseStyleID.HoldUp;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossItem;
        }

        /*
        public override bool CanUseItem(Player player)
        {
            bool hasntProj = true;
            foreach (Projectile proj in Main.projectile)
            {
                if (proj == null || !proj.active) continue;
                if (proj.type == ModContent.ProjectileType<PyrogenSummonAnimation>() && proj.owner == player.whoAmI)
                {
                    hasntProj = false;
                }
            }

            CalamityPlayer modPlayer = player.Calamity();
            return modPlayer.ZoneCalamity && !NPC.AnyNPCs(ModContent.NPCType<PyrogenBoss>()) && !BossRushEvent.BossRushActive && hasntProj;
        }

        public override bool? UseItem(Player player)
        {
            Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, ModContent.ProjectileType<PyrogenSummonAnimation>(), 0, 0, player.whoAmI);
            return true;
        }
        */

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<BrimstoneSlag>(50)
                .AddIngredient(ItemID.SoulofLight, 5)
                .AddIngredient(ItemID.SoulofNight, 5)
                .AddIngredient<EssenceofHavoc>(8)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class PyrogenSummonAnimation : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 1;
            Projectile.aiStyle = -1;
            Projectile.alpha = 255;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 60;
        }
        public override bool? CanDamage()
        {
            return false;
        }
        public override void AI()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC npc = NPC.NewNPCDirect(Projectile.GetSource_Death(), Projectile.Center, ModContent.NPCType<PyrogenBoss>(), target: Projectile.owner);
                npc.netUpdate = true;
                Projectile.Kill();
            }
        }
    }
}
