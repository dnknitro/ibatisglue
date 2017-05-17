using System;
using iBatisGlue.Parser;
using NUnit.Framework;
using Shouldly;

namespace iBatisGlue.Test
{
	public class StatementsTestFixture
	{
		[Test]
		public void ParseStatement_Test()
		{
			var fileMapList = iBatisGlueUtils.GetMapFileList(
				iBatisGlueUtils.iBatisMapConfigFile,
				iBatisGlueUtils.iBatisMapFilesBasePath
			);
			var result = iBatisGlueParser.GetResult(fileMapList, "SelectSomething");
			Console.WriteLine("---------------------------------------------------------------");
			Console.WriteLine(result);
			Console.WriteLine("---------------------------------------------------------------");
			result.ShouldContain("/*Persistence1.Persistence2.SelectSomething*/");
			result.ShouldContain("/*Persistence1.sql1*/");
			result.ShouldContain("/*Persistence1.sql1*/");
		}
	}
}