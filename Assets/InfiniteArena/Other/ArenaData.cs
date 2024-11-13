using I2.Loc;
using System;
using System.Text;
using Unity.Mathematics;

namespace Assets.InfiniteArena.Other
{
    public struct ArenaData
    {
        public int2 offset;
        public ObjectID objID;
        public bool mark;
        public readonly override bool Equals(object obj)
        {
            if (obj is ArenaData data)
            {
                return data.objID == objID && data.offset.Equals(offset);
            }
            return false;
        }
        public readonly override int GetHashCode() => HashCode.Combine(offset, objID);
        public readonly override string ToString() => new StringBuilder().Append(offset.ToString().Remove(0, 4))
            .AppendLine().Append(LocalizationManager.GetTranslation("Items/" + objID)).ToString();
    }
}
