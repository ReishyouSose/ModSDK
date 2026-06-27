using PugTilemap;
using System.Collections.Generic;
using System.Linq;

namespace Assets.BuildingBlueprint.Scripts.Core
{
    public class TileTargetDataBlock : ScriptableDataBlock
    {
        public TileType TileType;
        public static TileTargetDataBlock[] GetSortedArray(IEnumerable<TileTargetDataBlock> list)
        {
            return list.OrderBy(x => x.TileType).ToArray();
        }
    }
}
