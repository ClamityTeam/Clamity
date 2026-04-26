using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Clamity.Content.Biomes.FrozenHell.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Weapons.Magic
{
    [LegacyName("Everest")]
    public class PetalOfEverest : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";
        public static int AftershotCooldownFrames = 17;
        public static int FullChargeFrames = 88;
        public override void SetDefaults()
        {
            Item.width = 112;
            Item.height = 112;
            Item.DamageType = DamageClass.Magic;
            Item.damage = 60;
            Item.knockBack = 3.5f;
            Item.mana = 5;
            Item.useAnimation = Item.useTime = AftershotCooldownFrames;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.channel = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Shoot;

            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.rare = ModContent.RarityType<BurnishedAuric>();

            Item.shoot = ModContent.ProjectileType<PetalOfEverestHoldout>();
            Item.shootSpeed = 15f;
        }

        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

        public override void HoldItem(Player player) => player.Calamity().mouseRotationListener = true;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 spawnPosition = player.RotatedRelativePoint(player.MountedCenter, true);
            // 14NOV2024: Ozzatron: clamped mouse position unnecessary, only used for direction
            Projectile.NewProjectile(source, spawnPosition, player.Calamity().mouseWorld - spawnPosition, type, damage, knockback, player.whoAmI);
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<Mistlestorm>()
                .AddIngredient<ThePrince>()
                .AddIngredient<EndobsidianBar>(8)
                .AddTile<CosmicAnvil>()
                .Register();
        }
    }
    //AI values
    //ai[1] - 1 to now allow charge again same projectile
    //ai[2] - 1 is already releaces projectile
    public class PetalOfEverestHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<PetalOfEverest>();
        public override float MaxOffsetLengthFromArm => 60f;
        public override float BaseOffsetY => 0f;
        public override float RecoilResolveSpeed => 0.4f;
        public override string Texture => (GetType().Namespace + "." + Name).Replace('.', '/');
        public override Vector2 GunTipPosition => base.GunTipPosition - Vector2.UnitX.RotatedBy(Projectile.rotation) * 26;
        private ref float CurrentCharging => ref Projectile.ai[0];
        private bool FullyCharged => CurrentCharging >= PetalOfEverest.FullChargeFrames;
        public SlotId EverestChargeSlot;
        public static float BulletSpeed = 15f;
        public int time = 0;
        public int petalAttackCounter = 0;
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.projFrames[Type] = 7;
        }
        public override void KillHoldoutLogic()
        {
            if (Owner.CantUseHoldout(false) || HeldItem.type != Owner.HeldItem.type)
                Projectile.Kill();
        }

        public override void HoldoutAI()
        {
            if (SoundEngine.TryGetActiveSound(EverestChargeSlot, out var ChargeSound) && ChargeSound.IsPlaying)
                ChargeSound.Position = Projectile.Center;

            // Fire if the owner stops channeling or otherwise cannot use the weapon.
            if (Owner.CantUseHoldout()) //<< stoping channeling
            {
                KeepRefreshingLifetime = false;

                if (Projectile.ai[1] != 1f)
                {
                    Projectile.timeLeft = FullyCharged ? HeliumFlash.AftershotCooldownFrames * 3 : (int)(HeliumFlash.AftershotCooldownFrames * 1.5f);

                    ChargeSound?.Stop();

                    Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * BulletSpeed;
                    if (FullyCharged)
                    {
                        Projectile.ai[2] = 1f;
                        OffsetLengthFromArm -= 25;
                        //SoundEngine.PlaySound(HeliumFlash.ChargeFire, Projectile.Center);

                        //if (Main.myPlayer == Projectile.owner)
                        //    Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity, ModContent.ProjectileType<PetalOfEverestFlower>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0);


                        //TODO - Bloom flower shoot effect
                        /*Particle pulse = new CustomPulse(GunTipPosition, Vector2.Zero, Color.OrangeRed, "CalamityMod/Particles/ShatteredExplosion", Vector2.One, Main.rand.NextFloat(-10f, 10f), 0f, 0.05f, 14);
                        GeneralParticleHandler.SpawnParticle(pulse);
                        Particle pulse2 = new CustomPulse(GunTipPosition, Vector2.Zero, Color.Red, "CalamityMod/Particles/ShatteredExplosion", Vector2.One, Main.rand.NextFloat(-10f, 10f), 0f, 0.08f, 14);
                        GeneralParticleHandler.SpawnParticle(pulse2);

                        for (int i = 0; i < 17; i++)
                        {
                            Dust chargefull = Dust.NewDustPerfect(GunTipPosition, DustID.FireworksRGB);
                            chargefull.velocity = Projectile.velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(5, 25);
                            chargefull.scale = Main.rand.NextFloat(0.65f, 0.95f);
                            chargefull.noGravity = true;
                            chargefull.color = Color.Lerp(Color.White, Main.rand.NextBool(4) ? Color.Orange : Color.OrangeRed, 0.7f);
                        }

                        Vector2 shootDirection = Projectile.velocity.SafeNormalize(Vector2.Zero);
                        Particle pulse3 = new GlowSparkParticle(GunTipPosition, shootDirection * 18, false, 6, 0.057f, Color.OrangeRed, new Vector2(1.7f, 0.8f), true);
                        GeneralParticleHandler.SpawnParticle(pulse3);
                        for (int i = 0; i <= 18; i++)
                        {
                            Vector2 sparkVelocity = shootVelocity / 2f;

                            float sparkScale1 = Main.rand.NextFloat(0.3f, 0.8f);
                            Vector2 sparkvelocity1 = sparkVelocity.RotatedByRandom(0.45f) * Main.rand.NextFloat(0.5f, 0.7f);
                            Particle spark1 = new LineParticle(GunTipPosition, sparkvelocity1, false, 40, sparkScale1, Main.rand.NextBool() ? Color.Red : Color.DarkRed);
                            GeneralParticleHandler.SpawnParticle(spark1);

                            float sparkScale2 = Main.rand.NextFloat(0.4f, 1f);
                            Vector2 sparkvelocity2 = sparkVelocity.RotatedByRandom(0.2f) * Main.rand.NextFloat(0.9f, 1.6f);
                            Particle spark2 = new LineParticle(GunTipPosition, sparkvelocity2, false, 40, sparkScale2, Main.rand.NextBool() ? Color.DarkOrange : Color.OrangeRed);
                            GeneralParticleHandler.SpawnParticle(spark2);
                        }*/
                    }
                    else //effect on stop channeling before reaches full charge
                    {
                        /*SoundStyle fire = new("CalamityMod/Sounds/Item/HeliumFlashDudFire");
                        SoundEngine.PlaySound(fire with { Volume = 0.4f, Pitch = 0.3f }, Projectile.Center);
                        for (int i = 0; i < 12; i++)
                        {
                            Dust dust2 = Dust.NewDustPerfect(GunTipPosition, DustID.FireworksRGB, Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(1f, 3.5f));
                            dust2.scale = Main.rand.NextFloat(0.55f, 0.9f);
                            dust2.noGravity = false;
                            dust2.color = Color.Lerp(Color.White, Main.rand.NextBool(4) ? Color.Orange : Color.OrangeRed, 0.7f);
                        }*/
                    }
                    Projectile.ai[1] = 1f;
                }
            }
            else
            {
                if (Projectile.ai[1] != 1f)
                    CurrentCharging++;

                float bloomMaxCharge = PetalOfEverest.FullChargeFrames;
                float bloomRatio = MathHelper.Clamp(CurrentCharging, 0, bloomMaxCharge) / bloomMaxCharge;

                Projectile.frame = (int)MathHelper.Lerp(0, Main.projFrames[Type] - 1, bloomRatio);

                if (++petalAttackCounter >= (int)MathHelper.Lerp(20, 6, bloomRatio))
                {
                    petalAttackCounter = 0;
                    for (int i = -2; i < 3; i++)
                    {
                        if (i == 0) continue;
                        Vector2 petalVel1 = Projectile.velocity
                            .SafeNormalize(Vector2.UnitX)
                            .RotatedBy(MathHelper.ToRadians(MathHelper.Lerp(0, 60f * i + Main.rand.NextFloat(-20, 20), bloomRatio))) * 14;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, petalVel1, ModContent.ProjectileType<PetalOfEverestPetal>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                    }
                }

                // Sounds
                // TODO - Hey you need shoot sounds


                // Charge-up visuals
                if (CurrentCharging >= 10)
                {
                    if (!FullyCharged)
                    {
                        Particle streak = new ManaDrainStreak(Owner, Main.rand.NextFloat(0.06f + (CurrentCharging / 180), 0.08f + (CurrentCharging / 180)), Main.rand.NextVector2CircularEdge(2.5f, 2.5f) * Main.rand.NextFloat(0.3f * CurrentCharging, 0.3f * CurrentCharging), 0f, Color.Red, Color.Orange, 7, GunTipPosition);
                        GeneralParticleHandler.SpawnParticle(streak);
                    }
                }

                // Full charge effects
                if (CurrentCharging == PetalOfEverest.FullChargeFrames)
                {
                    //Shoot big flower

                    /*SoundStyle fire = new("CalamityMod/Sounds/Item/HeliumFlashReady");
                    SoundEngine.PlaySound(fire with { Volume = 1f, Pitch = 0f }, Projectile.Center);
                    for (int i = 0; i < 18; i++)
                    {
                        Vector2 dustVel = Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(1, 5);
                        Dust dust2 = Dust.NewDustPerfect(GunTipPosition + dustVel, DustID.FireworksRGB, dustVel * 0.7f);
                        dust2.scale = Main.rand.NextFloat(0.45f, 0.9f);
                        dust2.noGravity = true;
                        dust2.color = Color.Lerp(Color.White, Main.rand.NextBool(4) ? Color.Orange : Color.OrangeRed, 0.7f);
                    }*/
                }
            }
            if (Projectile.ai[1] == 1f)
            {
                CurrentCharging *= FullyCharged ? 0 : 0.9f;
            }

            //Lighting.AddLight(GunTipPosition, Color.OrangeRed.ToVector3() * 1.5f * Utils.GetLerpValue(0, HeliumFlash.FullChargeFrames, CurrentChargingFrames, true));

            time++;
        }

        public override void OnKill(int timeLeft)
        {
            if (SoundEngine.TryGetActiveSound(EverestChargeSlot, out var ChargeSound))
                ChargeSound?.Stop();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (time < 2)
                return false;

            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f) - (Owner.gravDir == -1 ? MathHelper.PiOver2 * Owner.direction : 0f);
            SpriteEffects flipSprite = (Projectile.spriteDirection * Owner.gravDir == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            int frameHeight = texture.Height / Main.projFrames[Type];
            int frameY = frameHeight * Projectile.frame;
            float rotation = Projectile.rotation;
            Rectangle rect = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 rotationPoint = rect.Size() * 0.5f;
            //Main.NewText(rect);

            /*if (!Owner.CantUseHoldout() && !FullyCharged)
            {
                float rumble = MathHelper.Clamp(CurrentCharging, 0f, HeliumFlash.FullChargeFrames);
                drawPosition += Main.rand.NextVector2Circular(rumble / 25f, rumble / 25f);
            }*/

            // Main staff
            Main.EntitySpriteDraw(texture, drawPosition, rect, Projectile.GetAlpha(lightColor), drawRotation + (MathHelper.ToRadians(45f * (Projectile.spriteDirection))), rotationPoint, Projectile.scale * Owner.gravDir, flipSprite);
            return false;
        }
    }
    public class PetalOfEverestPetal : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 15;
            Projectile.aiStyle = -1;
            AIType = -1;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        public static float MaxRotatingTime = 60;
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.ai[1] = Projectile.velocity.Length();
        }
        public override void AI()
        {
            Projectile.ai[0]++;
            Projectile.velocity = Vector2.Lerp(
                Projectile.velocity,
                (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX) * Projectile.ai[1] / 2,
                Projectile.ai[0] > MaxRotatingTime ? 0 : MathHelper.Clamp(Projectile.ai[0], 1, MaxRotatingTime) / MaxRotatingTime);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (CalamityUtils.AngleBetween(Projectile.velocity, Main.MouseWorld - Projectile.Center) < MathHelper.ToRadians(1))
                Projectile.ai[0] = MaxRotatingTime;
            if (Projectile.velocity.Length() < Projectile.ai[1])
                Projectile.velocity *= Projectile.ai[1] / Projectile.velocity.Length();

            //Lines of interacting with PetalOfEverestFlower
        }
    }
    /*public class PetalOfEverestFlower : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 15;
            Projectile.aiStyle = -1;
            AIType = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 30;
        }
        public static float MaxRotatingTime = 10;
        public override void AI()
        {

        }
    }*/
}
