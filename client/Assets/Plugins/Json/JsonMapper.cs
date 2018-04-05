
namespace LitJson
{
	public class JsonMapper
	{
		public static JsonData ToObject(string json)
		{
			object obj = MiniJSON.Deserialize(json);
			JsonData jsonData = obj == null ? null : new JsonData(obj);
			return jsonData;
		}

		public static string Serialize(object obj)
		{
			return MiniJSON.Serialize(obj);
		}

		public static object Deserialize(string json)
		{
			return MiniJSON.Deserialize(json);
		}

	}
}