using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.CalPlayer;
using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.NPCs;
using CalamityMod.Rarities;
using Clamity.Content.Projectiles;
using Microsoft.Xna.Framework;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Weapons.Rogue
{
    public class ExoBall : RogueWeapon
    {
        public override bool IsLoadingEnabled(Mod mod)
        {
            return false;
        }

        public override void SetDefaults()
        {
            Item.damage = 150;
            Item.DamageType = ModContent.GetInstance<RogueDamageClass>();
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.width = Item.height = 24;
            Item.useTime = Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5;
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.rare = ModContent.RarityType<BurnishedAuric>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;

            Item.shootSpeed = 7f;
            Item.shoot = ModContent.ProjectileType<ExoBallProjectile>();

        }

        public override float StealthDamageMultiplier => 0.75f;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.Calamity().StealthStrikeAvailable()) //setting the stealth strike
            {
                int stealth = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
                if (stealth.WithinBounds(Main.maxProjectiles))
                {
                    Main.projectile[stealth].Calamity().stealthStrike = true;
                    Main.projectile[stealth].localNPCHitCooldown = 30;
                }
                return false;
            }
            return true;
        }

        public override bool AltFunctionUse(Player player)
        {
            CalamityPlayer modPlayer = player.Calamity();
            //modPlayer.killSpikyBalls = true;
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<GodsParanoia>()
                .AddIngredient<MetalMonstrosity>()
                .AddIngredient<BurningStrife>()
                .AddIngredient<MiracleMatter>()
                .AddTile<CalamityMod.Tiles.Furniture.CraftingStations.DraedonsForge>()
                .Register();
        }
    }
    [PierceResistException]
    public class ExoBallProjectile : ModProjectile, ILocalizedModType
    {
        public override bool IsLoadingEnabled(Mod mod)
        {
            return false;
        }
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => ModContent.GetInstance<ExoBall>().Texture;
        public int kunaiStabbing = 10;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 24;
            Projectile.ignoreWater = true;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = ModContent.GetInstance<RogueDamageClass>();
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 40;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.35f, 0f, 0.25f);
            if (Main.rand.NextBool())
            {
                Dust flame = Dust.NewDustDirect(Projectile.position, 1, 1, Main.rand.NextBool(3) ? 56 : 242, 0f, 0f, 0, default, 0.5f);
                flame.alpha = Projectile.alpha;
                flame.velocity = Vector2.Zero;
                flame.noGravity = true;
            }

            //Projectile.StickyProjAI(50);

            ClamityUtils.FindModsClass("CalamityMod", "CalamityMod.Projectiles.CommonProjectileAI").GetMethod("StickyProjAI", BindingFlags.Public | BindingFlags.Static).Invoke(null, Projectile, 50, false);

            if (Projectile.ai[0] == 1f)
                Projectile.MaxUpdates = 1; // Prevents the homing function extra updates from carrying over
            else
            {
                Projectile.rotation += 0.2f * Projectile.direction;
                CalamityUtils.HomeInOnNPC(Projectile, false, 400f, Projectile.Calamity().stealthStrike ? 16f : 8f, 20f);
            }
        }


        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => ClamityUtils.FindModsClass("CalamityMod", "CalamityMod.Projectiles.CommonProjectileAI").GetMethod("ModifyHitNPCSticky", BindingFlags.Public | BindingFlags.Static).Invoke(null, Projectile, 10);
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (targetHitbox.Width > 8 && targetHitbox.Height > 8)
            {
                targetHitbox.Inflate(-targetHitbox.Width / 8, -targetHitbox.Height / 8);
            }
            return null;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }
        private void OnHitBolts()
        {
            for (int i = 0; i < 2; i++)
            {
                float startOffsetX = Main.rand.NextFloat(125f, 250f) * Main.rand.NextBool().ToDirectionInt();
                float startOffsetY = Main.rand.NextFloat(125f, 250f) * Main.rand.NextBool().ToDirectionInt();
                Vector2 startPos = Projectile.Center + new Vector2(startOffsetX, startOffsetY);

                Vector2 kunaiSp = Vector2.Normalize(Projectile.Center - startPos) * 25f;
                int idx = Projectile.NewProjectile(Projectile.GetSource_FromThis(), startPos, kunaiSp, ModContent.ProjectileType<ExoNewBeam>(), Projectile.damage / 3, Projectile.knockBack / 3f, Projectile.owner, ai2: Projectile.Calamity().stealthStrike && Main.rand.NextBool(15) ? 1 : 0);
                Main.projectile[idx].DamageType = ModContent.GetInstance<RogueDamageClass>();
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MiracleBlight>(), 120);
            OnHitBolts();
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<MiracleBlight>(), 120);
            OnHitBolts();
        }
    }
}
