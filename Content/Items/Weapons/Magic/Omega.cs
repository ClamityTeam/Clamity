using CalamityMod;
using CalamityMod.CalPlayer;
using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Weapons.Magic
{
    [LegacyName("OmegaRay")]
    public class Omega : ModItem, ILocalizedModType
    {
        public static float FireRate = 15f;
        public static float StarterWinup = 60f;
        public static float WingmanFireRate = 10f;

        public new string LocalizationCategory => "Items.Weapons.Magic";

        public override void SetDefaults()
        {
            Item.width = 122;
            Item.height = 54;
            Item.damage = 80;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 8;
            Item.useTime = Item.useAnimation = 4;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 1.5f;
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.shootSpeed = 6f;
            Item.channel = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<OmegaHoldout>();
            Item.rare = ModContent.RarityType<Violet>();
        }
        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0 && player.ownedProjectileCounts[ModContent.ProjectileType<OmegaWingman>()] < 4;

        // Makes the rotation of the mouse around the player sync in multiplayer.
        public override void HoldItem(Player player)
        {
            CalamityPlayer calPlayer = player.Calamity();
            calPlayer.mouseWorldListener = true;
            calPlayer.mouseRotationListener = true;
            calPlayer.rightClickListener = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 4; i++)
            {
                int t = 0;
                switch (i)
                {
                    case 0: t = -2; break;
                    case 1: t = -1; break;
                    case 2: t = 1; break;
                    case 3: t = 2; break;
                }
                Projectile holdout2 = Projectile.NewProjectileDirect(source, player.MountedCenter, Vector2.Zero, ModContent.ProjectileType<OmegaWingman>(), damage, knockback, player.whoAmI, 0, 0, t);
                holdout2.velocity = (player.Calamity().mouseWorld - player.MountedCenter).SafeNormalize(Vector2.Zero);
            }

            Projectile holdout = Projectile.NewProjectileDirect(source, player.MountedCenter, Vector2.Zero, ModContent.ProjectileType<OmegaHoldout>(), damage, knockback, player.whoAmI, 0, 0, 0);
            holdout.velocity = (player.Calamity().mouseWorld - player.MountedCenter).SafeNormalize(Vector2.Zero);

            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<Omicron>()
                .AddIngredient<AuricBar>(5)
                .AddTile<CosmicAnvil>()
                .Register();
        }
    }
    public class OmegaHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<Omega>();
        public override Vector2 GunTipPosition => base.GunTipPosition - Vector2.UnitY.RotatedBy(Projectile.rotation) * 4f * Projectile.spriteDirection;
        public override float MaxOffsetLengthFromArm => 10f;
        public override float OffsetXUpwards => -5f;
        public override float BaseOffsetY => -5f;
        public override float OffsetYDownwards => 5f;

        public ref float ShootingTimer => ref Projectile.ai[0];
        public ref float PostFireCooldown => ref Projectile.ai[1];
        public ref float MaxFireRateShots => ref Projectile.ai[2];

        public float Windup { get; set; } = Omega.StarterWinup;
        public Color EffectsColor { get; set; } = Color.MediumVioletRed;

        public override void KillHoldoutLogic()
        {
            if (Owner.CantUseHoldout() && PostFireCooldown <= 0)
                Projectile.Kill();
        }

        public override void HoldoutAI()
        {
            if (PostFireCooldown > 0)
                PostFiringCooldown();

            if (Owner.Calamity().mouseRight && PostFireCooldown <= 0)
            {
                if (Owner.CheckMana(Owner.ActiveItem(), (int)(HeldItem.mana * Owner.manaCost) * 16, true, false))
                {
                    PostFireCooldown = 100;
                    Shoot(true);
                    ShootingTimer = 0;
                }
                else
                {
                    if (Projectile.soundDelay <= 0)
                    {
                        SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = -0.5f }, Projectile.Center);
                        Projectile.soundDelay = 50;
                        ShootingTimer = 0;
                    }
                }
            }
            else if (ShootingTimer >= Omega.FireRate)
            {
                if (Owner.CheckMana(Owner.ActiveItem(), -1, true, false) && PostFireCooldown <= 0)
                {
                    MaxFireRateShots++;

                    if (MaxFireRateShots == 5)
                    {
                        Windup = 60;
                        MaxFireRateShots = 1;
                    }

                    Shoot(false);

                    ShootingTimer = 0;

                    if (Windup > 10 && MaxFireRateShots > 0)
                        Windup -= 12;
                    else
                        Windup = 10;
                }
                else if (PostFireCooldown <= 0)
                {
                    SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = -0.5f }, Projectile.Center);
                    Projectile.Kill();
                }

            }

            ShootingTimer++;
        }

        public void Shoot(bool yBeam)
        {
            Vector2 shootDirection = Projectile.velocity.SafeNormalize(Vector2.Zero);
            Vector2 firingVelocity1 = (shootDirection * 8).RotatedBy(0.1f * Utils.GetLerpValue(10, 55, Windup, true));
            Vector2 firingVelocity2 = (shootDirection * 8).RotatedBy(-0.1f * Utils.GetLerpValue(10, 55, Windup, true));
            Vector2 firingVelocity3 = (shootDirection * 10);

            if (yBeam)
            {
                SoundStyle fire = new("CalamityMod/Sounds/Item/OmicronBeam");
                SoundEngine.PlaySound(fire with { Volume = 0.9f }, Projectile.Center);

                for (int k = 0; k < 6; k++)
                {
                    Particle pulse2 = new GlowSparkParticle(GunTipPosition, shootDirection * 28, false, 8, 0.087f, EffectsColor, new Vector2(2.3f, 0.9f), true);
                    GeneralParticleHandler.SpawnParticle(pulse2);
                }

                Owner.Calamity().GeneralScreenShakePower = 6.5f;
                Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), GunTipPosition, firingVelocity3, ModContent.ProjectileType<OmicronBeam>(), Projectile.damage * 32, Projectile.knockBack, Projectile.owner, 0, 0);

                for (int i = 0; i < 8; i++)
                {
                    SparkParticle spark2 = new SparkParticle(GunTipPosition + Main.rand.NextVector2Circular(10, 10), firingVelocity3 * Main.rand.NextFloat(0.7f, 1.3f), false, Main.rand.Next(20, 30), Main.rand.NextFloat(0.4f, 0.55f), EffectsColor);
                    GeneralParticleHandler.SpawnParticle(spark2);
                }
            }
            else
            {
                SoundStyle fire = new("CalamityMod/Sounds/Item/ArcNovaDiffuserBigShot");
                SoundEngine.PlaySound(fire with { Volume = 0.2f, Pitch = 0.9f }, Projectile.Center);

                for (int i = 0; i < 7; i++)
                {
                    firingVelocity3 = (shootDirection * 10).RotatedBy((0.035f * (i + 1)) * Utils.GetLerpValue(0, 55, Windup, true));
                    Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), GunTipPosition, firingVelocity3 * (1 - i * 0.1f), ModContent.ProjectileType<WingmanShot>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0, 2);
                }
                for (int i = 0; i < 7; i++)
                {
                    firingVelocity3 = (shootDirection * 10).RotatedBy((-0.035f * (i + 1)) * Utils.GetLerpValue(0, 55, Windup, true));
                    Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), GunTipPosition, firingVelocity3 * (1 - i * 0.1f), ModContent.ProjectileType<WingmanShot>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0, 2);
                }

                Particle pulse3 = new GlowSparkParticle(GunTipPosition, shootDirection * 18, false, 6, 0.057f, EffectsColor, new Vector2(1.7f, 0.8f), true);
                GeneralParticleHandler.SpawnParticle(pulse3);
            }

            // Inside here go all the things that dedicated servers shouldn't spend resources on.
            // Like visuals and sounds.
            if (Main.dedServ)
                return;

            for (int k = 0; k < 10; k++)
            {
                Vector2 shootVel = (shootDirection * 15).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.1f, 1.8f);

                Dust dust2 = Dust.NewDustPerfect(GunTipPosition, Main.rand.NextBool(4) ? 267 : 66, shootVel);
                dust2.scale = Main.rand.NextFloat(1.15f, 1.45f);
                dust2.noGravity = true;
                dust2.color = Main.rand.NextBool() ? Color.Lerp(EffectsColor, Color.White, 0.5f) : EffectsColor;
            }

            // By decreasing the offset length of the gun from the arms, we give an effect of recoil.
            if (yBeam)
                OffsetLengthFromArm -= 32f;
            else
                OffsetLengthFromArm -= 6f;
        }

        public void PostFiringCooldown()
        {
            Owner.channel = true;
            if (PostFireCooldown > 0 && Main.rand.NextBool())
            {
                Vector2 smokeVel = new Vector2(0, -8) * Main.rand.NextFloat(0.1f, 1.1f);
                Particle smoke = new HeavySmokeParticle(GunTipPosition, smokeVel, EffectsColor, Main.rand.Next(30, 50 + 1), Main.rand.NextFloat(0.1f, 0.4f), 0.5f, Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextBool(), required: true);
                GeneralParticleHandler.SpawnParticle(smoke);

                Dust dust = Dust.NewDustPerfect(GunTipPosition, DustID.SteampunkSteam, smokeVel.RotatedByRandom(0.1f), 80, default, Main.rand.NextFloat(0.2f, 0.8f));
                dust.noGravity = false;
                dust.color = EffectsColor;
            }
            ShootingTimer = 0;
            PostFireCooldown--;
        }

        public override void SendExtraAIHoldout(BinaryWriter writer) => writer.Write(Windup);

        public override void ReceiveExtraAIHoldout(BinaryReader reader) => Windup = reader.ReadSingle();
    }
    public class OmegaWingman : ModProjectile
    {
        public override LocalizedText DisplayName => CalamityUtils.GetItemName<Wingman>();
        //public override string Texture => "CalamityMod/Items/Weapons/Magic/Wingman";

        public Color StaticEffectsColor = Color.MediumVioletRed;
        private ref float ShootingTimer => ref Projectile.ai[0];
        private float FiringTime = 10;
        private float PostFireCooldown = 0;
        public bool MovingUp = true;
        public float xOffset = 1;
        public float yOffset = 0;
        public int time = 0;
        public int firingDelay = 15;
        public int launchDelay = 0;

        private ref float OffsetLength => ref Projectile.localAI[0];

        private Player Owner;

        private float MaxOffsetLength = 5f;

        public bool recharging = false;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 9;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 142;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            Projectile.hide = true;
        }

        public override void AI()
        {
            Owner ??= Main.player[Projectile.owner];

            if (time > 1 && Owner.ownedProjectileCounts[ModContent.ProjectileType<OmegaHoldout>()] < 1 && PostFireCooldown <= 0)
                Projectile.Kill();

            Lighting.AddLight(Projectile.Center, StaticEffectsColor.ToVector3() * 0.2f);

            //if (time == 0)
            //    MovingUp = (Projectile.ai[2] == 1 || Projectile.ai[2] == 2);

            firingDelay--;
            Item heldItem = Owner.ActiveItem();

            // Update damage based on curent magic damage stat (so Mana Sickness affects it)
            Projectile.damage = heldItem is null ? 0 : Owner.GetWeaponDamage(heldItem);

            // If there's no player, or the player is the server, or the owner's stunned, there'll be no holdout.
            if (PostFireCooldown == 0 && launchDelay == 0 && Owner.CantUseHoldout() || heldItem.type != ModContent.ItemType<Omega>())
            {
                if (PostFireCooldown <= 0)
                    Projectile.Kill();
            }

            if (PostFireCooldown > 0)
                PostFiringCooldown();

            bool isShot = Projectile.ai[2] == 1 || Projectile.ai[2] == -1;

            if (launchDelay > 0 || (PostFireCooldown <= 0 && (Owner.Calamity().mouseRight || (firingDelay <= 0 && Projectile.ai[2] == 1 || Projectile.ai[2] == -1 || Projectile.ai[2] == 2 || Projectile.ai[2] == -2))))
            {
                // If the player's pressing RMB, it'll shoot the grenade.
                if (launchDelay > 0 || Owner.Calamity().mouseRight)
                {
                    if (launchDelay < 50)
                        launchDelay++;

                    if (launchDelay >= 50 && (Owner.CheckMana(Owner.ActiveItem(), (int)(heldItem.mana * Owner.manaCost) * 2, true, false)))
                    {
                        //Shoot(true);
                        Shoot(isShot);
                        PostFireCooldown = 50;
                        ShootingTimer = 0;
                        launchDelay = 0;
                    }
                }
                else if (ShootingTimer >= FiringTime * (isShot ? 1 : 6))
                {
                    if (Owner.CheckMana(Owner.ActiveItem(), -1, false, false))
                    {
                        //Shoot(false);
                        Shoot(!isShot);
                        ShootingTimer = 0;
                    }
                    else if (PostFireCooldown <= 0)
                        Projectile.Kill();
                }
            }

            // The center of the player, taking into account if they have a mount or not.
            Vector2 ownerPosition = Owner.MountedCenter;

            // The vector between the player and the mouse.
            Vector2 ownerToMouse = Owner.Calamity().mouseWorld - ownerPosition;

            // Deals with the holdout's rotation and direction, the owner's arms, etc.
            ManagePlayerProjectileMembers(ownerToMouse);

            // When we change the distance of the gun from the arms for the recoil,
            // recover to the original position smoothly.
            if (OffsetLength != MaxOffsetLength)
                OffsetLength = MathHelper.Lerp(OffsetLength, MaxOffsetLength, 0.1f);

            ShootingTimer++;
            time++;
            Projectile.soundDelay--;

            Projectile.netSpam = 0;
            Projectile.netUpdate = true;
        }

        private void ManagePlayerProjectileMembers(Vector2 ownerToMouse)
        {
            Vector2 rotationVector = Projectile.rotation.ToRotationVector2();
            float velocityRotation = Projectile.velocity.ToRotation();
            int direction = MathF.Sign(ownerToMouse.X);
            Vector2 lengthOffset = rotationVector * OffsetLength;

            if (time % 30 == 0)
                MovingUp = !MovingUp;

            //xOffset = MathHelper.Lerp(xOffset, placementOffset.X, 0.01f);
            yOffset = MathHelper.Lerp(yOffset, 150 * (MovingUp ? Projectile.ai[2] : -Projectile.ai[2]), (0.085f - (FiringTime * 0.0012f)));

            Vector2 placementOffset = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * (yOffset);
            Vector2 location = Owner.MountedCenter + placementOffset;
            Projectile.Center = lengthOffset + location;
            Projectile.velocity = velocityRotation.AngleTowards(ownerToMouse.ToRotation(), 0.2f).ToRotationVector2();
            Projectile.rotation = (Owner.Calamity().mouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX).ToRotation();
            Projectile.timeLeft = 2;
            Projectile.spriteDirection = Projectile.direction = direction;
        }

        private void Shoot(bool isGrenade)
        {
            Vector2 shootDirection = (Owner.Calamity().mouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX);

            // The position of the tip of the gun.
            Vector2 tipPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(-0.05f * Projectile.direction) * 12f;

            Vector2 firingVelocity = shootDirection * 10;
            if (isGrenade)
            {
                SoundStyle fire = new("CalamityMod/Sounds/Item/DeadSunExplosion");
                SoundEngine.PlaySound(fire with { Volume = 0.2f, Pitch = -0.4f, PitchVariance = 0.2f }, Projectile.Center);
                if (Main.myPlayer == Projectile.owner)
                {
                    Projectile bomb = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), tipPosition, firingVelocity, ModContent.ProjectileType<WingmanGrenade>(), Projectile.damage * 14, Projectile.knockBack * 5, Projectile.owner, 0, 2);
                    bomb.timeLeft = 530;
                    Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), tipPosition, firingVelocity * 1.2f, ModContent.ProjectileType<WingmanGrenade>(), Projectile.damage * 14, Projectile.knockBack * 5, Projectile.owner, 0, 2);
                }
            }
            else
            {
                SoundStyle fire = new("CalamityMod/Sounds/Item/MagnaCannonShot");
                SoundEngine.PlaySound(fire with { Volume = 0.25f, Pitch = 1f, PitchVariance = 0.35f }, Projectile.Center);

                if (Main.myPlayer == Projectile.owner)
                {
                    Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), tipPosition, firingVelocity, ModContent.ProjectileType<WingmanShot>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0, 2);
                    Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), tipPosition, firingVelocity.RotatedBy(-0.05) * 0.85f, ModContent.ProjectileType<WingmanShot>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0, 2);
                    Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), tipPosition, firingVelocity.RotatedBy(0.05) * 0.85f, ModContent.ProjectileType<WingmanShot>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0, 2);
                }
            }

            // Inside here go all the things that dedicated servers shouldn't spend resources on.
            // Like visuals and sounds.
            if (Main.dedServ)
                return;

            for (int k = 0; k < 6; k++)
            {
                Vector2 shootVel = (shootDirection * 10).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.1f, 1.8f);

                Dust dust2 = Dust.NewDustPerfect(tipPosition, Main.rand.NextBool(4) ? 264 : 66, shootVel);
                dust2.scale = Main.rand.NextFloat(1.15f, 1.45f);
                dust2.noGravity = true;
                dust2.color = Main.rand.NextBool() ? Color.Lerp(StaticEffectsColor, Color.White, 0.5f) : StaticEffectsColor;
            }

            Particle pulse = new GlowSparkParticle((tipPosition - shootDirection * 14), shootDirection * 20, false, Main.rand.Next(7, 11 + 1), 0.035f, StaticEffectsColor, new Vector2(1.5f, 0.9f), true);
            GeneralParticleHandler.SpawnParticle(pulse);

            // By decreasing the offset length of the gun from the arms, we give an effect of recoil.
            if (isGrenade)
                OffsetLength -= 27f;
            else
                OffsetLength -= 5f;
        }

        private void PostFiringCooldown()
        {
            Owner.channel = true;
            Vector2 tipPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(-0.05f * Projectile.direction) * 12f;

            if (PostFireCooldown > 0 && Main.rand.NextBool())
            {
                Vector2 smokeVel = new Vector2(0, -8) * Main.rand.NextFloat(0.1f, 1.1f);
                Particle smoke = new HeavySmokeParticle(tipPosition, smokeVel, StaticEffectsColor, Main.rand.Next(30, 50 + 1), Main.rand.NextFloat(0.1f, 0.4f), 0.5f, Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextBool(), required: true);
                GeneralParticleHandler.SpawnParticle(smoke);

                Dust dust = Dust.NewDustPerfect(tipPosition, DustID.SteampunkSteam, smokeVel.RotatedByRandom(0.1f), 80, default, Main.rand.NextFloat(0.2f, 0.8f));
                dust.noGravity = false;
                dust.color = StaticEffectsColor;
            }

            ShootingTimer = 0;
            firingDelay = 15;
            PostFireCooldown--;
        }

        public override void OnSpawn(IEntitySource source) => OffsetLength = MaxOffsetLength;

        // Because we use the velocity as a direction, we don't need it to change its position.
        public override bool ShouldUpdatePosition() => false;

        public override bool? CanDamage() => false;

        public override bool PreDraw(ref Color lightColor)
        {
            if (time <= 0)
                return false;

            Texture2D texture;
            if (Projectile.ai[2] == 1 || Projectile.ai[2] == -1)
                texture = ModContent.Request<Texture2D>(Texture).Value;
            else
                texture = ModContent.Request<Texture2D>(Texture + "Alt").Value;


            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color drawColor = Projectile.GetAlpha(lightColor);
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            if (Projectile.spriteDirection == -1 ? MovingUp : !MovingUp)
                flipSprite |= SpriteEffects.FlipVertically;

            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], Color.Lerp(StaticEffectsColor, Color.White, 0.5f) * 0.2f, 1, texture);
            Main.EntitySpriteDraw(texture, drawPosition, null, drawColor, drawRotation, rotationPoint, Projectile.scale, flipSprite);

            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => behindNPCs.Add(index);

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.rotation);
            writer.Write(Projectile.spriteDirection);
            writer.Write(OffsetLength);
            writer.Write(time);
            writer.Write(firingDelay);
            writer.Write(launchDelay);
            writer.Write(MovingUp);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.rotation = reader.ReadSingle();
            Projectile.spriteDirection = reader.ReadInt32();
            OffsetLength = reader.ReadSingle();
            time = reader.ReadInt32();
            firingDelay = reader.ReadInt32();
            launchDelay = reader.ReadInt32();
            MovingUp = reader.ReadBoolean();
        }
    }
}
