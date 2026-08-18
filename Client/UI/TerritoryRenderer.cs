using System;
using System.Collections.Generic;
using EFT.Communications;
using JsonType;
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

    public void Show(LocationSettings internalLocations)
    {
        LocationData locationData = Plugin.StateManager.State.Locations;
        
        List<Vector2> points = [];
        List<Color> colors = [];

        Plugin.PluginLogger.LogInfo("Test renderer.");
        foreach (string location in LocationData.ValidMaps)
        {
            Plugin.PluginLogger.LogInfo($"Test location: {location}.");
            
            LocationState? locationState = locationData[location];

            if (locationState == null)
            {
                Plugin.PluginLogger.LogWarning($"Failed to get location for: {location}");
                return;
            }
            
            Plugin.PluginLogger.LogInfo(JsonConvert.SerializeObject(internalLocations.locations));
            
            Vector2 locationPos = internalLocations.locations[location].RelativeMapPos;
            
            points.Add(locationPos);
            colors.Add(_colors[locationState.Holder]);
        }

        Voronator voronoi = new(points, new Vector2(0, 0), MapTransform.rect.size);

        Texture2D newMap = new((int)Math.Ceiling(MapTransform.rect.width),
            (int)Math.Ceiling(MapTransform.rect.height), TextureFormat.RGBA32, false);
        
        for (int i = 0; i < points.Count; i++)
        {
            List<Vector2> vertices = voronoi.GetClippedPolygon(i);

            DrawFilledPolygon(newMap, points[i], vertices, colors[i]);
        }

        Plugin.PluginLogger.LogInfo("Test finalSprite.");
        Sprite finalSprite =
            Sprite.Create(newMap, new Rect(0, 0, newMap.width, newMap.height), Vector2.one * 0.5f, 100f);
        
        MapImage.sprite = finalSprite;
        MapImage.enabled = true;
    }
    
    private void DrawFilledPolygon(Texture2D texture, Vector2 point, List<Vector2> vertices, Color color)
    {
        Vector2Int clamp = new(texture.width - 1, texture.height - 1);
        
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
        Stack<Vector2Int> floodFillStack = new();
        floodFillStack.Push(Vector2Int.RoundToInt(point));

        while (floodFillStack.Count > 0)
        {
            Vector2Int pxCoord = floodFillStack.Pop();

            if (pxCoord.x < 0 || pxCoord.y < 0 || pxCoord.x > clamp.x || pxCoord.y > clamp.y)
                continue;

            if (texture.GetPixel(pxCoord.x, pxCoord.y) == color)
                continue;
            
            texture.SetPixel(pxCoord.x, pxCoord.y, color);
            
            floodFillStack.Push(new Vector2Int(pxCoord.x - 1, pxCoord.y));
            floodFillStack.Push(new Vector2Int(pxCoord.x + 1, pxCoord.y));
            floodFillStack.Push(new Vector2Int(pxCoord.x, pxCoord.y - 1));
            floodFillStack.Push(new Vector2Int(pxCoord.x, pxCoord.y + 1));
        }
        
        texture.Apply();
    }
}