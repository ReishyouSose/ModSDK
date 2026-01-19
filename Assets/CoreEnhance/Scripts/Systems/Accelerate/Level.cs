using Assets.CoreEnhance.Scripts.Components;
using Unity.Entities;

namespace Assets.CoreEnhance.Scripts.Systems.Accelerate
{
    [UpdateBefore(typeof(AddSkillValueSystem))]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class AccelerateLevelSystem : PugSimulationSystemBase
    {
        protected override void OnUpdate()
        {
            int multipler = 10;
            var ecb = CreateCommandBuffer();
            Entities.ForEach((Entity e, ref AddSkillValueCD skill) =>
            {
                skill.amount *= multipler;
                ecb.AddComponent<ProcessedTagCD>(e);
            })
                .WithName("Accelerate_Level")
                .WithBurst()
                .WithNone<ProcessedTagCD>()
                .Schedule();
            base.OnUpdate();
        }
    }
}
