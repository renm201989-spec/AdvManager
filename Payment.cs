using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvBillingSystem.ACM
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int ClientId { get; set; }
        public string CaseType { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal AmountPaid { get; set; }
        public string Remarks { get; set; }
        public decimal? CourtFees { get; set; } = 0;
        public decimal? ClericalFees { get; set; } = 0;

    }
}
