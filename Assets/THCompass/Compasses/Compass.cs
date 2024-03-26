using Assets.THCompass.DataStruct;
using Assets.THCompass.DropManager.Rule;
using System.Collections.Generic;

namespace Assets.THCompass.Compasses
{
    public abstract class Compass
    {
        public abstract BossID BossID { get; }
        public abstract AreaType Area { get; }
        public abstract bool BelongsToSlime { get; }
        public virtual ObjectID BossSummoner { get; } = ObjectID.None;
        public virtual void RegisterUniqueDrop(List<DropRule> common, List<DropRule> grand) { }
    }
}
