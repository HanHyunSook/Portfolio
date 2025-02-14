#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Tools.Map.ToolManager
{
    public interface IToolManager
    {
        int Count { get; }

        void Refresh();
        void Clear();

        ToolObject.ToolObjectGroupBase FindGroupObject(int id);

        ToolObject.ToolObjectGroupBase CreateAddNode(Vector3 position);
        void RemoveNode(int id);

        int GetUniqueGroupID();
        int GetUniqueUnitID(ToolData.IToolDataNode nodeData);
        bool CheckAllDuplicateId();

        UniTask OnSaveAsync(int mapID, float delta);
        bool ToExcelFromDB(int mapID);

        UniTask OnLoadAsync(int mapID, float delta);

        public static int MakeNodeID(ToolData.IToolDataNode nodeData)
        {
            switch (nodeData.GroupType)
            {
                case eGroupType.Actor:
                {
                    return nodeData.OrderID + 1;
                }
                case eGroupType.Path:
                {
                    return nodeData.GroupID * 1000 + nodeData.OrderID + 1;
                }
                default:
                {
                    return nodeData.GroupID * 100 + nodeData.OrderID + 1;
                }
            }
        }
    }

    public abstract class ToolManagerBase<GData, GObject, GTable, UData> : MonoBehaviour, IToolManager
        where GData : ToolData.IToolDataGroup, new()
        where GObject : ToolObject.ToolObjectGroupBase
        where GTable : Data.TableDataBase
        where UData : ToolData.IToolDataNode, new()
    {
        // 노드 리스트
        private List<GObject> _nodeList = new List<GObject>();
        public List<GObject> NodeList => _nodeList;

        public int Count => NodeList.Count;

        public ToolObject.ToolObjectGroupBase FindGroupObject(int id)
        {
            return NodeList.FirstOrDefault(node => node.GroupData.ID == id);
        }

        protected virtual GData CreateData(Vector3 position)
        {
            var gData = new GData();
            gData.Position = position;
            return gData;
        }

        public virtual ToolObject.ToolObjectGroupBase CreateAddNode(Vector3 position)
        {
            return CreateAddNode(CreateData(position));
        }

        protected virtual ToolObject.ToolObjectGroupBase CreateAddNode(GData gData)
        {
            var gNode = ToolObject.ToolObjectBase.Create<GObject>(transform, gData);
            gNode.DisplayName();

            if (gNode != null)
            {
                Add(gNode);
            }
            return gNode;
        }

        public abstract ToolObject.ToolObjectGroupBase Clone(GObject gNode);

        protected virtual void Add(GObject gNode)
        {
            NodeList.Add(gNode);
        }

        public virtual void Refresh()
        {
            var nodeList = new List<GObject>();
            foreach (Transform node in transform)
            {
                var script = node.GetComponent<GObject>();
                if (script)
                {
                    nodeList.Add(script);
                }
                else
                {
                    Destroy(node.gameObject);
                }
            }

            NodeList.Clear();
            NodeList.AddRange(nodeList);

            int idx = 0;
            NodeList.ForEach(n => n.transform.SetSiblingIndex(idx++));
        }

        public virtual void RemoveNode(int id)
        {
            NodeList.Where(node => node.GroupData.ID == id)
                .ToList()
                .ForEach(RemoveNode);
        }

        public virtual void RemoveNode(GObject node)
        {
            NodeList.Remove(node as GObject);
            if (node)
            {
                Destroy(node.gameObject);
            }
        }

        public void Clear()
        {
            NodeList.RemoveAll(n => !n);
            NodeList.ForEach(node =>
            {
                Destroy(node.gameObject);
            });
            NodeList.Clear();
        }

        protected int FindUniqueID(int uniqueID, List<int> nodeIds, List<int> tableIds = null)
        {
            if (tableIds == null)
                tableIds = new List<int>();

            bool find = false;
            while (!find)
            {
                if (!nodeIds.Any(id => id == uniqueID) &&
                    !tableIds.Any(id => id == uniqueID))
                {
                    find = true;
                }
                else
                {
                    uniqueID++;
                }
            }
            return uniqueID;
        }
        public abstract int GetUniqueGroupID();
        public abstract int GetUniqueUnitID(ToolData.IToolDataNode nodeData);
        public abstract bool CheckAllDuplicateId();

        #region Table
        protected abstract List<GTable> LoadAllTables(int mapID);
        public abstract bool ToExcelFromDB(int mapID);
        #endregion Table

        #region Save
        public async UniTask OnSaveAsync(int mapID, float delta)
        {
            Refresh();
            OnSaveBegin(mapID);
            await MapToolModel.Instance.OnAsync(gNode =>
            {
                gNode.Refresh();
                OnSaveData(mapID, gNode);
            }, NodeList, delta);
            OnSaveEnd(mapID);
        }
        protected abstract void OnSaveData(int mapID, GObject node);
        protected virtual void OnSaveBegin(int mapID) { }
        protected virtual void OnSaveEnd(int mapID) { }
        #endregion Save

        #region Load
        public async UniTask OnLoadAsync(int mapID, float delta)
        {
            var table = LoadAllTables(mapID);

            Clear();
            OnLoadBegin(mapID);
            await MapToolModel.Instance.OnAsync(tableData =>
            {
                OnLoadData(tableData);
            }, table, delta);
            OnLoadEnd(mapID);
        }
        protected abstract void OnLoadData(GTable tableData);
        public abstract void OnLoad(int groupID);
        protected virtual void OnLoadBegin(int mapID) { }
        protected virtual void OnLoadEnd(int mapID) { }
        #endregion Load
    }
}
#endif // UNITY_EDITOR
