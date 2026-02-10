using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables;
using CalamityMod.Items.Weapons.Rogue;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.CalamityUtils;
using Clamity.Content.Items.Weapons.Rogue;
using CalamityMod.Items.Placeables.Abyss;

namespace Clamity.Content.Items.Weapons.Rogue
{
    public class StarfishFromTheDepth : RogueWeapon
    {
        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 52;
            Item.rare = ItemRarityID.Lime;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;

            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.UseSound = null;
            Item.autoReuse = true;

            Item.damage = 150;
            Item.knockBack = 5.5f;
            Item.shoot = ModContent.ProjectileType<StarfishFromTheDepthProj>();

            Item.shootSpeed = 3f;
            Item.DamageType = ModContent.GetInstance<RogueDamageClass>();
        }

        public override bool AltFunctionUse(Player player)
        {
            return true; // Enables right-click
        }

        public override bool CanUseItem(Player player)
        {
            // Right-click: force all owned starfish projectiles to return
            if (player.altFunctionUse == 2)
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];

                    if (p.active &&
                        p.owner == player.whoAmI &&
                        p.type == ModContent.ProjectileType<StarfishFromTheDepthProj>() &&
                        p.ai[0] != 1f)
                    {
                        p.ai[0] = 1f;
                        p.netUpdate = true;
                    }
                }

                return false; // Do not swing or throw
            }

            // Left-click: only allow one active projectile
            return !Main.projectile.Any(p =>
                p.active &&
                p.owner == player.whoAmI &&
                p.type == ModContent.ProjectileType<StarfishFromTheDepthProj>());
        }



        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            if (proj.WithinBounds(Main.maxProjectiles))
                Main.projectile[proj].Calamity().stealthStrike = player.Calamity().StealthStrikeAvailable();
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<FishboneBoomerang>()
                .AddIngredient<Voidstone>(20)
                .AddIngredient<DepthCells>(20)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    public class StarfishFromTheDepthProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => ModContent.GetInstance<StarfishFromTheDepth>().Texture;

        public static int ChargeupTime => 10;
        public static int Lifetime => 1500;
        public float OverallProgress => 1 - Projectile.timeLeft / (float)Lifetime;
        public float ThrowProgress => 1 - Projectile.timeLeft / (float)(Lifetime);
        public float ChargeProgress => 1 - (Projectile.timeLeft - Lifetime) / (float)(ChargeupTime);

        private bool hasHit;

        public Player Owner => Main.player[Projectile.owner];
        public ref float Returning => ref Projectile.ai[0];
        //public ref float Bouncing => ref Projectile.ai[1];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 38;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime + ChargeupTime;
            Projectile.DamageType = ModContent.GetInstance<RogueDamageClass>();
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override bool ShouldUpdatePosition()
        {
            return ChargeProgress >= 1;
        }

        public override bool? CanDamage()
        {
            //We don't want the anticipation to deal damage.
            if (ChargeProgress < 1)
                return false;

            return base.CanDamage();
        }

        //Swing animation keys
        public CurveSegment pullback = new CurveSegment(EasingType.PolyOut, 0f, 0f, MathHelper.PiOver4 * -1.2f, 2);
        public CurveSegment throwout = new CurveSegment(EasingType.PolyOut, 0.7f, MathHelper.PiOver4 * -1.2f, MathHelper.PiOver4 * 1.2f + MathHelper.PiOver2, 3);
        internal float ArmAnticipationMovement() => PiecewiseAnimation(ChargeProgress, new CurveSegment[] { pullback, throwout });

        public override void AI()
        {
            //Anticipation animation. Make the player look like theyre holding the fish skeleton
            if (ChargeProgress < 1)
            {
                float armRotation = ArmAnticipationMovement() * Owner.direction;

                Owner.heldProj = Projectile.whoAmI;

                Projectile.Center = Owner.MountedCenter + Vector2.UnitY.RotatedBy(armRotation * Owner.gravDir) * -40f * Owner.gravDir;
                Projectile.rotation = (-MathHelper.PiOver2 + armRotation) * Owner.gravDir;

                Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathHelper.Pi + armRotation);

                return;
            }

            if (ChargeProgress >= 1f && Owner.HeldItem.type != ModContent.ItemType<StarfishFromTheDepth>())
            {
                Returning = 1f;
            }

            //Play the throw sound when the throw ACTUALLY BEGINS.
            //Additionally, make the projectile collide and set its speed and velocity
            if (Projectile.timeLeft == Lifetime)
            {

                SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
                Projectile.Center = Owner.MountedCenter + Projectile.velocity * 12f;
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 17.5f;
                Projectile.tileCollide = true;

                if (Projectile.Calamity().stealthStrike)
                {
                    int index1 = Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity.RotatedBy(0.4f).SafeNormalize(Vector2.Zero) * 17.5f, Projectile.type, Projectile.damage, Projectile.knockBack, Projectile.owner);
                    Main.projectile[index1].timeLeft = Lifetime - 2;
                    index1 = Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity.RotatedBy(-0.4f).SafeNormalize(Vector2.Zero) * 17.5f, Projectile.type, Projectile.damage, Projectile.knockBack, Projectile.owner);
                    Main.projectile[index1].timeLeft = Lifetime - 2;
                }
            }

            //Boomerang spinny sounds
            if (Projectile.soundDelay <= 0)
            {
                SoundEngine.PlaySound(SoundID.Item7, Projectile.Center);
                Projectile.soundDelay = 8;
            }

            Projectile.rotation += (MathHelper.PiOver4 / 4f + MathHelper.PiOver4 / 2f * Math.Clamp(ThrowProgress * 2f, 0, 1)) * Math.Sign(Projectile.velocity.X);

            /*if (Projectile.velocity.Length() < 2f)
            {
                Returning = 1f;
                Projectile.numHits = 0;
            }*/

            /*if (Returning == 0f && Projectile.velocity.Length() > 2f && Projectile.timeLeft < (205 + ChargeupTime))
            {
                Projectile.velocity *= 0.88f;
            }*/

            if (Returning == 1f && Projectile.velocity.Length() < 20f)
            {
                Projectile.velocity *= 1.1f;
            }

            for (int i = 0; i < 5; i++)
            {
                Vector2 dustPos = Projectile.Center + (i * MathHelper.TwoPi / 5f + Projectile.rotation + MathHelper.PiOver2).ToRotationVector2() * 14f;
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.RedsWingsRun, (i * MathHelper.TwoPi / 5f + Projectile.rotation * Math.Sign(Projectile.velocity.X)).ToRotationVector2() * 3f, newColor: new Color(0, 255, 255));
                dust.noGravity = true;
            }



            if (Returning == 1f) //Returning
            {
                Projectile.tileCollide = false;
                //Aim back at the player
                Projectile.velocity = Projectile.velocity.Length() * (Owner.MountedCenter - Projectile.Center).SafeNormalize(Vector2.One);
                Projectile.timeLeft = 10;

                if ((Projectile.Center - Owner.MountedCenter).Length() < 24f)
                {
                    Projectile.Kill();
                }

                /*if (Projectile.numHits >= 5)
                {
                    ImpactEffects();
                    Projectile.Kill();
                }*/
            }
            else //Homing to target
            {

                NPC newTarget = null;
                float closestNPCDistance = 2000f;
                float targettingDistance = 4000f;
                foreach (NPC n in Main.ActiveNPCs)
                {
                    if (n.CanBeChasedBy(Projectile))
                    {
                        float potentialNewDistance = (Projectile.Center - n.Center).Length();
                        if (potentialNewDistance < targettingDistance && potentialNewDistance < closestNPCDistance)
                        {
                            closestNPCDistance = potentialNewDistance;
                            newTarget = n;
                        }
                    }
                }
                if (newTarget != null && Projectile.numHits <= 10)
                {
                    float inertia = 30f;
                    Vector2 targetVector = Projectile.SafeDirectionTo(newTarget.Center);
                    Projectile.velocity = (Projectile.velocity * (inertia - 1f) + (targetVector) * 17.5f) / inertia;
                }
                if (Projectile.numHits > 10)
                {
                    Returning = 1f;
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ImpactEffects();

            float streakRotation;
            for (int i = 0; i < 5; i++)
            {
                streakRotation = Main.rand.NextFloat(MathHelper.TwoPi);

                for (int j = 0; j < 4; j++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + streakRotation.ToRotationVector2() * (2f + 0.4f * j), DustID.RedsWingsRun, streakRotation.ToRotationVector2() * (0.6f * j + 3f), Scale: 1.4f, newColor: new Color(0, 255, 255));
                    dust.noGravity = true;
                }
            }

            if (Projectile.numHits > 10)
            {
                //Projectile.velocity *= 0.3f;
                Returning = 1f;
            }
            else
            {
                Projectile.velocity = -Projectile.velocity.RotatedByRandom(0.4f) * (Projectile.numHits == 0 ? 1 : 2);
            }

            Player player = Main.player[Projectile.owner];

            if (!hasHit)
            {
                hasHit = true;

                Item heldItem = player.HeldItem;

                // Get damage modifier for the item's damage type (melee, ranged, rogue, etc.)
                StatModifier damageMod = player.GetTotalDamage(heldItem.DamageType);

                float finalDamage = damageMod.ApplyTo(heldItem.damage);
                Projectile.damage = (int)Math.Round(finalDamage);
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            ImpactEffects();
            //Projectile.velocity = Projectile.oldVelocity.Length() * 0.3f * (Owner.MountedCenter - Projectile.Center).SafeNormalize(Vector2.One);
            //Returning = 1f;
            return false;
        }

        public void ImpactEffects()
        {
            SoundStyle bonkSound = SoundID.DD2_SkeletonHurt with { Volume = SoundID.DD2_SkeletonHurt.Volume * 0.8f, Pitch = SoundID.DD2_SkeletonHurt.Pitch + 0.1f * Projectile.numHits };

            SoundEngine.PlaySound(bonkSound, Projectile.Center);
            int goreNumber = Main.rand.Next(4);

            for (int i = 0; i < goreNumber; i++)
            {
                int goreID = Main.rand.NextBool() ? 266 : Main.rand.NextBool() ? 971 : 972;
                Gore bone = Gore.NewGorePerfect(Projectile.GetSource_FromAI(), Projectile.position, Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(5f, 5f), goreID);
                bone.scale = Main.rand.NextFloat(0.6f, 1f) * (goreID == 972 ? 0.7f : 1f); //Shrink the larger bones
                bone.type = goreID; //Gotta do that or else itll spawn gores from the general pool :(
            }
        }
    }
}
