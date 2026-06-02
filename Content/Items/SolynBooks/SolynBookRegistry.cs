using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace Clamity.Content.Items.SolynBooks
{
    public enum SolynBooks
    {
        BaseBook1,
        BaseBook2,
        BaseBook3,
        BaseBook4,
        HowToClamity
    }
    public class SolynBookRegistry : ModSystem
    {
        public static Mod WotG;
        public Dictionary<SolynBooks, object> autoloadableSolynBookList = new Dictionary<SolynBooks, object>();
        public override void Load()
        {
            ModLoader.TryGetMod("NoxusBoss", out WotG);

            if (ModLoader.HasMod("NoxusBoss"))
            {
                //CreateSolynBook(, "Clamity/Content/Items/SolynBooks/", SolynBooks.);
                CreateSolynBook(0, $"Clamity/Content/Items/SolynBooks/", SolynBooks.BaseBook1);
                CreateSolynBook(0, $"Clamity/Content/Items/SolynBooks/", SolynBooks.BaseBook2);
                CreateSolynBook(1, $"Clamity/Content/Items/SolynBooks/", SolynBooks.BaseBook3);
                CreateSolynBook(1, $"Clamity/Content/Items/SolynBooks/", SolynBooks.BaseBook4);
                CreateSolynBook(2, $"Clamity/Content/Items/SolynBooks/", SolynBooks.HowToClamity);
            }
        }
        public static int GetBookItem(SolynBooks book) => Clamity.mod.Find<ModItem>(book.ToString()).Type;
        /// <summary>
        /// Register and creates a custom solyn book
        /// </summary>
        /// <param name="rarity">Value from 1 to 3</param>
        /// <param name="baseTexturePath">Base texture path</param>
        /// <param name="book">Book name from enum</param>
        public void CreateSolynBook(int rarity, string baseTexturePath, SolynBooks book)
        {
            Assembly wotg = WotG.Code;
            var types = AssemblyManager.GetLoadableTypes(wotg);
            Type bookDataType = FindType(types, "NoxusBoss.Core.Autoloaders.SolynBooks.LoadableBookData");
            object bookData = Activator.CreateInstance(bookDataType);
            bookDataType.GetField("Rarity", BindingFlags.Public | BindingFlags.Instance).SetValue(bookData, rarity); //Sets 3 stars for rarity of book
            bookDataType.GetField("TexturePath", BindingFlags.Public | BindingFlags.Instance).SetValue(bookData, baseTexturePath + book.ToString()); //Sets a texture path
            object result = FindType(types, "NoxusBoss.Core.Autoloaders.SolynBooks.SolynBookAutoloader").GetMethod("Create", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { Mod, bookData });
            autoloadableSolynBookList.Add(book, result);
        }
        public override void PostSetupContent()
        {
            if (ModLoader.HasMod("NoxusBoss"))
            {
                Assembly wotg = WotG.Code;
                var types = AssemblyManager.GetLoadableTypes(wotg);
                MethodInfo method = FindType(types, "NoxusBoss.Core.Graphics.UI.Books.SolynBookRewardsSystem").GetMethod("AddRewardForBook", BindingFlags.Static | BindingFlags.Public);

                void AddRewardForBook(SolynBooks book, MethodInfo method, string ItemName, int MinStack = 1, int MaxStack = 1, bool GiftedDirectlyFromSolyn = false)
                {
                    Type solynReward = FindType(types, "NoxusBoss.Core.Graphics.UI.Books.SolynReward");
                    object rewardData = Activator.CreateInstance(solynReward);
                    solynReward.GetField("ItemName", BindingFlags.Public | BindingFlags.Instance).SetValue(rewardData, ItemName);
                    solynReward.GetField("MinStack", BindingFlags.Public | BindingFlags.Instance).SetValue(rewardData, MinStack);
                    solynReward.GetField("MaxStack", BindingFlags.Public | BindingFlags.Instance).SetValue(rewardData, MaxStack);
                    solynReward.GetField("GiftedDirectlyFromSolyn", BindingFlags.Public | BindingFlags.Instance).SetValue(rewardData, GiftedDirectlyFromSolyn);
                    method.Invoke(null, autoloadableSolynBookList[book], rewardData);
                }

                AddRewardForBook(SolynBooks.HowToClamity, method, "LifeforcePotion", 2, 4);
                AddRewardForBook(SolynBooks.HowToClamity, method, "CalmingPotion", 2, 4);
                foreach (SolynBooks i in new List<SolynBooks> { SolynBooks.BaseBook1, SolynBooks.BaseBook2, SolynBooks.BaseBook3, SolynBooks.BaseBook4, })
                {
                    AddRewardForBook(i, method, "IronskinPotion", 2, 4);
                    AddRewardForBook(i, method, "RegenerationPotion", 2, 4);
                }
            }
        }
        private static Type? FindType(Type[] array, string name)
        {
            foreach (var type in array)
            {
                if (type.FullName is not null)
                {
                    //Find a needed type of code
                    if (type.FullName == name)
                    {
                        return type;
                    }
                }
            }
            return null;
        }
        public override void Unload()
        {
            WotG = null;
        }
    }
}
