namespace yandex_pract.Services;

public interface IBuildingService
{
	public void AddBuilding(BuildingData buildingData);
	public void RemoveBuilding(BuildingData buildingData);
	public void UpdateBuilding(BuildingData buildingData, bool createIfNotExists = false);
	public void ChangeAddress(int id, string newAddress);
	public List<BuildingData> GetBuildings();
	public (bool containsResult, BuildingData? data) GetBuilding(int id);
}