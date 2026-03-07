using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.Melee;
using Clamity.Content.Bosses.Clamitas.Drop;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;


namespace Clamity.Content.Bosses.Clamitas.Crafted.Weapons
{
    public class ClamitasCrusher : ClamCrusher
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.rare = ItemRarityID.Lime;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.damage = 300;
            Item.shoot = ModContent.ProjectileType<ClamitasCrusherProjectile>();
            Item.shootSpeed = 25f;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ClamCrusher>()
                .AddIngredient<HuskOfCalamity>(6)
                //.AddIngredient<AshesofCalamity>(6)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    public class ClamitasCrusherProjectile : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;

        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead || player.noItems || player.CCed || Vector2.Distance(Projectile.Center, player.Center) > 900f)
            {
                Projectile.Kill();
                return;
            }
            if (Main.myPlayer == Projectile.owner && Main.mapFullscreen)
            {
                Projectile.Kill();
                return;
            }
            Vector2 playerCenter = player.MountedCenter;
            bool flag = true;
            bool flag2 = false;

            float num6 = 800f;
            float num7 = 3f;
            float num9 = 6f;

            int throwTime = 10;
            float throwSpeed = 24f;
            float recoverDistance = 16f;
            float recoverDistance2 = 48f;
            int attackCooldown = 20;

            float num11 = 1f;
            float num12 = 14f;
            int num13 = 60;
            int num14 = 10;

            int num16 = 10;
            int num17 = throwTime + 5;

            //SetStats(ref throwTime, ref throwSpeed, ref recoverDistance, ref recoverDistance2, ref attackCooldown);
            float meleeSpeed = player.GetAttackSpeed(DamageClass.Melee);
            float num18 = 1f * meleeSpeed;
            throwSpeed *= num18;
            num11 *= num18;
            num12 *= num18;
            num7 *= num18;
            recoverDistance *= num18;
            num9 *= num18;
            recoverDistance2 *= num18;
            float num19 = throwSpeed * throwTime;
            float num20 = num19 + 160f;
            Projectile.localNPCHitCooldown = num14;
            switch ((int)Projectile.ai[0])
            {
                case 0: //rotating
                    {
                        flag2 = true;
                        if (Projectile.owner == Main.myPlayer)
                        {
                            Vector2 origin = playerCenter;
                            Vector2 mouseWorld = Main.MouseWorld;
                            Vector2 value3 = origin.DirectionTo(mouseWorld).SafeNormalize(Vector2.UnitX * player.direction);
                            player.ChangeDir((value3.X > 0f) ? 1 : (-1));
                            if (!player.channel) //throw
                            {
                                Projectile.ai[0] = 1f;
                                Projectile.ai[1] = 0f;
                                Projectile.velocity = value3 * throwSpeed + player.velocity;
                                Projectile.Center = playerCenter;
                                Projectile.netUpdate = true;
                                for (int k = 0; k < 200; k++)
                                {
                                    Projectile.localNPCImmunity[k] = 0;
                                }
                                Projectile.localNPCHitCooldown = num16;
                                break;
                            }
                        }
                        Projectile.localAI[1] += 0.33f; //NOTE for future me - changed from 1f to 0.33f to make slower. If i starm making base flail class please make 1f
                        Vector2 value4 = new Vector2(player.direction).RotatedBy((float)Math.PI * 10f * (Projectile.localAI[1] / 60f) * player.direction);
                        value4.Y *= 0.8f;
                        if (value4.Y * player.gravDir > 0f)
                        {
                            value4.Y *= 0.5f;
                        }
                        Projectile.Center = playerCenter + value4 * 30f;
                        Projectile.velocity = Vector2.Zero;
                        Projectile.localNPCHitCooldown = attackCooldown;
                        break;
                    }
                case 1: //fly after throw
                    {
                        bool flag4 = Projectile.ai[1]++ >= throwTime;
                        flag4 |= Projectile.Distance(playerCenter) >= num6;
                        if (player.controlUseItem)
                        {
                            Projectile.ai[0] = 6f;
                            Projectile.ai[1] = 0f;
                            Projectile.netUpdate = true;
                            Projectile.velocity *= 0.2f;
                            break;
                        }
                        if (flag4) //start returning  after time out
                        {
                            Projectile.ai[0] = 2f;
                            Projectile.ai[1] = 0f;
                            Projectile.netUpdate = true;
                            Projectile.velocity *= 0.3f;
                        }
                        player.ChangeDir((player.Center.X < Projectile.Center.X) ? 1 : (-1));
                        Projectile.localNPCHitCooldown = num16;
                        break;
                    }
                case 2: //return
                    {
                        Vector2 value2 = Projectile.DirectionTo(playerCenter).SafeNormalize(Vector2.Zero);
                        if (Projectile.Distance(playerCenter) <= recoverDistance)
                        {
                            Projectile.Kill();
                            return;
                        }
                        if (player.controlUseItem)
                        {
                            Projectile.ai[0] = 6f;
                            Projectile.ai[1] = 0f;
                            Projectile.netUpdate = true;
                            Projectile.velocity *= 0.2f;
                        }
                        else
                        {
                            Projectile.velocity *= 0.98f;
                            Projectile.velocity = Projectile.velocity.MoveTowards(value2 * recoverDistance, num7);
                            player.ChangeDir((player.Center.X < Projectile.Center.X) ? 1 : (-1));
                        }
                        break;
                    }
                case 3: //what is this mode
                    {
                        if (!player.controlUseItem)
                        {
                            Projectile.ai[0] = 4f;
                            Projectile.ai[1] = 0f;
                            Projectile.netUpdate = true;
                            break;
                        }
                        float num21 = Projectile.Distance(playerCenter);
                        Projectile.tileCollide = Projectile.ai[1] == 1f;
                        bool flag3 = num21 <= num19;
                        if (flag3 != Projectile.tileCollide)
                        {
                            Projectile.tileCollide = flag3;
                            Projectile.ai[1] = Projectile.tileCollide ? 1 : 0;
                            Projectile.netUpdate = true;
                        }
                        if (num21 > num13)
                        {
                            if (num21 >= num19)
                            {
                                Projectile.velocity *= 0.5f;
                                Projectile.velocity = Projectile.velocity.MoveTowards(Projectile.DirectionTo(playerCenter).SafeNormalize(Vector2.Zero) * num12, num12);
                            }
                            Projectile.velocity *= 0.98f;
                            Projectile.velocity = Projectile.velocity.MoveTowards(Projectile.DirectionTo(playerCenter).SafeNormalize(Vector2.Zero) * num12, num11);
                        }
                        else
                        {
                            if (Projectile.velocity.Length() < 6f)
                            {
                                Projectile.velocity.X *= 0.96f;
                                Projectile.velocity.Y += 0.2f;
                            }
                            if (player.velocity.X == 0f)
                            {
                                Projectile.velocity.X *= 0.96f;
                            }
                        }
                        player.ChangeDir((player.Center.X < Projectile.Center.X) ? 1 : (-1));
                        break;
                    }
                case 4: //permanent return state
                    {
                        Projectile.tileCollide = false;
                        Vector2 vector = Projectile.DirectionTo(playerCenter).SafeNormalize(Vector2.Zero);
                        if (Projectile.Distance(playerCenter) <= recoverDistance2)
                        {
                            Projectile.Kill();
                            return;
                        }
                        Projectile.velocity *= 0.98f;
                        Projectile.velocity = Projectile.velocity.MoveTowards(vector * recoverDistance2, num9);
                        Vector2 target = Projectile.Center + Projectile.velocity;
                        Vector2 value = playerCenter.DirectionFrom(target).SafeNormalize(Vector2.Zero);
                        if (Vector2.Dot(vector, value) < 0f)
                        {
                            Projectile.Kill();
                            return;
                        }
                        player.ChangeDir((player.Center.X < Projectile.Center.X) ? 1 : (-1));
                        break;
                    }
                case 5: //mode after tile colliding
                    if (Projectile.ai[1]++ >= num17)
                    {
                        Projectile.ai[0] = 6f;
                        Projectile.ai[1] = 0f;
                        Projectile.netUpdate = true;
                    }
                    else
                    {
                        Projectile.localNPCHitCooldown = num16;
                        Projectile.velocity.Y += 0.6f;
                        Projectile.velocity.X *= 0.95f;
                        player.ChangeDir((player.Center.X < Projectile.Center.X) ? 1 : (-1));
                    }
                    break;
                case 6: //gravity i think
                    if (!player.controlUseItem || Projectile.Distance(playerCenter) > num20)
                    {
                        Projectile.ai[0] = 4f;
                        Projectile.ai[1] = 0f;
                        Projectile.netUpdate = true;
                    }
                    else
                    {
                        Projectile.velocity.Y += 0.8f;
                        Projectile.velocity.X *= 0.95f;
                        player.ChangeDir((player.Center.X < Projectile.Center.X) ? 1 : (-1));
                    }
                    break;
            }

            Projectile.direction = (Projectile.velocity.X > 0f) ? 1 : (-1);
            Projectile.spriteDirection = Projectile.direction;
            Projectile.ownerHitCheck = flag2;
            if (flag)
            {
                if (Projectile.velocity.Length() > 1f)
                {
                    Projectile.rotation = Projectile.velocity.ToRotation() + Projectile.velocity.X * 0.1f;
                }
                else
                {
                    Projectile.rotation += Projectile.velocity.X * 0.1f;
                }
            }
            Projectile.timeLeft = 2;
            player.heldProj = Projectile.whoAmI;
            player.SetDummyItemTime(2);
            player.itemRotation = Projectile.DirectionFrom(playerCenter).ToRotation();
            if (Projectile.Center.X < playerCenter.X)
            {
                player.itemRotation += (float)Math.PI;
            }
            player.itemRotation = MathHelper.WrapAngle(player.itemRotation);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Projectile.ai[0] == 0f)
            {
                modifiers.FinalDamage *= 1.2f;
            }
            if (Projectile.ai[0] == 1f || Projectile.ai[0] == 2f)
            {
                modifiers.FinalDamage *= 2;
            }
        }
        private void OMGHEEXPLODES()
        {
            //NOTE need make explosion effect

            for (int i = 0; i < 15; i++)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2(0, -Main.rand.NextFloat(10f, 15f)).RotatedByRandom(MathHelper.PiOver4 / 2), ModContent.ProjectileType<ClamitasCrusherProjectile>(), Projectile.damage / 3, Projectile.knockBack, Projectile.owner, i == 1 ? 1 : 0, Main.rand.NextFloat(-0.1f, 0.1f));
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            int num = 10;
            int i = 0;
            Vector2 velocity = Projectile.velocity;
            float num2 = 0.2f;
            if (Projectile.ai[0] == 1f || Projectile.ai[0] == 5f)
            {
                num2 = 0.4f;
            }
            if (Projectile.ai[0] == 6f)
            {
                num2 = 0f;
            }
            if (oldVelocity.X != Projectile.velocity.X)
            {
                if (Math.Abs(oldVelocity.X) > 4f)
                {
                    i = 1;
                }
                Projectile.velocity.X = (0f - oldVelocity.X) * num2;
                Projectile.localAI[0] += 1f;
            }
            if (oldVelocity.Y != Projectile.velocity.Y)
            {
                if (Math.Abs(oldVelocity.Y) > 4f)
                {
                    i = 1;
                }
                Projectile.velocity.Y = (0f - oldVelocity.Y) * num2;
                Projectile.localAI[0] += 1f;
            }
            if (Projectile.ai[0] == 1f)
            {
                Projectile.ai[0] = 5f;
                Projectile.localNPCHitCooldown = num;
                Projectile.netUpdate = true;
                Point scanAreaStart = Projectile.TopLeft.ToTileCoordinates();
                Point scanAreaEnd = Projectile.BottomRight.ToTileCoordinates();
                i = 2;
                CreateImpactExplosion(2, Projectile.Center, ref scanAreaStart, ref scanAreaEnd, Projectile.width, out var causedShockwaves);
                CreateImpactExplosion2_FlailTileCollision(Projectile.Center, causedShockwaves, velocity);
                Projectile.position -= velocity;
            }
            if (i > 0)
            {
                Projectile.netUpdate = true;
                for (int j = 0; j < i; j++)
                {
                    Collision.HitTiles(Projectile.position, velocity, Projectile.width, Projectile.height);
                }
                SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            }
            if (Projectile.ai[0] != 3f && Projectile.ai[0] != 0f && Projectile.ai[0] != 5f && Projectile.ai[0] != 6f && Projectile.localAI[0] >= 10f)
            {
                Projectile.ai[0] = 4f;
                Projectile.netUpdate = true;
            }
            /*
			if (Projectile.wet)
			{
				wetVelocity = Projectile.velocity;
			}
			*/
            return false;
        }
        private static void CreateImpactExplosion(int dustAmountMultiplier, Vector2 explosionOrigin, ref Point scanAreaStart, ref Point scanAreaEnd, int explosionRange, out bool causedShockwaves)
        {
            causedShockwaves = false;
            int num = 4;
            for (int i = scanAreaStart.X; i <= scanAreaEnd.X; i++)
            {
                for (int j = scanAreaStart.Y; j <= scanAreaEnd.Y; j++)
                {
                    if (Vector2.Distance(explosionOrigin, new Vector2(i * 16, j * 16)) > explosionRange)
                    {
                        continue;
                    }
                    Tile tileSafely = Framing.GetTileSafely(i, j);
                    if (!tileSafely.HasTile || !Main.tileSolid[tileSafely.TileType] || Main.tileSolidTop[tileSafely.TileType] || Main.tileFrameImportant[tileSafely.TileType])
                    {
                        continue;
                    }
                    Tile tileSafely2 = Framing.GetTileSafely(i, j - 1);
                    if (tileSafely2.HasTile && Main.tileSolid[tileSafely2.TileType] && !Main.tileSolidTop[tileSafely2.TileType])
                    {
                        continue;
                    }
                    int num2 = WorldGen.KillTile_GetTileDustAmount(fail: true, tileSafely, i, j) * dustAmountMultiplier;
                    for (int k = 0; k < num2; k++)
                    {
                        Dust obj = Main.dust[WorldGen.KillTile_MakeTileDust(i, j, tileSafely)];
                        obj.velocity.Y -= 3f + num * 1.5f;
                        obj.velocity.Y *= Main.rand.NextFloat();
                        obj.scale += num * 0.03f;
                    }
                    if (num >= 2)
                    {
                        for (int l = 0; l < num2 - 1; l++)
                        {
                            Dust obj2 = Main.dust[WorldGen.KillTile_MakeTileDust(i, j, tileSafely)];
                            obj2.velocity.Y -= 1f + num;
                            obj2.velocity.Y *= Main.rand.NextFloat();
                        }
                    }
                    if (num2 > 0)
                    {
                        causedShockwaves = true;
                    }
                }
            }
        }
        private void CreateImpactExplosion2_FlailTileCollision(Vector2 explosionOrigin, bool causedShockwaves, Vector2 velocityBeforeCollision)
        {
            Vector2 spinningpoint = new(7f, 0f);
            Vector2 value = new(1f, 0.7f);
            Color color = Color.White * 0.5f;
            Vector2 value2 = velocityBeforeCollision.SafeNormalize(Vector2.Zero);
            for (float num = 0f; num < 8f; num += 1f)
            {
                Vector2 value3 = spinningpoint.RotatedBy(num * ((float)Math.PI * 2f) / 8f) * value;
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke);
                dust.alpha = 0;
                if (!causedShockwaves)
                {
                    dust.alpha = 50;
                }
                dust.color = color;
                dust.position = explosionOrigin + value3;
                dust.velocity.Y -= 0.8f;
                dust.velocity.X *= 0.8f;
                dust.fadeIn = 0.3f + Main.rand.NextFloat() * 0.4f;
                dust.scale = 0.4f;
                dust.noLight = true;
                dust.velocity += value2 * 2f;
            }
            if (!causedShockwaves)
            {
                for (float num2 = 0f; num2 < 8f; num2 += 1f)
                {
                    Vector2 value4 = spinningpoint.RotatedBy(num2 * ((float)Math.PI * 2f) / 8f) * value;
                    Dust dust2 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke);
                    dust2.alpha = 100;
                    dust2.color = color;
                    dust2.position = explosionOrigin + value4;
                    dust2.velocity.Y -= 1f;
                    dust2.velocity.X *= 0.4f;
                    dust2.fadeIn = 0.3f + Main.rand.NextFloat() * 0.4f;
                    dust2.scale = 0.4f;
                    dust2.noLight = true;
                    dust2.velocity += value2 * 1.5f;
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 mountedCenter = Main.player[Projectile.owner].MountedCenter;
            Texture2D texture2D2 = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Melee/ClamCrusherChain").Value;
            Vector2 projCenter = Projectile.Center;
            Rectangle? sourceRectangle = null;
            Vector2 origin = new Vector2((float)texture2D2.Width * 0.5f, (float)texture2D2.Height * 0.5f);
            float projHeight = (float)texture2D2.Height;
            Vector2 actualCenter = mountedCenter - projCenter;
            float flailRotate = (float)Math.Atan2((double)actualCenter.Y, (double)actualCenter.X) - MathHelper.PiOver2;
            bool isActive = true;
            if (float.IsNaN(projCenter.X) && float.IsNaN(projCenter.Y))
            {
                isActive = false;
            }
            if (float.IsNaN(actualCenter.X) && float.IsNaN(actualCenter.Y))
            {
                isActive = false;
            }
            while (isActive)
            {
                if (actualCenter.Length() < projHeight + 1f)
                {
                    isActive = false;
                }
                else
                {
                    Vector2 value2 = actualCenter;
                    value2.Normalize();
                    projCenter += value2 * projHeight;
                    actualCenter = mountedCenter - projCenter;
                    Color colorArea = Lighting.GetColor((int)projCenter.X / 16, (int)(projCenter.Y / 16f));
                    Main.spriteBatch.Draw(texture2D2, projCenter - Main.screenPosition, sourceRectangle, colorArea, flailRotate, origin, 1f, SpriteEffects.None, 0);
                }
            }
            return true;
        }

    }
    public class ClamitasCrusherGore : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
        }
        public override void AI()
        {
            Projectile.velocity.Y += 0.5f;
            Projectile.velocity.X *= 0.98f;
            Projectile.rotation += Projectile.ai[1];
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D t = ModContent.Request<Texture2D>("Clamity/Content/Bosses/Clamitas/Drop/" + (Projectile.ai[0] == 1 ? "ClamitousPearl" : "HuskOfCalamity")).Value;

            Main.spriteBatch.Draw(t, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, t.Size() / 2f, Vector2.One, SpriteEffects.None, 0);
            return false;
        }
    }
}
