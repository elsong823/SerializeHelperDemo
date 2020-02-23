using System.Collections.Generic;
using LitJson;
using ELGame;
using System;
using UnityEngine;

public class BattleMap
    :IRecyclable, ISerializeData
{
    public int mapRow;
    public int mapColumn;

    public List<MapGrid> allGrids;

    /// <summary>
    /// 创建一张地图
    /// </summary>
    /// <param name="mapRow">行数</param>
    /// <param name="mapColumn">列数</param>
    /// <returns></returns>
    public static BattleMap Create(int mapRow, int mapColumn)
    {
        BattleMap battleMap = ELGame.SingletonRecyclePool<BattleMap>.Get();

        battleMap.mapRow = mapRow;
        battleMap.mapColumn = mapColumn;

        battleMap.Setup();
        battleMap.RandomGridType();

        return battleMap;
    }

    /// <summary>
    /// 生成地图，在设置行列后调用
    /// </summary>
    private void Setup()
    {
        RemoveAllGrids();

        if (allGrids == null)
            allGrids = new List<MapGrid>(mapRow * mapColumn);

        //行
        for (int r = 0; r < mapRow; r++)
        {
            //列
            for (int c = 0; c < mapColumn; c++)
            {
                allGrids.Add(MapGrid.Create(allGrids.Count, r, c));
            }
        }
    }

    /// <summary>
    /// 随机设置一些格子的类型
    /// </summary>
    private void RandomGridType()
    {
        if (allGrids == null)
            return;

        int gridAmount = allGrids.Count;
        for (int i = 0; i < gridAmount; i++)
        {
            //一定概率生成特殊格子
            allGrids[i].gridType = UnityEngine.Random.Range(0, 100) >= 95 ? MapGrid.GridType.Special : MapGrid.GridType.Normal;
        }
    }

    /// <summary>
    /// 移除所有格子
    /// </summary>
    private void RemoveAllGrids()
    {
        if (allGrids != null)
        {
            int amount = allGrids.Count;
            for (int i = 0; i < amount; i++)
            {
                allGrids[i].Return();
            }
            allGrids.Clear();
        }
    }

    public void OnRecycle()
    {
        RemoveAllGrids();
        mapRow = 0;
        mapColumn = 0;
    }

    public void Return()
    {
        ELGame.SingletonRecyclePool<BattleMap>.Return(this);
    }
    
    public void Serialize(JsonWriter jsonWriter)
    {
        //保存地图行、列数
        jsonWriter.WriteKeyValue("mapRow", mapRow);
        jsonWriter.WriteKeyValue("mapColumn", mapColumn);

        //只记录特殊格子
        List<MapGrid> specialGrids = new List<MapGrid>();
        int allGridsAmount = allGrids.Count;
        for (int i = 0; i < allGridsAmount; i++)
        {
            if (allGrids[i].gridType == MapGrid.GridType.Special)
                specialGrids.Add(allGrids[i]);
        }
        if(specialGrids.Count > 0)
        {
            jsonWriter.WriteList("specialGrids", specialGrids);
        }
    }

    public void Deserialize(JsonReader jsonReader)
    {
        if (jsonReader == null)
            return;

        DeserializeHelper dh = DeserializeHelper.Create();
        dh.IntDeserializeCallback = IntDeserialize;
        dh.ArrayDeserializeCallback = ArrayDeserialize;
        dh.Deserialize(jsonReader, true);
    }

    /// <summary>
    /// 数组
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="jsonReader"></param>
    /// <returns></returns>
    private bool ArrayDeserialize(string propertyName, JsonReader jsonReader)
    {
        if (jsonReader == null)
            return false;

        if(propertyName == "specialGrids")
        {
            DeserializeArrayHelper dah = DeserializeArrayHelper.Create();

            dah.IntDeserializeCallback = delegate (int index, int intValue)
            {
                if(intValue >= allGrids.Count)
                {
                    Debug.LogErrorFormat("反序列化特殊格子失败，index {0} 超过最大格子数量, {1}", intValue, allGrids.Count);
                    return;
                }
                allGrids[intValue].gridType = MapGrid.GridType.Special;
            };
            dah.Deserialize(jsonReader, true);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 反序列化为int
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="intValue"></param>
    private void IntDeserialize(string propertyName, int intValue)
    {
        switch (propertyName)
        {
            case "mapRow":
                mapRow = intValue;
                break;

            case "mapColumn":
                mapColumn = intValue;
                //到这里时重建地图
                Setup();
                break;
                
            default:
                break;
        }
    }
}
