
using ELGame;
using LitJson;

public class BattleUnit
    : ELGame.IRecyclable, ELGame.ISerializeData
{
    public int id;

    public int atk;     //攻击力
    public int hp;      //当前生命
    public int maxHp;   //最大生命

    public void OnRecycle()
    {
        id = 0;
        atk = 0;
        hp = 0;
        maxHp = 0;
    }

    public void Return()
    {
        ELGame.SingletonRecyclePool<BattleUnit>.Return(this);
    }

    public void Serialize(JsonWriter jsonWriter)
    {
        jsonWriter.WriteObjectStart();
        {
            jsonWriter.WriteKeyValue("id", id);
            jsonWriter.WriteKeyValue("atk", atk);
            jsonWriter.WriteKeyValue("hp", hp);
            jsonWriter.WriteKeyValue("maxHp", maxHp);
        }
        jsonWriter.WriteObjectEnd();
    }

    public void Deserialize(JsonReader jsonReader)
    {
        if (jsonReader == null)
            return;

        DeserializeHelper dh = DeserializeHelper.Create();
        dh.IntDeserializeCallback = IntDeserialize;
        dh.Deserialize(jsonReader, true);
    }

    /// <summary>
    /// 反序列化Int值
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="intValue"></param>
    private void IntDeserialize(string propertyName, int intValue)
    {
        switch (propertyName)
        {
            case "id":
                id = intValue;
                break;

            case "atk":
                atk = intValue;
                break;

            case "hp":
                hp = intValue;
                break;

            case "maxHp":
                maxHp = intValue;
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// 创建一个战斗单位
    /// </summary>
    /// <returns></returns>
    public static BattleUnit Create(int id)
    {
        BattleUnit battleUnit = ELGame.SingletonRecyclePool<BattleUnit>.Get();

        battleUnit.id = id;
        battleUnit.atk = UnityEngine.Random.Range(10, 20);
        battleUnit.maxHp = UnityEngine.Random.Range(100, 150);
        battleUnit.hp = UnityEngine.Random.Range(1, battleUnit.maxHp + 1);

        return battleUnit;
    }
}
