// ReSharper disable PossibleNullReferenceException

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace iBatisGlue
{
	internal class Parser
	{
		public static Dictionary<string, iBatisStatement> Statements = new Dictionary<string, iBatisStatement>();

		public static void ParseMap(string file)
		{
			var doc = iBatisGlueUtils.LoadXml(file);

			var nsmgr = new XmlNamespaceManager(doc.DocumentElement.OwnerDocument.NameTable);
			nsmgr.AddNamespace("d", iBatisGlueUtils.XML_MAPPING_NS);

			var namespacee = doc.DocumentElement.SelectSingleNode("/d:sqlMap/@namespace", nsmgr).Value;

			var statementNodes = doc.DocumentElement.SelectNodes("/d:sqlMap/d:statements/*", nsmgr);
			foreach(XmlNode node in statementNodes)
			{
				var statement = ParseNode(node, namespacee);
				if(statement == null) continue;
				Statements.Add(statement.FullID, statement);
			}
		}

		static iBatisStatement ParseNode(XmlNode node, string namespacee)
		{
			if(node.Attributes["id"] == null) return null;
			var statement = new iBatisStatement
			{
				StatementType = (iBatisStatementType)Enum.Parse(typeof(iBatisStatementType), node.Name, true),
				Namespace = namespacee,
				ID = node.Attributes["id"].Value,
				Node = node,
			};

			var nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
			nsmgr.AddNamespace("d", iBatisGlueUtils.XML_MAPPING_NS);

			var includes = node.SelectNodes(".//d:include[@refid]", nsmgr);
			if(includes != null && includes.Count > 0)
			{
				foreach(XmlNode include in includes)
				{
					var id = include.Attributes["refid"].Value;
					if(id.Contains(".")) throw new InvalidOperationException(id);
					if(!statement.Includes.ContainsKey(id))
					{
						statement.Includes.Add(id, null);
					}
				}
			}

			return statement;
		}

		static readonly Regex _fixTags = new Regex("^(\\s*)(.*)><(.*)$", RegexOptions.Multiline | RegexOptions.Compiled);

		public static string ProcessStatementList(IList<string> fileMapList, string toFind)
		{
			foreach(var fileMap in fileMapList)
				ParseMap(fileMap);

			var statement = Statements.Values.Where(x => x.ID == toFind).SingleOrDefault();
			if(statement == null) return String.Empty;
			ProcessStatement(statement);
			return statement.Result;
		}

		static void ProcessStatement(iBatisStatement statement)
		{
			statement.Result = String.Format("/*{1}*/{2}{0}", statement.Node.InnerXml, statement.FullID, Environment.NewLine);
			foreach(var include in statement.Includes.Keys.ToList())
			{
				statement.Includes[include] = Statements.Where(x => x.Key.EndsWith("." + include)).Single().Value;
				ProcessStatement(statement.Includes[include]);
			}
			foreach(var pair in statement.Includes)
			{
				var regex = String.Format("<include\\s+refid=\"{0}\".*?/>", pair.Key);
				statement.Result = Regex.Replace(statement.Result, regex, Environment.NewLine + pair.Value.Result);
			}
			statement.Result = statement.Result.Replace(iBatisGlueUtils.XML_TO_REMOVE1, String.Empty);
			if(iBatisGlueUtils.FixTagNewLines)
				statement.Result = _fixTags.Replace(statement.Result, "$1$2>\r\n$1<$3");
			statement.IsProcessed = true;
		}
	}
}