using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using log4net;

namespace Tct.DeployService
{
	public class DatabaseConfiguration
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(DatabaseConfiguration));

		public static DatabaseConfiguration Parse(string connectionString)
		{
			if (string.IsNullOrEmpty(connectionString))
				return null;
			
			
			var res = new DatabaseConfiguration();
			var match = Regex.Match(connectionString, @"Data Source=(.*?)(;|$)");
			if (!match.Success)
			{
				logger.WarnFormat("No data source found in connectionstring \"{0}\"", connectionString);
				return null;
			}

			res.Address = match.Groups[1].Value;
			match = Regex.Match(connectionString, @"Initial Catalog=(.*?)(;|$)");
			if (!match.Success)
			{
				logger.WarnFormat("No database found in connectionstring \"{0}\"", connectionString);
				return null;
			}

			res.Database = match.Groups[1].Value;
			match = Regex.Match(connectionString, @"User Id=(.*?)(;|$)");
			if (!match.Success)
			{
				logger.DebugFormat("No username found in connectionstring \"{0}\"", connectionString);
			}
			else
			{
				res.User = match.Groups[1].Value;	
			}

			match = Regex.Match(connectionString, @"Password=(.*?)(;|$)");
			if (!match.Success)
			{
				logger.DebugFormat("No username found in connectionstring \"{0}\"", connectionString);
			}
			else
			{
				res.Password = match.Groups[1].Value;
			}

			return res;
		}

		public override string ToString()
		{
			if (string.IsNullOrEmpty(User) || string.IsNullOrEmpty(Password))
			{
				return string.Format("Data Source={0};Initial Catalog={1};Integrated Security=True", Address, Database);
			}

			return string.Format("Data Source={0};Initial Catalog={1};User Id={2};Password={3}", Address, Database, User, Password);			
		}

		public string Database { get; set; }
		public string Address { get; set; }
		public string User { get; set; }
		public string Password { get; set; }
	}
}
