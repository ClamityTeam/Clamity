using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using static Terraria.Player;

namespace Clamity
{
    public static class ClamityUtils
    {
        /*[StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct EnemyStats
        {
            public static SortedDictionary<int, double> ExpertDamageMultiplier;

            public static SortedDictionary<int, int[]> ContactDamageValues;

            public static SortedDictionary<Tuple<int, int>, int[]> ProjectileDamageValues;

            public static SortedDictionary<int, Tuple<bool, int[]>> DebuffImmunities;
        }*/
        public static ClamityPlayer Clamity(this Player player) => player.GetModPlayer<ClamityPlayer>();
        public static ClamityGlobalProjectile Clamity(this Projectile proj) => proj.GetGlobalProjectile<ClamityGlobalProjectile>();
        public static ClamityGlobalNPC Clamity(this NPC npc) => npc.GetGlobalNPC<ClamityGlobalNPC>();
        public static ClamityGlobalItem Clamity(this Item item) => item.GetGlobalItem<ClamityGlobalItem>();
        public static LocalizedText GetText(string key) => Language.GetOrRegister("Mods.Clamity." + key, (Func<string>)null);
        public static bool ContainType(int type, params int[] array)
        {
            bool num = false;
            foreach (int i in array)
            {
                if (i == type)
                {
                    num = true;
                    break;
                }
            }
            return num;
        }
        /*public static double GetExpertDamageMultiplierClamity(this NPC npc, bool? master = null)
        {
            if (!EnemyStats.ExpertDamageMultiplier.TryGetValue(npc.type, out var value))
            {
                return 1.0;
            }

            return value;
        }
        public static void GetNPCDamageClamity(this NPC npc)
        {
            double num = npc.GetExpertDamageMultiplierClamity() * (Main.masterMode ? 3.0 : 2.0);
            if (!EnemyStats.ContactDamageValues.TryGetValue(npc.type, out var value))
            {
                npc.damage = 1;
            }

            int num2 = value[0];
            int num3 = ((value[1] == -1) ? (-1) : ((int)Math.Round((double)value[1] / num)));
            int num4 = ((value[2] == -1) ? (-1) : ((int)Math.Round((double)value[2] / num)));
            int num5 = ((value[3] == -1) ? (-1) : ((int)Math.Round((double)value[3] / num)));
            int num6 = ((value[4] == -1) ? (-1) : ((int)Math.Round((double)value[4] / num)));
            int num7 = (Main.masterMode ? num6 : (CalamityWorld.death ? num5 : (CalamityWorld.revenge ? num4 : (Main.expertMode ? num3 : num2))));
            if (num7 != -1)
            {
                npc.damage = num7;
            }
        }*/
        public static void Move(this Projectile projectile, Vector2 vector, float speed, float turnResistance = 10f,
            bool toPlayer = false)
        {
            Terraria.Player player = Main.player[projectile.owner];
            Vector2 moveTo = toPlayer ? player.Center + vector : vector;
            Vector2 move = moveTo - projectile.Center;
            float magnitude = Magnitude(move);
            if (magnitude > speed)
            {
                move *= speed / magnitude;
            }

            move = (projectile.velocity * turnResistance + move) / (turnResistance + 1f);
            magnitude = Magnitude(move);
            if (magnitude > speed)
            {
                move *= speed / magnitude;
            }

            projectile.velocity = move;
        }
        public static float Magnitude(Vector2 mag) // For the Move code above
        {
            return (float)Math.Sqrt(mag.X * mag.X + mag.Y * mag.Y);
        }

