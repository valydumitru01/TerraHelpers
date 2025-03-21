using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Elements;

namespace Companions
{
    public class Companion
    {
        public string ID;
        public bool Active;
        public int NPCType;
        public Item[] Inventory = new Item[10]; // 10 slots for inventory

        public Companion() {
            for (int i = 0; i < Inventory.Length; i++)
                Inventory[i] = new Item();
        }
        
        public void SpawnCompanionNPC(Player player)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return; // Ensure only the server spawns NPCs
            
            IEntitySource source = player.GetSource_Misc("CompanionSpawn");
            NPC.NewNPC(source, (int)player.position.X, (int)player.position.Y, NPCType);
        }

        public TagCompound Save()
        {
            var items = new List<TagCompound>();
            foreach (var item in Inventory)
                items.Add(ItemIO.Save(item));
            
            return new TagCompound {
                ["ID"] = ID,
                ["Active"] = Active,
                ["NPCType"] = NPCType,
                ["Inventory"] = items
            };
        }

        public static Companion Load(TagCompound tag)
        {
            var comp = new Companion {
                ID = tag.GetString("ID"),
                Active = tag.GetBool("Active"),
                NPCType = tag.GetInt("NPCType"),
            };
            var items = tag.GetList<TagCompound>("Inventory");
            for (int i = 0; i < items.Count; i++)
                comp.Inventory[i] = ItemIO.Load(items[i]);
            return comp;
        }
    }

    public class Companions : Mod
    {
        public static Companions Instance { get; private set; }
        public override void Load()
        {
            if (!Main.dedServ) {
                CompanionUI.LoadUI();
            }
        }
    }

    class CompanionModPlayer : ModPlayer
    {
        public List<Companion> companions = new List<Companion>();

        public override void SaveData(TagCompound tag)
        {
            var companionsList = new List<TagCompound>();
            foreach (var comp in companions) companionsList.Add(comp.Save());
            tag["companions"] = companionsList;
        }

        public override void LoadData(TagCompound tag)
        {
            companions.Clear();
            foreach (TagCompound compTag in tag.GetList<TagCompound>("companions"))
                companions.Add(Companion.Load(compTag));
        }

        public override void OnEnterWorld()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                foreach (var comp in companions.Where(c => c.Active))
                {
                    comp.SpawnCompanionNPC(Player);
                }
            }
        }
    }

    public class CompanionUI : UIState
    {
        private UIPanel panel;
        private UIList companionList;

        public static UserInterface CompanionInterface;
        public static CompanionUI Instance;
        
        public static void LoadUI() {
            CompanionInterface = new UserInterface();
            Instance = new CompanionUI();
            CompanionInterface.SetState(Instance);
        }

        public override void OnInitialize()
        {
            panel = new UIPanel();
            panel.SetPadding(10);
            panel.Left.Set(400f, 0f);
            panel.Top.Set(200f, 0f);
            panel.Width.Set(250f, 0f);
            panel.Height.Set(400f, 0f);
            Append(panel);

            companionList = new UIList();
            companionList.Top.Set(10f, 0f);
            companionList.Width.Set(220f, 0f);
            companionList.Height.Set(350f, 0f);
            panel.Append(companionList);
        }

        public void UpdateCompanions(List<Companion> companions)
        {
            companionList.Clear();
            foreach (var companion in companions)
            {
                UIText text = new UIText(companion.ID);
                text.OnLeftClick += (evt, el) => OpenCompanionUI(companion);
                companionList.Add(text);
            }
        }

        private void OpenCompanionUI(Companion companion)
        {
            Main.NewText($"Opened companion UI for {companion.ID}");
            // TODO: Open the inventory/equipment UI for the selected companion
        }
    }

    public class CompanionModSystem : ModSystem
    {
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int index = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");
            if (index != -1)
            {
                layers.Insert(index, new LegacyGameInterfaceLayer("Companions UI",
                    () => {
                        if (CompanionUI.CompanionInterface?.CurrentState != null)
                            CompanionUI.CompanionInterface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    }, InterfaceScaleType.UI));
            }
        }
    }
}