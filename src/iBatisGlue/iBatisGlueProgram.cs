using System;
using System.Reflection;
using System.Windows.Forms;
using iBatisGlue.Parser;

namespace iBatisGlue
{
	internal class iBatisGlueProgram
	{
		[STAThread]
		static void Main(string[] args)
		{
			if (args.Length < 1)
			{
				Console.WriteLine($@"iBatisGlue [Version {Assembly.GetExecutingAssembly().GetName().Version}]");
				Console.WriteLine(@"(C) Copyright 2009-2010 Volodymyr Shcherbyna");
				Console.WriteLine(
					@"usage: iBatisGlue.ConsoleApp.exe iBatisStatementID [iBatisMapConfigFile] [iBatisMapFilesBasePath]");
				Console.WriteLine(
					@"example: iBatisGlue.ConsoleApp.exe SelectAccountByID c:\Projects\Db\PortalSql.SqlMap.config c:\Projects\Db\Maps\");
				return;
			}

			var statementName = args[0];

			var fileMapList = iBatisGlueUtils.GetMapFileList(
				args.Length > 1 ? args[1] : iBatisGlueUtils.iBatisMapConfigFile,
				args.Length > 2 ? args[2] : iBatisGlueUtils.iBatisMapFilesBasePath
			);
			var result = iBatisGlueParser.GetResult(fileMapList, statementName);

			if (result.Length > 0)
			{
				if (iBatisGlueUtils.CopyToClipboard)
				{
					Clipboard.SetDataObject(result, true);
				}
				Console.Write(result);
			}
			else
			{
				Console.WriteLine($"{statementName} was not found in the map list");
			}
		}
	}
}