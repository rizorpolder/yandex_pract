using System.Collections.Generic;

namespace yandex_pract.CustomEventService.Dto;

public class PaginatedResultDto
{
	private readonly int _pageIndex;
	private readonly int _entriesCount;
	private readonly List<EventDto> _data;

	public int PageIndex => _pageIndex;

	public int EntriesCount => _entriesCount;

	public int EntriesInPage => _data.Count;

	public List<EventDto> Data => _data;

	public PaginatedResultDto(List<EventDto> data, int pageIndex, int entriesCount)
	{
		_pageIndex = pageIndex;
		_entriesCount = entriesCount;
		_data = data;
	}
}