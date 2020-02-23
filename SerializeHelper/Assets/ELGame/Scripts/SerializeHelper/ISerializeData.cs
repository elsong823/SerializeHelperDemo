using LitJson;

namespace ELGame
{
    /// <summary>
    /// 实现此接口的类，可以序列化和反序列化：通过LitJson
    /// </summary>
    public interface ISerializeData
    {
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="jsonWriter"></param>
        void Serialize(JsonWriter jsonWriter);

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="jsonReader"></param>
        void Deserialize(JsonReader jsonReader);
    }
}