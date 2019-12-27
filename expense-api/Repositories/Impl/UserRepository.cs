using Dapper;
using expense_api.Models;
using expense_api.Utils;
using System;
using System.Threading.Tasks;

namespace expense_api.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ISqlConnHelper _sqlConnHelper;

        public UserRepository(ISqlConnHelper sqlConnHelper)
        {
            _sqlConnHelper = sqlConnHelper;
        }

        /// <summary>
        /// Insert user if does not exists in transactions.user table
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<Result> Create(User user)
        {
            Result result = new Result();
            try
            {
                using (var conn = _sqlConnHelper.Connection)
                {
                    DateTime updatedDateTime = DateTime.Now;
                    string sqlInsertUser = @"insert into [dbo].[users] (
                                            user_id,
                                            first_name,
                                            last_name,
                                            cost_centre,
                                            email,
                                            updated_date)
                                    values (@userId,
                                            @firstName,
                                            @lastName,
                                            @costCentre,
                                            @email,
                                            @updatedDate)";

                    DynamicParameters dp = new DynamicParameters();
                    dp.Add("userId", user.UserId, System.Data.DbType.String, System.Data.ParameterDirection.Input, 100);
                    dp.Add("firstName", user.FirstName, System.Data.DbType.String, System.Data.ParameterDirection.Input, 100);
                    dp.Add("lastName", user.LastName, System.Data.DbType.String, System.Data.ParameterDirection.Input, 100);
                    dp.Add("costCentre", user.CostCentre, System.Data.DbType.String, System.Data.ParameterDirection.Input, 100);
                    dp.Add("email", user.Email, System.Data.DbType.String, System.Data.ParameterDirection.Input, 100);
                    dp.Add("updateddate", updatedDateTime, System.Data.DbType.DateTime, System.Data.ParameterDirection.Input);

                    conn.Open();
                    try
                    {
                        var response = await conn.ExecuteAsync(sqlInsertUser, dp, commandType: System.Data.CommandType.Text);
                        result.IsSuccess = true;
                        return result;
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.ToLower().Contains("primary key") || ex.Message.ToLower().Contains("duplicate key"))
                        {
                            result.Error = $"Error: User Id {user.UserId} already exists in the database.";
                        }
                        else
                        {
                            result.Error = ex.Message;
                        }
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error
                result.Error = ex.Message;
                return result;
            }
        }

        public async Task<Result<bool>> IsUserExists(User user)
        {
            Result<bool> result = new Result<bool>();
            try
            {
                using (var conn = _sqlConnHelper.Connection)
                {
                    string sqlSelect = @"select count(user_id) as count from [dbo].[users] where user_id = @userId;";
                    DynamicParameters dp = new DynamicParameters();
                    dp.Add("userId", user.UserId, System.Data.DbType.String, System.Data.ParameterDirection.Input, 100);

                    conn.Open();
                    var response = await conn.QueryFirstAsync(sqlSelect, dp, commandType: System.Data.CommandType.Text);
                    if (response.count > 0)
                    {
                        result.Entity = true;
                        result.IsSuccess = true;
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                result.Entity = false;
                return result;
            }
        }

    }
}
