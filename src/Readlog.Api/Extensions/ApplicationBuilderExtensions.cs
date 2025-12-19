using Swashbuckle.AspNetCore.SwaggerUI;

namespace Readlog.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseSwagger(this IApplicationBuilder app)
    {
        app.UseSwagger(options =>
        {
            options.RouteTemplate = "swagger/{documentName}/swagger.json";
        });

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Readlog API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "Readlog API Documentation";
            options.DefaultModelsExpandDepth(2);
            options.DefaultModelRendering(ModelRendering.Model);
            options.DocExpansion(DocExpansion.List);
            options.EnableFilter();
            options.EnableDeepLinking();
            options.DisplayRequestDuration();
        });

        return app;
    }
}
