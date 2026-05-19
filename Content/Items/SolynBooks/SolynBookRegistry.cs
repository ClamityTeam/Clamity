using System;
using System.Reflection;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace Clamity.Content.Items.SolynBooks
{
    public enum SolynBookList
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
        public override void Load()
        {
            ModLoader.TryGetMod("NoxusBoss", out WotG);

            if (ModLoader.HasMod("NoxusBoss"))
            {
                //CreateSolynBook(3, "Clamity/Content/Items/SolynBooks/");
                foreach (string i in Enum.GetNames(typeof(SolynBookList)))
                {
                    int rarity = 1;
                    if (i == Enum.GetName(SolynBookList.HowToClamity))
                        rarity = 2;

                    CreateSolynBook(rarity, $"Clamity/Content/Items/SolynBooks/{i}");
                }
            }
        }
        public static int GetBookItem(SolynBookList book) => Clamity.mod.Find<ModItem>(book.ToString()).Type;
        /// <summary>
        /// Register and creates a custom solyn book
        /// </summary>
        /// <param name="rarity">Value from 1 to 3</param>
        /// <param name="texturePath">Full texture path</param>
        public void CreateSolynBook(int rarity, string texturePath)
        {
            Assembly wotg = WotG.Code;
            var types = AssemblyManager.GetLoadableTypes(wotg);
            Type bookDataType = FindType(types, "NoxusBoss.Core.Autoloaders.SolynBooks.LoadableBookData");
            object bookData = Activator.CreateInstance(bookDataType);
            bookDataType.GetField("Rarity", BindingFlags.Public | BindingFlags.Instance).SetValue(bookData, rarity); //Sets 3 stars for rarity of book
            bookDataType.GetField("TexturePath", BindingFlags.Public | BindingFlags.Instance).SetValue(bookData, texturePath); //Sets a texture path
            object result = FindType(types, "NoxusBoss.Core.Autoloaders.SolynBooks.SolynBookAutoloader").GetMethod("Create", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { Mod, bookData });

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
