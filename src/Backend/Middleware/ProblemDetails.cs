using System;
using System.Collections.Generic;

namespace yandex_pract.Middleware;

[Serializable]
public class ProblemDetails
{
	public string? Type { get; set; }

	public string? Title { get; set; }

	public int? Status { get; set; }

	public string? Detail { get; set; }

	public string? Instance { get; set; }

	public IDictionary<string, object?> Extensions { get; set; } =
		new Dictionary<string, object?>(StringComparer.Ordinal);
}

