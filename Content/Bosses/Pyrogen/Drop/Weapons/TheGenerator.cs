using CalamityMod;
using CalamityMod.Items;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Pyrogen.Drop.Weapons
{
    public class TheGenerator : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            if (!ModLoader.TryGetMod("Redemption", out var redemption))
                return;
            redemption.Call("addElementItem", 2, Type);
        }
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 46;
            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
            Item.rare = ItemRarityID.Pink;

            Item.useTime = Item.useAnimation = 24;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.Item9;
            Item.noMelee = true;

            Item.shoot = ModContent.ProjectileType<TheGeneratorSigil>();
            Item.shootSpeed = 5f;

            Item.damage = 77;
            Item.DamageType = DamageClass.Magic;
            Item.knockBack = 2f;
            Item.mana = 17;
        }
    }
    public class TheGeneratorSigil : ModProjectile, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override void SetStaticDefaults()
        {
            if (!ModLoader.TryGetMod("Redemption", out var redemption))
                return;
            redemption.Call("addElementProj", 2, Type);
        }
        public override void SetDefaults()
        {
            //Projectile.CloneDefaults(ProjectileID.Bullet);
            Projectile.width = Projectile.height = 32;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 500;
            Projectile.tileCollide = false;
            //Projectile.extraUpdates = 3;
            AIType = 14;
            //Projectile.Calamity().pointBlankShotDuration = 18;
        }
        public int TargetIndex = -1;
        public override void AI()
        {
            if (TargetIndex >= 0)
            {
                if (!Main.npc[TargetIndex].active || !Main.npc[TargetIndex].CanBeChasedBy())
                {
                    TargetIndex = -1;
                }
                else
                {
                    Vector2 value = Projectile.SafeDirectionTo(Main.npc[TargetIndex].Center)/* * (Projectile.velocity.Length() + 3.5f)*/;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, value, 0.01f);
                }
            }

            if (TargetIndex == -1)
            {
                NPC nPC = Projectile.Center.ClosestNPCAt(1600f);
                if (nPC != null)
                {
                    TargetIndex = nPC.whoAmI;
                }
            }
            foreach (NPC npc in Main.npc)
            {
                if (npc == null) continue;
                if (!npc.active && !npc.boss && npc.knockBackResist == 0f) continue;
                float distance = (npc.Center - Projectile.Center).Length();
                if (distance < 1600f)
                {
                    Vector2 value2 = npc.SafeDirectionTo(Projectile.Center) * (Projectile.velocity.Length() + 3.5f);
                    npc.velocity = Vector2.Lerp(npc.velocity, value2, 0.05f * npc.knockBackResist);
                }
            }
            Projectile.rotation = -Projectile.velocity.X * 0.05f;

            if (Projectile.ai[0] > 0)
                Projectile.ai[0]--;
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    Vector2 vec = Vector2.UnitY.RotatedBy(MathHelper.TwoPi / 6 * i + Projectile.rotation);
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + vec * 10f, DustID.GemAmethyst, vec * 3f);
                    dust.noGravity = true;
                }
                Projectile.ai[0] = 2;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.ShadowFlame, 120);
        }
    }
}
