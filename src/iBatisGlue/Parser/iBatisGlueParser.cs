using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using iBatisGlue.Domain;

// ReSharper disable PossibleNullReferenceException

namespace iBatisGlue.Parser
{
	public class iBatisGlueParser
	{
		public static Dictionary<string, iBatisStatement> Statements { get; private set; }
		public static Dictionary<string, iBatisResultMap> ResultMaps { get; private set; }

		private static void ParseMap(string file)
		{
			var doc = iBatisGlueUtils.LoadXml(file);

			var nsmgr = new XmlNamespaceManager(doc.DocumentElement.OwnerDocument.NameTable);
			nsmgr.AddNamespace("d", iBatisGlueUtils.XML_MAPPING_NS);

			var namespacee = doc.DocumentElement.SelectSingleNode("/d:sqlMap/@namespace", nsmgr).Value;


			var statementNodes = doc.DocumentElement.SelectNodes("/d:sqlMap/d:statements/*", nsmgr);
			foreach (XmlNode node in statementNodes)
			{
				var statement = ParseStatementNode(node, namespacee);
				if (statement == null) continue;
				if (Statements.ContainsKey(statement.FullID))
					throw new ArgumentException($"Statement '{statement.FullID}' was already added to Statements (From {Statements[statement.FullID].FromFile}). Error in {file}");
				Statements.Add(statement.FullID, statement);
				statement.FromFile = file;
			}


			var resultMapNodes = doc.DocumentElement.SelectNodes("/d:sqlMap/d:resultMaps/*", nsmgr);
			foreach (XmlNode node in resultMapNodes)
			{
				var resultMap = ParseResultMapNode(node, namespacee);
				if (resultMap == null) continue;
				if (ResultMaps.ContainsKey(resultMap.FullID))
					throw new ArgumentException($"Result map '{resultMap.FullID}' was already added to ResultMaps (From {ResultMaps[resultMap.FullID].FromFile}). Error in {file}");
				ResultMaps.Add(resultMap.FullID, resultMap);
				resultMap.FromFile = file;
			}
		}

		private static iBatisStatement ParseStatementNode(XmlNode node, string namespacee)
		{
			if (node.Attributes["id"] == null) return null;
			var statement = new iBatisStatement
			{
				StatementType = (iBatisStatementType) Enum.Parse(typeof(iBatisStatementType), node.Name, true),
				Namespace = namespacee,
				ID = node.Attributes["id"].Value,
				Node = node,
			};

			var nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
			nsmgr.AddNamespace("d", iBatisGlueUtils.XML_MAPPING_NS);

			var includes = node.SelectNodes(".//d:include[@refid]", nsmgr);
			if (includes != null && includes.Count > 0)
			{
				foreach (XmlNode include in includes)
				{
					var refid = GetFullID(statement, include.Attributes["refid"].Value);
					//var match = Regex.Match(refid, @"^(.+)?\.(.+)$");
					//string idNamespace = null;
					//if (match.Groups[1].Success)
					//{
					//	idNamespace = match.Groups[1].Value;
					//}
					//var id = match.Groups[2].Value;
					//if(id.Contains(".")) throw new InvalidOperationException($"Invalid refid '{id}'");
					if (!statement.Includes.ContainsKey(refid))
					{
						statement.Includes.Add(refid, null);
					}
				}
			}

			return statement;
		}

		private static iBatisResultMap ParseResultMapNode(XmlNode node, string namespacee)
		{
			if (node.Attributes["id"] == null) return null;
			var resultMap = new iBatisResultMap
			{
				//StatementType = (iBatisStatementType) Enum.Parse(typeof(iBatisStatementType), node.Name, true),
				Namespace = namespacee,
				ID = node.Attributes["id"].Value,
				Node = node,
			};

			var extends = node.Attributes["extends"];
			if (extends != null)
				resultMap.ExtendsResultMap = extends.Value;

			var nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
			nsmgr.AddNamespace("d", iBatisGlueUtils.XML_MAPPING_NS);

			return resultMap;
		}

		private static readonly Regex _fixTags = new Regex("^(\\s*)(.*)><(.*)$", RegexOptions.Multiline | RegexOptions.Compiled);

		public static string GetResult(IList<string> fileMapList, string toFind)
		{
			if (Statements == null)
			{
				Statements = new Dictionary<string, iBatisStatement>();
				ResultMaps = new Dictionary<string, iBatisResultMap>();

				foreach (var fileMap in fileMapList)
					ParseMap(fileMap);
			}

			var statement = Statements.Values.SingleOrDefault(x => x.ID == toFind);
			if (statement != null)
			{
				ProcessStatement(statement);
				return statement.Result.ToString();
			}

			var resultMap = ResultMaps.Values.SingleOrDefault(x => x.ID == toFind);
			if (resultMap != null)
			{
				ProcessResultMap(resultMap);
				return resultMap.Result.ToString();
			}

			return string.Empty;
		}

