using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

namespace iBatisGlue.Common
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
		public static Dictionary<string, iBatisStatement> Statements = new Dictionary<string, iBatisStatement>();

		public iBatisStatementType StatementType { get; private set; }
		public string Namespace { get; private set; }
		public string ID { get; private set; }
		public XmlNode Node { get; private set; }
		public readonly List<string> Includes = new List<string>();

		public string Result { get; private set; }

		public string FullID
		{
			get { return string.Format("{0}.{1}", this.Namespace, this.ID); }
		}

		public static iBatisStatement ParseNode(XmlNode node, string namespacee)
		{
			if (node.Attributes["id"] == null) return null;
			var statement = new iBatisStatement
			                	{
			                		StatementType = (iBatisStatementType) Enum.Parse(typeof (iBatisStatementType), node.Name, true),
			                		Namespace = namespacee,
			                		ID = node.Attributes["id"].Value,
			                		Node = node,
			                	};

			var nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
			nsmgr.AddNamespace("d", iBatisGlueUtils.XML_MAPPING_NS);

			var includes = node.SelectNodes("d:include[@refid]", nsmgr);
			if (includes != null && includes.Count > 0)
			{
				foreach (XmlNode include in includes)
				{
					var id = include.Attributes["refid"].Value;
					if (!id.Contains("."))
					{
						id = string.Format("{0}.{1}", namespacee, id);
						include.Attributes["refid"].Value = id;
					}
					if (!statement.Includes.Contains(id))
					{
						statement.Includes.Add(id);
					}
				}
			}

			Statements.Add(statement.FullID, statement);
			return statement;
		}


		private static int ProcessStatementListCount = 0;
		private static Regex _fixTags = new Regex("^(\\s*)(.*)><(.*)$", RegexOptions.Multiline | RegexOptions.Compiled);

		public static void ProcessStatementList(IList<string> fileMapList)
		{
			foreach (var fileMap in fileMapList)
				iBatisMapInfo.ParseMap(fileMap);

			iBatisStatement.ProcessStatementList();
		}

		private static void ProcessStatementList()
		{
			ProcessStatementListCount++;
			var reProcess = false;

			foreach (var statement in Statements.Values)
			{
				statement.Result = string.Format("/*{1}*/{2}{0}", statement.Node.InnerXml, statement.FullID, Environment.NewLine);

				foreach (var include in statement.Includes)
				{
					iBatisStatement statement1;
					if (Statements.TryGetValue(include, out statement1))
					{
						if (statement1.Result == null)
						{
							//wait till statement1.Result is processed then try again in another ProcessStatementList() pass
							statement.Result = null;
							reProcess = true;
							break;
						}
						//NOTE: currently sub-includes are not supported
						var regex = string.Format("<include\\s+refid=\"{0}\".*?/>", include);
						statement.Result = Regex.Replace(statement.Result, regex, Environment.NewLine + statement1.Result);
					}
				}

				if (statement.Result != null)
				{
					statement.Result = statement.Result.Replace(iBatisGlueUtils.XML_TO_REMOVE1, string.Empty);
					if (iBatisGlueUtils.FixTagNewLines)
						statement.Result = _fixTags.Replace(statement.Result, "$1$2>\r\n$1<$3");
				}
			}

			if (reProcess && ProcessStatementListCount < 10000)
			{
				ProcessStatementList();
			}
		}
	}
}