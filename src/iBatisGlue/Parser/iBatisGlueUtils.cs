using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace iBatisGlue.Parser
{
	public static class iBatisGlueUtils
	{
		public const string XML_DATA_MAPPER_NS = @"http://ibatis.apache.org/dataMapper";
		public const string XML_MAPPING_NS = @"http://ibatis.apache.org/mapping";
		public const string XML_TO_REMOVE1 = " xmlns=\"" + XML_MAPPING_NS + "\"";

		public static readonly string iBatisMapConfigFile;
		public static readonly string iBatisMapFilesBasePath;
		//public static readonly bool FixTagNewLines;
		public static readonly bool CopyToClipboard;

		static iBatisGlueUtils()
		{
			iBatisMapConfigFile = GetStringConfigValue("iBatisMapConfigFile");
			iBatisMapFilesBasePath = GetStringConfigValue("iBatisMapFilesBasePath");
			//FixTagNewLines = bool.Parse(GetStringConfigValue("FixTagNewLines") ?? "false");
			CopyToClipboard = bool.Parse(GetStringConfigValue("CopyToClipboard") ?? "false");
		}

		static string GetStringConfigValue(string key)
		{
			var val = ConfigurationManager.AppSettings.Get(key);
			if (val == null)
				Console.WriteLine("[Warning] Please specify value for '{0}' key in the configuration file", key);

			return val;
		}

		public static XmlDocument LoadXml(string file)
		{
			var doc = new XmlDocument();
			doc.Load(file);
			if (doc.DocumentElement == null)
				throw new NullReferenceException(string.Format("doc.DocumentElement is null (from {0})", file));
			return doc;
		}

		public static List<string> GetMapFileList(string iBatisMapConfigFileParam, string basePath)
		{
			var doc = LoadXml(iBatisMapConfigFileParam);
			var nsmgr = new XmlNamespaceManager(doc.NameTable);
			nsmgr.AddNamespace("dm", XML_DATA_MAPPER_NS);
			// ReSharper disable once PossibleNullReferenceException
			var fileNodes = doc.DocumentElement.SelectNodes("/dm:sqlMapConfig/dm:sqlMaps/dm:sqlMap/@embedded", nsmgr);
			if (fileNodes == null)
			{
				Console.WriteLine("zero filename nodes found");
				return new List<string>();
			}

			var filenameOnlyList = new List<string>();
			{
				foreach (XmlNode fileNode in fileNodes)
				{
					var chunks = fileNode.Value.Split(',')[0].Split('.');
					var filename = string.Format("{0}.{1}", chunks[chunks.Length - 2], chunks[chunks.Length - 1]);
					filenameOnlyList.Add(filename.ToUpper());
				}
			}

			var fileMapList = new List<string>();
			{
				var files = Directory.GetFiles(basePath, "*", SearchOption.AllDirectories);

				foreach (var filenameAndPath in files)
				{
					var fileInfo = new FileInfo(filenameAndPath);
					if (filenameOnlyList.Contains(fileInfo.Name.ToUpper()))
					{
						fileMapList.Add(fileInfo.FullName);
					}
				}
			}
			return fileMapList;
		}

		public static string FormatSQL( string sql )
		{
			var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sql.sql");
			File.WriteAllText(file, sql);

			var processInfo = new ProcessStartInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SqlFormatter.exe"), file);
			processInfo.WindowStyle = ProcessWindowStyle.Hidden;
			var process = Process.Start(processInfo);
			process.WaitForExit(5000);
			return File.ReadAllText(file);
		}
	}
}