using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using TodoJserna.common.Models;
using TodoJserna.common.Responses;
using TodoJserna.Function.Entities;
namespace TodoJserna.Function.Function
{
    public static class Todoapi
    {
        [FunctionName(nameof(CreateTodo))]
        public static async Task<IActionResult> CreateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Todo")] HttpRequest req,
            [Table("Todo", Connection = "AzureWebJobsStorage")] Microsoft.WindowsAzure.Storage.Table.CloudTable todotable,
            ILogger log)
        {
            log.LogInformation("Recived a new todo");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            todo todo = JsonConvert.DeserializeObject<todo>(requestBody);

            if (string.IsNullOrEmpty(todo?.TaskDescription))
            {
                return new BadRequestObjectResult(new Response
                {
                    Issuccess = false,
                    Message = "the request must have a taks descripcion"
                });
            }
            TodoEntity todoEntity = new TodoEntity
            {
                CreatedTime = DateTime.UtcNow,
                ETag = "*",
                IsCompleted = false,
                PartitionKey = "Todo",
                RowKey = Guid.NewGuid().ToString(),
                TaskDescription = todo.TaskDescription
            };

            TableOperation addOperation = TableOperation.Insert(todoEntity);
            await todotable.ExecuteAsync(addOperation);

            string message = "New todo Store in table";
            log.LogInformation(message);


            return new OkObjectResult(new Response
            {
                Issuccess = true,
                Message = message,
                Result = todoEntity
            });
        }
    }
}
