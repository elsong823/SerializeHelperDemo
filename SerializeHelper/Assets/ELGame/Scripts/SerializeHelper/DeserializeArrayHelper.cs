using System;
using LitJson;

namespace ELGame
{
    /// <summary>
    /// 数组反序列化器，专门用于反序列化数组
    /// </summary>
    public class DeserializeArrayHelper
        : IRecyclable
    {
        //跳过的数量
        private int jumpObjCount = 0;
        private int jumpArrayCount = 0;

        //反序列索引
        private int deserializeIndex = 0;
        //int类型的反序列化回调
        public Action<int, int> IntDeserializeCallback;
        //float类型的反序列化回调
        public Action<int, float> FloatDeserializeCallback;
        //string类型的反序列化回调
        public Action<int, string> StringDeserializeCallback;
        //bool类型的反序列化回调
        public Action<int, bool> BoolDeserializeCallback;
        //对象类型的反序列化回调
        public Action<int, JsonReader> ObjectDeserializeCallback;
        //数组类型的反序列化回调
        public Action<int, JsonReader> ArrayDeserializeCallback;

        /// <summary>
        /// 创建一个可回收的反序列器
        /// </summary>
        /// <returns></returns>
        public static DeserializeArrayHelper Create()
        {
            return SingletonRecyclePool<DeserializeArrayHelper>.Get();
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

                switch (jsonReader.Token)
                {
                    case JsonToken.ArrayEnd:
                        if (autoReturn)
                            SingletonRecyclePool<DeserializeArrayHelper>.Return(this);
                        return;

                    case JsonToken.ObjectStart:
                        //有对象，但是没处理对象？
                        if (ObjectDeserializeCallback == null)
                        {
                            ++jumpObjCount;
                            ++deserializeIndex;
                        }
                        else
                            ObjectDeserializeCallback(deserializeIndex++, jsonReader);
                        break;

                    case JsonToken.ArrayStart:
                        //有数组，但是没处理数组？
                        if (ArrayDeserializeCallback == null)
                        {
                            ++jumpArrayCount;
                            ++deserializeIndex;
                        }
                        else
                            ArrayDeserializeCallback(deserializeIndex++, jsonReader);
                        break;

                    case JsonToken.Int:
                        //没处理数值类型
                        if (IntDeserializeCallback == null)
                        {
                            ++deserializeIndex;
                        }
                        else
                        {
                            int intValue = SerializeConst.INT_INVALID;
                            int.TryParse(jsonReader.Value.ToString(), out intValue);
                            IntDeserializeCallback(deserializeIndex++, intValue);
                        }
                        break;

                    case JsonToken.Double:
                        //没处理数值类型
                        if (FloatDeserializeCallback == null)
                        {
                            ++deserializeIndex;
                        }
                        else
                        {
                            float floatValue = SerializeConst.FLOAT_INVALID;
                            float.TryParse(jsonReader.Value.ToString(), out floatValue);
                            FloatDeserializeCallback(deserializeIndex++, floatValue);
                        }
                        break;

                    case JsonToken.String:
                        //没处理字符串类型
                        if (StringDeserializeCallback == null)
                        {
                            ++deserializeIndex;
                        }
                        else
                        {
                            StringDeserializeCallback(deserializeIndex++, jsonReader.Value.ToString());
                        }
                        break;

                    case JsonToken.Boolean:
                        //没处理bool类型
                        if (BoolDeserializeCallback == null)
                        {
                            ++deserializeIndex;
                        }
                        else
                        {
                            bool boolValue = false;
                            bool.TryParse(jsonReader.Value.ToString(), out boolValue);
                            BoolDeserializeCallback(deserializeIndex++, boolValue);
                        }
                        break;

                    default:
                        break;
                }
            }

            if (autoReturn)
                Return();
        }

        #region IRecyclable
        public void OnRecycle()
        {
            IntDeserializeCallback = null;
            StringDeserializeCallback = null;
            BoolDeserializeCallback = null;
            ObjectDeserializeCallback = null;
            ArrayDeserializeCallback = null;
            deserializeIndex = 0;
            jumpObjCount = 0;
            jumpArrayCount = 0;
        }

        public void Return()
        {
            SingletonRecyclePool<DeserializeArrayHelper>.Return(this);
        }
        #endregion
    }
}