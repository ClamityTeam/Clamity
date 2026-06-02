using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Pyrogen.Drop.Weapons
{
    public class HellsBells : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            if (!ModLoader.TryGetMod("Redemption", out var redemption))
                return;
            redemption.Call("addElementItem", 2, Type);
        }
        public override void SetDefaults()
        {
            Item.width = Item.height = 42;
            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
            Item.rare = ItemRarityID.Pink;

            Item.useTime = Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = new SoundStyle?(SoundID.Item44);
            Item.noMelee = true;
            Item.autoReuse = true;

            Item.damage = 39;
            Item.DamageType = DamageClass.Summon;
            Item.knockBack = 2f;
            Item.mana = 10;

            Item.shoot = ModContent.ProjectileType<HellsBellsSummon>();
        }

        public override bool Shoot(
          Player player,
          EntitySource_ItemUse_WithAmmo source,
          Vector2 position,
          Vector2 velocity,
          int type,
          int damage,
          float knockback)
        {
            if (player.altFunctionUse != 2)
            {
                int index = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0.0f, 0.0f, 0.0f);
                if (Main.projectile.IndexInRange(index))
                    Main.projectile[index].originalDamage = Item.damage;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj != null)
                    {
                        if (proj.active && proj.type == ModContent.ProjectileType<HellsBellsSummon>() && proj.owner == player.whoAmI && proj.Center.WithinRange(player.Center + (Main.MouseWorld - player.Center).SafeNormalize(Vector2.Zero) * 8, 32))
                        {
                            proj.velocity = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.Zero) * 10;
                        }
                    }
                }
            }
            return false;
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (player.altFunctionUse != 2)
            {
                position = Main.MouseWorld;
                velocity = Vector2.Zero;
            }
        }
    }
    public class HellsBellsSummon : ModProjectile, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Projectiles.Summon.Minion";
        public Player Owner => Main.player[Projectile.owner];

        public ClamityPlayer moddedOwner => Owner.Clamity();

        public ref float CheckForSpawning => ref Projectile.localAI[0];

        private int cooldown = 0;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            if (!ModLoader.TryGetMod("Redemption", out var redemption))
                return;
            redemption.Call("addElementItem", 2, Type);
        }
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 30;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.minion = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 35;
        }
        public override bool? CanHitNPC(NPC target)
        {
            return false;
        }
        public static readonly SoundStyle BellSound = new SoundStyle("CalamityMod/Sounds/Item/CalamityBell");
        public override void AI()
        {
            Owner.AddBuff(ModContent.BuffType<HellsBellsBuff>(), 3600, true, false);
            if (Projectile.type == ModContent.ProjectileType<HellsBellsSummon>())
            {
                if (Owner.dead)
                    moddedOwner.hellsBell = false;
                if (moddedOwner.hellsBell)
                    Projectile.timeLeft = 2;
            }
            if (CheckForSpawning == 0)
            {
                ++CheckForSpawning;
            }
            for (int i = 0; i < Main.npc.Length; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active) continue;
                bool colliding = false;
                if (Projectile.ModProjectile.Colliding(Projectile.Hitbox, npc.Hitbox) == null && Projectile.Colliding(Projectile.Hitbox, npc.Hitbox))
                    colliding = true;
                if (Projectile.ModProjectile.Colliding(Projectile.Hitbox, npc.Hitbox) == true)
                    colliding = true;

                if (colliding && cooldown == 0)
                {
                    SoundEngine.PlaySound(BellSound, Projectile.Center);
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<HellsBellsRing>(), Projectile.damage, 1f, Projectile.owner);
                    cooldown = 30;
                }

            }
            for (int i = 0; i < Main.projectile.Length; i++)
            {
                if (Main.projectile[i].type != Projectile.type || Projectile.whoAmI == i) continue;
                bool colliding = false;
                if (Projectile.ModProjectile.Colliding(Projectile.Hitbox, Main.projectile[i].Hitbox) == null && Projectile.Colliding(Projectile.Hitbox, Main.projectile[i].Hitbox))
                    colliding = true;
                if (Projectile.ModProjectile.Colliding(Projectile.Hitbox, Main.projectile[i].Hitbox) == true)
                    colliding = true;
                if (colliding) Projectile.velocity -= (Main.projectile[i].Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 0.5f;
            }
            Vector2 value = Projectile.SafeDirectionTo(Owner.Center + (Main.MouseWorld - Owner.Center).SafeNormalize(Vector2.Zero) * 8) * (Projectile.velocity.Length() + 3.5f);
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, value, 0.05f);
            Projectile.rotation = Projectile.velocity.X * 0.1f + MathF.Sin(cooldown) * (cooldown / 30f);
            if (cooldown > 0)
                cooldown--;
        }
        public override bool OnTileCollide(Vector2 oldVelocity) => false;
    }
    public class HellsBellsRing : ModProjectile, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetStaticDefaults()
        {
            if (!ModLoader.TryGetMod("Redemption", out var redemption))
                return;
            redemption.Call("addElementProj", 2, Type);
        }
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 75;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 10;
            Projectile.alpha = 255;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 3;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }
        public bool Spawned
        {
            get => Projectile.ai[0] == 1f;
            set => Projectile.ai[0] = value ? 1f : 0f;
        }
        public override void AI()
        {
            if (!Spawned)
            {
                Spawned = true;

                GeneralParticleHandler.SpawnParticle(new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.Orange, new Vector2(0.5f, 0.5f), Main.rand.NextFloat(12f, 25f), 0.2f, 1.4f, 20));
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 60);
            //Main.projectile[Projectile.whoAmI].originalDamage *= 0.85f;
        }
    }
    public class HellsBellsBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            ClamityPlayer clamityPlayer = player.Clamity();
            if (player.ownedProjectileCounts[ModContent.ProjectileType<HellsBellsSummon>()] > 0)
                clamityPlayer.hellsBell = true;
            if (!clamityPlayer.hellsBell)
            {
                player.DelBuff(buffIndex);
                --buffIndex;
            }
            else
                player.buffTime[buffIndex] = 18000;
        }
    }
}
