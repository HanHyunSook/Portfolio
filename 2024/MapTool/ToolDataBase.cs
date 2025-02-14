#if UNITY_EDITOR
using Common;
using Data;
using Data.Json;

namespace Tools.Map
{
    public enum eGroupType
    {
        None = 0,
        Actor,
        Path,
        Zone,
        QuestZone,
        FootSoundZone,
    }
}

namespace Tools.Map.ToolData
{
    using UniRx;

    public interface IToolData
    {
        #region Observer
        Subject<IToolData> OnSubject { get; }
        public System.IObservable<IToolData> OnChangeObservable => OnSubject;
        #endregion Observer

        #region Editor
        UnityEngine.Color EditorColor { get; set; }
        bool EditorIsShow { get; set; }
        #endregion Editor

        #region Data Override
        TableEnum.eBranchVersion BranchVersion { get; set; }
        int ID { get; set; }
        UnityEngine.Vector3 Position { get; set; }
        #endregion Data Override

        void UpdateTransform(UnityEngine.Transform trans);

        void Doking()
        {
            Position = World.Utility.TerrainPosition(Position.x, Position.z);
        }
    }

    public abstract class ToolDataBase<UData, UJson, UTable> : IToolData
        where UData : ToolDataBase<UData, UJson, UTable>, new()
        where UJson : MapToolSaveDataColorBase, new()
        where UTable : TableDataBase, new()
    {
        protected Subject<IToolData> _subject = new Subject<IToolData>();
        public Subject<IToolData> OnSubject => _subject;
        public void Notify() => OnSubject?.OnNext(this);

        #region Editor
        private UJson _jsonData = null;
        public UJson JsonData
        {
            get => _jsonData;
            protected set => _jsonData = value;
        }


        public UnityEngine.Color EditorColor
        {
            get => JsonData.AreaColor;
            set => JsonData.AreaColor = JsonNode.ConvertColor(value);
        }
        public bool EditorIsShow
        {
            get => JsonData.IsAreaShow;
            set => JsonData.IsAreaShow = value;
        }
        #endregion Editor

        #region Data Override
        private UTable _tableData = null;
        public UTable TableData => _tableData;

        public abstract TableEnum.eBranchVersion BranchVersion { get; set; }
        public abstract int ID { get; set; }
        private UnityEngine.Vector3 _position = UnityEngine.Vector3.zero;
        public virtual UnityEngine.Vector3 Position
        {
            get => _position;
            set { _position = value; Notify(); }
        }
        #endregion Data Override

        ~ToolDataBase()
        {
            _subject?.Dispose();
        }

        protected abstract UData CloneData();

        public virtual void SetTableData(UTable data)
        {
            if (data != null)
            {
                _tableData = data;
            }
            else
            {
                _tableData = new UTable();
                CreateTableData();
            }
        }

        public virtual void SetJsonData(UJson json, bool emptyCreate = false)
        {
            _jsonData = json;
            if (emptyCreate && !_jsonData)
            {
                _jsonData = new UJson();
            }
        }

        protected virtual void CreateTableData()
        {

        }

        public abstract int UniqueId();

        public virtual void UpdateTransform(UnityEngine.Transform trans)
        {
            bool isChange = false;
            if (!Position.Equals(trans.position))
            {
                isChange = true;
                Position = trans.position;
            }
            if (isChange)
            {
                Notify();
            }
        }
    }
}
#endif
