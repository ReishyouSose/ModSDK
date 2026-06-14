using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.GeneralConfigMenu.Scripts.ConfigSync
{
    public struct SyncAllConfigRpc : IRpcCommand { }
    public struct ConfigSyncRpc : IRpcCommand
    {
        public ushort SyncID;
        public ushort ConfigCount;
        public Entity Player;
        public ConfigSyncRpc(int syncID, int configCount, Entity player)
        {
            SyncID = (ushort)syncID;
            ConfigCount = (ushort)configCount;
            Player = player;
        }
    }
    public struct ConfigEntryRpc : IRpcCommand
    {
        public ushort SyncID;
        public ushort ConfigID;
        public ushort ChunkCount;
        public ushort ActualLength;
        public ConfigEntryRpc(int syncID, int configID, int chunkCount, int actualLength)
        {
            SyncID = (ushort)syncID;
            ConfigID = (ushort)configID;
            ChunkCount = (ushort)chunkCount;
            ActualLength = (ushort)actualLength;
        }
    }
    public struct ConfigChunkRpc : IRpcCommand
    {
        public ushort SyncID;
        public ushort ConfigID;
        public ushort Offset;
        public FixedBytes16 Data;

        /// <summary>
        /// 从字节数组创建 ConfigChunkRpc（静态工厂方法）
        /// </summary>
        public ConfigChunkRpc(int syncID, int configID, int offset, byte[] value)
        {
            SyncID = (ushort)syncID;
            ConfigID = (ushort)configID;
            Offset = (ushort)offset;
            Data = new();
            int length = value.Length;
            for (int i = 0; i < 16; i++)
            {
                int index = i + offset;
                if (index >= length)
                    return;
                SetByte(i, value[index]);
            }
        }
        public void SetByte(int index, byte value)
        {
            switch (index)
            {
                case 0:
                    Data.byte0000 = value;
                    break;
                case 1:
                    Data.byte0001 = value;
                    break;
                case 2:
                    Data.byte0002 = value;
                    break;
                case 3:
                    Data.byte0003 = value;
                    break;
                case 4:
                    Data.byte0004 = value;
                    break;
                case 5:
                    Data.byte0005 = value;
                    break;
                case 6:
                    Data.byte0006 = value;
                    break;
                case 7:
                    Data.byte0007 = value;
                    break;
                case 8:
                    Data.byte0008 = value;
                    break;
                case 9:
                    Data.byte0009 = value;
                    break;
                case 10:
                    Data.byte0010 = value;
                    break;
                case 11:
                    Data.byte0011 = value;
                    break;
                case 12:
                    Data.byte0012 = value;
                    break;
                case 13:
                    Data.byte0013 = value;
                    break;
                case 14:
                    Data.byte0014 = value;
                    break;
                case 15:
                    Data.byte0015 = value;
                    break;
            }
        }

        /// <summary>
        /// 获取指定位置的字节（实例方法）
        /// </summary>
        public readonly byte GetByte(int index)
        {
            return index switch
            {
                0 => Data.byte0000,
                1 => Data.byte0001,
                2 => Data.byte0002,
                3 => Data.byte0003,
                4 => Data.byte0004,
                5 => Data.byte0005,
                6 => Data.byte0006,
                7 => Data.byte0007,
                8 => Data.byte0008,
                9 => Data.byte0009,
                10 => Data.byte0010,
                11 => Data.byte0011,
                12 => Data.byte0012,
                13 => Data.byte0013,
                14 => Data.byte0014,
                15 => Data.byte0015,
                _ => 0
            };
        }
    }
}