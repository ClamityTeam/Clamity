using CalamityMod;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Particles;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Weapons.Ranged.Bows
{
    public class WitheredBoneBow : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";
        public override void SetDefaults()
        {
            Item.width = 54;
            Item.height = 114;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = Item.useAnimation = 10;

            Item.useAmmo = AmmoID.Arrow;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.shootSpeed = 10;

            Item.damage = 100;
            Item.DamageType = DamageClass.Ranged;
            Item.knockBack = 5;

            Item.Calamity().devItem = true;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            TooltipLine nameLine = tooltips.FirstOrDefault(x => x.Name == "ItemName" && x.Mod == "Terraria");
            if (nameLine != null)
            {
                nameLine.OverrideColor = CalamityUtils.ColorSwap(Color.LightGray, Color.Blue, 4);
            }
        }
        public override Vector2? HoldoutOffset() => new Vector2?(new Vector2(-5f, 0.0f));
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo spawnSource, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 source = player.RotatedRelativePoint(player.MountedCenter);
            float piOver10 = MathHelper.Pi * 0.1f;
            int totalProjectiles = 5;

            velocity.Normalize();
            velocity *= 40f;
            bool canHit = Collision.CanHit(source, 0, 0, source + velocity, 0, 0);
            for (int p = 0; p < totalProjectiles; p++)
            {
                float offsetAmt = p - (totalProjectiles - 1f) / 2f;
                Vector2 offset = velocity.RotatedBy(piOver10 * offsetAmt);
                if (!canHit)
                    offset -= velocity;

                if (CalamityUtils.CheckWoodenAmmo(type, player))
                {
                    int proj = Projectile.NewProjectile(spawnSource, source.X + offset.X, source.Y + offset.Y, velocity.X, velocity.Y, ModContent.ProjectileType<WitheredBoneBowProj>(), (int)(damage * 1.1), knockback, player.whoAmI);
                    if (proj.WithinBounds(Main.maxProjectiles))
                    {
                        //Main.projectile[proj].arrow = true;
                        //Main.projectile[proj].extraUpdates += 1;
                        //Main.projectile[proj].Clamity().extraAI[0] = 0;
                    }
                }
                else
                {
                    int proj = Projectile.NewProjectile(spawnSource, source.X + offset.X, source.Y + offset.Y, velocity.X, velocity.Y, type, damage, knockback, player.whoAmI);
                    Main.projectile[proj].noDropItem = true;
                }
            }
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Marrow)
                .AddIngredient<RemsRevenge>()
                .AddIngredient<ShadowspecBar>(5)
                .AddTile<DraedonsForge>()
                .Register();
        }
    }
    public class WitheredBoneBowProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        private const int Lifetime = 600;
        // Radius of the "circle of inaccuracy" surrounding the mouse. Blue bullets will aim at this circle.
        private const float MouseAimDeviation = 13f;
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = true;
            //Projectile.aiStyle = ProjAIStyleID.Arrow;
            //AIType = ProjectileID.WoodenArrowFriendly;
            Projectile.aiStyle = -1;
            AIType = -1;
            //Projectile.MaxUpdates = 2;
            Projectile.penetrate = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.timeLeft = Lifetime;
            //Projectile.Calamity().pointBlankShotDuration = CalamityGlobalProjectile.DefaultPointBlankDuration;
            //Projectile.extraUpdates = 1;
            Projectile.arrow = true;
            Projectile.Clamity().extraAI[0] = 0;
            Projectile.ArmorPenetration = 15;

        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            //Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.GemSapphire);
            //dust.noGravity = true;
            GeneralParticleHandler.SpawnParticle(new GenericSparkle(Projectile.Center, Projectile.velocity / 4, Color.LightBlue, Color.Cyan, 0.5f, 10));

            CalamityUtils.HomeInOnNPC(Projectile, false, 1000, 30, 10);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<WitherDebuff>(), 120);

            if (Projectile.Clamity().extraAI[0] <= 2 && Projectile.TryGetOwner(out Player player))
            {
                //Main.NewText("Trigger " + Projectile.Clamity().extraAI[0].ToString());

                // Step 1 of the warp: Place the bullet behind the player, opposite the mouse cursor.
                Vector2 playerToMouseVec = CalamityUtils.SafeDirectionTo(player, Main.MouseWorld, -Vector2.UnitY);
                float warpDist = Main.rand.NextFloat(70f, 96f);
                float warpAngle = Main.rand.NextFloat(-MathHelper.Pi / 3f, MathHelper.Pi / 3f);
                Vector2 warpOffset = -warpDist * playerToMouseVec.RotatedBy(warpAngle);
                Projectile.position = player.MountedCenter + warpOffset;

                // Step 2 of the warp: Angle the bullet so that it is pointing at the mouse cursor.
                // This intentionally has a slight inaccuracy.
                Vector2 mouseTargetVec = Main.MouseWorld + Main.rand.NextVector2Circular(MouseAimDeviation, MouseAimDeviation);
                Vector2 bulletToMouseVec = CalamityUtils.SafeDirectionTo(Projectile, mouseTargetVec, -Vector2.UnitY);
                Projectile.velocity = bulletToMouseVec * 100f;
                //Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

                Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.position, Projectile.velocity, Type, Projectile.damage, Projectile.knockBack, Projectile.owner, Projectile.ai[0] + 1);
                proj.Clamity().extraAI[0] = Projectile.Clamity().extraAI[0] + 1;

                GeneralParticleHandler.SpawnParticle(new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.LightBlue, new Vector2(0.5f, 0.5f), Main.rand.NextFloat(12f, 25f), 0f, 1f, 20));
                for (int i = 0; i < 3; i++)
                {
                    GeneralParticleHandler.SpawnParticle(new SparkParticle(Projectile.Center, Vector2.UnitX.RotatedBy(MathHelper.TwoPi / 4 * i), affectedByGravity: false, 10, 1f, Color.LightBlue));
                }
            }
        }
    }
}
