using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using yandex_pract.Models;
using yandex_pract.Services;

namespace yandex_pract.Controllers;

[ApiController, Route("[controller]")]
public class BuildingController : ControllerBase
{
	private readonly IBuildingService _buildings;

	public BuildingController(IBuildingService buildingService)
	{
		_buildings = buildingService;
	}

	[HttpGet("/{buildingId}")]
	public ActionResult<BuildingData> GetBuilding(int buildingId)
	{
		var result = _buildings.GetBuilding(buildingId);
		if (!result.containsResult)
			return NoContent();
		return result.data;
	}

	[HttpGet("/buildingsAll")]
	public ActionResult<List<int>> GetAllBuildings()
	{
		var buildings = _buildings.GetBuildings();
		if (buildings.Count > 0) return new List<int> { 134, 135, 136 };
		return new NoContentResult();
	}

	[HttpPost("/add/{buildingId:int}")]
	public IActionResult CreateBuilding(int buildingId, [FromQuery] string address, [FromQuery] string city,
		[FromQuery] string country)
	{
		var building = new BuildingData()
		{
			Id = buildingId,
			Address = address,
			City = city,
			Country = country
		};
		_buildings.AddBuilding(building);
		return Ok();
	}


	[HttpPost]
	public IActionResult AddAddress(AddressDto addressDto)
	{
		if (!TryValidateModel(addressDto))
		{
			return BadRequest(ModelState);
		}

		var address = new Address()
		{
			Building = addressDto.Building,
			Street = addressDto.Street
		};
		return Ok(address);
	}
}