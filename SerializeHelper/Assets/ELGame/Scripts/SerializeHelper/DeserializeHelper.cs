using System;
using LitJson;
using UnityEngine;

namespace ELGame
{
    /// <summary>
    /// 反序列化器，每个里面包含一套对各种类型数据的解析器
    /// </summary>
    public class DeserializeHelper
        : IRecyclable
    {
        private int jumpObjCount = 0;
        private int jumpArrayCount = 0;
        
        //int类型的反序列化回调
        public Action<string, int> IntDeserializeCallback;
        //float类型的反序列化回调
        public Action<string, float> FloatDeserializeCallback;
        //string类型的反序列化回调
        public Action<string, string> StringDeserializeCallback;
        //bool类型的反序列化回调
        public Action<string, bool> BoolDeserializeCallback;
        //对象类型的反序列化回调
        public Func<string, JsonReader, bool> ObjectDeserializeCallback;
        //数组类型的反序列化回调
        public Func<string, JsonReader, bool> ArrayDeserializeCallback;
        
        /// <summary>
        /// 创建一个可回收的反序列器
        /// </summary>
        /// <returns></returns>
        public static DeserializeHelper Create()
        {
            return SingletonRecyclePool<DeserializeHelper>.Get();
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="jsonReader"></param>
        /// <param name="autoReturn">反序列化后自动归还？</param>
        public void Deserialize(JsonReader jsonReader, bool autoReturn)
        {
            if (jsonReader == null)
            {
                if (autoReturn)
                    Return();
                return;
            }

            while (jsonReader.Read())
            {
                //当前有需要跳过的内容
                //没有被处理的object和array，都会被自动跳过
                if (jumpObjCount > 0 || jumpArrayCount > 0)
                {
                    switch (jsonReader.Token)
                    {
                        case JsonToken.ObjectStart:
                            ++jumpObjCount;
                            break;
                        case JsonToken.ObjectEnd:
                            --jumpObjCount;
                            break;
                        case JsonToken.ArrayStart:
                            ++jumpArrayCount;
                            break;
                        case JsonToken.ArrayEnd:
                            --jumpArrayCount;
                            break;

                        default:
                            break;
                    }
                    continue;
                }

                //反序列化结束
                if (jsonReader.Token == JsonToken.ArrayEnd || jsonReader.Token == JsonToken.ObjectEnd)
                {
                    if (autoReturn)
                        Return();
                    return;
                }

                if (jsonReader.Token == JsonToken.PropertyName)
                {
                    //记录下这个属性名
                    string propertyName = jsonReader.Value.ToString();

                    //读取一下值
                    jsonReader.Read();

                    switch (jsonReader.Token)
                    {
                        case JsonToken.Int:
                            //反序列化
                            if (IntDeserializeCallback != null)
                            {
                                int intValue = SerializeConst.INT_INVALID;
                                int.TryParse(jsonReader.Value.ToString(), out intValue);
                                IntDeserializeCallback(propertyName, intValue);
                            }
                            break;

                        case JsonToken.Double:
                            //反序列化
                            if (FloatDeserializeCallback != null)
                            {
                                float floatValue = SerializeConst.FLOAT_INVALID;
                                float.TryParse(jsonReader.Value.ToString(), out floatValue);
                                FloatDeserializeCallback(propertyName, floatValue);
                            }
                            break;

                        case JsonToken.String:
                            //反序列化
                            StringDeserializeCallback?.Invoke(propertyName, jsonReader.Value.ToString());
                            break;

                        case JsonToken.Boolean:
                            //反序列化
                            if (BoolDeserializeCallback != null)
                            {
                                bool boolValue = false;
                                bool.TryParse(jsonReader.Value.ToString(), out boolValue);
                                BoolDeserializeCallback(propertyName, boolValue);
                            }
                            break;

                        case JsonToken.ObjectStart:
                            if (ObjectDeserializeCallback == null || !ObjectDeserializeCallback(propertyName, jsonReader))
                            {
                                ++jumpObjCount;
                                Debug.LogWarningFormat("由于没有设置对 对象：{0}的反序列化，因此跳过整个对象！", propertyName);
                            }
                            break;

                        case JsonToken.ArrayStart:
                            if (ArrayDeserializeCallback == null || !ArrayDeserializeCallback(propertyName, jsonReader))
                            {
                                ++jumpArrayCount;
                                Debug.LogWarningFormat("由于没有设置对 数组：{0}的反序列化，因此跳过整个数组！", propertyName);
                            }
                            break;

                        default:
                            break;
                    }
                }
            }

            //反序列化完毕后是否自动归还
            if (autoReturn)
                Return();
        }

        #region IRecyclable
        public void OnRecycle()
        {
            IntDeserializeCallback = null;
            FloatDeserializeCallback = null;
            StringDeserializeCallback = null;
            BoolDeserializeCallback = null;
            ObjectDeserializeCallback = null;
            ArrayDeserializeCallback = null;
            jumpObjCount = 0;
            jumpArrayCount = 0;
        }

        public void Return()
        {
            SingletonRecyclePool<DeserializeHelper>.Return(this);
        }
        #endregion
    }
}