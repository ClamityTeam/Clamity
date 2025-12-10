using CalamityMod;
using CalamityMod.Items;
using CalamityMod.NPCs.Other;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items
{
    public class WitherOnAStick : ColdheartIcicle
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            ItemID.Sets.AnimatesAsSoul[Type] = true;
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(10, 4));
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.Calamity().devItem = true;
        }
        public override void HoldStyle(Player player, Rectangle heldItemFrame)
        {
            player.itemLocation += new Vector2(0, -8);
        }
        public override void HoldItemFrame(Player player)
        {
            player.SetCompositeArmBack(enabled: true, Player.CompositeArmStretchAmount.Full, (float)(-player.direction) * (MathF.PI / 4f));
        }
        public override void HoldItem(Player player)
        {
            int num = ((player.direction == 1) ? 5 : (-base.Item.width - 5));
            Rectangle hitBox = new Rectangle((int)player.Center.X + num, (int)player.position.Y - 10, base.Item.width, base.Item.height);
            ActiveEntityIterator<NPC>.Enumerator enumerator = Main.ActiveNPCs.GetEnumerator();
            while (enumerator.MoveNext())
            {
                NPC current = enumerator.Current;
                if (!current.dontTakeDamage && current.type != ModContent.NPCType<THELORDE>() && hitBox.Intersects(current.getRect()))
                {
                    int damage = current.SimpleStrikeNPC(current.lifeMax / 200, player.direction, crit: true);
                    SoundStyle style = CnidarianJellyfishOnTheString.SlapSound with
                    {
                        Volume = 2f,
                        MaxInstances = 200
                    };
                    SoundEngine.PlaySound(in style, current.Center);
                    if (Main.netMode != 2)
                    {
                        BloodShed(hitBox, current, damage, player);
                    }
                }
            }
        }

        public new void BloodShed(Rectangle hitBox, NPC target, int damage, Player player)
        {
            float lerpValue = Utils.GetLerpValue(950f, 2000f, damage, clamped: true);
            Vector2 relativePosition = Vector2.Lerp(hitBox.Center.ToVector2(), target.Center, 0.65f);
            Vector2 relativePosition2 = target.Center + Main.rand.NextVector2Circular(target.width, target.height) * 0.04f;
            Vector2 spinninpoint = new Vector2(relativePosition2.X * (float)player.direction, relativePosition2.Y).SafeNormalize(Vector2.UnitY);

            for (int i = 0; i < 16; i++)
            {
                int lifetime = Main.rand.Next(22, 36);
                float num = Main.rand.NextFloat(0.6f, 0.8f);
                Color value = Color.Lerp(Color.Cyan, Color.LightBlue, Main.rand.NextFloat());
                value = Color.Lerp(value, new Color(51, 22, 94), Main.rand.NextFloat(0.65f));
                if (Main.rand.NextBool(20))
                {
                    num *= 2f;
                }

                Vector2 velocity = spinninpoint.RotatedByRandom(0.81000000238418579) * Main.rand.NextFloat(11f, 23f);
                velocity.Y -= 12f;
                GeneralParticleHandler.SpawnParticle(new BloodParticle(relativePosition2, velocity, lifetime, num, value));
            }

            for (int j = 0; j < 9; j++)
            {
                float scale = Main.rand.NextFloat(0.2f, 0.33f);
                Color color = Color.Lerp(Color.Cyan, Color.LightBlue, Main.rand.NextFloat(0.5f, 1f));
                Vector2 velocity2 = spinninpoint.RotatedByRandom(0.89999997615814209) * Main.rand.NextFloat(9f, 14.5f);
                GeneralParticleHandler.SpawnParticle(new BloodParticle2(relativePosition2, velocity2, 20, scale, color));
            }

            return;
        }
    }
}
