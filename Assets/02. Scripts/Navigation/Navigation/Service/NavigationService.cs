﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class NavigationService
{
    private readonly INavigationRepository navigationRepository;
    private readonly GpsService gpsService;
    private readonly PoiService poiService;
    public NavigationService(INavigationRepository navigationRepository, GpsService gpsService, PoiService poiService)
    {
        this.navigationRepository = navigationRepository;
        this.gpsService = gpsService;
        this.poiService = poiService;
    }

    public async Task<Map> FindMapByCurrentLocation(int zoomLevel)
    {
        var findCoordsTask = gpsService.FindCurrentCoordinates();
        var poiInfosTask = poiService.FindPoiInfo();
        await Task.WhenAll(findCoordsTask, poiInfosTask);

        var currentCoords = findCoordsTask.Result;
        var poiInfos = poiInfosTask.Result;

        List<Coords> coords = new(); // API에 보낼 좌표값
        List<PoiInfo> calculatedPoiInfo = new();
        float currentScale = CalcScale(zoomLevel);
        foreach (var poiInfo in poiInfos)
        {
            // float x = (poiInfo.Coords.Longitude - currentCoords.Longitude) * 6378 * Mathf.Cos(poiInfo.Coords.Latitude * Mathf.Deg2Rad) / currentScale * zoomLevel;
            // float y = (poiInfo.Coords.Latitude - currentCoords.Latitude) * 6378 / currentScale * zoomLevel;
            CalcPosition(out float x, out float y, currentCoords, poiInfo.Coords, zoomLevel);
            calculatedPoiInfo.Add(poiInfo with { PositionX = x, PositionY = y });
            coords.Add(poiInfo.Coords);
        }
        var response = await navigationRepository
            .FindMapByCurrentLocation(
                new(
                    currentCoords.Latitude, currentCoords.Longitude, 540,
                    1200, zoomLevel, coords
                ));
        Texture2D tex = new(1080, 2400);
        tex.LoadImage(response.BinaryImage);
        return new Map(tex, currentCoords, calculatedPoiInfo);
    }

    private void CalcPosition(out float x, out float y, Coords center, Coords coords, int zoomLevel)
    {
        float currentScale = CalcScale(zoomLevel);
        x = (coords.Longitude - center.Longitude) * 6378 * Mathf.Cos(coords.Latitude * Mathf.Deg2Rad) / currentScale * zoomLevel;
        y = (coords.Latitude - center.Latitude) * 6378 / currentScale * zoomLevel;
    }

    private float CalcScale(int level)
    {
        return (float)156_543 / (1 << level + 1);
    }
}