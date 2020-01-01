using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace expense_api.Models
{
    public class RequestSendEmail
    {
        public User user { get; set; }
        public int expenseId { get; set; }
    }
}
