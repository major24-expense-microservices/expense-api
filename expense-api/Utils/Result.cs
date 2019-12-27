using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace expense_api.Utils
{
    public class Result
    {
        public bool IsSuccess { get; set; }
        public string Error { get; set; }
        public List<string> Errors { get; set; }
        public Result()
        {
            Errors = new List<string>();
        }
        public void AddError(string error)
        {
            this.Errors.Add(error);
        }
    }

    public class Result<T> : Result
    {
        public T Entity;
    }
}
