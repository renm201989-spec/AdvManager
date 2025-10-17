using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvBillingSystem.ACM
{
    public class Client
    {
        public int ClientId { get; set; }
        public string CaseNumber { get; set; }
        public string CaseType { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Remarks { get; set; }
        public string VisitedDt { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? TotalFees { get; set; } = 0;
        public decimal? OtherFees { get; set; } = 0;
        public decimal? TotalPaid { get; set; } = 0;
        public decimal? Balance { get; set; } = 0;
        public int ActiveUser { get; set; } = 1;
    }
}
