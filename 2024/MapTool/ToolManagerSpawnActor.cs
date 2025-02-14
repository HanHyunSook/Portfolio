#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Common;
using Cysharp.Text;
using UnityEngine;

namespace Tools.Map.ToolManager
{
    using GData = ToolData.ToolDataSpawnActor;
    using GObject = ToolObject.ToolObjectSpawnActor;
    using GTable = Data.Table.SpawnActorTable;
    using NData = ToolData.ToolDataSpawnActorSafe;

    public class ToolManagerSpawnActor : ToolManagerBase<GData, GObject, GTable, NData>
    {
        protected override GData CreateData(Vector3 position)
        {
            var gData = base.CreateData(position);
            gData.MapID = MapToolModel.Instance.MapID;
            return gData;
        }

        public override ToolObject.ToolObjectGroupBase Clone(GObject gNode)
        {
            var gData = gNode.ObjectData.Clone() as GData;

            // Clone Group Object
            var gObject = CreateAddNode(gData);

            gNode.NodeList.ForEach(uNode =>
            {
                var nData = uNode.ObjectData.Clone(gData.ID) as NData;
                nData.SetJsonData(gData.JsonData);

                // Clone Node Object
                gObject.CreateAddNode(nData);
            });
            return gObject;
        }

        public override int GetUniqueGroupID()
        {
            var nodeIds = NodeList
                .Select(g => g.ObjectData.ID)
                .ToList();
            var tableIds = MapToolUtility.UsedTableIDs(typeof(GTable).Name);

            int uniqueID = MapToolModel.Instance.MapID * 1000 + 1;
            return FindUniqueID(uniqueID, nodeIds, tableIds);
        }

        public override int GetUniqueUnitID(ToolData.IToolDataNode nodeData)
        {
            var nodeIds = NodeList
                .Where(gNode => gNode.ObjectData.ID == nodeData.GroupID)
                .SelectMany(g => g.NodeList.Select(s => s.ObjectData.ID))
                .ToList();

            int uniqueID = IToolManager.MakeNodeID(nodeData);
            return FindUniqueID(uniqueID, nodeIds);
        }

        public override bool CheckAllDuplicateId()
        {
            var gDuplicateList = NodeList.GroupBy(gs => gs.ObjectData.ID)
                .Where(gs => gs.Count() > 1)
                .ToList();

            if (gDuplicateList.Count > 0)
            {
                var idStr = ZString.CreateStringBuilder();
                foreach (var duplicate in gDuplicateList)
                {
                    idStr.Append(ZString.Format("[ID: {0}, Count: {1}] ", duplicate.Key, duplicate.Count()));
                }

                Common.Debug.LogError(ZString.Format("{0}에 {1} 이 중복 사용되고 있습니다.", typeof(GTable).Name, idStr));
                return true;
            }
            return false;
        }

        #region Table
        protected override List<GTable> LoadAllTables(int mapID)
        {
            return Common.SQLiteConnector.Instance.GetDatasWhere<GTable>(typeof(GTable).Name, ZString.Format("MapID = {0} ORDER BY ActorType ASC, ZoneID ASC, ID ASC", mapID));
        }

        public override bool ToExcelFromDB(int mapID)
        {
            return MapToolModel.ToExcelFromDB(typeof(GTable).Name);
        }
        #endregion Table

        #region Save Load
        protected override void OnSaveData(int mapID, GObject gNode)
        {
            var uNodePos = new List<Vector3>();
            gNode.NodeList.ForEach(uNode =>
            {
                uNodePos.Add(uNode.ObjectData.Position);
            });
            MapToolModel.Instance.MapData.JsonDataListAdd(gNode.ObjectData.Save(mapID, uNodePos));
        }

        protected override void OnSaveBegin(int mapID)
        {
            base.OnSaveBegin(mapID);
            MapToolModel.DeleteFromDB(typeof(GTable).Name, ZString.Format("MapID IN({0})", mapID));
        }

        protected override void OnLoadData(GTable gTable)
        {
            var mJson = MapToolModel.Instance.MapData.JsonData;
            var gData = new GData(mJson.SpawnNodeList.SingleOrDefault(json => json.Id == gTable.ID), gTable);
            var gJson = gData.JsonData;

            // Create Group Object
            var gObject = CreateAddNode(gData);

            var posList = World.Utility.GetSpawnActorPosList(gData.TableData);
            if (posList.Count > 1)
            {
                posList.RemoveAt(0);
                posList.ForEach(pos =>
                {
                    var nData = new NData(gJson, gTable.ID);
                    nData.Position = pos;

                    // Create Node Object
                    gObject.CreateAddNode(nData);
                });
            }
        }

        public override void OnLoad(int groupID)
        {
            var gTable = Common.SQLiteConnector.Instance.GetDatasWhere<GTable>(typeof(GTable).Name, ZString.Format("ID = {0} ORDER BY ActorType ASC, ZoneID ASC, ID ASC", groupID)).SingleOrDefault();
            OnLoadData(gTable);
        }
        #endregion Save Load
    }
}
#endif // UNITY_EDITOR
