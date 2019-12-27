using expense_api.Models;
using expense_api.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace expense_api.Repositories
{
    public interface IExpenseRepository
    {
        Task<Expense> GetById(int id);
        Task<ExpenseReport> GetByIdForReport(int id);
        Task<Result<int>> Save(Expense expense);
    }
}
