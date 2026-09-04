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
    public Image MapImage = null!;
    
    [SerializeField]
    public RectTransform MapTransform = null!;
    
    [NonSerialized]
    public MongoID? LastState;

    private static readonly Vector2Int[] FillDirs = [new(1, 0), new(-1, 0), new(0, 1), new(0, -1)];
    public static readonly Dictionary<string, Vector3> LocationPositions = new();
    

    public void Show()
    {
        LocationData locationData = Plugin.StateManager.State.Locations;

        if (Plugin.StateManager.State.StateId == LastState)
            return;
        
        LastState = Plugin.StateManager.State.StateId;
        
        List<Vector2> points = [];
        List<Color> colors = [];
        
        Texture2D newMap = new((int)Math.Ceiling(MapTransform.rect.width),
            (int)Math.Ceiling(MapTransform.rect.height), TextureFormat.RGBA32, true);
        
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
            
            //any locations that aren't on the visible map
            if (!LocationPositions.TryGetValue(location, out Vector3 locPos))
            {
                continue;
            }
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(MapTransform, locPos, 
                null, out Vector2 locationPos);

            locationPos = MapTransform.rect.size + locationPos;
            
            points.Add(locationPos);
            colors.Add(Plugin.StateManager.ServerData.Factions[locationState.Holder].Color);
        }
        
        Voronator voronoi = new(points, new Vector2(0, 0), MapTransform.rect.size);

        Color[] pixels = newMap.GetPixels();
        for (int i = 0; i < points.Count; i++)
        {
            List<Vector2> vertices = voronoi.GetClippedPolygon(i);
        
            try
            {
                DrawFilledPolygon(newMap, ref pixels, points[i], vertices, colors[i]);
            }
            catch (Exception e)
            {
                Plugin.PluginLogger.LogError($"Failed to draw polygon for point: {i}");
            }
        }
        newMap.SetPixels(pixels);
        newMap.Apply();

        Sprite finalSprite =
            Sprite.Create(newMap, new Rect(0, 0, newMap.width, newMap.height), Vector2.one * 0.5f, 300f);
        
        MapImage.sprite = finalSprite;
        MapImage.enabled = true;
    }
    
    private void DrawFilledPolygon(Texture2D texture, ref Color[] imagePixels, Vector2 point, List<Vector2> vertices, Color color)
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
                
                imagePixels[rounded.y * texture.width + rounded.x] = color;

            } while (currentVertex != nextPoint);
        }
        
        //fill it
        Queue<Vector2Int> floodFill = new();
        Vector2Int firstPixel = Vector2Int.RoundToInt(point);
        floodFill.Enqueue(firstPixel);
        
        imagePixels[firstPixel.y * texture.width + firstPixel.x] = color;

        while (floodFill.Count > 0)
        {
            Vector2Int pxCoord = floodFill.Dequeue();

            for (int i = 0; i < 4; i++)
            {
                Vector2Int neighbor = new Vector2Int(pxCoord.x, pxCoord.y) + FillDirs[i];
                int neighborIndex = neighbor.y * texture.width + neighbor.x;
                
                if (neighbor.x < 0 || neighbor.y < 0 || neighbor.x > clamp.x || neighbor.y > clamp.y ||
                    imagePixels[neighborIndex] == color)
                    continue;
                
                imagePixels[neighborIndex] = color;
                floodFill.Enqueue(neighbor);
            }
        }
    }
}