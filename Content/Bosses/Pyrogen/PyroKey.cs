using CalamityMod.Events;
using CalamityMod.Items.Materials;
using CalamityMod.Particles;
using Clamity.Content.Bosses.Pyrogen.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;


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
            bool hasntProj = true;
            foreach (Projectile proj in Main.projectile)
            {
                if (proj == null || !proj.active) continue;
                if (proj.type == ModContent.ProjectileType<PyrogenSummonAnimation>() && proj.owner == player.whoAmI)
                {
                    hasntProj = false;
                }
            }
            if (player.ZoneDesert && !NPC.AnyNPCs(ModContent.NPCType<PyrogenBoss>()) && hasntProj)
            {
                return !BossRushEvent.BossRushActive;
            }

            return false;
        }

        public override bool AltFunctionUse(Player player)
        {
            return CanUseItem(player);
        }

        public override bool? UseItem(Player player)
        {
            SoundEngine.PlaySound(in SoundID.Roar, player.Center);
            if (player.altFunctionUse == 2)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<PyrogenBoss>());
                }
                else
                {
                    NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, -1, -1, null, player.whoAmI, ModContent.NPCType<PyrogenBoss>());
                }
            }
            else
                Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, ModContent.ProjectileType<PyrogenSummonAnimation>(), 0, 0, player.whoAmI);

            return true;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frameI, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D value = ModContent.Request<Texture2D>(Texture).Value;
            spriteBatch.Draw(value, position, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D value = ModContent.Request<Texture2D>(Texture).Value;
            spriteBatch.Draw(value, base.Item.position - Main.screenPosition, null, lightColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().AddRecipeGroup("AnySandBlock", 50).AddIngredient(ItemID.SoulofLight, 5).AddIngredient(ItemID.SoulofNight, 5)
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
            Player player = Main.player[Projectile.owner];
            if (player.dead || player.ghost)
                Projectile.Kill();

            Projectile.Center = player.Center - new Vector2(0, 200);
            if (Projectile.timeLeft % 10 == 0 /*&& Projectile.timeLeft > 40*/)
            {
                Color color = Color.Lerp(Color.Red, Color.Yellow, 1f - Projectile.timeLeft / 60f);
                GeneralParticleHandler.SpawnParticle(new DirectionalPulseRing(Projectile.Center, Vector2.Zero, color, new Vector2(0.5f, 0.5f), Main.rand.NextFloat(12f, 25f), 10f, 0f, 20));
            }

        }
        public override void OnKill(int timeLeft)
        {
            GeneralParticleHandler.SpawnParticle(new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.Red, new Vector2(0.5f, 0.5f), Main.rand.NextFloat(12f, 25f), 0f, 20f, 40));
            NPC.NewNPCDirect(Projectile.GetSource_Death(), Projectile.Center, ModContent.NPCType<PyrogenBoss>());
        }
    }
}
