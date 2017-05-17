using System.Text;
using System.Xml;

namespace iBatisGlue.Domain
{
	public class iBatisElementBase
	{
		public XmlNode Node { get; set; }

		public StringBuilder Result { get; set; }
		public bool IsProcessed { get; set; }

		public string FromFile { get; set; }
	}
}