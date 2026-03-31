using System.Diagnostics;


namespace yandex_pract.Services
{
	[System.Serializable]
	public class BuildingData
	{
		public int Id;
		public string Address;
		public string City;
		public string Country;
	}

	public class BuildingService : IBuildingService
	{
		private static Dictionary<int,BuildingData> _buildings { get; set; } = [];

		public BuildingService()
		{
			//_buildings = new Dictionary<int, BuildingData>();
		}

		public void AddBuilding(BuildingData buildingData)
		{
			if (_buildings.TryGetValue(buildingData.Id, out var data))
			{
				Debug.WriteLine($"Already exists!{buildingData.Id}");
				return;
			}

			_buildings[buildingData.Id] = buildingData;
		}

		public void RemoveBuilding(BuildingData buildingData)
		{
			_buildings.Remove(buildingData.Id);
		}

		public void UpdateBuilding(BuildingData buildingData, bool createIfNotExists = false)
		{
			if (!createIfNotExists)
			{
				if (!_buildings.TryGetValue(buildingData.Id, out var data))
				{
					Debug.WriteLine($"Building not found!{buildingData.Id}");
					return;
				}
			}

			_buildings[buildingData.Id] = buildingData;
		}

		public void ChangeAddress(int id, string newAddress)
		{
			if (!_buildings.TryGetValue(id, out var data))
				return;

			data.Address = newAddress;
		}

		public List<BuildingData> GetBuildings()
		{
			return _buildings.Values.ToList();
		}

		public (bool containsResult, BuildingData? data) GetBuilding(int id)
		{
			var containsResult = false;
			containsResult = _buildings.TryGetValue(id, out var data);
			return (containsResult, data);
		}
		
		
	}
}