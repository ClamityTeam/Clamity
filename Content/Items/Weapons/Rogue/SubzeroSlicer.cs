using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Projectiles.Summon;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Clamity.Content.Biomes.FrozenHell.Items;
using Clamity.Content.Items.Weapons.Melee;
using Clamity.Content.Rarities;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.CalamityUtils;

namespace Clamity.Content.Items.Weapons.Rogue
{
    [LegacyName("FrozenStarShuriken")]
    public class SubzeroSlicer : RogueWeapon
    {
        public override float StealthDamageMultiplier => 0.5f;
        public override void SetDefaults()
        {
            Item.width = 1;
            Item.height = 1;
            Item.rare = ModContent.RarityType<EvercoldCyan>();
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;

            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = Item.useAnimation = 45;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;

            Item.damage = 350;
            Item.DamageType = ModContent.GetInstance<RogueDamageClass>();
            Item.knockBack = 4f;

            Item.shootSpeed = 7f;
            Item.shoot = ModContent.ProjectileType<SubzeroSlicerProjectile>();
        }
        public override bool CanUseItem(Player player)
        {
            return base.CanUseItem(player);
            return !Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ModContent.ProjectileType<SubzeroSlicerProjectile>());
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int index = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0/*Main.rand.Next(3)*/);
            //Main.projectile[index].ai[0] = 2;
            if (player.Calamity().StealthStrikeAvailable())
            {
                Main.projectile[index].Calamity().stealthStrike = true;
                Main.projectile[index].ai[0] = 0;
            }
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<BlazingStar>()
                .AddIngredient<FrostcrushValari>()
                .AddIngredient(ItemID.Trimarang)
                .AddIngredient<EndobsidianBar>(8)
                .AddTile<CosmicAnvil>()
                .Register();
        }
    }
    public class SubzeroSlicerProjectile : ModProjectile, ILocalizedModType, IModType
    {
        public override string Texture => ModContent.GetInstance<SubzeroSlicer>().Texture;
        public new string LocalizationCategory => "Projectiles.Rogue";

        public static int ChargeupTime => 10;
        public static int Lifetime => 500;
        public float OverallProgress => 1 - Projectile.timeLeft / (float)Lifetime;
        public float ThrowProgress => 1 - Projectile.timeLeft / (float)(Lifetime);
        public float ChargeProgress => 1 - (Projectile.timeLeft - Lifetime) / (float)(ChargeupTime);
        public Player Owner => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 4;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 90;
            Projectile.aiStyle = -1;
            AIType = -1;
            Projectile.friendly = true;
            //Projectile.tileCollide = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime + ChargeupTime;
            Projectile.DamageType = ModContent.GetInstance<RogueDamageClass>();
            //Projectile.extraUpdates = 1;
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
                Projectile.spriteDirection = Owner.direction;

                Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathHelper.Pi + armRotation);

                return;
            }
            else
            {
                Projectile.rotation += (MathHelper.PiOver4 / 4f + MathHelper.PiOver4 / 2f * Math.Clamp(ThrowProgress * 2f, 0, 1)) * Math.Sign(Projectile.velocity.X);
                Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
            }

            //Play the throw sound when the throw ACTUALLY BEGINS.
            //Additionally, make the projectile collide and set its speed and velocity
            if (Projectile.timeLeft == Lifetime)
            {

                SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
                Projectile.Center = Owner.MountedCenter + Projectile.velocity * 12f;
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 20f;
                //Projectile.tileCollide = true;
                Owner.heldProj = -1;

                if (Projectile.Calamity().stealthStrike)
                {
                    //WardrobeHummus, here also balancing. Remove this comment after balancing.
                    int index1 = Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity.RotatedBy(0.4f).SafeNormalize(Vector2.Zero) * 17.5f, Projectile.type, Projectile.damage, Projectile.knockBack, Projectile.owner);
                    Main.projectile[index1].timeLeft = Lifetime - 2;
                    Main.projectile[index1].ai[0] = 0;
                    index1 = Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity.RotatedBy(-0.4f).SafeNormalize(Vector2.Zero) * 17.5f, Projectile.type, Projectile.damage, Projectile.knockBack, Projectile.owner);
                    Main.projectile[index1].timeLeft = Lifetime - 2;
                    Main.projectile[index1].ai[0] = 0;
                }
            }

            //Boomerang spinny sounds
            if (Projectile.soundDelay <= 0)
            {
                SoundEngine.PlaySound(SoundID.Item7, Projectile.Center);
                Projectile.soundDelay = 8;
            }

            //I don't know why weapon losts their damage if player changes item from this weapon to other item
            //He instantly decreases up to -2.1 mil
            //Updated 1: I find a bug and he is bug from Fargo's Souls Mod and he affects and on Fishbone Boomerang
            /*if (Projectile.damage < 1)
            {
                Projectile.ai[1] = 1f;
                Projectile.tileCollide = false;
                //Projectile.Kill();
            }*/

            //Movement AI
            if (Projectile.ai[1] == 1f) //Returning
            {
                Projectile.tileCollide = false;
                //Aim back at the player
                Projectile.velocity = Projectile.velocity.Length() * (Owner.MountedCenter - Projectile.Center).SafeNormalize(Vector2.One);
                if (Projectile.velocity.Length() < 20)
                    Projectile.velocity *= 1.05f;
                if (Projectile.velocity.Length() < 2f)
                    Projectile.velocity = Projectile.Center.SafeDirectionTo(Owner.MountedCenter) * 2;
                Projectile.timeLeft = 10;

                if (Projectile.ai[0] == 0)
                {
                    Projectile.ai[2] = MathHelper.Clamp(Projectile.ai[2] - 0.05f, 0, 1f);
                    Projectile.ExpandHitboxBy((int)(500 * Projectile.ai[2]));
                }

                if ((Projectile.Center - Owner.MountedCenter).Length() < 50f) //24
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
                if (Projectile.timeLeft < 11)
                {
                    Projectile.ai[1] = 1;
                    Projectile.tileCollide = false;
                }
                switch ((int)Projectile.ai[0])
                {
                    case 0: //Vortex

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
                        if (newTarget != null)
                        {
                            Vector2 targetVector = Projectile.SafeDirectionTo(newTarget.Center);
                            if (Projectile.Clamity().extraAI[0] == 0) //Homing to target
                            {
                                //Projectile.Clamity().extraAI[1] = MathHelper.Clamp(Projectile.Clamity().extraAI[1] + 0.1f, 0, 10f);
                                Projectile.ai[2] = MathHelper.Clamp(Projectile.ai[2] - 0.02f, 0, Projectile.Calamity().stealthStrike ? 1f : 0.5f);
                                Projectile.velocity = targetVector * Projectile.velocity.Length();
                                if (Projectile.velocity.Length() < 120)
                                    Projectile.velocity += Projectile.velocity.SafeNormalize(Vector2.Zero) * .5f;

                                if (newTarget.Distance(Projectile.Center) < 100)
                                {
                                    Projectile.Clamity().extraAI[0] = 1;
                                }
                            }
                            else //Active vortex
                            {
                                Projectile.ai[2] = MathHelper.Clamp(Projectile.ai[2] + 0.01f, 0, Projectile.Calamity().stealthStrike ? 1f : 0.5f);
                                Projectile.velocity *= 0.85f;
                                Projectile.velocity += newTarget.velocity / 2;

                                if (newTarget.Distance(Projectile.Center) > 100)
                                {
                                    Projectile.Clamity().extraAI[0] = 0;
                                }
                            }
                        }
                        else if (Projectile.timeLeft < Lifetime - 60)
                        {
                            Projectile.ai[1] = 1f;
                            Projectile.tileCollide = false;
                        }
                        Projectile.ExpandHitboxBy((int)(500 * Projectile.ai[2]));

                        break;
                    //TODO - Rework Subzero Slicer Again.
                    //Type 1 and 2 no longer used
                    case 1: //Fast slices
                        NPC newTarget1 = null;
                        float closestNPCDistance1 = 2000f;
                        float targettingDistance1 = 4000f;
                        foreach (NPC n in Main.ActiveNPCs)
                        {
                            if (n.CanBeChasedBy(Projectile))
                            {
                                float potentialNewDistance1 = (Projectile.Center - n.Center).Length();
                                if (potentialNewDistance1 < targettingDistance1 && potentialNewDistance1 < closestNPCDistance1)
                                {
                                    closestNPCDistance1 = potentialNewDistance1;
                                    newTarget1 = n;
                                }
                            }
                        }
                        if (newTarget1 != null)
                        {
                            Projectile.velocity = CalamityUtils.RotateTowards(Projectile.velocity, Projectile.AngleTo(newTarget1.Center), 0.2f);
                            if (Projectile.Distance(newTarget1.Center) > 500)
                            {
                                if (Projectile.velocity.Length() < 30)
                                    Projectile.velocity += Projectile.velocity.SafeNormalize(Vector2.Zero) * 0.1f;
                            }
                            else
                            {
                                if (Projectile.velocity.Length() > 20.1f)
                                    Projectile.velocity -= Projectile.velocity.SafeNormalize(Vector2.Zero) * 0.1f;

                            }
                        }
                        else if (Projectile.timeLeft < Lifetime - 60)
                        {
                            Projectile.ai[1] = 1f;
                            Projectile.tileCollide = false;
                        }
                        break;
                    case 2: //Dash with projectiles
                        if (++Projectile.Clamity().extraAI[0] > 60)
                        {

                            NPC newTarget2 = null;
                            float closestNPCDistance2 = 2000f;
                            float targettingDistance2 = 2000f;
                            foreach (NPC n in Main.ActiveNPCs)
                            {
                                if (n.CanBeChasedBy(Projectile))
                                {
                                    float potentialNewDistance2 = (Projectile.Center - n.Center).Length();
                                    if (potentialNewDistance2 < targettingDistance2 && potentialNewDistance2 < closestNPCDistance2)
                                    {
                                        closestNPCDistance2 = potentialNewDistance2;
                                        newTarget2 = n;
                                    }
                                }
                            }
                            if (newTarget2 != null)
                            {
                                //Projectile.velocity = (newTarget2.Center - Projectile.Center) / 10f;
                                Projectile.velocity = Projectile.SafeDirectionTo(newTarget2.Center) * 30;
                                for (int i = 0; i < 5; i++)
                                {
                                    int index1 = Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity + Main.rand.NextVector2Circular(10, 10), ModContent.ProjectileType<SubzeroSlicerProjectileBolt>(), Projectile.damage / 10, Projectile.knockBack / 2, Projectile.owner);
                                    //Main.projectile[index1].penetrate = 2;
                                }
                            }
                            else if (Projectile.timeLeft < Lifetime - 60)
                            {
                                Projectile.ai[1] = 1f;
                                Projectile.tileCollide = false;
                            }
                            Projectile.Clamity().extraAI[0] = 0;
                        }
                        Projectile.velocity *= 0.95f;
                        break;
                }
            }
            //Main.NewText($"{Projectile.timeLeft} {CanDamage() is null}");
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.ai[1] = 1f;
            Projectile.tileCollide = false;
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.ai[0] == 1)
            {
                Projectile.NewProjectile(Projectile.GetSource_OnHit(target), Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.Zero) / 100, ModContent.ProjectileType<SubzeroSlicerProjectileSlash>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner, target.whoAmI);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.ai[0] == 0)
            {
                Texture2D texture = ModContent.Request<Texture2D>("Clamity/Assets/Textures/SlicerVortex").Value;
                Utilities.UseBlendState(Main.spriteBatch, BlendState.Additive);
                //Utilities.PrepareForShaders(Main.spriteBatch);
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, Color.LightSkyBlue, Main.GlobalTimeWrappedHourly * 20, texture.Size() / 2, Projectile.ai[2], SpriteEffects.None, 0);
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, Color.LightSkyBlue, -Main.GlobalTimeWrappedHourly * 14, texture.Size() / 2, Projectile.ai[2], SpriteEffects.FlipHorizontally, 0);
                Utilities.UseBlendState(Main.spriteBatch, BlendState.AlphaBlend);
            }

            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], Color.LightSkyBlue * .9f, texture: ModContent.Request<Texture2D>(Texture + "_Trail").Value);
            Texture2D t = ModContent.Request<Texture2D>(Texture).Value;
            Main.spriteBatch.Draw(t, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, t.Size()/2, Projectile.scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            return false;
        }
    }
    public class SubzeroSlicerProjectileBolt : GhostFire, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.DamageType = ModContent.GetInstance<RogueDamageClass>();
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            //Player player = Main.player[Projectile.owner];
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D lightTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/SmallGreyscaleCircle").Value;

            Color baseColor = Color.Cyan, baseColor2 = Color.LightBlue;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float colorInterpolation = (float)Math.Cos(Projectile.timeLeft / 32f + Main.GlobalTimeWrappedHourly / 20f + i / (float)Projectile.oldPos.Length * MathHelper.Pi) * 0.5f + 0.5f;
                Color color = Color.Lerp(baseColor, baseColor2, colorInterpolation) * 0.4f;
                color.A = 0;
                Vector2 drawPosition = Projectile.oldPos[i] + lightTexture.Size() * 0.5f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY) + new Vector2(-28f, -28f); //Last vector is to offset the circle so that it is displayed where the hitbox actually is, instead of a bit down and to the right.
                Color outerColor = color;
                Color innerColor = color * 0.5f;
                float intensity = 0.9f + 0.15f * (float)Math.Cos(Main.GlobalTimeWrappedHourly % 60f * MathHelper.TwoPi);
                intensity *= MathHelper.Lerp(0.15f, 1f, 1f - i / (float)Projectile.oldPos.Length);
                if (Projectile.timeLeft <= 60) //Shrinks to nothing when projectile is nearing death
                {
                    intensity *= Projectile.timeLeft / 60f;
                }
                // Become smaller the futher along the old positions we are.
                Vector2 outerScale = new Vector2(1f) * intensity;
                Vector2 innerScale = new Vector2(1f) * intensity * 0.7f;
                outerColor *= intensity;
                innerColor *= intensity;
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, outerColor, 0f, lightTexture.Size() * 0.5f, outerScale * 0.6f, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, innerColor, 0f, lightTexture.Size() * 0.5f, innerScale * 0.6f, SpriteEffects.None, 0);
            }
            return false;
        }
    }
    public class SubzeroSlicerProjectileSlash : BaseSlash
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.DamageType = ModContent.GetInstance<RogueDamageClass>();
        }
        public override float Scale => 1.5f;
        public override Color FirstColor => Color.LightBlue;
        public override Color SecondColor => Color.Cyan;
        public override bool? CanHitNPC(NPC target)
        {
            return target.whoAmI != Projectile.ai[0];
        }
    }
}
