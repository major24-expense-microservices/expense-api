using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using expense_api.Models;
using expense_api.Repositories;
using expense_api.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace expense_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpensesController : ControllerBase
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly IUserRepository _userRepository;

        public ExpensesController(IExpenseRepository expenseRepository, IUserRepository userRepository)
        {
            _expenseRepository = expenseRepository;
            _userRepository = userRepository;
        }

        // POST: api/Expenses
        [HttpPost]
        public async Task<ActionResult<Result>> Post([FromBody] Expense expense)
        {
            try
            {
                User user = expense.User;
                Result<bool> resultValUser = ValidateUser(user);
                if (!resultValUser.Entity)
                {
                    return BadRequest(resultValUser);
                }
                Result<bool> resultValExpense = ValidateExpense(expense);
                if (!resultValExpense.Entity)
                {
                    return BadRequest(resultValExpense);
                }
                // Validation passed.
                // Before saving expense, ensure User exists in expense-microservice database?
                // Ensure user already exists? If not add the user to the user table...
                Result<bool> userFoundResult = await _userRepository.IsUserExists(user);
                if (!userFoundResult.Entity)
                {
                    Result userResult = await _userRepository.Create(user);
                    if (!userResult.IsSuccess)
                    {
                        // if user creation is unsuccessfull, no need to proceed and save transactions..
                        return BadRequest(userResult);
                    }
                }

                Result<int> result = await _expenseRepository.Save(expense);
                if (result.Entity > 0)
                {
                    return Created("", result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                Result result = new Result() { IsSuccess = false, Error = $"Error Saving record. {ex.Message}" };
                return StatusCode(500, result);
            }
        }

        private Result<bool> ValidateUser(User user)
        {
            Result<bool> result = new Result<bool>();
            result.Entity = true;
            if (user == null || (user.UserId == null || user.UserId == ""))
            {
                result.AddError("User id is required");
            }
            if (user.FirstName == "" || user.LastName == "")
            {
                result.AddError("User first and last names are required");
            }
            if (result.Errors.Count > 0)
            {
                result.Entity = false;
            }
            return result;
        }

        private Result<bool> ValidateExpense(Expense expense)
        {
            Result<bool> result = new Result<bool>();
            result.Entity = true;
            if (expense.ApproverId == null || expense.ApproverId == "")
            {
                result.AddError("Approver id is required");
            }
            if (expense.CostCentre == null || expense.CostCentre == "")
            {
                result.AddError("CostCentre is required");
            }
            if (expense.ExpenseItems == null || expense.ExpenseItems.Length < 1)
            {
                result.AddError("Expense item(s) is required");
            }
            if (result.Errors.Count > 0)
            {
                result.Entity = false;
            }
            return result;
        }





    }
}
