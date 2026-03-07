using Unity.Entities;
using Unity.NetCode;

namespace Assets.PointShop.Scripts
{
    [GhostComponent]
    public struct BossDefeatedInfo : IComponentData
    {
        [GhostField]
        public bool Slime;

        [GhostField]
        public bool Devourer;

        [GhostField]
        public bool LarvaHive;

        [GhostField]
        public bool Shaman;

        [GhostField]
        public bool Bird;

        [GhostField]
        public bool PoisonSlime;

        [GhostField]
        public bool Octopus;

        [GhostField]
        public bool SlipperySlime;

        [GhostField]
        public bool Scarab;

        [GhostField]
        public bool LavaSlime;

        [GhostField]
        public bool HydraNature;

        [GhostField]
        public bool HydraSea;

        [GhostField]
        public bool HydraDesert;

        [GhostField]
        public bool Atlantian;

        [GhostField]
        public bool CoreCommander;

        [GhostField]
        public bool WallSlime;

        [GhostField]
        public bool Cicada;

        [GhostField]
        public bool Robot;
    }
}
