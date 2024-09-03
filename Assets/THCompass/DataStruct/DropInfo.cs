namespace Assets.THCompass.DataStruct
{
    public struct DropInfo
    {
        public ObjectID itemID;
        public int stack;
        public DropInfo(ObjectID itemID, int stack)
        {
            this.itemID = itemID;
            this.stack = stack;
        }
    }
}
