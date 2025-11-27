using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Weapons.Summon;
using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Weapons.Summon.Minions
{
    public class PlanterrorStaff : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";
        public override void SetStaticDefaults()
        {
            //if (ModLoader.TryGetMod("Redemption", out var redemption))
            //    redemption.Call("addElementItem", 2, Type);
        }
        public override void SetDefaults()
        {
            Item.width = Item.height = 42;
            Item.value = CalamityGlobalItem.RarityRedBuyPrice;
            Item.rare = ItemRarityID.Red;

            Item.useTime = Item.useAnimation = 19;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.autoReuse = true;

            Item.damage = 100;
            Item.DamageType = DamageClass.Summon;
            Item.knockBack = 2f;
            Item.mana = 12;

            Item.shoot = ModContent.ProjectileType<PlanterrorStaffSummon>();
        }

        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse != 2)
            {
                int index = Projectile.NewProjectile(source, Main.MouseWorld, Main.rand.NextVector2Circular(2f, 2f), type, damage, knockback, player.whoAmI, 0.0f, 0.0f, 0.0f);
                if (Main.projectile.IndexInRange(index))
                    Main.projectile[index].originalDamage = Item.damage;
            }

            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<PlantationStaff>()
                .AddIngredient(ItemID.FlintlockPistol)
                .AddIngredient(ItemID.LunarBar, 5)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    public class PlanterrorStaffSummon : ModProjectile, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Projectiles.Summon.Minion";
        public Player Owner => Main.player[Projectile.owner];
        public ClamityPlayer ModdedOwner => Owner.Clamity();
        public NPC Target => Owner.Center.MinionHoming(PlantationStaff.EnemyDistanceDetection, Owner);
        public enum AIState
        {
            Idle,
            Shooting,
            Ramming
        }
        public AIState State
        {
            get => (AIState)Projectile.ai[0];
            set => Projectile.ai[0] = (int)value;
        }
        public ref float AITimer => ref Projectile.ai[1];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 8;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 4f;
            Projectile.localNPCHitCooldown = 10;
            Projectile.width = Projectile.height = 48;
            Projectile.penetrate = -1;

            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.netImportant = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            int tentacleAmount = 6;
            for (int tentacleIndex = 0; tentacleIndex < tentacleAmount; tentacleIndex++)
            {
                Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<PlanterrorStaffTentacle>(), Projectile.damage, Projectile.knockBack, Owner.whoAmI, tentacleIndex, Projectile.whoAmI);
            }
        }
        public override void AI()
        {
            CheckMinionExistence();
            DoAnimation();

            switch (State)
            {
                case AIState.Idle:
                    IdleState();
                    break;
                case AIState.Shooting:
                    ShootingState();
                    break;
                case AIState.Ramming:
                    RammingState();
                    break;
            }
        }
        private void IdleState()
        {
            // The projectile points towards its direction.
            Projectile.rotation = Projectile.velocity.ToRotation();

            // The minion will hover around the owner at a certain distance.
            if (!Projectile.WithinRange(Owner.Center, 160f))
            {
                Projectile.velocity = (Projectile.velocity + Projectile.SafeDirectionTo(Owner.Center)) * 0.9f;
                Projectile.netUpdate = true;
            }

            // If the owner starts to get far, the minion will follow straight to the owner.
            if (!Projectile.WithinRange(Owner.Center, 320f))
            {
                Projectile.velocity = Projectile.SafeDirectionTo(Owner.Center) * MathF.Max(5f, Owner.velocity.Length());
                Projectile.netUpdate = true;
            }

            // If the owner's far enough, teleport the minion on the owner.
            if (!Projectile.WithinRange(Owner.Center, PlantationStaff.EnemyDistanceDetection))
            {
                Projectile.Center = Owner.Center;
                Projectile.netUpdate = true;
            }

            if (Target is not null)
                SwitchState(AIState.Shooting);
        }
        private void ShootingState()
        {
            if (Target is not null)
            {
                ShootingMovement();
                AITimer++;
                if (AITimer >= 300)
                    SwitchState(AIState.Ramming);
            }
            else
                SwitchState(AIState.Idle);
        }
        private void RammingState()
        {
            if (Target is not null)
            {
                AITimer++;
                if (AITimer <= PlantationStaff.TimeBeforeRamming / 3 * 2)
                {
                    // The minion slows down to give a flavor effect of preparation.
                    Projectile.velocity *= 0.985f;
                }
                else
                {
                    // If the minion's not withing a certain range of the target, go towards it.
                    // When it reaches that range, the velocity will not change anymore until they hit
                    // the outside range, repeating this process and giving the effect of ramming.
                    if (!Projectile.WithinRange(Target.Center, 400f))
                        RamMovement();

                    // But, if the minion was already inside the target when it appeared,
                    // meaning it doesn't have the velocity to go to the outside range.
                    // We'll make it ram once to be start the effect of ramming,
                    // with an amount of grace so the minion doesn't change it's velocity constantly.
                    else if (Projectile.velocity.Length() < PlantationStaff.ChargingSpeed - 5f)
                        RamMovement();

                    if (AITimer >= PlantationStaff.RamTime / 3 + PlantationStaff.TimeBeforeRamming)
                        SwitchState(AIState.Shooting);
                }
            }
            else
                SwitchState(AIState.Idle);
        }
        private void ShootingMovement()
        {
            Vector2 targetDirection = Projectile.SafeDirectionTo(Target.Center);

            // The minion will look towards the target.
            Projectile.rotation = targetDirection.ToRotation();

            // If the minion is not within a certain range of the target, go towards it.
            if (!Projectile.WithinRange(Target.Center, 480f))
            {
                Projectile.velocity = (Projectile.velocity * 30f + targetDirection * PlantationStaff.ChargingSpeed) / 31f;
                Projectile.netUpdate = true;
            }

            // But if the minion gets to close, separate it from the target.
            if (Projectile.WithinRange(Target.Center, 400f))
            {
                Projectile.velocity = (Projectile.velocity * 30f + -targetDirection * PlantationStaff.ChargingSpeed) / 31f;
                Projectile.netUpdate = true;
            }
        }

        private void RamMovement()
        {
            Projectile.velocity = Projectile.SafeDirectionTo(Target.Center) * PlantationStaff.ChargingSpeed * 3 / 2;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.netUpdate = true;
        }

        private void SwitchState(AIState state)
        {
            State = state;
            AITimer = 0f;

            if (state == AIState.Ramming)
            {
                int sporeCloudAmount = 12;
                for (int sporeCloudIndex = 0; sporeCloudIndex < sporeCloudAmount; sporeCloudIndex++)
                {
                    float angle = MathHelper.TwoPi / sporeCloudAmount * sporeCloudIndex;
                    Vector2 velocity = angle.ToRotationVector2() * PlantationStaff.SporeStartVelocity;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<PlantationStaffSporeCloud>(), Projectile.damage, 10f, Owner.whoAmI, Main.rand.Next(3));
                }

                SoundEngine.PlaySound(SoundID.Roar with { Volume = .3f, Pitch = 1f, PitchVariance = .1f }, Projectile.Center);
            }

            Projectile.netUpdate = true;
        }

        private void CheckMinionExistence()
        {
            Owner.AddBuff(ModContent.BuffType<PlanterrorStaffBuff>(), 2);
            if (Type != ModContent.ProjectileType<PlanterrorStaffSummon>())
                return;

            if (Owner.dead)
                ModdedOwner.guntera = false;
            if (ModdedOwner.guntera)
                Projectile.timeLeft = 2;
        }

        private void DoAnimation()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 8)
            {
                Projectile.frameCounter = 0;

                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Type];

                if (State == AIState.Ramming && Projectile.frame < 4)
                    Projectile.frame = 4;

                if (State != AIState.Ramming && Projectile.frame > 3)
                    Projectile.frame = 0;
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (State == AIState.Ramming)
                modifiers.SourceDamage *= 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;

            if (State == AIState.Ramming && CalamityConfig.Instance.Afterimages)
                CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor);

            Main.EntitySpriteDraw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }
    public class PlanterrorStaffTentacle : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public Player Owner => Main.player[Projectile.owner];
        public ClamityPlayer ModdedOwner => Owner.Clamity();
        public NPC Target => Projectile.Center.MinionHoming(PlantationStaff.EnemyDistanceDetection, Owner);
        public Projectile MainMinion => Main.projectile[(int)MainMinionIndex];

        public ref float TentacleIndex => ref Projectile.ai[0];
        public ref float MainMinionIndex => ref Projectile.ai[1];
        public ref float AITimer => ref Projectile.localAI[0];

        public Vector2 DesiredLocation;
        public float OnShootRotation;
        public int MaxShootTimer;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionShot[Type] = true;
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Summon;
            Projectile.localNPCHitCooldown = -1;
            Projectile.width = Projectile.height = 24;
            Projectile.penetrate = -1;

            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.netImportant = true;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(AITimer);
            writer.Write(MaxShootTimer);
            writer.Write(OnShootRotation);
            writer.WritePackedVector2(DesiredLocation);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            AITimer = reader.ReadSingle();
            MaxShootTimer = reader.ReadInt32();
            OnShootRotation = reader.ReadSingle();
            DesiredLocation = reader.ReadPackedVector2();
        }

        public override void AI()
        {
            CheckMinionExistence();

            AttackState();
        }
        private void AttackState()
        {
            //AITimer = MathHelper.Clamp(--AITimer, 0, int.MaxValue);
            if (AITimer > 0)
                AITimer--;


            Projectile.Center = Vector2.Lerp(Projectile.Center, MainMinion.Center + DesiredLocation, .4f) - Vector2.UnitX.RotatedBy(OnShootRotation) * MathF.Pow(MathF.Sin(AITimer * MathF.PI / MaxShootTimer), 2);
            if (Target is null || !Target.active)
                Projectile.rotation = MainMinion.rotation;
            else
            {
                Projectile.rotation = (Target.Center - MainMinion.Center).ToRotation();

                if (AITimer == 0)
                {
                    Item bullet = null;
                    for (int i = 54; i < 58; i++)
                    {
                        if (Owner.inventory[i].ammo != AmmoID.Bullet)
                            continue;

                        bullet = Owner.inventory[i];
                        break;
                    }

                    OnShootRotation = Projectile.rotation;
                    SoundEngine.PlaySound(SoundID.Item11, Projectile.Center);
                    int index = Projectile.NewProjectile(Projectile.GetSource_FromAI(),
                                                         Projectile.Center,
                                                         Projectile.DirectionTo(Target.Center) * (bullet == null ? 7f : 5f + bullet.shootSpeed),
                                                         bullet == null ? ProjectileID.Bullet : bullet.shoot,
                                                         Projectile.damage + (bullet == null ? 0 : bullet.damage),
                                                         Projectile.knockBack,
                                                         Projectile.owner);
                    Main.projectile[index].DamageType = DamageClass.Summon;
                    Main.projectile[index].tileCollide = false;
                    AITimer = MaxShootTimer = 30 + Main.rand.Next(30);

                }
            }
        }
        private void CheckMinionExistence()
        {
            if (Projectile.ai[1] < 0 || Projectile.ai[1] >= Main.maxProjectiles)
            {
                Projectile.Kill();
                return;
            }

            // If something has gone wrong with either the tentacle or the host plant, destroy the projectile.
            if (Projectile.type != ModContent.ProjectileType<PlanterrorStaffTentacle>() || !MainMinion.active || MainMinion.type != ModContent.ProjectileType<PlanterrorStaffSummon>())
            {
                Projectile.Kill();
                return;
            }

            if (ModdedOwner.guntera)
                Projectile.timeLeft = 2;
        }
        public override void OnSpawn(IEntitySource source)
        {
            DesiredLocation = (MathHelper.TwoPi / 6f * TentacleIndex).ToRotationVector2().RotatedByRandom(MathHelper.PiOver4 / 1.5f) * 100f;
            Projectile.netUpdate = true;
            AITimer = MaxShootTimer = Main.rand.Next(0, 60);
        }

        public override bool? CanDamage() => false;

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.JunglePlants);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (MainMinionIndex < 0 || MainMinionIndex >= Main.maxProjectiles)
                return false;

            // If something has gone wrong with either the tentacle or the host plant, return.
            if (Type != ModContent.ProjectileType<PlanterrorStaffTentacle>() || !MainMinion.active || MainMinion.type != ModContent.ProjectileType<PlanterrorStaffSummon>())
                return false;

            Vector2 source = MainMinion.Center;
            Texture2D chain = ModContent.Request<Texture2D>(Texture + "_Chain").Value;
            Vector2 goal = Projectile.Center;
            Rectangle? sourceRectangle = null;
            float textureHeight = chain.Height;
            Vector2 drawVector = source - goal;
            float rotation = drawVector.ToRotation() - MathHelper.PiOver2;
            bool shouldDraw = true;
            if (float.IsNaN(goal.X) && float.IsNaN(goal.Y))
            {
                shouldDraw = false;
            }
            if (float.IsNaN(drawVector.X) && float.IsNaN(drawVector.Y))
            {
                shouldDraw = false;
            }
            while (shouldDraw)
            {
                if (drawVector.Length() < textureHeight + 1f)
                {
                    shouldDraw = false;
                }
                else
                {
                    Vector2 value2 = drawVector;
                    value2.Normalize();
                    goal += value2 * textureHeight;
                    drawVector = source - goal;
                    Color color = Lighting.GetColor((int)goal.X / 16, (int)(goal.Y / 16f));
                    Main.EntitySpriteDraw(chain, goal - Main.screenPosition, sourceRectangle, color, rotation, chain.Size() / 2f, 1f, SpriteEffects.None, 0);
                }
            }

            return true;
        }

        // It draws the host plant in here in order to have it draw over the tentacles.
        public override void PostDraw(Color lightColor)
        {
            // Only 1 tentacle needs to draw this, the last one spawned because it's latest in the projectile array.
            if (TentacleIndex < 5)
                return;

            if (MainMinionIndex < 0 || MainMinionIndex >= Main.maxProjectiles)
                return;

            // If something has gone wrong with either the tentacle or the host plant, return.
            if (Projectile.type != ModContent.ProjectileType<PlanterrorStaffTentacle>() || !MainMinion.active || MainMinion.type != ModContent.ProjectileType<PlanterrorStaffSummon>())
                return;

            Texture2D texture = TextureAssets.Projectile[MainMinion.type].Value;
            int height = texture.Height / Main.projFrames[MainMinion.type];
            int frameHeight = height * MainMinion.frame;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (MainMinion.spriteDirection == -1)
                spriteEffects = SpriteEffects.FlipHorizontally;
            Color color = Lighting.GetColor((int)MainMinion.Center.X / 16, (int)(MainMinion.Center.Y / 16f));

            Main.EntitySpriteDraw(texture, MainMinion.Center - Main.screenPosition + new Vector2(0f, MainMinion.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(new Rectangle(0, frameHeight, texture.Width, height)), color, MainMinion.rotation, new Vector2(texture.Width / 2f, height / 2f), MainMinion.scale, spriteEffects, 0);
        }

    }
    public class PlanterrorStaffBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            ClamityPlayer clamityPlayer = player.Clamity();
            if (player.ownedProjectileCounts[ModContent.ProjectileType<PlanterrorStaffSummon>()] > 0)
                clamityPlayer.guntera = true;
            if (!clamityPlayer.guntera)
            {
                player.DelBuff(buffIndex);
                --buffIndex;
            }
            else
                player.buffTime[buffIndex] = 18000;
        }
    }
}