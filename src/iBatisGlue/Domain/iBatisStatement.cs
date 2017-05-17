using System.Collections.Generic;

namespace iBatisGlue.Domain
{
	public enum iBatisStatementType
	{
		Sql,
		Select,
		Insert,
		Update,
		Delete,
		Statement
	}

	public class iBatisStatement : iBatisElementIDBase
	{
		public iBatisStatementType StatementType { get; set; }
		public readonly Dictionary<string, iBatisStatement> Includes = new Dictionary<string, iBatisStatement>();
	}
}