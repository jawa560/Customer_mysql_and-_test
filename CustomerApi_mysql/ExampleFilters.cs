using CustomerApi.Controllers;
using CustomerApi.Data;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

public class ExampleFilters : ISchemaFilter, IOperationFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(Customer))
        {
            schema.Example = new OpenApiObject
            {
                ["id"] = new OpenApiInteger(1),
                ["name"] = new OpenApiString("王曉明"),
                ["birthday"] = new OpenApiString("1990-01-01"),
                ["gender"] = new OpenApiString("男"),
                ["address"] = new OpenApiString("台南市永華路一段1號"),
                ["phone"] = new OpenApiString("123-456-7890"),
                ["note1"] = new OpenApiString("註解 1"),
                ["note2"] = new OpenApiString("註解 2")
            };
        }
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.Name == "Post" && context.MethodInfo.DeclaringType == typeof(CustomersController))
        {
            operation.RequestBody.Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
            {
                ["example"] = new OpenApiExample
                {
                    Value = new OpenApiObject
                    {
                        ["name"] = new OpenApiString("王曉明"),
                        ["birthday"] = new OpenApiString("1990-01-01"),
                        ["gender"] = new OpenApiString("男"),
                        ["address"] = new OpenApiString("台南市永華路一段1號"),
                        ["phone"] = new OpenApiString("098-765-4321"),
                        ["note1"] = new OpenApiString("註解 1"),
                        ["note2"] = new OpenApiString("註解 2")
                    }
                }
            };
            
        };
        if (operation.Responses.ContainsKey("400") && operation.Responses["400"].Content != null)
        {
            operation.Responses["400"].Content.Clear();
        };
        if (operation.Responses.ContainsKey("401") && operation.Responses["401"].Content != null)
        {
            operation.Responses["401"].Content.Clear();
        };
        if (operation.Responses.ContainsKey("404") && operation.Responses["404"].Content != null)
        {
            operation.Responses["404"].Content.Clear();
        };
        //if (operation.Responses.ContainsKey("201") && operation.Responses["201"].Content != null)
        //{
        //    operation.Responses["201"].Content.Clear();
        //}

    }
}
