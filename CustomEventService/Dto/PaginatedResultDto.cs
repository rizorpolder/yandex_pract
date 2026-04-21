using System.Collections.Generic;

namespace yandex_pract.CustomEventService.Dto;

public class PaginatedResultDto<T>
{
	public int PageIndex { get; }
	public int EntriesCount { get; }
	public List<T> Data { get; }
}