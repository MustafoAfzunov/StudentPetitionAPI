using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using StudentPetitionAPI.Application.DTOs.Requests;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace StudentPetitionAPI.Swagger;

public class RequestExamplesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.RequestBody?.Content is null)
        {
            return;
        }

        if (!operation.RequestBody.Content.TryGetValue("application/json", out var mediaType))
        {
            return;
        }

        var bodyParameter = context.MethodInfo
            .GetParameters()
            .FirstOrDefault(p => IsRequestDto(p.ParameterType));

        if (bodyParameter is null)
        {
            return;
        }

        var example = CreateExample(bodyParameter.ParameterType);
        if (example is not null)
        {
            mediaType.Example = example;
            mediaType.Examples = new Dictionary<string, OpenApiExample>
            {
                ["default"] = new OpenApiExample
                {
                    Summary = "Sample request",
                    Value = example
                }
            };
        }
    }

    private static bool IsRequestDto(Type type) =>
        type.Namespace?.StartsWith("StudentPetitionAPI.Application.DTOs.Requests", StringComparison.Ordinal) == true;

    private static IOpenApiAny? CreateExample(Type requestType) => requestType.Name switch
    {
        nameof(LoginRequest) => new OpenApiObject
        {
            ["email"] = new OpenApiString("student@test.com"),
            ["password"] = new OpenApiString("Student123!")
        },
        nameof(CreateStudentRequest) => new OpenApiObject
        {
            ["firstName"] = new OpenApiString("Ada"),
            ["lastName"] = new OpenApiString("Lovelace"),
            ["email"] = new OpenApiString("student@test.com"),
            ["studentNumber"] = new OpenApiString("S-10001")
        },
        nameof(CreatePetitionRequest) => new OpenApiObject
        {
            ["studentId"] = new OpenApiInteger(1),
            ["petitionType"] = new OpenApiString("CourseRetake"),
            ["title"] = new OpenApiString("Request to retake Calculus I"),
            ["description"] = new OpenApiString("I would like to retake the course next semester.")
        },
        nameof(UpdatePetitionRequest) => new OpenApiObject
        {
            ["petitionType"] = new OpenApiString("AcademicLeave"),
            ["title"] = new OpenApiString("Request academic leave"),
            ["description"] = new OpenApiString("Updated description for the academic leave request.")
        },
        nameof(ReviewPetitionRequest) => new OpenApiObject
        {
            ["status"] = new OpenApiString("Approved"),
            ["reviewedBy"] = new OpenApiString("reviewer@test.com"),
            ["reviewComment"] = new OpenApiString("Documents verified. Petition approved.")
        },
        _ => null
    };
}
