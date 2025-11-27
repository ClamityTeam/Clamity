using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.Summon;
using CalamityMod.CalPlayer;
using CalamityMod.Items;
using CalamityMod.Items.Weapons.Summon;
using CalamityMod.Projectiles.Summon;
using Clamity.Content.Bosses.Clamitas.Drop;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace Clamity.Content.Bosses.Clamitas.Crafted.Weapons
{
    public class HellstoneShellfishStaff : ShellfishStaff
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();


            if (!ModLoader.TryGetMod("Redemption", out var redemption))
                return;
            redemption.Call("addElementItem", 2, Type);
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.rare = ItemRarityID.Lime;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.damage = 200;
            Item.shoot = ModContent.ProjectileType<HellstoneShellfishStaffMinion>();
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ShellfishStaff>()
                .AddIngredient(ItemID.HellstoneBar, 15)
                .AddIngredient<HuskOfCalamity>(6)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    public class HellstoneShellfishStaffBuff : ShellfishBuff
    {
        public override void Update(Player player, ref int buffIndex)
        {
            CalamityPlayer calamityPlayer = player.Calamity();
            if (player.ownedProjectileCounts[ModContent.ProjectileType<HellstoneShellfishStaffMinion>()] > 0)
            {
                calamityPlayer.shellfish = true;
            }

            if (!calamityPlayer.shellfish)
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
            else
            {
                player.buffTime[buffIndex] = 18000;
            }
        }
    }
    public class HellstoneShellfishStaffMinion : Shellfish, ILocalizedModType, IModType
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            if (!ModLoader.TryGetMod("Redemption", out var redemption))
                return;
            redemption.Call("addElementProj", 2, Type);
        }
        public new string LocalizationCategory => "Projectiles.Summon.Minion";
        private int playerStill;

        private bool fly;

        private bool spawnDust = true;
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 180);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ProjectileID.Volcano, Projectile.damage, 0, Projectile.owner);
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            CalamityPlayer calamityPlayer = player.Calamity();
            Projectile.Calamity();
            if (spawnDust)
            {
                int num = 20;
                for (int i = 0; i < num; i++)
                {
                    int num2 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y + 16f), Projectile.width, Projectile.height - 16, DustID.Water);
                    Main.dust[num2].velocity *= 2f;
                    Main.dust[num2].scale *= 1.15f;
                }

                spawnDust = false;
            }

            bool num3 = Projectile.type == ModContent.ProjectileType<HellstoneShellfishStaffMinion>();
            player.AddBuff(ModContent.BuffType<HellstoneShellfishStaffBuff>(), 3600);
            if (num3)
            {
                if (player.dead)
                {
                    calamityPlayer.shellfish = false;
                }

                if (calamityPlayer.shellfish)
                {
                    Projectile.timeLeft = 2;
                }
            }

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 3)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }

            if (Projectile.frame > 1)
            {
                Projectile.frame = 0;
            }

            if (Projectile.ai[0] == 0f)
            {
                Projectile.ai[1] += 1f;
                if (!fly)
                {
                    Projectile.tileCollide = true;
                    float num4 = (player.Center - Projectile.Center).Length();
                    if (Projectile.velocity.Y == 0f && (Projectile.velocity.X != 0f || num4 > 200f))
                    {
                        float num5 = Utils.SelectRandom(Main.rand, 5f, 7.5f, 10f);
                        Projectile.velocity.Y -= num5;
                    }

                    Projectile.velocity.Y += 0.3f;
                    float num6 = 1000f;
                    bool flag = false;
                    float num7 = 0f;
                    if (player.HasMinionAttackTargetNPC)
                    {
                        NPC nPC = Main.npc[player.MinionAttackTargetNPC];
                        if (nPC.CanBeChasedBy(Projectile))
                        {
                            float num8 = Vector2.Distance(nPC.Center, Projectile.Center);
                            if (!flag && num8 < num6)
                            {
                                num7 = nPC.Center.X;
                                flag = true;
                            }
                        }
                    }

                    if (!flag)
                    {
                        for (int j = 0; j < Main.maxNPCs; j++)
                        {
                            NPC nPC2 = Main.npc[j];
                            if (nPC2.CanBeChasedBy(Projectile))
                            {
                                float num9 = Vector2.Distance(nPC2.Center, Projectile.Center);
                                if (!flag && num9 < num6)
                                {
                                    num7 = nPC2.Center.X;
                                    flag = true;
                                }
                            }
                        }
                    }

                    if (flag)
                    {
                        if (num7 - Projectile.position.X > 0f)
                        {
                            float num10 = Utils.SelectRandom(Main.rand, 0.15f, 0.2f);
                            Projectile.velocity.X += num10;
                            if (Projectile.velocity.X > 8f)
                            {
                                Projectile.velocity.X = 8f;
                            }
                        }
                        else
                        {
                            float num11 = Utils.SelectRandom(Main.rand, 0.15f, 0.2f);
                            Projectile.velocity.X -= num11;
                            if (Projectile.velocity.X < -8f)
                            {
                                Projectile.velocity.X = -8f;
                            }
                        }
                    }
                    else
                    {
                        if (num4 > 800f)
                        {
                            fly = true;
                            Projectile.velocity.X = 0f;
                            Projectile.velocity.Y = 0f;
                            Projectile.tileCollide = false;
                        }

                        if (num4 > 200f)
                        {
                            if (player.position.X - Projectile.position.X > 0f)
                            {
                                float num12 = Utils.SelectRandom(Main.rand, 0.05f, 0.1f, 0.15f);
                                Projectile.velocity.X += num12;
                                if (Projectile.velocity.X > 6f)
                                {
                                    Projectile.velocity.X = 6f;
                                }
                            }
                            else
                            {
                                float num13 = Utils.SelectRandom(Main.rand, 0.05f, 0.1f, 0.15f);
                                Projectile.velocity.X -= num13;
                                if (Projectile.velocity.X < -6f)
                                {
                                    Projectile.velocity.X = -6f;
                                }
                            }
                        }

                        if (num4 < 200f && Projectile.velocity.X != 0f)
                        {
                            if (Projectile.velocity.X > 0.5f)
                            {
                                float num14 = Utils.SelectRandom(Main.rand, 0.05f, 0.1f, 0.15f);
                                Projectile.velocity.X -= num14;
                            }
                            else if (Projectile.velocity.X < -0.5f)
                            {
                                float num15 = Utils.SelectRandom(Main.rand, 0.05f, 0.1f, 0.15f);
                                Projectile.velocity.X += num15;
                            }
                            else if (Math.Abs(Projectile.velocity.X) < 0.5f)
                            {
                                Projectile.velocity.X = 0f;
                            }
                        }
                    }
                }
                else if (fly)
                {
                    Vector2 vector = player.Center - Projectile.Center + new Vector2(0f, 0f);
                    float num16 = vector.Length();
                    vector.Normalize();
                    vector *= 14f;
                    Projectile.velocity = (Projectile.velocity * 40f + vector) / 41f;
                    Projectile.rotation = Projectile.velocity.X * 0.03f;
                    if (num16 > 1500f)
                    {
                        Projectile.Center = player.Center;
                        Projectile.netUpdate = true;
                    }

                    if (num16 < 100f)
                    {
                        if (player.velocity.Y == 0f)
                        {
                            playerStill++;
                        }
                        else
                        {
                            playerStill = 0;
                        }

                        if (playerStill > 30 && !Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height))
                        {
                            fly = false;
                            Projectile.tileCollide = true;
                            Projectile.rotation = 0f;
                            Projectile.velocity.X *= 0.3f;
                            Projectile.velocity.Y *= 0.3f;
                        }
                    }
                }

                if (Projectile.velocity.X > 0.25f)
                {
                    Projectile.spriteDirection = -1;
                }
                else if (Projectile.velocity.X < -0.25f)
                {
                    Projectile.spriteDirection = 1;
                }
            }

            if (Projectile.ai[0] != 1f)
            {
                return;
            }

            Projectile.rotation = 0f;
            Projectile.tileCollide = false;
            bool flag2 = false;
            bool flag3 = false;
            Projectile.localAI[0] += 1f;
            if (Projectile.localAI[0] % 30f == 0f)
            {
                flag3 = true;
            }

            int num17 = (int)Projectile.ai[1];
            NPC nPC3 = Main.npc[num17];
            if (Projectile.localAI[0] >= 600000f)
            {
                flag2 = true;
            }
            else if (num17 < 0 || num17 >= Main.maxNPCs)
            {
                flag2 = true;
            }
            else if (nPC3.active && !nPC3.dontTakeDamage && nPC3.defense < 9999)
            {
                Projectile.Center = nPC3.Center - Projectile.velocity * 2f;
                Projectile.gfxOffY = nPC3.gfxOffY;
                if (flag3)
                {
                    nPC3.HitEffect(0, 1.0);
                    nPC3.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 180);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ProjectileID.Volcano, Projectile.damage, 0, Projectile.owner);
                }
            }
            else
            {
                flag2 = true;
            }

            if (flag2)
            {
                Projectile.ai[0] = 0f;
                Projectile.localAI[0] = 0f;
                Projectile.velocity.X = 0f;
                Projectile.velocity.Y = 0f;
            }
        }
    }
}
