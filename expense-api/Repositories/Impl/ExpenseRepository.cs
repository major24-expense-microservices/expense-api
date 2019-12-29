using Dapper;
using expense_api.Models;
using expense_api.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace expense_api.Repositories
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly ISqlConnHelper _sqlConnHelper;

        public ExpenseRepository(ISqlConnHelper sqlConnHelper)
        {
            _sqlConnHelper = sqlConnHelper;
        }

        public async Task<Result<int>> Save(Expense expense)
        {
            // expense contains one parent record and expense items in an array
            // items array contains new OOP and exiting CR transactions
            // for OOP, id is set as 0  
            // CR trans comes with existing id from sql table
            // If id == 0, then Insert to trans table, else update trans table
            // Once inserted, get the id from sql server and add to array. then use to insert to items table
            try
            {
                using (var conn = _sqlConnHelper.Connection)
                {
                    string sqlExpense = @"insert into expenses (user_id, cost_centre, approver_id, status)
                                values (@userid, @costcentre, @approverid, @status);
                                SELECT CAST(SCOPE_IDENTITY() as int)";

                    string sqlTransInsert = @"insert into expensed_transactions (id, expense_id, trans_type, description, amount, tax, trans_date, category)
                                values (@id, @expenseid, @transtype, @description, @amount, @tax, @transdate, @category);";

                    conn.Open();
                    var transaction = conn.BeginTransaction();
                    try
                    {
                        string expenseStatus = "Submitted";
                        DynamicParameters dpExps = new DynamicParameters();
                        dpExps.Add("userid", expense.User.UserId, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                        dpExps.Add("costcentre", expense.CostCentre, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                        dpExps.Add("approverid", expense.ApproverId, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                        dpExps.Add("status", expenseStatus, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                        var resultIdExps = await conn.QueryFirstAsync<int>(sqlExpense, dpExps, transaction);

                        var items = expense.ExpenseItems;
                        foreach (var item in items)
                        {
                            DynamicParameters dp = new DynamicParameters();
                            dp.Add("id", item.Id, System.Data.DbType.Guid, System.Data.ParameterDirection.Input);
                            dp.Add("expenseid", resultIdExps, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                            dp.Add("transtype", item.TransType, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                            dp.Add("description", item.Description, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                            dp.Add("amount", item.Amount, System.Data.DbType.Decimal, System.Data.ParameterDirection.Input);
                            dp.Add("tax", item.Tax, System.Data.DbType.Decimal, System.Data.ParameterDirection.Input);
                            dp.Add("transdate", item.TransDate, System.Data.DbType.Date, System.Data.ParameterDirection.Input);
                            dp.Add("category", item.Category, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                            await conn.ExecuteAsync(sqlTransInsert, dp, transaction);
                        }

                        transaction.Commit();
                        return new Result<int>() { IsSuccess = true, Entity = resultIdExps };
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        string errMsg = "";
                        if (ex.Message.ToLower().Contains("primary key") || ex.Message.ToLower().Contains("duplicate key"))
                        {
                            errMsg = $"Duplicate transaction id key violation. One or more of the transactions id already exists in expense table";
                        }
                        return new Result<int>() { IsSuccess = false, Error = errMsg };
                        // throw new Exception("Error inserting expense data: " + ex.Message);
                    }

                }

            }
            catch (Exception ex)
            {
                // Log error
                return new Result<int>() { IsSuccess = false, Error = ex.Message };
                // throw new Exception("Error accessing db: " + ex.Message);
            }
        }

        public async Task<Result<Expense>> GetById(int id)
        {
            try
            {
                using (var conn = _sqlConnHelper.Connection)
                {
                    string sql = @"select Id, user_id as userid, cost_centre as costcentre, approver_id as approverid, status, submitted_date as submitteddate, updated_date as updateddate
                                    from [dbo].[expenses]";
                    DynamicParameters dp = new DynamicParameters();
                    dp.Add("id", id, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);

                    conn.Open();
                    var result = await conn.QueryFirstOrDefaultAsync<Result<Expense>>(sql, dp);
                    return result;
                }
            }
            catch (Exception ex)
            {
                return new Result<Expense>() { IsSuccess = false, Error = ex.Message };
                // throw new Exception("Error getting expense by id" + ex.Message);
            }
        }

        public async Task<Result<ExpenseReport>> GetByIdForReport(int id)
        {
            Result<ExpenseReport> response = new Result<ExpenseReport>();
            try
            {
                ExpenseReport expenseReport = new ExpenseReport();
                using (var conn = _sqlConnHelper.Connection)
                {
                    // multi query
                    string sqlExp = @"SELECT [id], [user_id] as userid, [cost_centre] as costcentre, [approver_id] as approverid, [status], [submitted_date] as submitteddate
                                        FROM [dbo].[expenses] where id = @expenseid;";

                    string sqlExpItems = @"select t.id, t.expense_id as expenseid, t.trans_type as transtype, t.description, t.amount, t.tax, clkp.description as category, t.trans_date as transdate
		                                    from [dbo].[expensed_transactions] t
		                                    inner join [dbo].[category_lookup] clkp
		                                    on t.category = clkp.category
		                                    where t.expense_id = @expenseid;";

                    string sqlUser = @"select u.user_id as userid, u.first_name as firstname, u.last_name as lastname, email from [dbo].[users] u
	                                        inner join [dbo].[expenses] e
	                                        on e.user_id = u.user_id
	                                        where e.id = @expenseid;";

                    var queries = $"{sqlExp} {sqlExpItems} {sqlUser}";

                    conn.Open();
                    using (var multi = conn.QueryMultiple(queries, new { expenseid = id }))
                    {
                        var expense = multi.Read<Expense>();
                        var expItems = multi.Read<ExpenseItem>().ToArray();
                        var user = multi.Read<User>();

                        expenseReport.Expense = expense.FirstOrDefault();
                        expenseReport.ExpenseItems = expItems;
                        expenseReport.User = user.FirstOrDefault();
                    }

                    response.Entity = expenseReport;
                    return await Task.FromResult(response);
                }
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
                return await Task.FromResult(response);
                // throw new Exception("Error getting expense by id" + ex.Message);
            }
        }
    }
}
