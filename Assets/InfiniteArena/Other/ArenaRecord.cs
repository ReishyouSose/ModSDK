using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;

namespace Assets.InfiniteArena.Other
{
    public static class ArenaRecord
    {
        public static NativeList<NativeList<ArenaData>> ArenaDatas { get; private set; }
        private static int2 writer;
        private static List<ArenaData> data;
        public static void Load()
        {
            ArenaDatas = InitArenas();
        }
        private static NativeList<NativeList<ArenaData>> InitArenas()
        {
            data = new();
            #region style1
            MoveTo(-2, 1);
            Left(4);
            Up();
            Up(1, false);
            Mark();
            MoveTo(-4, 2, false);

            MoveTo(-1, 2);
            Up(4);
            Left();
            Left(1, false);
            Mark();
            MoveTo(-2, 4, false);

            Flip(true, false);
            Flip(false, true);
            NativeList<ArenaData> arena1 = Register();
            #endregion

            #region style2
            MoveTo(-2, 1);
            Left(2);
            Up(1, false);
            MoveTo(-1, 2);
            Up(3);
            Left();
            Left(1, false);
            Mark();

            MoveTo(-2, -1);
            Left(3);
            Down();
            Down(1, false);
            Mark();
            MoveTo(-1, -2);
            Down(2);
            Left(1, false);
            Flip(true, true);
            NativeList<ArenaData> arena2 = Register();
            #endregion

            #region style3
            MoveTo(-1, 2);
            Up(3);
            Up(1, false);
            MoveTo(-2, 1);
            Left(2);
            Flip(true, false);

            Right();
            Right(1, false);
            Mark();
            Up();
            Up(1, false);
            Flip(false, true);

            MoveTo(2, -4, false);
            MoveTo(2, -6);
            Right(1, false);
            Mark();
            MoveTo(-2, -4, false);
            MoveTo(-2, -6);
            Left(1, false);
            Mark();

            MoveTo(-5, 3, false);
            Down(6, false);
            MoveTo(-6, 3, false);
            Down(6, false);
            NativeList<ArenaData> arena3 = Register();
            #endregion

            #region style4
            MoveTo(-1, 2);
            Up(5);
            Left();
            Up();
            Left(2);

            writer.x++;
            writer.y--;
            for (int i = 0; i < 3; i++)
            {
                Left();
                Down();
                Down(1, false);
                Mark();
            }
            Flip(false, true);
            Flip(true, false);
            NativeList<ArenaData> arena4 = Register();
            #endregion

            #region style5
            MoveTo(-2, 1);
            Left(4);
            Down(2);
            Left(1, false);

            MoveTo(-1, 2);
            Up(2);
            Left(3);
            Down();
            Left(1, false);

            MoveTo(1, 2);
            Up(4);
            Left(2);
            Left(1, false);

            MoveTo(2, 1);
            Right(4);
            Down(3);
            Down(1, false);

            MoveTo(2, -1);
            Right(2);
            Down(3);
            Left();
            Down(1, false);

            MoveTo(1, -2);
            Down(4);
            Left(2);
            Down(1, false);

            MoveTo(-1, -2);
            Down(2);
            Left(2);
            Down(1, false);

            MoveTo(-2, -1);
            Left(2);
            Down(2);
            Left(1, false);
            NativeList<ArenaData> arena5 = Register();
            #endregion
            data = null;
            return new NativeList<NativeList<ArenaData>>(Allocator.Persistent)
            {
                arena1,
                arena2,
                arena3,
                arena4,
                arena5
            };
        }
        private static void Mark()
        {
            ArenaData d = data[^1];
            d.mark = true;
            data[^1] = d;
        }
        private static void Write(bool wire = true)
        {
            data.Add(new()
            {
                offset = writer,
                objID = wire ? ObjectID.ElectricalWire : ObjectID.EnemySpawnerPlatform
            });
        }
        private static void MoveTo(int x, int y, bool wire = true)
        {
            writer = new(x, y);
            Write(wire);
        }
        private static void Up(int step = 1, bool wire = true) => Move(0, 1, step, wire);
        private static void Down(int step = 1, bool wire = true) => Move(0, -1, step, wire);
        private static void Left(int step = 1, bool wire = true) => Move(-1, 0, step, wire);
        private static void Right(int step = 1, bool wire = true) => Move(1, 0, step, wire);
        private static void Move(int offX, int offY, int step = 1, bool wire = true)
        {
            for (int i = 0; i < step; i++)
            {
                writer.x += offX;
                writer.y += offY;
                Write(wire);
            }
        }
        private static NativeList<ArenaData> Register()
        {
            NativeList<ArenaData> arena = new(Allocator.Persistent);
            foreach (var info in data)
            {
                arena.Add(info);
            }
            data.Clear();
            return arena;
        }
        private static void Flip(bool vertical, bool horizen)
        {
            var temp = data.ToList();
            using var currents = temp.GetEnumerator();
            while (currents.MoveNext())
            {
                var current = currents.Current;
                int2 offset = current.offset;
                if (vertical)
                {
                    offset.x = -offset.x;
                }
                if (horizen)
                {
                    offset.y = -offset.y;
                }
                data.Add(new()
                {
                    objID = current.objID,
                    offset = offset,
                    mark = current.mark,
                });
            }
            writer = data[^1].offset;
        }
        public static Dictionary<ArenaData, bool> GetMatch(this NativeList<ArenaData> arena)
        {
            Dictionary<ArenaData, bool> maps = new();
            foreach (ArenaData a in arena)
            {
                maps.Add(a, false);
            }
            return maps;
        }
        public static Dictionary<ArenaData, bool> GetMark(this NativeList<ArenaData> arena)
        {
            Dictionary<ArenaData, bool> maps = new();
            foreach (ArenaData a in arena)
            {
                maps.Add(a, a.mark);
            }
            return maps;
        }
    }
}
