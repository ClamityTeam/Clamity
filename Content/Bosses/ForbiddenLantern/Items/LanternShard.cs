using CalamityMod;
using CalamityMod.Events;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.NPCs.Cryogen;
using CalamityMod.Particles;
using CalamityMod.UI.DialogueDisplay;
using CalamityMod.UI.DialogueDisplay.DisplayEffects;
using Clamity.Content.Bosses.ForbiddenLantern.NPCs;
using Clamity.Content.Bosses.Pyrogen.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.ForbiddenLantern.Items
{
    public class LanternShard : ModItem, ILocalizedModType, IModType
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
                if (proj.type == ModContent.ProjectileType<LanternShardDrama>())
                {
                    hasntProj = false;
                }
            }
            if (player.Calamity().ZoneSunkenSea && !NPC.AnyNPCs(ModContent.NPCType<TheForbiddenLantern>()) && hasntProj)
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
            SoundEngine.PlaySound(in Cryogen.ShieldRegenSound, player.Center);
            if (player.altFunctionUse == 2)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<TheForbiddenLantern>());
                }
                else
                {
                    NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, -1, -1, null, player.whoAmI, ModContent.NPCType<TheForbiddenLantern>());
                }
                //DialogueDisplaySystem.StartDialogue("Mods.Clamity.TheForbiddenLantern.Intro", player.Center, 0, 120, false, new BossText());
            }
            else
                Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, ModContent.ProjectileType<LanternShardDrama>(), 0, 0, player.whoAmI);

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
            CreateRecipe()
                .AddIngredient<SeaRemains>(5)
                .AddIngredient<SeaPrism>(12)
                .AddIngredient<Navystone>(30)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    public class LanternShardDrama : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 1;
            Projectile.aiStyle = -1;
            Projectile.alpha = 255;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = MaxTimeLeft;
        }
        public static int MaxTimeLeft => 60;
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
            /*if (Projectile.timeLeft % 10 == 0)
            {
                Color color = Color.Lerp(Color.Red, Color.Yellow, 1f - Projectile.timeLeft / 60f);
                GeneralParticleHandler.SpawnParticle(new DirectionalPulseRing(Projectile.Center, Vector2.Zero, color, new Vector2(0.5f, 0.5f), Main.rand.NextFloat(12f, 25f), 10f, 0f, 20));
            }*/

        }
        public override void OnKill(int timeLeft)
        {

            GeneralParticleHandler.SpawnParticle(new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.Lerp(Color.Aquamarine, Color.LightBlue, 1), new Vector2(0.5f, 0.5f), Main.rand.NextFloat(12f, 25f), 0f, 20f, 40));
            GeneralParticleHandler.SpawnParticle(new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.Lerp(Color.Aquamarine, Color.LightBlue, .5f), new Vector2(0.5f, 0.5f), Main.rand.NextFloat(12f, 25f), 0f, 17.5f, 40));
            GeneralParticleHandler.SpawnParticle(new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.Lerp(Color.Aquamarine, Color.LightBlue, 0), new Vector2(0.5f, 0.5f), Main.rand.NextFloat(12f, 25f), 0f, 15f, 40));
            NPC npc = NPC.NewNPCDirect(Projectile.GetSource_Death(), Projectile.Center, ModContent.NPCType<TheForbiddenLantern>());
            
            //ClamityUtils.BossIntroDialogue("Pyrogen", npc);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D l = ModContent.Request<Texture2D>("CalamityMod/Projectiles/StarProj").Value;
            float rotation = MathF.Sin(Projectile.timeLeft / MaxTimeLeft * MathHelper.Pi / 2);
            float scale = MathF.Sin(Projectile.timeLeft / MaxTimeLeft * MathHelper.Pi);

            Main.spriteBatch.Draw(l, Projectile.Center - Main.screenPosition, null, Color.White, rotation, l.Size()/2, new Vector2(1f, 4f* scale) * Projectile.scale, SpriteEffects.None, 1);
            Main.spriteBatch.Draw(l, Projectile.Center - Main.screenPosition, null, Color.White, rotation + MathHelper.PiOver2, l.Size()/2, new Vector2(1f, 4f * scale) * Projectile.scale, SpriteEffects.None, 1);



            return false;
        }
    }
}
