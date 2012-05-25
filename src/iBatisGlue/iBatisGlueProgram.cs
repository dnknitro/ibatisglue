using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace iBatisGlue
{
	internal class iBatisGlueProgram
	{
		[STAThread]
		static void Main(string[] args)
		{
			if(args.Length < 1)
			{
				Console.WriteLine(string.Format(@"iBatisGlue [Version {0}]", Assembly.GetExecutingAssembly().GetName().Version));
				Console.WriteLine(@"(C) Copyright 2009-2010 Volodymyr Shcherbyna");
				Console.WriteLine(
					@"usage: iBatisGlue.ConsoleApp.exe iBatisStatementID [iBatisMapConfigFile] [iBatisMapFilesBasePath]");
				Console.WriteLine(
					@"example: iBatisGlue.ConsoleApp.exe SelectAccountByID c:\Projects\Db\PortalSql.SqlMap.config c:\Projects\Db\Maps\");
				return;
			}

			var configFile = (args.Length > 1) ? args[1] : iBatisGlueUtils.iBatisMapConfigFile;
			var doc = iBatisGlueUtils.LoadXml(configFile);
			var nsmgr = new XmlNamespaceManager(doc.NameTable);
			nsmgr.AddNamespace("dm", iBatisGlueUtils.XML_DATA_MAPPER_NS);
			// ReSharper disable PossibleNullReferenceException
			var fileNodes = doc.DocumentElement.SelectNodes("/dm:sqlMapConfig/dm:sqlMaps/dm:sqlMap/@embedded", nsmgr);
			// ReSharper restore PossibleNullReferenceException
			if(fileNodes == null)
			{
				Console.WriteLine("zero filename nodes found");
				return;
			}


			var filenameOnlyList = new List<string>();
			{
				foreach(XmlNode fileNode in fileNodes)
				{
					var chunks = fileNode.Value.Split(',')[0].Split('.');
					var filename = string.Format("{0}.{1}", chunks[chunks.Length - 2], chunks[chunks.Length - 1]);
					filenameOnlyList.Add(filename.ToUpper());
				}
			}


			var fileMapList = new List<string>();
			{
				var basePath = (args.Length > 2) ? args[2] : iBatisGlueUtils.iBatisMapFilesBasePath;
				var files = Directory.GetFiles(basePath, "*", SearchOption.AllDirectories);

				foreach(var filenameAndPath in files)
				{
					var fileInfo = new FileInfo(filenameAndPath);
					if(filenameOnlyList.Contains(fileInfo.Name.ToUpper()))
					{
						fileMapList.Add(fileInfo.FullName);
					}
				}
			}

			var result = Parser.ProcessStatementList(fileMapList, args[0]);

			if(result.Length > 0)
			{
				if(iBatisGlueUtils.CopyToClipboard)
				{
					Clipboard.SetDataObject(result, true);
				}
				Console.Write(result);
			}
			else
			{
				Console.WriteLine(string.Format("{0} was not found in the map list", args[0]));
			}
		}
	}
}