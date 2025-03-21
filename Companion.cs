using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Companions
{
    public class Companion
    {
        public string ID;       // Unique identifier for the companion
        public bool Active;     // Indicates if the companion is currently active (spawned)
        public int NPCType;     // The NPC type ID associated with this companion

        // Spawns the companion NPC at the player's position
        public void Spawn(Player player)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return; // Only execute on the server side

            if (Active) return; // Already active

            // Spawn the NPC at the player's position
            int npcIndex = NPC.NewNPC(player.GetSource_Misc("CompanionSpawn"), (int)player.position.X, (int)player.position.Y, NPCType);
            if (npcIndex >= 0 && npcIndex < Main.npc.Length)
            {
                Main.npc[npcIndex].ai[0] = player.whoAmI; // Assign the player as the target
                Active = true;
            }
        }

        // Despawns the companion NPC
        public void Despawn()
        {
            if (!Active) return; // Already inactive

            // Find the NPC instance and set it as inactive
            for (int i = 0; i < Main.npc.Length; i++)
            {
                if (Main.npc[i].type == NPCType && Main.npc[i].active)
                {
                    Main.npc[i].active = false;
                    Active = false;
                    break;
                }
            }
        }

        // Saves the companion data
        public TagCompound Save()
        {
            return new TagCompound
            {
                ["ID"] = ID,
                ["Active"] = Active,
                ["NPCType"] = NPCType
            };
        }

        // Loads the companion data
        public static Companion Load(TagCompound tag)
        {
            return new Companion
            {
                ID = tag.GetString("ID"),
                Active = tag.GetBool("Active"),
                NPCType = tag.GetInt("NPCType")
            };
        }
    }
}