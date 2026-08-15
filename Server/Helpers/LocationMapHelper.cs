using SPTarkov.Common.Extensions;
using SPTarkov.DI.Annotations;
using TerritoryServer.Models;
using TerritoryServer.Servers;

namespace TerritoryServer.Helpers;

[Injectable(InjectionType.Singleton)]
public class LocationMapHelper(DataConfig gameData, StateServer stateServer)
{
    public bool CanReach(string start, string end, bool factionOnly)
    {
        return CalculateNav(start, end, factionOnly) != null;
    }

    public int GetDistance(string start, string end, bool factionOnly)
    {
        NavCell? calculation = CalculateNav(start, end, factionOnly);

        if (calculation == null)
            return -1;

        return calculation.Distance;
    }

    //navigate from start to end
    //basically djikstra's algorithm
    private NavCell? CalculateNav(string start, string end, bool factionOnly)
    {
        if (start == end)
        {
            return new NavCell(start, 0);
        }
        
        if (gameData.LocationNeighbors[start]!.Count == 0 || 
            gameData.LocationNeighbors[end]!.Count == 0)
        {
            return null;
        }

        List<NavCell> openList = [new(start, 0)];
        HashSet<string> closedList = [];

        LocationData<LocationState> currentLocations = stateServer.CurrentSave.Locations!;
        string faction = currentLocations[start].Holder;
        
        while (openList.Count > 0)
        {
            NavCell currentCell = openList.PopFirst();
            string currentLoc = currentCell.Location;
            
            List<string> neighbors = gameData.LocationNeighbors[currentLoc];
            foreach (string neighbor in neighbors)
            {
                if (neighbor == end)
                {
                    return new NavCell(neighbor, currentCell.Distance + 1);
                }
                
                if (closedList.Contains(neighbor) || 
                    (factionOnly && currentLocations[currentLoc].Holder != faction))
                    continue;
                
                openList.Add(new NavCell(neighbor, currentCell.Distance + 1));
            }

            closedList.Add(currentLoc);
        }

        return null;
    }
}

internal class NavCell(string location, int distance)
{
    public string Location { get; } = location;
    public int Distance { get; } = distance;
}