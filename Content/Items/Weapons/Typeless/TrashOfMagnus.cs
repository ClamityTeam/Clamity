using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Weapons.Typeless
{
    public class TrashOfMagnus : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Weapons.Typeless";
        public override void SetStaticDefaults()
        {
            Item.staff[Type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 56;
            Item.value = CalamityGlobalItem.RarityYellowBuyPrice;
            Item.rare = ItemRarityID.Yellow;

            Item.useTime = Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;

            Item.SetDamage<AverageDamageClass>(24, 2f);

            Item.SetShoot<TrashOfMagnusProjectile>(12f);
        }
    }
    public class TrashOfMagnusProjectile : ModProjectile, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Projectiles.Classless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.aiStyle = -1;
            AIType = -1;
            Projectile.penetrate = 7;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.DamageType = ModContent.GetInstance<AverageDamageClass>();
        }
        public override void AI()
        {
            //Dust dust = Dust.NewDustPerfect(Projectile.position + new Vector2(Main.rand.NextFloat(0, Projectile.width), Main.rand.NextFloat(0, Projectile.height)), ModContent.DustType<>);
            Player player = Main.player[Projectile.owner];

            float numberOfDusts = 2f;
            float rotFactor = 360f / numberOfDusts;
            if (player.miscCounter % 2 == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    DirectionalPulseRing pulse = new DirectionalPulseRing(Projectile.position + new Vector2(Main.rand.NextFloat(0, Projectile.width), Main.rand.NextFloat(0, Projectile.height)), Vector2.Zero, Main.rand.NextBool(3) ? Color.LimeGreen : Color.Green, new Vector2(1, 1), 0, Main.rand.NextFloat(0.18f, 0.4f), 0f, 35);
                    GeneralParticleHandler.SpawnParticle(pulse);
                }

                for (int i = 0; i < 7; i++)
                {
                    int DustID = Main.rand.NextBool(30) ? 220 : 89;
                    float rot = MathHelper.ToRadians(i * rotFactor);
                    Vector2 offset = new Vector2(0.3f, 0).RotatedBy(rot * Main.rand.NextFloat(0.2f, 0.3f));
                    Dust dust2 = Dust.NewDustPerfect(Projectile.position + new Vector2(Main.rand.NextFloat(0, Projectile.width), Main.rand.NextFloat(0, Projectile.height)) + offset, DustID);
                    dust2.scale = Main.rand.NextFloat(0.3f, 0.4f);
                    if (DustID == 220)
                        dust2.scale = Main.rand.NextFloat(1f, 1.2f);
                }
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Plague>(), 8 * 60);
        }
    }
}
