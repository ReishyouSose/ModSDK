using CoreLib.Submodule.Command.Data;
using CoreLib.Submodule.Command.Interface;
using Unity.Entities;

namespace Assets.SkyBlockHelper
{
    public class SpawnSceneCommandHandler : IServerCommandHandler
    {
        public CommandOutput Execute(string[] parameters, Entity sender)
        {
            if (parameters.Length < 3)
            {
                return "Params count must 3";
            }
            if (!int.TryParse(parameters[1], out int x))
            {
                return "Params 2 should int";
            }
            if (!int.TryParse(parameters[2], out int y))
            {
                return "Params 3 should int";
            }
            SpawnSceneSystem.SpawnScene(parameters[0], x, y);
            return "try spawn " + parameters[0];
        }

        public string GetDescription()
        {
            return "Use /spawnscene <sceneName> <x> <y> to spawn special scene";
        }

        public string[] GetTriggerNames()
        {
            return new string[] { "spawnscene" };
        }
    }
}
