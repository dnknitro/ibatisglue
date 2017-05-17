using System.Xml;

namespace iBatisGlue.Domain
{
	public class iBatisElementIDBase : iBatisElementBase
	{
		public string Namespace { get; set; }
		public string ID { get; set; }

		public string FullID => $"{Namespace}.{ID}";
	}
}