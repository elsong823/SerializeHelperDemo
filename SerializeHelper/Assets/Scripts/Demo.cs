using System.IO;
using ELGame;
using UnityEngine;
using UnityEngine.UI;
using LitJson;

public class Demo
    : MonoBehaviour
{
    //来序列化和反序列化的方式
    public enum Type
    {
        /// <summary>
        /// 使用JsonWriter和JsonReader
        /// </summary>
        ReaderAndWriter,
        /// <summary>
        /// 使用JsonMapper
        /// </summary>
        Mapper,
    }

    BattleField battleField;

    public Type workType;
    [SerializeField] Button btnSerializeBattleField;
    [SerializeField] Button btnDeserializeBattleField;

    /// <summary>
    /// JsonWriter序列化导出
    /// </summary>
    public string outputPath
    {
        get
        {
            return string.Format("{0}/../Output/battlefield.json", Application.dataPath);
        }
    }

    /// <summary>
    /// JsonReader反序列化后再序列化导出
    /// </summary>
    public string mirrorOutputPath
    {
        get
        {
            return string.Format("{0}/../Output/battlefield_mirror.json", Application.dataPath);
        }
    }

    /// <summary>
    /// JsonMapper序列化导出
    /// </summary>
    public string mapperOutputPath
    {
        get
        {
            return string.Format("{0}/../Output/battlefield_mapper.json", Application.dataPath);
        }
    }

    /// <summary>
    /// JsonMapper反序列化后再序列化导出
    /// </summary>
    public string mapperMirrorOutputPath
    {
        get
        {
            return string.Format("{0}/../Output/battlefield_mirror_mapper.json", Application.dataPath);
        }
    }

    private void Awake()
    {
        //随机种子
        int seed = Random.Range(0, 10000) ^ System.DateTime.Now.Millisecond;
        Random.InitState(seed);

        btnSerializeBattleField.onClick.AddListener(OnClickedSerialize);
        btnDeserializeBattleField.onClick.AddListener(OnClickedDeserialize);
    }

    private void OnClickedSerialize()
    {
        switch (workType)
        {
            case Type.ReaderAndWriter:
                SerializeBattleField();
                break;
            case Type.Mapper:
                SerializeBattleFieldByJsonMapper();
                break;
            default:
                break;
        }
    }

    private void OnClickedDeserialize()
    {
        switch (workType)
        {
            case Type.ReaderAndWriter:
                DeserializeBattleField();
                break;
            case Type.Mapper:
                DeserializeBattleFieldByJsonMapper();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 创建一场战斗
    /// </summary>
    private void CreateBattle()
    {
        //先创建一场战斗
        if (battleField != null)
            battleField.Return();

        //创建一场战斗，规定地图尺寸和参战单位的数量
        int mapRow = Random.Range(10, 15);
        int mapColumnnnn = Random.Range(10, 15);
        int randomUnitAmount = Random.Range(2, 5);

        //创建战场（战斗）
        battleField = BattleField.Create(
            mapRow, 
            mapColumnnnn, 
            randomUnitAmount);
    }

    #region 使用Reader、Writer序列化反序列化
    /// <summary>
    /// 序列化一场战斗
    /// </summary>
    private void SerializeBattleField()
    {
        CreateBattle();
        SerializeBattleFieldToPath(outputPath);
    }

    /// <summary>
    /// 将一场战斗序列化到路径...
    /// </summary>
    /// <param name="outputPath"></param>
    private void SerializeBattleFieldToPath(string outputPath)
    {
        string folderPath = Path.GetDirectoryName(outputPath);

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        using (TextWriter tw = new StreamWriter(outputPath))
        {
            LitJson.JsonWriter jsonWriter = new LitJson.JsonWriter(tw);
#if UNITY_EDITOR
            //弄得好看一点
            jsonWriter.PrettyPrint = true;
#endif
            jsonWriter.WriteObjectStart();
            {
                //序列化战斗
                jsonWriter.WriteObject("battleField", battleField);
            }
            jsonWriter.WriteObjectEnd();
            Debug.LogFormat("序列化战场完毕 => {0}", outputPath);

            System.Diagnostics.Process.Start("explorer.exe", folderPath);
        }
    }

    /// <summary>
    /// 反序列化一场战斗
    /// </summary>
    private void DeserializeBattleField()
    {
        if(!File.Exists(outputPath))
        {
            UnityEditor.EditorUtility.DisplayDialog("错误", "找不到战斗序列化文件，请先序列化一场战斗...", "得嘞");
            return;
        }
        battleField?.Return();
        battleField = BattleField.Create();
        using (TextReader rt = new StreamReader(outputPath))
        {
            LitJson.JsonReader jsonReader = new LitJson.JsonReader(rt);
            var dh = DeserializeHelper.Create();
            dh.ObjectDeserializeCallback = delegate (string propertyName, JsonReader reader)
            {
                if (propertyName == "battleField")
                {
                    battleField.Deserialize(reader);
                    return true;
                }
                return false;
            };
            //开始反序列化
            dh.Deserialize(jsonReader, true);
        }

        Debug.Log("反序列化完成");

        //对反序列化战场进行序列化
        if (battleField != null)
            SerializeBattleFieldToPath(mirrorOutputPath);
    }
    #endregion

    #region 使用Mapper序列化反序列化
    private void SerializeBattleFieldByJsonMapper()
    {
        CreateBattle();
        SerializeBattleFieldByJsonMapperToPath(mapperOutputPath);
    }

    /// <summary>
    /// 使用mapper序列化一场战斗
    /// </summary>
    private void SerializeBattleFieldByJsonMapperToPath(string outputPath)
    {
        string folderPath = Path.GetDirectoryName(outputPath);

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        using (TextWriter tw = new StreamWriter(outputPath))
        {
            LitJson.JsonWriter jsonWriter = new LitJson.JsonWriter(tw);
#if UNITY_EDITOR
            //弄得好看一点
            jsonWriter.PrettyPrint = true;
#endif
            JsonMapper.ToJson(battleField, jsonWriter);

            Debug.LogFormat("序列化战场完毕 => {0}", outputPath);

            System.Diagnostics.Process.Start("explorer.exe", folderPath);
        }
    }

    /// <summary>
    /// 使用mapper反序列化一场战斗
    /// </summary>
    private void DeserializeBattleFieldByJsonMapper()
    {
        if (!File.Exists(mapperOutputPath))
        {
            UnityEditor.EditorUtility.DisplayDialog("错误", "找不到战斗序列化文件，请先序列化一场战斗...", "得嘞");
            return;
        }
        battleField?.Return();
        battleField = JsonMapper.ToObject<BattleField>(File.ReadAllText(mapperOutputPath));

        Debug.Log("反序列化完成");

        //对反序列化战场进行序列化
        if (battleField != null)
        {
            SerializeBattleFieldByJsonMapperToPath(mapperMirrorOutputPath);
        }
    }
    #endregion
}
