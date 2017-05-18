using System;
using iBatisGlue.Parser;
using NUnit.Framework;
using Shouldly;

namespace iBatisGlue.Test
{
	public class StatementsTestFixture
	{
		[Test]
		public void ParseStatement_Test1()
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

		[Test]
		public void ParseStatement_Test2()
		{
			var fileMapList = iBatisGlueUtils.GetMapFileList(
				@"c:\Projects\Ameritox\trunk\src\Persistence\Configs\PortalSql.SqlMap.config",
				@"c:\Projects\Ameritox\trunk\src\Persistence\Maps\"
			);
			var result = iBatisGlueParser.GetResult(fileMapList, "GetOrderSnapshotInfosWithInsuranceType");
			Console.WriteLine("---------------------------------------------------------------");
			Console.WriteLine(result);
			Console.WriteLine("---------------------------------------------------------------");
			result.ShouldContain("/*Lancelot.Persistence.GetOrderSnapshotInfosWithInsuranceType*/");
		}
	}
}