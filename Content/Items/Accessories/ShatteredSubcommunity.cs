using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Accessories;
using CalamityMod.Rarities;
using CalamityMod.UI.CalamitasEnchants;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Clamity.Content.Items.Accessories
{
    public class ShatteredSubcommunity : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        // The percentage of a full Rage bar that is gained every second with the Shattered Community equipped.
        public const float BaseRagePerSecond = 0.02f;

        private static readonly Color rarityColorOne = new Color(128, 62, 128);
        private static readonly Color rarityColorTwo = new Color(245, 105, 245);

        internal const long BaseLevelCost = 2500L;
        internal static long LevelCost(int level) => BaseLevelCost * level;
        internal static long CumulativeLevelCost(int level) => (BaseLevelCost / 2L) * level * (level + 1);
        internal const int MaxLevel = 25; // was 60.
        internal const float CalmSpeedUpPerLevel = 0.001f; // 0.02 --> 0.045

        internal int level = 0;
        internal long totalCalmDone = 0L;

        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(5, 4));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<TheSubcommunity>();
            EnchantmentManager.ItemUpgradeRelationship.Add(ModContent.ItemType<TheSubcommunity>(), Type);
            HotPink.CustomColors.Add(Type, GetRarityColor); // Yes, this reuses Shattered Community's color (and this comment from HotPink.cs lol)
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.accessory = true;
            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
        }

        // Not overriding these Clones makes tooltips fail to function correctly due to HoverItem spaghetti.
        public override ModItem Clone(Item item)
        {
            var clone = (ShatteredSubcommunity)base.Clone(Item);
            clone.level = level;
            clone.totalCalmDone = totalCalmDone;
            return clone;
        }
        internal static Color GetRarityColor() => CalamityUtils.ColorSwap(rarityColorOne, rarityColorTwo, 3f);
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.Clamity().shatteredSubcommunity = true;
            player.Clamity().shatteredSubcommunityCalmPerSecond += level * CalmSpeedUpPerLevel;

            ShatteredSubcommunityPlayer scp = player.GetModPlayer<ShatteredSubcommunityPlayer>();
            scp.sc = this;

            player.pickSpeed -= TheSubcommunity.MiningSpeedMult;
            //calamityPlayer.calamityBonusLuck += baseBoost * TheSubcommunity.LuckMult;
            player.fishingSkill += TheSubcommunity.FishingPower;
            player.tileSpeed += TheSubcommunity.TileAndWallPlacingSpeedMult;
            player.wallSpeed += TheSubcommunity.TileAndWallPlacingSpeedMult;
            Player.tileRangeX += TheSubcommunity.TileRangeMult;
            Player.tileRangeY += TheSubcommunity.TileRangeMult;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // Add the proper description which changes depending on world difficulty
            //string desc = CalamityWorld.revenge ? this.GetLocalizedValue("RageModified") : this.GetLocalization("RageAdd").Format(CalamityKeybinds.RageHotKey.TooltipHotkeyString());
            //tooltips.FindAndReplace("[RAGEDESC]", desc);

            // Add the current level
            tooltips.FindAndReplace("[LEVEL]", level.ToString());

            // Add the current bonus
            tooltips.FindAndReplace("[BONUS]", (CalmSpeedUpPerLevel / BaseRagePerSecond * level * 100).ToString("0"));

            // Add the progress
            string progressKey = "[PROGRESS]";
            TooltipLine progressLine = tooltips.FirstOrDefault(x => x.Mod == "Terraria" && x.Text.Contains(progressKey));
            if (progressLine != null)
            {
                if (level < MaxLevel)
                {
                    long progressToNextLevel = totalCalmDone - CumulativeLevelCost(level);
                    long totalToNextLevel = LevelCost(level + 1);
                    double ratio = (double)progressToNextLevel / totalToNextLevel;
                    string percent = (100D * ratio).ToString("0.00");
                    progressLine.Text = progressLine.Text.Replace(progressKey, percent);
                }
                else
                    progressLine.Text = string.Empty;
            }

            // Add the current cumulative damage
            tooltips.FindAndReplace("[TIME]", totalCalmDone.ToString());
        }
        public override void SaveData(TagCompound tag)
        {
            tag.Add("level", level);
            tag.Add("totalCalmDone", totalCalmDone);
        }

        public override void LoadData(TagCompound tag)
        {
            level = tag.GetInt("level");
            // Shattered Community's level cap was reduced from 60 to 25, so cap out ones that were made higher previously.
            if (level > MaxLevel)
                level = MaxLevel;
            totalCalmDone = tag.GetLong("totalCalmDone");
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(level);
            writer.Write(totalCalmDone);
        }

        public override void NetReceive(BinaryReader reader)
        {
            level = reader.ReadInt32();
            totalCalmDone = reader.ReadInt64();
        }
    }
    public class ShatteredSubcommunityPlayer : ModPlayer
    {
        internal ShatteredSubcommunity sc = null;

        public override void ResetEffects() => sc = null;

        internal void AccumulateCalmDone(long time)
        {
            if (sc is null)
                return;

            // Actually accumulate the damage.
            sc.totalCalmDone += time;

            // Level up if applicable.
            if (sc.level < ShatteredSubcommunity.MaxLevel && sc.totalCalmDone > ShatteredSubcommunity.CumulativeLevelCost(sc.level + 1))
            {
                ++sc.level;
                LevelUpEffects(sc.Item);
            }
        }

        private void LevelUpEffects(Item item)
        {
            // Spawn the purple laser beam from failing the Dungeon Defenders event.
            var source = Player.GetSource_Accessory(item);
            int projID = ProjectileID.DD2ElderWins;
            Vector2 offset = new Vector2(0f, 800f); // The effect is extremely tall, so start it very low down
            Projectile fx = Projectile.NewProjectileDirect(source, Player.Center + offset, Vector2.Zero, projID, 0, 0f, Player.whoAmI);
            fx.friendly = false;
            fx.hostile = false;
            // On the 108th update, crystal debris is spawned, so we avoid that.
            fx.timeLeft = 107;
            fx.MaxUpdates = 2; // Make the animation play at double speed.

            // Play a weird dimensional lightning sound simultaneously.
            var extraSound = SoundID.DD2_EtherianPortalDryadTouch with { Volume = SoundID.DD2_EtherianPortalDryadTouch.Volume * 1.4f };
            SoundEngine.PlaySound(extraSound, Player.Center);

            // Display a level up text notification.
            Rectangle textArea = new Rectangle((int)Player.Center.X, (int)Player.Center.Y, 1, 1);
            Color textColor = new Color(236, 209, 236);
            CombatText.NewText(textArea, textColor, CalamityUtils.GetTextValueFromModItem<ShatteredCommunity>("LevelUpText"), false, false);
        }

    }
}
