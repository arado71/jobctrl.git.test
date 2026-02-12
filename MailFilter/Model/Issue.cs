using System;

namespace Tct.MailFilterService.Model
{
    public class Issue
    {
        public string IssueCode { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string Company { get; set; }
        public int State { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
        public int ModifiedBy { get; set; }
    }
}
