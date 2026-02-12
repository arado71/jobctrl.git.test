using System;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Tct.MailFilterService.Configuration;

namespace Tct.MailFilterService
{
    public enum IssueState
    {
        WaitingForCustomer = -1,
        CLOSED = 1,
        OPENED = 0
    }
    public class IssueContext : IDisposable
    {
        private SqlConnection connection;
	    private SqlTransaction tran;

        public IssueContext(string connectionString)
        {
            connection = new SqlConnection(connectionString);
            connection.Open();
        }
        public void Dispose()
        {
			if (tran != null)
				tran.Rollback();
            if (connection != null)
                connection.Close();
            connection = null;
        }
        public bool Exists(string id)
        {
            var res = connection.Query<int>("SELECT 1 FROM dbo.Issues WHERE IssueCode=@Id", new { Id = id }, tran);
            return res.Any() && res.First() == 1;
        }
        public void Add(string id, int companyId, string company, string name, IssueState state)
        {
            connection.Execute(@"INSERT INTO dbo.Issues 
                                (IssueCode,CompanyId,Company,Name,State,CreatedAt,ModifiedAt,CreatedBy,ModifiedBy) VALUES
                                (@IssueCode,@CompanyId,@Company,@Name,@State,@CreatedAt,@ModifiedAt,@CreatedBy,@ModifiedBy)",
                                new Model.Issue()
                                {
                                    IssueCode = id,
                                    CompanyId = companyId,
                                    Company = company.Length > 50 ? company.Substring(0, 50) : company,
                                    Name = name.Length > 100 ? name.Substring(0, 100) : name,
                                    State = (int)state,
                                    CreatedAt = DateTime.UtcNow,
                                    CreatedBy = 0,
                                    ModifiedAt = DateTime.UtcNow,
                                    ModifiedBy = 0
                                }, tran);
        }
        public void Update(string id, int companyId, string company, string name, IssueState stat)
        {
            connection.Execute("UPDATE dbo.Issues SET State=@State,ModifiedAt=@ModifiedAt WHERE IssueCode=@IssueCode and CompanyId = @CompanyId",
                                new Model.Issue()
                                {
                                    IssueCode = id,
                                    CompanyId = companyId,
									//Name = name.Length > 100 ? name.Substring(0, 100) : name,
                                    State = (int)stat,
                                    ModifiedAt = DateTime.UtcNow
                                }, tran);
        }

	    public void StartTransaction()
	    {
			if (tran == null)
				tran = connection.BeginTransaction();
	    }

	    public void Commit()
	    {
		    if (tran == null) return;
		    tran.Commit();
		    tran = null;
	    }

	    public void Rollback()
	    {
			if (tran == null) return;
			tran.Rollback();
		    tran = null;
	    }
    }
}
