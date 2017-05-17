using System.Collections.Generic;

namespace iBatisGlue.Domain
{
	public class iBatisResultMap : iBatisElementIDBase
	{
		public string ExtendsResultMap { get; set; }
		public iBatisResultMap Extends { get; set; }

		//public List<iBatisResultMapResult> Results { get; set; }
	}
}