using System.IO;
using ELGame;
using UnityEngine;
using UnityEngine.UI;
using LitJson;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Demo))]
public class DemoEditor
        : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
#if UNITY_STANDALONE_WIN
        if (GUILayout.Button("打开输出目录..."))
        {
            if(target is Demo)
            {
                Demo demo = target as Demo;
                if(demo != null)
                {
                    string folderPath = Path.GetDirectoryName(demo.serializeOutputPath);
                    if (!Directory.Exists(folderPath))
                    {
                        Debug.LogErrorFormat("Open local folder failed. => {0} not exist.", folderPath);
                        return;
                    }
                    System.Diagnostics.Process.Start("explorer.exe", folderPath);
                }
            }
        }
#endif
    }
}
#endif

[System.Serializable]
public class Apple
{
    [System.NonSerialized]
    public string color;
    public int Id;
}

public class Demo
    : MonoBehaviour
{
    BattleField battleField;

    [SerializeField] Button btnSerializeBattleField;
    [SerializeField] Button btnDeserializeBattleField;
    [HideInInspector] public string serializeOutputPath;
    [HideInInspector] public string serializeOutputPath_mirror;

    private void Awake()
    {
        //随机种子
        int seed = Random.Range(0, 10000) ^ System.DateTime.Now.Millisecond;
        Random.InitState(seed);

        btnSerializeBattleField.onClick.AddListener(SerializeBattleField);
        btnDeserializeBattleField.onClick.AddListener(DeserializeBattleField);
    }

    private void Start()
    {
        serializeOutputPath = string.Format("{0}/../Output/battlefield.json", Application.dataPath);
        serializeOutputPath_mirror = string.Format("{0}/../Output/battlefield_mirror.json", Application.dataPath);
    }

    /// <summary>
    /// 序列化一场战斗
    /// </summary>
    private void SerializeBattleField()
    {
        //先创建一场战斗
        if (battleField != null)
            battleField.Return();

        //创建一场战斗，规定地图尺寸和参战单位的数量
        int randomRowwwww = Random.Range(10, 15);
        int randomColumnnnn = Random.Range(10, 15);
        int randomUnitAmount = Random.Range(2, 5);
        battleField = BattleField.Create(randomRowwwww, randomColumnnnn, randomUnitAmount);
        SerializeBattleFieldToPath(serializeOutputPath);
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
        }
    }

    /// <summary>
    /// 反序列化一场战斗
    /// </summary>
    private void DeserializeBattleField()
    {
        if(!File.Exists(serializeOutputPath))
        {
            EditorUtility.DisplayDialog("错误", "找不到战斗序列化文件，请先序列化一场战斗...", "得嘞");
            return;
        }
        battleField?.Return();
        battleField = BattleField.Create();
        using (TextReader rt = new StreamReader(serializeOutputPath))
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
            SerializeBattleFieldToPath(serializeOutputPath_mirror);
    }
}
