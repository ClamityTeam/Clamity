using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Typeless;
using CalamityMod.Rarities;
using CalamityMod.Sounds;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Weapons.Typeless
{
    public class EyeOfNova : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Weapons.Typeless";
        public override void SetDefaults()
        {
            Item.DamageType = ModContent.GetInstance<AverageDamageClass>();
            Item.width = 60;
            Item.damage = 400;
            Item.rare = ModContent.RarityType<BurnishedAuric>();
            Item.useAnimation = Item.useTime = 10;
            Item.useStyle = 5;
            Item.knockBack = 5f;
            Item.UseSound = CommonCalamitySounds.LaserCannonSound;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.height = 50;
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.shoot = ModContent.ProjectileType<EyeOfNovaProjectile>();
            Item.shootSpeed = 12f;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = (ContentSamples.CreativeHelper.ItemGroup)580;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-15f, 0f);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<EyeofMagnus>()
                .AddIngredient<Celesticus>()
                .AddIngredient<GoldenGun>()
                .AddIngredient<MiracleMatter>()
                .AddTile<DraedonsForge>()
                .Register();
        }
    }
    public class EyeOfNovaProjectile : ModProjectile, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Projectiles.Classless";

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 2;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            float num = 5f;
            float num2 = 6f;
            if (Projectile.ai[1] == 0f)
            {
                Projectile.ai[1] = 1f;
                Projectile.localAI[0] = 0f - (float)Main.rand.Next(48);
            }
            else if (Projectile.ai[1] == 1f && Projectile.owner == Main.myPlayer)
            {
                int num3 = -1;
                float num4 = 150f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].active && Main.npc[i].CanBeChasedBy(Projectile))
                    {
                        Vector2 center = Main.npc[i].Center;
                        float num5 = Vector2.Distance(center, Projectile.Center);
                        if (num5 < num4 && num3 == -1 && Collision.CanHitLine(Projectile.Center, 1, 1, center, 1, 1))
                        {
                            num4 = num5;
                            num3 = i;
                        }
                    }
                }

                if (num4 < 8f)
                {
                    Projectile.Kill();
                    return;
                }

                if (num3 != -1)
                {
                    Projectile.ai[1] = num + 1f;
                    Projectile.ai[0] = num3;
                    Projectile.netUpdate = true;
                }
            }
            else if (Projectile.ai[1] > num)
            {
                Projectile.ai[1] += 1f;
                int num6 = (int)Projectile.ai[0];
                if (!Main.npc[num6].active || !Main.npc[num6].CanBeChasedBy(Projectile))
                {
                    Projectile.ai[1] = 1f;
                    Projectile.ai[0] = 0f;
                    Projectile.netUpdate = true;
                }
                else
                {
                    Projectile.velocity.ToRotation();
                    Vector2 vector = Main.npc[num6].Center - Projectile.Center;
                    if (vector.Length() < 20f)
                    {
                        Projectile.Kill();
                        return;
                    }

                    if (vector != Vector2.Zero)
                    {
                        vector.Normalize();
                        vector *= num2;
                    }

                    float num7 = 20f;
                    Projectile.velocity = (Projectile.velocity * (num7 - 1f) + vector) / num7;
                }
            }

            if (Projectile.ai[1] >= 1f && Projectile.ai[1] < num)
            {
                Projectile.ai[1] += 1f;
                if (Projectile.ai[1] == num)
                {
                    Projectile.ai[1] = 1f;
                }
            }

            int type = Utils.SelectRandom<int>(Main.rand, 56, 92, 229, 206, 181);
            int type2 = 261;
            Projectile.localAI[0] += 1f;
            if (Projectile.localAI[0] == 48f)
            {
                Projectile.localAI[0] = 0f;
            }
            else if (Projectile.alpha == 0)
            {
                for (int j = 0; j < 2; j++)
                {
                    Vector2 vector2 = Vector2.UnitX * -30f;
                    vector2 = -Vector2.UnitY.RotatedBy(Projectile.localAI[0] * (MathF.PI / 24f) + (float)j * MathF.PI) * new Vector2(10f, 20f) - Projectile.rotation.ToRotationVector2() * 10f;
                    int num8 = Dust.NewDust(Projectile.Center, 0, 0, type2, 0f, 0f, 160, new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB));
                    Main.dust[num8].scale = 1f;
                    Main.dust[num8].noGravity = true;
                    Main.dust[num8].position = Projectile.Center + vector2 + Projectile.velocity * 2f;
                    Main.dust[num8].velocity = Vector2.Normalize(Projectile.Center + Projectile.velocity * 2f * 8f - Main.dust[num8].position) * 2f + Projectile.velocity * 2f;
                }
            }

            if (Main.rand.NextBool(12))
            {
                Vector2 vector3 = -Vector2.UnitX.RotatedByRandom(0.2).RotatedBy(Projectile.velocity.ToRotation());
                int num9 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 160, 0f, 0f, 100, new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB));
                Main.dust[num9].velocity *= 0.1f;
                Main.dust[num9].position = Projectile.Center + vector3 * Projectile.width / 2f + Projectile.velocity * 2f;
                Main.dust[num9].fadeIn = 0.9f;
            }

            if (Main.rand.NextBool(64))
            {
                Vector2 vector4 = -Vector2.UnitX.RotatedByRandom(0.4).RotatedBy(Projectile.velocity.ToRotation());
                int num10 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 160, 0f, 0f, 155, new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB), 0.8f);
                Main.dust[num10].velocity *= 0.3f;
                Main.dust[num10].position = Projectile.Center + vector4 * Projectile.width / 2f;
                if (Main.rand.NextBool())
                {
                    Main.dust[num10].fadeIn = 1.4f;
                }
            }

            if (Main.rand.NextBool(4))
            {
                for (int k = 0; k < 2; k++)
                {
                    Vector2 vector5 = -Vector2.UnitX.RotatedByRandom(0.8).RotatedBy(Projectile.velocity.ToRotation());
                    int num11 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, type, 0f, 0f, 0, default(Color), 1.2f);
                    Main.dust[num11].velocity *= 0.3f;
                    Main.dust[num11].noGravity = true;
                    Main.dust[num11].position = Projectile.Center + vector5 * Projectile.width / 2f;
                    if (Main.rand.NextBool())
                    {
                        Main.dust[num11].fadeIn = 1.4f;
                    }
                }
            }

            if (Main.rand.NextBool(3))
            {
                Vector2 vector6 = -Vector2.UnitX.RotatedByRandom(0.2).RotatedBy(Projectile.velocity.ToRotation());
                int num12 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, type2, 0f, 0f, 100);
                Main.dust[num12].velocity *= 0.3f;
                Main.dust[num12].position = Projectile.Center + vector6 * Projectile.width / 2f;
                Main.dust[num12].fadeIn = 1.2f;
                Main.dust[num12].scale = 1.5f;
                Main.dust[num12].noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, (float)(255 - Projectile.alpha) * 0.25f / 255f, (float)(255 - Projectile.alpha) * 0f / 255f, (float)(255 - Projectile.alpha) * 0.25f / 255f);
            for (int l = 0; l < 2; l++)
            {
                int num13 = 14;
                int num14 = Dust.NewDust(Projectile.position, Projectile.width - num13 * 2, Projectile.height - num13 * 2, 263, 0f, 0f, 100, default(Color), 1.35f);
                Main.dust[num14].noGravity = true;
                Main.dust[num14].velocity *= 0.1f;
                Main.dust[num14].velocity += Projectile.velocity * 0.5f;
            }

            if (Main.rand.NextBool(8))
            {
                int num15 = 16;
                int num16 = Dust.NewDust(Projectile.position, Projectile.width - num15 * 2, Projectile.height - num15 * 2, 263, 0f, 0f, 100);
                Main.dust[num16].velocity *= 0.25f;
                Main.dust[num16].noGravity = true;
                Main.dust[num16].velocity += Projectile.velocity * 0.5f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D value = ModContent.Request<Texture2D>(Texture).Value;
            Main.EntitySpriteDraw(value, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, value.Size() / 2f, Projectile.scale, SpriteEffects.None);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MarkedforDeath>(), 480);
            target.AddBuff(ModContent.BuffType<Vaporfied>(), 480);
            target.AddBuff(ModContent.BuffType<MiracleBlight>(), 480);
            target.AddBuff(BuffID.Ichor, 480);
            if (target.canGhostHeal && !Main.player[Projectile.owner].moonLeech)
            {
                Player obj = Main.player[Projectile.owner];
                obj.statLife += 2;
                obj.statMana += 25;
                obj.HealEffect(2);
                obj.ManaEffect(25);
            }
        }

        public override void OnKill(int timeLeft)
        {
            int type = 263;
            int type2 = 160;
            int newSize = 50;
            float scale = 1.7f;
            float scale2 = 0.8f;
            float scale3 = 2f;
            Vector2 vector = (Projectile.rotation - MathF.PI / 2f).ToRotationVector2() * Projectile.velocity.Length() * Projectile.MaxUpdates;
            SoundEngine.PlaySound(in SoundID.Item14, Projectile.position);
            Projectile.ExpandHitboxBy(newSize);
            Projectile.maxPenetrate = -1;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.Damage();
            for (int i = 0; i < 40; i++)
            {
                int type3 = 160;
                int num = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, type3, 0f, 0f, 200, new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB), scale);
                Dust obj = Main.dust[num];
                obj.position = Projectile.Center + Vector2.UnitY.RotatedByRandom(Math.PI) * (float)Main.rand.NextDouble() * Projectile.width / 2f;
                obj.noGravity = true;
                obj.velocity *= 3f;
                obj.velocity += vector * Main.rand.NextFloat();
                num = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, type, 0f, 0f, 100, default(Color), scale2);
                obj.position = Projectile.Center + Vector2.UnitY.RotatedByRandom(Math.PI) * (float)Main.rand.NextDouble() * Projectile.width / 2f;
                obj.velocity *= 2f;
                obj.noGravity = true;
                obj.fadeIn = 1f;
                obj.color = Color.Crimson * 0.5f;
                obj.velocity += vector * Main.rand.NextFloat();
            }

            for (int j = 0; j < 20; j++)
            {
                int num2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, type2, 0f, 0f, 0, new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB), scale3);
                Dust obj2 = Main.dust[num2];
                obj2.position = Projectile.Center + Vector2.UnitX.RotatedByRandom(Math.PI).RotatedBy(Projectile.velocity.ToRotation()) * Projectile.width / 3f;
                obj2.noGravity = true;
                obj2.velocity *= 0.5f;
                obj2.velocity += vector * (0.6f + 0.6f * Main.rand.NextFloat());
            }
        }
    }
}
