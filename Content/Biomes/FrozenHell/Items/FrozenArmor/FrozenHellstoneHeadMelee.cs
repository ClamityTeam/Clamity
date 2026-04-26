using CalamityMod;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Items.Materials;
using CalamityMod.Particles;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.CalamityUtils;

namespace Clamity.Content.Biomes.FrozenHell.Items.FrozenArmor
{
    [AutoloadEquip(EquipType.Head), LegacyName("FrozenHellstoneVisor")]
    public class FrozenHellstoneHeadMelee : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Armor.FrozenHellstone";

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ModContent.RarityType<BurnishedAuric>();
            Item.defense = 60;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<FrozenHellstoneChestplate>() && legs.type == ModContent.ItemType<FrozenHellstoneGreaves>();

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(ModContent.GetInstance<TrueMeleeDamageClass>()) += 0.2f;
            player.Clamity().inflicingMeleeFrostburn = true;
        }

        public override void UpdateArmorSet(Player player)
        {
            var hotkey = CalamityKeybinds.ArmorSetBonusHotKey.TooltipHotkeyString();
            player.setBonus = this.GetLocalization("SetBonus").Format(hotkey);

            //player.setBonus = "Cannot be frozen.\nPress Armor Set Bonus to create an ice shield that parries attacks.[WIP]\nFailing to parry will cause you to overcool.[WIP]";
            player.Clamity().endobsidianSet = true;
            player.Clamity().endobsidianMelee = true;
            player.buffImmune[44] = true;
            player.buffImmune[324] = true;
            player.buffImmune[47] = true;
            player.aggro += 400;
        }

        public static void HandleParryCountdown(Player player)
        {
            player.Clamity().endobsidianMeleeTime--;

            if (player.Clamity().endobsidianMeleeTime > 0)
            {
                /*player.controlJump = false;
                player.controlDown = false;
                player.controlLeft = false;
                player.controlRight = false;
                player.controlUp = false;
                player.controlUseItem = false;
                player.controlUseTile = false;
                player.controlThrow = false;
                player.gravDir = 1f;
                player.velocity = Vector2.Zero;
                player.velocity.Y = -0.1f; //if player velocity is 0, the flight meter gets reset
                player.RemoveAllGrapplingHooks();*/
            }
            else
            {
                /*for (int i = 0; i < 8; i++)
                {
                    int theDust = Dust.NewDust(player.position, player.width, player.height, (int)CalamityDusts.Brimstone, 0f, 0f, 100, new Color(255, 255, 255), 2f);
                    Main.dust[theDust].noGravity = true;
                }*/
            }
        }


        public override void AddRecipes() => CreateRecipe().AddIngredient(ItemID.MoltenHelmet)
                                                           .AddIngredient(ItemID.FrostHelmet)
                                                           .AddIngredient<EndobsidianBar>(10)
                                                           .AddIngredient<EndothermicEnergy>(18)
                                                           .AddTile(TileID.Hellforge)
                                                           .Register();
    }
    public class EndobsidianParryBoom : ModProjectile, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public Player Owner => Main.player[Projectile.owner];
        public ref float Time => ref Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 480;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 20;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            if (Time == 0f)
            {
                for (int i = 0; i < 5; i++)
                {
                    Particle explosion = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Lerp(Color.Cyan, Color.LightBlue, Utils.GetLerpValue(0, 5, i, true)), "CalamityMod/Particles/SoftRoundExplosion", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0f, 0.48f + 0.01f * i, (int)(20 - i * 1.5f));
                    GeneralParticleHandler.SpawnParticle(explosion);
                }
                for (int i = 0; i < 3; i++)
                {
                    Particle explosion = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Lerp(Color.Cyan, Color.LightBlue, Utils.GetLerpValue(0, 3, i, true)), "CalamityMod/Particles/FlameExplosion", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0f, 0.48f + 0.02f * i, (int)(20 - i * 2f));
                    GeneralParticleHandler.SpawnParticle(explosion);
                }

                Particle outerGlow = new CustomPulse(Projectile.Center, Vector2.Zero, Color.PowderBlue, "CalamityMod/Particles/BloomCircle", Vector2.One, 0f, 0.1f, 8f, 24, true);
                GeneralParticleHandler.SpawnParticle(outerGlow);
                Particle innerGlow = new CustomPulse(Projectile.Center, Vector2.Zero, Color.White, "CalamityMod/Particles/BloomCircle", Vector2.One, 0f, 0.05f, 4f, 24, true);
                GeneralParticleHandler.SpawnParticle(innerGlow);

                float offset = Main.rand.NextFloat(MathHelper.TwoPi);
                int maxI = 8;
                for (int i = 0; i < maxI; i++)
                {
                    Vector2 velocity = (MathHelper.TwoPi * i / maxI + offset).ToRotationVector2();
                    Particle cross = new GlowSparkParticle(Projectile.Center, velocity, false, 12, 1.6f, Color.Cyan, new Vector2(1f, 0.1f), true);
                    GeneralParticleHandler.SpawnParticle(cross);
                }
            }

            Projectile.scale = MathHelper.Lerp(0f, 1f, PiecewiseAnimation(Time / 20f, new CurveSegment[] { new CurveSegment(EasingType.PolyOut, 0f, 0f, 1f, 4) }));
            /*if (Time < 10f)
            {
                for (int i = 0; i < 3; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.Next(71, 73 + 1), Main.rand.NextVector2CircularEdge(6f, 6f) * (Main.rand.NextFloat(1f, 1.2f) + Projectile.scale));
                    dust.noGravity = true;
                    dust.noLight = true;
                    dust.scale = Main.rand.NextFloat(0.8f, 1.2f) + Projectile.scale;
                    dust.alpha = Main.rand.Next(120, 180 + 1);
                }
            }*/

            Time++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<GlacialState>(), 480);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => modifiers.HitDirectionOverride = (Owner.Center.X < target.Center.X).ToDirectionInt();

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CircularHitboxCollision(Projectile.Center, Projectile.width * Projectile.scale, targetHitbox);
    }
    public class GlacialRevenge : ModBuff
    {
        public override void SetStaticDefaults()
        {
            //Main.buffNoTimeDisplay[Type] = true;
            Main.debuff[Type] = false;
            Main.buffNoSave[Type] = true;
            //Main.pvpBuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.Clamity().glacialRevenge = true;
        }

    }
}
