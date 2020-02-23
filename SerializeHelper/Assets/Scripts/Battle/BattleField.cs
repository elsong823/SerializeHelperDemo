using System.Collections.Generic;
using LitJson;
using ELGame;
using System;

public class BattleField
    :ELGame.IRecyclable, ELGame.ISerializeData
{
    public BattleMap battleMap;                 //地图
    public List<BattleUnit> allBattleUnits;     //所有战斗单位

    /// <summary>
    /// 创建一个空战斗
    /// </summary>
    /// <returns></returns>
    public static BattleField Create()
    {
        BattleField battleField = ELGame.SingletonRecyclePool<BattleField>.Get();
        return battleField;
    }

    /// <summary>
    /// 创建一场战斗
    /// </summary>
    /// <param name="mapRow">地图行</param>
    /// <param name="mapColumn">地图列</param>
    /// <param name="battleUnitAmount">战斗单位数量</param>
    /// <returns></returns>
    public static BattleField Create(int mapRow, int mapColumn, int battleUnitAmount)
    {
        BattleField battleField = ELGame.SingletonRecyclePool<BattleField>.Get();

        //创建一个地图呢
        battleField.battleMap = BattleMap.Create(mapRow, mapColumn);

        //添加战斗单位
        if (battleField.allBattleUnits == null)
            battleField.allBattleUnits = new List<BattleUnit>(battleUnitAmount);

        for (int i = 0; i < battleUnitAmount; i++)
        {
            battleField.allBattleUnits.Add(BattleUnit.Create(i));
        }

        return battleField;
    }

    /// <summary>
    /// 移除所有战斗单位
    /// </summary>
    private void RemoveAllBattleUntis()
    {
        if (allBattleUnits != null)
        {
            int amount = allBattleUnits.Count;
            for (int i = 0; i < amount; i++)
            {
                allBattleUnits[i].Return();
            }
            allBattleUnits.Clear();
        }
    }

    public void OnRecycle()
    {
        if(battleMap != null)
        {
            battleMap.Return();
            battleMap = null;
        }
        RemoveAllBattleUntis();
    }

    public void Return()
    {
        ELGame.SingletonRecyclePool<BattleField>.Return(this);
    }

    public void Serialize(JsonWriter jsonWriter)
    {
        if (battleMap != null)
            jsonWriter.WriteObject("battleMap", battleMap);

        if (allBattleUnits != null)
            jsonWriter.WriteList("battleUnits", allBattleUnits);
    }

    public void Deserialize(JsonReader jsonReader)
    {
        if (jsonReader == null)
            return;

        DeserializeHelper dh = DeserializeHelper.Create();
        //反序列化对象（暂时只有地图）
        dh.ObjectDeserializeCallback = ObjectDeserialize;
        dh.ArrayDeserializeCallback = ArrayDeserialize;
        dh.Deserialize(jsonReader, true);
    }

    /// <summary>
    /// 反序列化数组、列表
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="jsonReader"></param>
    /// <returns></returns>
    private bool ArrayDeserialize(string propertyName, JsonReader jsonReader)
    {
        if (propertyName == "battleUnits")
        {
            //先移除所有战斗单位
            RemoveAllBattleUntis();

            allBattleUnits = allBattleUnits ?? new List<BattleUnit>();

            DeserializeArrayHelper dah = DeserializeArrayHelper.Create();

            //这个数组的每个对象都是obj
            dah.ObjectDeserializeCallback = delegate (int index, JsonReader jr)
            {
                //反序列化每个战斗单位
                BattleUnit battleUnit = SingletonRecyclePool<BattleUnit>.Get();
                battleUnit.Deserialize(jr);
                allBattleUnits.Add(battleUnit);
            };

            dah.Deserialize(jsonReader, true);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 反序列化对象
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="jsonReader"></param>
    /// <returns></returns>
    private bool ObjectDeserialize(string propertyName, JsonReader jsonReader)
    {
        if(propertyName == "battleMap")
        {
            if (battleMap != null)
                battleMap.Return();
            //重新创建地图
            battleMap = SingletonRecyclePool<BattleMap>.Get();
            battleMap.Deserialize(jsonReader);
            return true;
        }
        return false;
    }
}
