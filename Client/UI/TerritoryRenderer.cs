using System;
using System.Collections.Generic;
using EFT.Communications;
using JsonType;
using TerritoryClient.Models;
using UnityEngine;
using VoronatorSharp;
using Vector2 = UnityEngine.Vector2;

namespace TerritoryClient.UI;

public class TerritoryRenderer : MonoBehaviour
{
    [SerializeField]
    public MeshFilter MeshFilter;
    
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
        List<Vector2> points = new()
        {
            new Vector2(40, 30),
            new Vector2(25, 40),
            new Vector2(65, 30),
            new Vector2(45, 50)
        };

        List<Color> colors = new()
        {
            Color.green,
            Color.green,
            Color.red,
            Color.blue
        };

        Voronator voronoi = new(points);
        Delaunator delaunator = voronoi.Delaunator;
        
        List<Vector3> finalVertices = new();
        List<Color> finalColors = new();
        
        for (int i = 0; i < points.Count; i++)
        {
            List<Vector2> vertices = voronoi.GetClippedPolygon(i);
            
            if (vertices == null)
                continue;

            foreach (Vector2 vertex in vertices)
            {
                finalVertices.Add(vertex);
                finalColors.Add(colors[i]);
            }
        }
        
        finalColors.Clear();
        finalVertices.Clear();
        for (int i = 0; i < delaunator.Points.Count; i++)
        {
            finalVertices.Add(delaunator.Points[i]);
            finalColors.Add(colors[i]);
        }

        Color[] colorArray = finalColors.ToArray();
        Vector3[] vertexArray = finalVertices.ToArray();

        Debug.Log("Test 1");
        
        Mesh territoryMesh = new()
        {
            vertices = vertexArray,
            triangles = delaunator.Triangles,
            colors = colorArray
        };

        MeshFilter.mesh = territoryMesh;
        
        Debug.Log("Test");
    }
}