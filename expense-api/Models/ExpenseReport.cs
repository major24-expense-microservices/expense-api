using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace expense_api.Models
{
    public class ExpenseReport
    {
        public Expense Expense { get; set; }
        public User User { get; set; }
        public ExpenseItem[] ExpenseItems { get; set; }
    }
}
