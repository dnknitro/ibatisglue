using System.Collections.Generic;
using System.Xml;

namespace iBatisGlue.Common
{
	internal class iBatisMapInfo
	{
		public string Namespace { get; private set; }
		public readonly IList<iBatisStatement> StatementList = new List<iBatisStatement>();

		public static iBatisMapInfo ParseMap(string file)
		{
			var doc = iBatisGlueUtils.LoadXml(file);

			// ReSharper disable PossibleNullReferenceException
			var nsmgr = new XmlNamespaceManager(doc.DocumentElement.OwnerDocument.NameTable);
			nsmgr.AddNamespace("d", iBatisGlueUtils.XML_MAPPING_NS);

			var namespacee = doc.DocumentElement.SelectSingleNode("/d:sqlMap/@namespace", nsmgr).Value;

			var mapInfo = new iBatisMapInfo
			              	{
			              		Namespace = namespacee,
			              	};

			var statementNodes = doc.DocumentElement.SelectNodes("/d:sqlMap/d:statements/*", nsmgr);
			foreach (XmlNode node in statementNodes)
			{
				var statement = iBatisStatement.ParseNode(node, namespacee);
				if(statement == null) continue;
				mapInfo.StatementList.Add(statement);
			}
			// ReSharper restore PossibleNullReferenceException

			return mapInfo;
		}
	}
}