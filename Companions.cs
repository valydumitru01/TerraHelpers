using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace Companions
{
    
    public class CompanionModSystem : ModSystem
    {
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            if (index != -1)
            {
                layers.Insert(index, new LegacyGameInterfaceLayer(
                    "CompanionMod: Companion UI",
                    delegate
                    {
                        if (CompanionMod.CompanionUserInterface?.CurrentState != null)
                        {
                            CompanionMod.CompanionUserInterface.Update(Main._drawInterfaceGameTime);
                            CompanionMod.CompanionUserInterface.Draw(Main.spriteBatch, Main._drawInterfaceGameTime);
                        }
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
    public class CompanionPlayer : ModPlayer
    {
        public List<Companion> Companions = new List<Companion>();

        // Save companion data
        public override void SaveData(TagCompound tag)
        {
            var companionTags = new List<TagCompound>();
            foreach (var companion in Companions)
            {
                companionTags.Add(companion.Save());
            }
            tag["Companions"] = companionTags;
        }

        // Load companion data
        public override void LoadData(TagCompound tag)
        {
            Companions.Clear();
            foreach (var companionTag in tag.GetList<TagCompound>("Companions"))
            {
                Companions.Add(Companion.Load(companionTag));
            }
        }

        // Spawn active companions when entering the world
        public override void OnEnterWorld()
        {
            foreach (var companion in Companions)
            {
                if (companion.Active)
                {
                    companion.Spawn(Player);
                }
            }
        }
        

    }
    
    public class CompanionMod : Mod
    {
        internal static UserInterface CompanionUserInterface;
        internal static CompanionUI CompanionUIState;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                CompanionUIState = new CompanionUI();
                CompanionUIState.Activate();
                CompanionUserInterface = new UserInterface();
                CompanionUserInterface.SetState(CompanionUIState);
            }
        }
        

    }
}