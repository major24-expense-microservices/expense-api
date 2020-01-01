using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using expense_api.Models;
using expense_api.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace expense_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QueueSendEmailController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public QueueSendEmailController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // POST: api/QueueSendEmail
        [HttpPost]
        // public async void Post([FromBody] RequestSendEmail requestSendEmail)
        public async Task<ActionResult<string>> Post([FromBody] RequestSendEmail requestSendEmail)
        {
            // Fire and forget
            /** iwr -Method POST -Uri https://ms-expense-react-func-app.azurewebsites.net/api/onExpSubmittedAddToQueToCreatePdfAndSendMail?code=<<...get..from...azure...fn..>> 
        *      -Headers @{ "content-type"="application/json" } 
        *      -Body `{user: {userId: 'user1'}, expenseId: 2}`
        */
            try
            {
                string url = _configuration.GetSection("Api").GetSection("SendGridEmail").Value;
                using (var client = new HttpClient())
                using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                using (var httpContent = new UtilHttpContent().CreateHttpContent(requestSendEmail))
                {
                    request.Content = httpContent;
                    using (var response = await client
                        .SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            // var content = await response.Content.ReadAsStringAsync();
                            // return StatusCode((int)response.StatusCode, content);
                            // LOG Success

                        }
                        return response.Content.ToString();
                    }
                }

            }
            catch (Exception ex)
            {
                // LOG ERROR
                string error = ex.Message.ToString();
                return null;
            }
        }


    }
}