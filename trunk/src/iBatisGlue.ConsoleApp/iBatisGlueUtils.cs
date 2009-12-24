using System;
using System.Configuration;
using System.Xml;

namespace iBatisGlue.Common
{
	internal static class iBatisGlueUtils
	{
		public const string XML_DATA_MAPPER_NS = @"http://ibatis.apache.org/dataMapper";
		public const string XML_MAPPING_NS = @"http://ibatis.apache.org/mapping";
		public const string XML_TO_REMOVE1 = " xmlns=\"" + XML_MAPPING_NS + "\"";

		public static readonly string iBatisMapConfigFile;
		public static readonly string iBatisMapFilesBasePath;
		public static readonly bool FixTagNewLines;

		static iBatisGlueUtils()
		{
			iBatisMapConfigFile = GetStringConfigValue("iBatisMapConfigFile");
			iBatisMapFilesBasePath = GetStringConfigValue("iBatisMapFilesBasePath");
			FixTagNewLines = bool.Parse(GetStringConfigValue("FixTagNewLines") ?? "false");
		}

		private static string GetStringConfigValue(string key)
		{
			var val = ConfigurationManager.AppSettings.Get(key);
			if (val == null)
				Console.WriteLine(string.Format("[Warning] Please specify value for '{0}' key in the configuration file", key));

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
	}
}