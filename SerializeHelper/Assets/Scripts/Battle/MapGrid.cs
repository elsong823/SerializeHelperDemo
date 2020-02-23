using LitJson;
using ELGame;

public class MapGrid
    : IRecyclable, ISerializeData
{
    public enum GridType
    {
        Normal,     //普通格子
        Special,    //特殊格子
    }

    public GridType gridType;   //格子类型

    public int index;           //所在地图索引
    public int row;             //所在行
    public int column;          //所在列

    /// <summary>
    /// 创建一个格子
    /// </summary>
    /// <param name="index">在地图上的索引</param>
    /// <param name="row">所在行</param>
    /// <param name="column">所在列</param>
    /// <returns></returns>
    public static MapGrid Create(int index, int row, int column)
    {
        MapGrid mapGrid = ELGame.SingletonRecyclePool<MapGrid>.Get();

        mapGrid.index = index;
        mapGrid.row = row;
        mapGrid.column = column;

        return mapGrid;
    }

    public void OnRecycle()
    {
        gridType = GridType.Normal;
        row = 0;
        column = 0;
        index = 0;
    }

    public void Return()
    {
        ELGame.SingletonRecyclePool<MapGrid>.Return(this);
    }

    public void Serialize(JsonWriter jsonWriter)
    {
        //只记录坐标
        jsonWriter.Write(index);
    }

    public void Deserialize(JsonReader jsonReader)
    {
    }

}