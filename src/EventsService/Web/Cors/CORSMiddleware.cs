using Microsoft.AspNetCore.Cors.Infrastructure;

namespace Web.Cors;

public static class CORSMiddleware
{
	public static CorsPolicy CreatePolicyBuilder()
	{
		var builder = new CorsPolicyBuilder();
		builder.AllowAnyHeader();
		builder.AllowCredentials();
		builder.WithMethods("GET", "POST", "PUT", "DELETE");


		return builder.Build();
	}
}