        #region SetValues
        public static Item SetShoot<T>(this Item item, float shootSpeed) where T : ModProjectile
        {
            item.shoot = ModContent.ProjectileType<T>();
            item.shootSpeed = shootSpeed;
            return item;
        }
        public static Item SetDamage<D>(this Item item, int damage, float knockback) where D : DamageClass
        {
            item.damage = damage;
            item.DamageType = ModContent.GetInstance<D>();
            item.knockBack = knockback;
            return item;
        }
        #endregion
        public static bool HandleChaining(this Projectile projectile, ICollection<int> hitTargets, ICollection<int> foundTargets, int max, Func<NPC, bool> condition = null)
        {
            //Applies foundTargets from the last call to hitTargets
            foreach (var f in foundTargets)
            {
                if (!hitTargets.Contains(f))
                {
                    //If check can be removed here, but left in in case of debugging/additional method
                    hitTargets.Add(f);
                }
            }
            foundTargets.Clear();

            //Seek suitable targets the projectile collides with
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];

                if (!npc.active || npc.dontTakeDamage || projectile.friendly && npc.townNPC) //The only checks that truly prevent damage. No chaseable or immortal, those can still be hit
                {
                    continue;
                }

                //Simple code instead of complicated recreation of vanilla+modded collision code here (which only runs clientside, but this has to be all-side)
                //Hitbox has to be for "next update cycle" because AI (where this should be called) runs before movement updates, which runs before damaging takes place
                Rectangle hitbox = new Rectangle((int)(projectile.position.X + projectile.velocity.X), (int)(projectile.position.Y + projectile.velocity.Y), projectile.width, projectile.height);
                ProjectileLoader.ModifyDamageHitbox(projectile, ref hitbox);

                if (!projectile.Colliding(hitbox, npc.Hitbox)) //Intersecting hitboxes + special checks. Safe to use all-side, lightning aura uses it
                {
                    continue;
                }

                if (condition != null && !condition(npc))
                {
                    //If custom condition returns false
                    continue;
                }

                foundTargets.Add(i);
            }

            if (hitTargets.Count >= max)
            {
                projectile.Kill();
                return true;
            }

