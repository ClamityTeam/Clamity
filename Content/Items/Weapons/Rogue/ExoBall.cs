using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.CalPlayer;
using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Rarities;
using Clamity.Content.Projectiles;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Weapons.Rogue
{
    public class ExoBall : RogueWeapon
    {

        public override void SetDefaults()
        {
            Item.damage = 314;
            Item.DamageType = ModContent.GetInstance<RogueDamageClass>();
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.width = Item.height = 24;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5;
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.rare = ModContent.RarityType<Violet>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;

            Item.shootSpeed = 7f;
            Item.shoot = ModContent.ProjectileType<ExoBallProjectile>();

        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.shoot = ProjectileID.None;
                return player.ownedProjectileCounts[ModContent.ProjectileType<ExoBallProjectile>()] > 0;
            }
            else
            {
                Item.shoot = ModContent.ProjectileType<ExoBallProjectile>();
                return player.ownedProjectileCounts[ModContent.ProjectileType<ExoBallProjectile>()] < 10;
            }
        }

        public override float StealthDamageMultiplier => 0.75f;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.killSpikyBalls = false;
            if (modPlayer.StealthStrikeAvailable()) //setting the stealth strike
            {
                int stealth = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
                if (stealth.WithinBounds(Main.maxProjectiles))
                    Main.projectile[stealth].Calamity().stealthStrike = true;
                return false;
            }
            return true;
        }

        public override bool AltFunctionUse(Player player)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.killSpikyBalls = true;
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<Nychthemeron>()
                .AddIngredient<GodsParanoia>()
                .AddIngredient<HellsSun>()
                .AddIngredient<MiracleMatter>()
                .AddTile<CalamityMod.Tiles.Furniture.CraftingStations.DraedonsForge>()
                .Register();
        }
    }
    public class ExoBallProjectile : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => ModContent.GetInstance<ExoBall>().Texture;
        public int kunaiStabbing = 10;

        public override void SetStaticDefaults()
        {
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
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
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

            Projectile.StickyProjAI(50);

            if (Projectile.ai[0] == 1f)
            {
                kunaiStabbing++;
                if (kunaiStabbing >= 90 || (Projectile.Calamity().stealthStrike && kunaiStabbing >= 60))
                {
                    kunaiStabbing = 0;
                    float startOffsetX = Main.rand.NextFloat(200f, 400f) * (Main.rand.NextBool() ? -1f : 1f);
                    float startOffsetY = Main.rand.NextFloat(200f, 400f) * (Main.rand.NextBool() ? -1f : 1f);
                    Vector2 startPos = new Vector2(Projectile.position.X + startOffsetX, Projectile.position.Y + startOffsetY);
                    float dx = Projectile.position.X - startPos.X;
                    float dy = Projectile.position.Y - startPos.Y;

                    // Add some randomness / inaccuracy
                    dx += Main.rand.NextFloat(-5f, 5f);
                    dy += Main.rand.NextFloat(-5f, 5f);
                    float speed = Main.rand.NextFloat(20f, 25f);
                    float dist = (float)Math.Sqrt((double)(dx * dx + dy * dy));
                    dist = speed / dist;
                    dx *= dist;
                    dy *= dist;
                    Vector2 kunaiSp = new Vector2(dx, dy);
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    if (Projectile.owner == Main.myPlayer)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            int idx = Projectile.NewProjectile(Projectile.GetSource_FromThis(), startPos, kunaiSp, ModContent.ProjectileType<ExoNewBeam>(), Projectile.damage / 2, Projectile.knockBack / 3f, Projectile.owner, 0f, 0f);
                            Main.projectile[idx].DamageType = ModContent.GetInstance<RogueDamageClass>();
                        }
                    }
                }
            }
            else
            {
                Projectile.rotation += 0.2f * (float)Projectile.direction;
                CalamityUtils.HomeInOnNPC(Projectile, false, 400f, Projectile.Calamity().stealthStrike ? 10f : 5f, 20f);
            }

            Player player = Main.player[Projectile.owner];
            CalamityPlayer modPlayer = player.Calamity();
            if (modPlayer.killSpikyBalls == true)
            {
                Projectile.active = false;
                Projectile.netUpdate = true;
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => Projectile.ModifyHitNPCSticky(10);

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

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<MiracleBlight>(), 120);

        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<MiracleBlight>(), 120);
    }
}
