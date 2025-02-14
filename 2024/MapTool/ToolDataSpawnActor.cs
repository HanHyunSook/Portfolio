#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Common;
using UnityEngine;

namespace Tools.Map.ToolData
{
    using GData = ToolData.ToolDataSpawnActor;
    using GJson = Data.Json.MapToolSaveDataSpawnActor;
    using GTable = Data.Table.SpawnActorTable;
    using NData = ToolData.ToolDataSpawnActorSafe;
    using NTable = Data.Table.SpawnActorTable;

    public class ToolDataSpawnActor : ToolDataGroup<GData, GJson, GTable, NData, NTable>
    {
        public override eGroupType GroupType => eGroupType.Actor;

        public class ActorRateData
        {
            public int ActorID;
            public int ActorRate;

            public ActorRateData(int id, int rate)
            {
                ActorID = id;
                ActorRate = rate;
            }
        }

        #region Editor
        public string EditorMemo
        {
            get => JsonData.Memo;
            set { JsonData.Memo = value; Notify(); }
        }

        public bool EditorIsShowArea
        {
            get => JsonData.IsAreaRadiusShow;
            set { JsonData.IsAreaRadiusShow = value; Notify(); }
        }
        #endregion Editor

        #region Data Override
        public override TableEnum.eBranchVersion BranchVersion
        {
            get => TableData.BranchVersion;
            set { TableData.BranchVersion = value; Notify(); }
        }
        public override int ID
        {
            get => TableData.ID;
            set { TableData.ID = value; Notify(); }
        }
        public int MapID
        {
            get => TableData.MapID;
            set { TableData.MapID = value; Notify(); }
        }
        public int ZoneID
        {
            get => TableData.ZoneID;
            set { TableData.ZoneID = value; Notify(); }
        }
        public TableEnum.eActorType ActorType
        {
            get => TableData.ActorType;
            set { TableData.ActorType = value; Notify(); }
        }

        #region Actor
        public int ActorCount
        {
            get => TableData.Count;
            set { TableData.Count = value; Notify(); }
        }
        public float ActorRotation
        {
            get => TableData.ActorRotation;
            set { TableData.ActorRotation = value; Notify(); }
        }
        public bool IsSaveActorList => ActorType == TableEnum.eActorType.Monster;
        private List<ActorRateData> _actorDatas;
        public List<ActorRateData> ActorDatas
        {
            get
            {
                if (_actorDatas.Count == 0)
                {
                    _actorDatas.Add(new ActorRateData(0, 0));
                }
                return _actorDatas;
            }
        }
        public bool isObstacle
        {
            get => TableData.isObstacle;
            set { TableData.isObstacle = value; Notify(); }
        }
        public float HitColliderRadius
        {
            get => TableData.HitColliderRadius;
            set { TableData.HitColliderRadius = value; Notify(); }
        }
        #endregion Actor

        #region Event Actor
        public bool IsSaveEventData => ActorType == TableEnum.eActorType.Monster || ActorType == TableEnum.eActorType.NpcAction;
        public bool EventSpawnUse
        {
            get => TableData.EventSpawnUse;
            set { TableData.EventSpawnUse = value; Notify(); }
        }
        public int EventCount
        {
            get => TableData.EventCount;
            set { TableData.EventCount = value; Notify(); }
        }
        public float EventActorRotation
        {
            get => TableData.EventActorRotation;
            set { TableData.EventActorRotation = value; Notify(); }
        }
        private List<ActorRateData> _eventDatas;
        public List<ActorRateData> EventDatas
        {
            get
            {
                if (_eventDatas.Count == 0)
                {
                    _eventDatas.Add(new ActorRateData(0, 0));
                }
                return _eventDatas;
            }
        }
        #endregion Event Actor

        #region Respawn
        public bool ActorInitialSpawn
        {
            get => TableData.ActorInitialSpawn;
            set { TableData.ActorInitialSpawn = value; Notify(); }
        }
        public TableEnum.eRespawnType RespawnType
        {
            get => TableData.RespawnType;
            set { TableData.RespawnType = value; Notify(); }
        }
        public int RespawnCount
        {
            get => TableData.RespawnCount;
            set { TableData.RespawnCount = value; Notify(); }
        }
        public int RespawnTime
        {
            get => TableData.RespawnTime;
            set { TableData.RespawnTime = value; Notify(); }
        }
        #endregion Respawn

        #region Area
        public TableEnum.eAreaType AreaType
        {
            get => TableData.AreaType;
            set { TableData.AreaType = value; Notify(); }
        }
        public float AreaRotation
        {
            get => TableData.AreaRotation;
            set { TableData.AreaRotation = value; Notify(); }
        }
        private float _areaSizeSphere = 1;
        public float AreaSizeSphere
        {
            get => _areaSizeSphere;
            set { _areaSizeSphere = value; Notify(); }
        }
        private Vector2 _areaSizeCure = Vector2.one;
        public Vector2 AreaSizeCure
        {
            get => _areaSizeCure;
            set { _areaSizeCure = value; Notify(); }
        }
        #endregion Area