            return false;
        }


        public static float InverseLerp(float from, float to, float x, bool clamped = true)
        {
            float inverse = (x - from) / (to - from);
            if (!clamped)
                return inverse;

            return MathHelper.Clamp(inverse, 0f, 1f);
        }
        /// <summary>
        /// Subdivides a rectangle into frames.
        /// </summary>
        /// <param name="rectangle">The base rectangle.</param>
        /// <param name="horizontalFrames">The amount of horizontal frames to subdivide into.</param>
        /// <param name="verticalFrames">The amount of vertical frames to subdivide into.</param>
        /// <param name="frameX">The index of the X frame.</param>
        /// <param name="frameY">The index of the Y frame.</param>
        public static Rectangle Subdivide(this Rectangle rectangle, int horizontalFrames, int verticalFrames, int frameX, int frameY)
        {
            int width = rectangle.Width / horizontalFrames;
            int height = rectangle.Height / verticalFrames;
            return new Rectangle(rectangle.Left + width * frameX, rectangle.Top + height * frameY, width, height);
        }
        public static Vector2 ProportionalOffset(NPC npc, float proportionality = 1f) => Main.rand.NextVector2Circular(npc.width * proportionality * 0.5f, npc.height * proportionality * 0.5f);
        public static Vector2 ProportionalOffset(Projectile npc, float proportionality = 1f) => Main.rand.NextVector2Circular(npc.width * proportionality * 0.5f, npc.height * proportionality * 0.5f);
        public static Vector2 ProportionalOffset(Player npc, float proportionality = 1f) => Main.rand.NextVector2Circular(npc.width * proportionality * 0.5f, npc.height * proportionality * 0.5f);
        public static Vector2 UpdateAim(Projectile projectile, Vector2 source, float speed)
        {
            Vector2 vector2_1 = Vector2.Normalize(Vector2.Subtract(Main.MouseWorld, source));
            if (Utils.HasNaNs(vector2_1))
            {
                vector2_1 = -Vector2.UnitY;
            }
            return Vector2.Multiply(Vector2.Normalize(Vector2.Lerp(vector2_1, Vector2.Normalize(projectile.velocity), 0.89f)), speed);
        }
        public static void UpdateHeldProjDoVelocity(Player Player, Vector2 rotatedRelativePoint, Projectile projectile, float? offsetAmount = null, float? lagSpeed = null)
        {
            if (Player.whoAmI == Main.myPlayer)
            {
                float speed = 1f;
                if (Player.HeldItem.shoot == projectile.type)
                {
                    speed = Player.HeldItem.shootSpeed * projectile.scale;
                }
                Vector2 newVelocity = Utils.SafeNormalize(Main.MouseWorld - rotatedRelativePoint, Vector2.UnitX * Player.direction) * speed;
                if (lagSpeed.HasValue)
                {
                    newVelocity = UpdateAim(projectile, rotatedRelativePoint, lagSpeed.Value);
                }
                if (projectile.velocity.X != newVelocity.X || projectile.velocity.Y != newVelocity.Y)
                {
                    projectile.netUpdate = true;
                }
                projectile.velocity = newVelocity;
                if (offsetAmount.HasValue)
                {
                    float offset = offsetAmount.Value * projectile.scale;
                    float velocityAngle = Utils.ToRotation(projectile.velocity);
                    projectile.position = rotatedRelativePoint - projectile.Size * 0.5f + Utils.ToRotationVector2(velocityAngle) * offset;
                }
            }

        }
        public static void UpdateHeldProj(Player Player, Vector2 rotatedRelativePoint, float offsetAmount, Projectile projectile, bool setTimeleft = true, bool updateArm = true)
        {
            float velocityAngle = Utils.ToRotation(projectile.velocity);
            projectile.rotation = velocityAngle + Utils.ToInt(projectile.spriteDirection == -1) * (float)Math.PI;
            projectile.direction = Utils.ToDirectionInt(Math.Cos(velocityAngle) > 0.0);
            float offset = offsetAmount * projectile.scale;
            projectile.position = rotatedRelativePoint - projectile.Size * 0.5f + Utils.ToRotationVector2(velocityAngle) * offset;
            projectile.spriteDirection = projectile.direction;
            Player.ChangeDir(projectile.direction);
            if (setTimeleft)
            {
                projectile.timeLeft = 2;
            }
            Player.heldProj = projectile.whoAmI;
            if (updateArm)
            {
                Player.SetCompositeArmFront(true, (CompositeArmStretchAmount)0, Utils.ToRotation(projectile.velocity) - (float)Math.PI / 2f);
            }
        }

        public static Recipe ReplaceIngredient(this Recipe recipe, int oldItem, int newItem, int count = 1)
        {
            int index = recipe.IngredientIndex(oldItem);
            if (index != -1)
            {
                recipe.requiredItem.RemoveAt(index);
                recipe.requiredItem.Insert(index, new Item(newItem) { stack = count });
            }
            return recipe;
        }
        public static void AddOrReplace<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value) where TKey : notnull
        {
            if (dict.ContainsKey(key)) dict[key] = value;
            else dict.Add(key, value);
        }
        #region System.Reflection magic
        public static Type FindModsClass(this Mod mod, string className)
        {
            foreach (var type in AssemblyManager.GetLoadableTypes(mod.Code))
            {
                if (type.FullName is not null)
                {
                    //Find a needed type of code
                    if (type.FullName == className)
                    {
                        return type;
                    }
                }
            }
            return null;
        }
        public static Type FindModsClass(string modName, string className)
        {
            foreach (var type in AssemblyManager.GetLoadableTypes(ModLoader.GetMod(modName).Code))
            {
                if (type.FullName is not null)
                {
                    //Find a needed type of code
                    if (type.FullName == className)
                    {
                        return type;
                    }
                }
            }
            return null;
        }
        public static object? Invoke(this MethodBase a, object? obj, params object?[] parameters) => a.Invoke(obj, parameters);
        #endregion
    }
}
