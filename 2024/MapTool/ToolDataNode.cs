#if UNITY_EDITOR
using Data;
using Data.Json;

namespace Tools.Map.ToolData
{
    public interface IToolDataNode : IToolData
    {
        eGroupType GroupType { get; }

        int OrderID { get; set; }
        int GroupID { get; set; }

        int UniqueId();
        IToolDataNode Clone();
        IToolDataNode Clone(int groupID);
    }

    public abstract class ToolDataNode<NData, GJson, NTable>
        : ToolDataBase<NData, GJson, NTable>, IToolDataNode
        where NData : ToolDataNode<NData, GJson, NTable>, new()
        where GJson : MapToolSaveDataColorBase, new()
        where NTable : TableDataBase, new()
    {
        public abstract eGroupType GroupType { get; }

        public abstract int OrderID { get; set; }
        public abstract int GroupID { get; set; }

        public virtual void Save(IToolDataGroup group, int mapID)
        {
            GroupID = group.ID;
            BranchVersion = group.BranchVersion;
            Common.SQLiteConnector.Instance.Connection.InsertOrReplace(TableData);
        }

        public virtual IToolDataNode Clone()
        {
            var copyData = CloneData();
            copyData.ID = UniqueId();

            return copyData;
        }
        public virtual IToolDataNode Clone(int groupID)
        {
            var copyData = CloneData();
            copyData.GroupID = groupID;
            copyData.ID = copyData.UniqueId();

            return copyData;
        }

        protected override NData CloneData()
        {
            var copyData = new NData();
            var tableProps = typeof(NTable).GetProperties();

            foreach (var prop in tableProps)
            {
                prop.SetValue(copyData.TableData, prop.GetValue(this.TableData));
            }

            copyData.JsonData = JsonData;
            copyData.Position = Position;
            copyData.OrderID = OrderID;
            copyData.GroupID = GroupID;
            copyData.ID = UniqueId();
            return copyData;
        }

        public override int UniqueId() => MapToolUtility.UniqueUnitID(this);
    }
}
#endif
