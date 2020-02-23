using System.Collections.Generic;
using LitJson;

namespace ELGame
{
    public static class SerializeHelper
    {
        /// <summary>
        /// 写一个值
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public static void WriteKeyValue(this JsonWriter writer, string propertyName, bool value)
        {
            writer.WritePropertyName(propertyName);
            writer.Write(value);
        }

        /// <summary>
        /// 写一个值
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public static void WriteKeyValue(this JsonWriter writer, string propertyName, int value)
        {
            writer.WritePropertyName(propertyName);
            writer.Write(value);
        }

        /// <summary>
        /// 写一个值
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public static void WriteKeyValue(this JsonWriter writer, string propertyName, double value)
        {
            writer.WritePropertyName(propertyName);
            writer.Write(value);
        }

        /// <summary>
        /// 写一个值
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public static void WriteKeyValue(this JsonWriter writer, string propertyName, string value)
        {
            writer.WritePropertyName(propertyName);
            writer.Write(value);
        }

        /// <summary>
        /// 写一个对象
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="propertyName"></param>
        /// <param name=""></param>
        public static void WriteObject(this JsonWriter writer, string propertyName, ISerializeData serializeData)
        {
            if(serializeData != null)
            {
                writer.WritePropertyName(propertyName);
                writer.WriteObjectStart();
                {
                    serializeData.Serialize(writer);
                }
                writer.WriteObjectEnd();
            }
        }

        /// <summary>
        /// 序列化一个列表
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="propertyName"></param>
        /// <param name="list"></param>
        public static void WriteList<T>(this JsonWriter writer, string propertyName, List<T> list)
            where T : ISerializeData
        {
            if(list != null)
            {
                writer.WritePropertyName(propertyName);
                writer.WriteArrayStart();
                {
                    int count = list.Count;
                    for (int i = 0; i < count; i++)
                    {
                        list[i].Serialize(writer);
                    }
                }
                writer.WriteArrayEnd();
            }
        }

        /// <summary>
        /// 序列化一个数组
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="propertyName"></param>
        /// <param name="array"></param>
        public static void WriteArray<T>(this JsonWriter writer, string propertyName, T[] array)
            where T : ISerializeData
        {
            if (array != null)
            {
                writer.WritePropertyName(propertyName);
                writer.WriteArrayStart();
                {
                    int length = array.Length;
                    for (int i = 0; i < length; i++)
                    {
                        array[i].Serialize(writer);
                    }
                }
                writer.WriteArrayEnd();
            }
        }
    }
}