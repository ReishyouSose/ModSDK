using PugTilemap;
using Unity.Collections;

namespace Assets.CoreEnhance.Scripts.Helpers
{
    public static class TileHelper
    {
        public static void TryGetResource(NativeArray<TileCD> tiles, out bool ore, out bool wood)
        {
            ore = wood = false;
            foreach (var tile in tiles)
            {
                switch (tile.tileType)
                {
                    case TileType.ore:
                    case TileType.ancientCrystal:
                        ore = true;
                        return;
                    case TileType.bigRoot:
                        wood = true;
                        return;
                }
            }
            return;
        }
    }
}
