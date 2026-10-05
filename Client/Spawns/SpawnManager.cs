using System.Runtime.CompilerServices;
using EFT;

namespace TerritoryClient.Spawns;

public static class SpawnManager
{
    public static ConditionalWeakTable<AbstractGame, TerritoriesSpawnScenario> LocalGameSpawnScenarios = new();
}