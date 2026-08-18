using System;
using System.Collections.Generic;
using EFT;
using EFT.Communications;
using Newtonsoft.Json;
using TerritoryClient.Models;
using UnityEngine;
using UnityEngine.UI;
using VoronatorSharp;
using Vector2 = UnityEngine.Vector2;

namespace TerritoryClient.UI;

public class TerritoryRenderer : MonoBehaviour
{
    [SerializeField] 
    public Image MapImage;
    
    [SerializeField]
    public RectTransform MapTransform;

    private Dictionary<string, Color> _colors = [];
    private MongoID? _lastState;
    private static readonly Vector2Int[] FillDirs = [new(1, 0), new(-1, 0), new(0, 1), new(0, -1)];
    public static readonly Dictionary<string, Vector3> LocationPositions = new();
    
    public void Awake()
    {
        foreach ((string faction, string colorData) in Plugin.StateManager.ServerData.FactionColors)
        {
            if (!ColorUtility.TryParseHtmlString(colorData, out Color color))
            {
                NotificationManager.DisplayMessageNotification($"Failed to parse color {colorData}!");
                return;
            }

            _colors[faction] = color;
        }
    }

    public void Show()
    {
        LocationData locationData = Plugin.StateManager.State.Locations;

        if (Plugin.StateManager.State.StateId == _lastState)
            return;
        
        _lastState = Plugin.StateManager.State.StateId;
        
        List<Vector2> points = [];
        List<Color> colors = [];

        Plugin.PluginLogger.LogInfo("Test renderer.");


        Vector3 mapOffset = MapTransform.rect.size / 2f + new Vector2(70, 120);
        
        foreach (string location in LocationData.ValidMaps)
        {
            if (location == "factory4_night" || location == "sandbox_high")
                continue;
            
            LocationState? locationState = locationData[location];

            if (locationState == null)
            {
                Plugin.PluginLogger.LogWarning($"Failed to get location for: {location}");
                continue;
            }

            // Vector2Int startingPos = new((int)locationSettings.IconX - 110, (int)locationSettings.IconY - 90);
            // if (location == "bigmap")
            // {
            //     for (int x = startingPos.x - 8; x < startingPos.x + 9; x++)
            //     {
            //         for (int y = startingPos.y - 8; y < startingPos.y + 9; y++)
            //         {
            //             newMap.SetPixel(x, y, Color.red);
            //         }
            //     }
            // }
            
            //any locations that aren't on the visible map
            if (!LocationPositions.TryGetValue(location, out Vector3 locPos))
            {
                continue;
            }
            
            Vector2 locationPos = locPos - mapOffset;
            
            points.Add(locationPos);
            colors.Add(_colors[locationState.Holder]);
        }
        
        Voronator voronoi = new(points, new Vector2(0, 0), MapTransform.rect.size);
        
        Plugin.PluginLogger.LogInfo(JsonConvert.SerializeObject(LocationPositions));
        Plugin.PluginLogger.LogInfo(JsonConvert.SerializeObject(points));
        
        Texture2D newMap = new((int)Math.Ceiling(MapTransform.rect.width),
            (int)Math.Ceiling(MapTransform.rect.height));
        
        for (int i = 0; i < points.Count; i++)
        {
            List<Vector2> vertices = voronoi.GetClippedPolygon(i);

            try
            {
                DrawFilledPolygon(newMap, points[i], vertices, colors[i]);
            }
            catch (Exception e)
            {
                Plugin.PluginLogger.LogError($"Failed to draw polygon for point: {i}");
            }
        }
        
        newMap.Apply();

        Plugin.PluginLogger.LogInfo("Test finalSprite.");
        Sprite finalSprite =
            Sprite.Create(newMap, new Rect(0, 0, newMap.width, newMap.height), Vector2.one * 0.5f, 300f);
        
        MapImage.sprite = finalSprite;
        MapImage.enabled = true;
    }
    
    private void DrawFilledPolygon(Texture2D texture, Vector2 point, List<Vector2> vertices, Color color)
    {
        Vector2Int clamp = new(texture.width - 1, texture.height - 1);

        if (vertices.Count < 3)
            return;
        
        //draw polygon
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector2 currentVertex = vertices[i];
            Vector2 nextPoint = i == vertices.Count - 1 ? vertices[0] : vertices[i + 1];

            do
            {
                currentVertex = Vector2.MoveTowards(currentVertex, nextPoint, 0.1f);
                Vector2Int rounded = Vector2Int.FloorToInt(currentVertex);
                
                rounded.Clamp(Vector2Int.zero, clamp);
                
                texture.SetPixel(rounded.x, rounded.y, color);

            } while (currentVertex != nextPoint);
        }
        
        //fill it
        Queue<Vector2Int> floodFill = new();
        Vector2Int firstPixel = Vector2Int.RoundToInt(point);
        floodFill.Enqueue(firstPixel);
        
        texture.SetPixel(firstPixel.x, firstPixel.y, color);

        while (floodFill.Count > 0)
        {
            Vector2Int pxCoord = floodFill.Dequeue();

            for (int i = 0; i < 4; i++)
            {
                Vector2Int neighbor = new Vector2Int(pxCoord.x, pxCoord.y) + FillDirs[i];
                
                if (neighbor.x < 0 || neighbor.y < 0 || neighbor.x > clamp.x || neighbor.y > clamp.y ||
                    texture.GetPixel(neighbor.x, neighbor.y) == color)
                    continue;
                
                texture.SetPixel(neighbor.x, neighbor.y, color);
                floodFill.Enqueue(neighbor);
            }
        }
    }
}