        #region Random
        #endregion Random
        public bool IsRandom
        {
            get => TableData.IsRandom;
            set { TableData.IsRandom = value; Notify(); }
        }
        #endregion Data Override

        public ToolDataSpawnActor()
        {
            Load(null, null);
            ID = UniqueId();
        }

        public ToolDataSpawnActor(GJson gJson, GTable gTable)
        {
            Load(gJson, gTable);
        }

        protected override GData CloneData()
        {
            var copyData = base.CloneData();

            copyData._areaSizeSphere = AreaSizeSphere;
            copyData._areaSizeCure = AreaSizeCure;

            copyData.ActorDatas.Clear();
            copyData.ActorDatas.AddRange(ActorDatas);
            copyData.EventDatas.Clear();
            copyData.EventDatas.AddRange(EventDatas);
            return copyData;
        }

        public GJson Save(int mapID, List<Vector3> uNodePos)
        {
            MapID = mapID;

            string strPos = string.Empty;
            strPos = MapToolUtility.GetPositionString(Position);
            if (uNodePos.Count > 0)
            {
                uNodePos.ForEach(pos =>
                {
                    string safeStr = MapToolUtility.GetPositionString(pos);
                    strPos += strPos.Length > 0 ? $"|{safeStr}" : safeStr;
                });
            }
            strPos = strPos.Replace("(", "");
            strPos = strPos.Replace(")", "");
            TableData.Position = strPos;

            string strSize = string.Empty;
            switch (AreaType)
            {
                case TableEnum.eAreaType.Sphere:
                {
                    strSize = AreaSizeSphere.ToString();
                    AreaRotation = 0;
                    break;
                }
                case TableEnum.eAreaType.Cure:
                {
                    strSize = $"{AreaSizeCure.x},{AreaSizeCure.y}";
                    break;
                }
                default:
                {
                    strSize = "0";
                    AreaRotation = 0;
                    break;
                }
            }
            TableData.Size = strSize;

            if (IsSaveActorList)
            {
                TableData.ActorID = string.Join(",", ActorDatas.Select(value => value.ActorID.ToString()));
                TableData.ActorRate = string.Join(",", ActorDatas.Select(value => value.ActorRate.ToString()));
            }
            else
            {
                TableData.ActorID = ActorDatas[0].ActorID.ToString();
                TableData.ActorRate = string.Empty;
            }

            if (IsSaveEventData && EventSpawnUse)
            {
                // 이벤트 정보 저장 가능 타입, 이벤트 세팅을 하겠다!
                // Count 와 ActorRotation 값은 인스펙터에서 설정한 값 그대로 사용
                // ActorID, ActorRate 는 리스트 값을 기반으로 배열화하여 저장
                TableData.EventActorID = string.Join(",", EventDatas.Select(value => value.ActorID.ToString()));
                TableData.EventActorRate = string.Join(",", EventDatas.Select(value => value.ActorRate.ToString()));
            }
            else
            {
                // 이벤트 정보 저장하지 않는 타입이거나
                // 이벤트 세팅을 하지 않겠다 선언하면 디폴트 값으로 저장
                TableData.EventSpawnUse = false;
                TableData.EventCount = 0;
                TableData.EventActorRotation = 0;
                TableData.EventActorID = string.Empty;
                TableData.EventActorRate = string.Empty;
            }

            if (!isObstacle)
            {
                HitColliderRadius = 0;
            }

            return base.Save(mapID);
        }

        public void Load(GJson gJson, GTable gTable)
        {
            SetJsonData(gJson, true);
            SetTableData(gTable);

            Position = World.Utility.GetSpawnActorMainPos(TableData);
            World.Utility.GetSpawnActorSize(TableData, out float width, out float height);
            AreaSizeSphere = width;
            AreaSizeCure = new Vector2(width, height);

            if (_actorDatas == null)
                _actorDatas = new List<ActorRateData>();
            _actorDatas.Clear();
            var actorIDs = World.Utility.GetSpawnActorIDList(TableData);
            var actorRates = World.Utility.GetSpawnActorRateList(TableData);
            for (int i = 0; i < actorIDs.Count; i++)
            {
                int rate = 0;
                if (actorRates.Count > i)
                {
                    rate = actorRates[i];
                }
                _actorDatas.Add(new ActorRateData(actorIDs[i], rate));
            }

            if (_eventDatas == null)
                _eventDatas = new List<ActorRateData>();
            _eventDatas.Clear();
            var eventIDs = World.Utility.GetEventActorIDList(TableData);
            var eventRates = World.Utility.GetEventActorRateList(TableData);
            for (int i = 0; i < eventIDs.Count; i++)
            {
                int rate = 0;
                if (eventRates.Count > i)
                {
                    rate = eventRates[i];
                }
                _eventDatas.Add(new ActorRateData(eventIDs[i], rate));
            }
        }

        protected override void CreateTableData()
        {
            base.CreateTableData();
            TableData.ActorInitialSpawn = true;
        }

        public void FindZoneID()
        {
            ZoneID = MapToolModel.Instance.GetZoneID(Position);
        }
    }
}
#endif