		private static void ProcessStatement(iBatisStatement statement)
		{
			if (statement.IsProcessed) return;
			var result = $"/*{statement.FullID}*/{Environment.NewLine}{statement.Node.InnerXml}";
			foreach (var include in statement.Includes.Keys.ToList())
			{
				if(!Statements.ContainsKey(include))
					throw new KeyNotFoundException($"'{include}' key no found in Statements");
				statement.Includes[include] = Statements[include];
				ProcessStatement(statement.Includes[include]);
			}
			foreach (var pair in statement.Includes)
			{
				var regex = string.Format("<include\\s+refid=\"{0}\".*?/>", pair.Key);
				result = Regex.Replace(result, regex, Environment.NewLine + pair.Value.Result);
			}
			result = result.Replace(iBatisGlueUtils.XML_TO_REMOVE1, string.Empty);
			result = _fixTags.Replace(result, "$1$2>\r\n$1<$3");

			statement.Result = new StringBuilder(result);
			statement.IsProcessed = true;
		}

		private static void ProcessResultMap(iBatisResultMap resultMap)
		{
			if (resultMap.IsProcessed) return;
			//Console.WriteLine($"ProcessResultMap {resultMap.FullID}");
			var result = $"<!--{resultMap.FullID}-->{Environment.NewLine}{resultMap.Node.OuterXml}<!--END OF {resultMap.FullID}-->".Replace(iBatisGlueUtils.XML_TO_REMOVE1, string.Empty);
			resultMap.Result = new StringBuilder( _fixTags.Replace(result, "$1$2>\r\n$1<$3") );
			resultMap.Result.Replace("<result ", $"{Environment.NewLine}	<result ");
			resultMap.Result.Replace("<constructor>", $"{Environment.NewLine}	<constructor>");
			resultMap.Result.Replace("</constructor>", $"{Environment.NewLine}	</constructor>");
			resultMap.Result.Replace("<argument ", $"{Environment.NewLine}		<argument ");
			resultMap.Result.Replace("</resultMap>", $"{Environment.NewLine}</resultMap>");

			var toReplace = new Dictionary<string, string>();
			string fullID = null;
			Func<string> counterKey = () => $"____statement____{fullID}____";
			var r = new Regex(@"^(\s*)<.+?select=['""](.+?)['""].*?>.*?$", RegexOptions.Multiline);
			for (;;)
			{
				//counter++;
				var match = r.Match(resultMap.Result.ToString());
				if (!match.Success) break;
				fullID = GetFullID(resultMap, match.Groups[2].Value);
				var fullMatch = match.Value.Replace("\r", "").Replace("\n", "");
				var statement = Statements[fullID];
				ProcessStatement(statement);
				var tempString = counterKey();
				resultMap.Result.Replace(fullMatch, tempString, match.Index, fullMatch.Length);
				toReplace[tempString] = $"{fullMatch}{Environment.NewLine}{PadLeft(match.Groups[1].Value + "	", statement.Result.ToString())}";
			}

			r = new Regex(@"^(\s*)<.+?resultMapping=['""](.+?)['""].*?>.*?$", RegexOptions.Multiline);
			for (;;)
			{
				//counter++;
				var match = r.Match(resultMap.Result.ToString());
				if (!match.Success) break;
				var fullMatch = match.Value.Replace("\r", "").Replace("\n", "");
				fullID = GetFullID(resultMap, match.Groups[2].Value);
				if(!ResultMaps.ContainsKey(fullID))
					throw new ArgumentException($"{fullID} key is not present in ResultMaps");
				var statement = ResultMaps[fullID];
				ProcessResultMap(statement);
				var tempString = counterKey();
				resultMap.Result.Replace(fullMatch, tempString, match.Index, fullMatch.Length);
				toReplace[tempString] = $"{fullMatch}{Environment.NewLine}{PadLeft(match.Groups[1].Value + "	", statement.Result.ToString())}";
			}

			//Console.WriteLine($"ProcessResultMap {resultMap.FullID} - continue");

			foreach( var pair in toReplace )
			{
				//Console.WriteLine($"	Replacing {pair.Key}");
				resultMap.Result.Replace( pair.Key, pair.Value );
			}


			if (!string.IsNullOrEmpty(resultMap.ExtendsResultMap))
			{
				var fullExtendsResultMapName = GetFullID(resultMap, resultMap.ExtendsResultMap);
				if (!ResultMaps.ContainsKey(fullExtendsResultMapName))
					throw new KeyNotFoundException($"'{fullExtendsResultMapName}' key no found in ResultMaps");
				resultMap.Extends = ResultMaps[fullExtendsResultMapName];
				ProcessResultMap(resultMap.Extends);
				var match = Regex.Match(resultMap.Result.ToString(), "<resultMap.+?>");
				resultMap.Result.Insert(match.Index + match.Length, Environment.NewLine + PadLeft("	", resultMap.Extends.Result.ToString()));
				//resultMap.Result.Append(Environment.NewLine);
				//resultMap.Result.Append(resultMap.Extends.Result);
			}

			resultMap.IsProcessed = true;
		}

		private static string PadLeft(string pad, string value)
		{
			return Regex.Replace(value, "^", pad, RegexOptions.Multiline);
		}

		private static string GetFullID(iBatisElementIDBase currentNode, string id)
		{
			return !id.Contains(".") ? $"{currentNode.Namespace}.{id}" : id;
		}
	}
}