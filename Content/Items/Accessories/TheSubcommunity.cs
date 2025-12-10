using CalamityMod;
using CalamityMod.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Accessories
{
    public class TheSubcommunity : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 4));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 64;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.accessory = true;
            Item.rare = ItemRarityID.Lime;
        }
        private static readonly int TotalCountedBosses = 42;
        internal static float CalculatePower(bool killsOnly = false)
        {
            int numBosses = 0;
            numBosses += NPC.downedSlimeKing.ToInt();
            numBosses += DownedBossSystem.downedDesertScourge.ToInt();
            numBosses += NPC.downedBoss1.ToInt();
            numBosses += DownedBossSystem.downedCrabulon.ToInt();
            numBosses += NPC.downedBoss2.ToInt();
            numBosses += (DownedBossSystem.downedHiveMind || DownedBossSystem.downedPerforator).ToInt();
            numBosses += NPC.downedQueenBee.ToInt();
            numBosses += NPC.downedBoss3.ToInt();
            numBosses += NPC.downedDeerclops.ToInt();
            numBosses += DownedBossSystem.downedSlimeGod.ToInt(); // 10
            numBosses += Main.hardMode.ToInt();
            numBosses += NPC.downedQueenSlime.ToInt();
            numBosses += DownedBossSystem.downedCryogen.ToInt();
            numBosses += NPC.downedMechBoss1.ToInt();
            numBosses += DownedBossSystem.downedAquaticScourge.ToInt();
            numBosses += NPC.downedMechBoss2.ToInt();
            numBosses += DownedBossSystem.downedBrimstoneElemental.ToInt();
            numBosses += NPC.downedMechBoss3.ToInt();
            numBosses += DownedBossSystem.downedCalamitasClone.ToInt();
            numBosses += NPC.downedPlantBoss.ToInt(); // 20
            numBosses += DownedBossSystem.downedLeviathan.ToInt();
            numBosses += DownedBossSystem.downedAstrumAureus.ToInt();
            numBosses += NPC.downedGolemBoss.ToInt();
            numBosses += DownedBossSystem.downedPlaguebringer.ToInt();
            numBosses += NPC.downedFishron.ToInt();
            numBosses += NPC.downedEmpressOfLight.ToInt();
            numBosses += DownedBossSystem.downedRavager.ToInt();
            numBosses += NPC.downedAncientCultist.ToInt();
            numBosses += DownedBossSystem.downedAstrumDeus.ToInt();
            numBosses += NPC.downedMoonlord.ToInt(); // 30
            numBosses += DownedBossSystem.downedGuardians.ToInt();
            numBosses += DownedBossSystem.downedDragonfolly.ToInt();
            numBosses += DownedBossSystem.downedProvidence.ToInt();
            numBosses += DownedBossSystem.downedCeaselessVoid.ToInt();
            numBosses += DownedBossSystem.downedStormWeaver.ToInt();
            numBosses += DownedBossSystem.downedSignus.ToInt();
            numBosses += DownedBossSystem.downedPolterghast.ToInt();
            numBosses += DownedBossSystem.downedBoomerDuke.ToInt();
            numBosses += DownedBossSystem.downedDoG.ToInt();
            numBosses += DownedBossSystem.downedYharon.ToInt(); // 40
            numBosses += DownedBossSystem.downedExoMechs.ToInt();
            numBosses += DownedBossSystem.downedCalamitas.ToInt();
            float bossDownedRatio = numBosses / (float)TotalCountedBosses;
            return killsOnly ? bossDownedRatio : MathHelper.Lerp(0.05f, 0.2f, bossDownedRatio);
        }
        public const float MiningSpeedMult = 2.5f;
        public const float LuckMult = 2.5f;
        public const int FishingPower = 250;
        public const float TileAndWallPlacingSpeedMult = 2.5f;
        //public const float WallPlacingSpeedMult = TileAndWallPlacingSpeedMult;
        public const int TileRangeMult = 25;

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            ClamityPlayer modPlayer = player.Clamity();
            modPlayer.subcommunity = true;
        }
        public override void ModifyTooltips(List<TooltipLine> list)
        {
            float power = CalculatePower();
            string statList = this.GetLocalization("StatsList").Format(
                (MiningSpeedMult * power * 100).ToString("N1"),
                (LuckMult * power).ToString("N2"),
                (int)(FishingPower * power),
                (TileAndWallPlacingSpeedMult * power * 100).ToString("N1"),
                (int)(TileRangeMult * power),
                (CalculatePower(true) * 100).ToString("N0"));
            list.FindAndReplace("[STATS]", statList);
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            CalamityUtils.DrawInventoryCustomScale(
                spriteBatch,
                texture: TextureAssets.Item[Type].Value,
                position,
                frame,
                drawColor,
                itemColor,
                origin,
                scale,
                wantedScale: 0.7f,
                drawOffset: new(0f, 0f)
            );
            return false;
        }

    }
}
