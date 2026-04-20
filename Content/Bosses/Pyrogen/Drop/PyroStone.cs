using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items;
using CalamityMod.NPCs;
using CalamityMod.NPCs.AcidRain;
using Clamity.Content.Bosses.Pyrogen.NPCs;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;


namespace Clamity.Content.Bosses.Pyrogen.Drop
{
    public class PyroStone : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(4, 4));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
            Item.rare = ItemRarityID.Pink;
            Item.accessory = true;
        }
        public override void UpdateAccessory(Player player, bool hideVisual) => player.Clamity().pyroStone = true;
        public override void UpdateVanity(Player player) => player.Clamity().pyroStoneVanity = true;
    }
    public class PyroShieldAccessory : ModProjectile, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Projectiles.Classless";

        public Player Owner => Main.player[Projectile.owner];

        public override string Texture => ModContent.GetInstance<PyrogenShield>().Texture;

        public override void SetStaticDefaults() => ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;

        public override void SetDefaults()
        {
            Projectile.width = 172;
            Projectile.height = 172;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 90000;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 25;
        }

        public override void AI()
        {
            ClamityPlayer clamityPlayer = Main.player[Projectile.owner].Clamity();
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.rotation += MathF.PI / 48f;
            Projectile.Center = Owner.Center;
            if (!clamityPlayer.pyroStoneVanity)
                Lighting.AddLight(Projectile.Center, Projectile.Opacity * 0.9f, Projectile.Opacity * 0.1f, Projectile.Opacity * 0.1f);
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Vector2.UnitX.RotatedBy(MathHelper.TwoPi / 8 * i + Projectile.rotation) * 90f, DustID.Flare, Vector2.UnitX.RotatedBy(MathHelper.TwoPi / 8 * i), Scale: 1.5f);
                dust.noGravity = true;
            }
            /*if (this.Owner != null && Owner.active && !this.Owner.dead && !(clamityPlayer.pyroStoneVanity || clamityPlayer.pyroStone))
                return;
            Projectile.Kill();*/
            //Main.NewText("PyroStone messenge: " + clamityPlayer.pyroStone.ToString() + " " + clamityPlayer.pyroStoneVanity.ToString());
            if (Owner == null || !Owner.active || Owner.dead || !(clamityPlayer.pyroStone || clamityPlayer.pyroStoneVanity))
                Projectile.Kill();

        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.player[Projectile.owner].Clamity().pyroStoneVanity)
                return;
            target.AddBuff(BuffID.OnFire3, 180, false);
            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 60, false);
            if (target.knockBackResist <= 0 || !CalamityGlobalNPC.ShouldAffectNPC(target))
                return;
            float num = MathHelper.Clamp(1f - target.knockBackResist, 0.0f, 1f);
            Vector2 vector2 = target.Center - Projectile.Center;
            vector2.Normalize();
            target.velocity = vector2 * num;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (Main.player[Projectile.owner].Clamity().pyroStoneVanity)
                return;
            target.AddBuff(BuffID.OnFire3, 180, true, false);
            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 60, true, false);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 center = Projectile.Center;
            Vector2 size = Projectile.Size;
            float radius = size.Length() * 0.5f;
            Rectangle targetHitbox1 = targetHitbox;
            return new bool?(CalamityUtils.CircularHitboxCollision(center, radius, targetHitbox1));
        }

        public override bool? CanHitNPC(NPC target)
        {
            ClamityPlayer clamityPlayer = Main.player[Projectile.owner].Clamity();
            return target.catchItem != 0 && target.type != ModContent.NPCType<Radiator>() || clamityPlayer.pyroStoneVanity ? new bool?(false) : new bool?();
        }
    }
}
