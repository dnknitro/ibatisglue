using System.Collections.Generic;
using System.Xml;

namespace iBatisGlue
{
	internal enum iBatisStatementType
	{
		Sql,
		Select,
		Insert,
		Update,
		Delete,
		Statement
	}

	internal class iBatisStatement
	{
		public iBatisStatementType StatementType { get; set; }
		public string Namespace { get; set; }
		public string ID { get; set; }
		public XmlNode Node { get; set; }
		public readonly Dictionary<string, iBatisStatement> Includes = new Dictionary<string, iBatisStatement>();
		public bool IsProcessed { get; set; }

		public string Result { get; set; }

		public string FullID
		{
			get
			{
				return string.Format("{0}.{1}", this.Namespace, this.ID);
			}
		}
	}
}