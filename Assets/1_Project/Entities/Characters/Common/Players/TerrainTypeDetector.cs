using UnityEngine;

public class TerrainTypeDetector
{
    private Terrain currentTerrain;
    private TerrainData currentData;
    private int alphamapWidth;
    private int alphamapHeight;
    private int numTextures;
    private Vector3 lastSampledPosition;

    public TerrainTypeDetector()
    {
        // We will look up the correct terrain per-frame now
    }

    private Terrain GetTerrainAtPosition(Vector3 worldPosition)
    {
        Terrain[] terrains = Terrain.activeTerrains;
        foreach (var terrain in terrains)
        {
            if (terrain.GetComponent<Collider>().bounds.Contains(worldPosition))
                return terrain;
        }
        return null;
    }

    private Vector2Int WorldToAlphamapCoords(Vector3 worldPosition, Terrain terrain)
    {
        Vector3 terrainOrigin = terrain.transform.position;
        TerrainData terrainData = terrain.terrainData;

        Vector3 terrainLocalPos = worldPosition - terrainOrigin;
        float normalizedX = terrainLocalPos.x / terrainData.size.x;
        float normalizedZ = terrainLocalPos.z / terrainData.size.z;

        int mapX = Mathf.Clamp(Mathf.FloorToInt(normalizedX * terrainData.alphamapWidth), 0, terrainData.alphamapWidth - 1);
        int mapZ = Mathf.Clamp(Mathf.FloorToInt(normalizedZ * terrainData.alphamapHeight), 0, terrainData.alphamapHeight - 1);

        return new Vector2Int(mapX, mapZ);
    }

    public int GetSurfaceType(Vector3 worldPosition)
    {
        currentTerrain = GetTerrainAtPosition(worldPosition);
        if (currentTerrain == null)
        {
            Debug.LogWarning("No terrain found under player.");
            return 0;
        }

        currentData = currentTerrain.terrainData;
        alphamapWidth = currentData.alphamapWidth;
        alphamapHeight = currentData.alphamapHeight;
        numTextures = currentData.alphamapLayers;

        Vector2Int coord = WorldToAlphamapCoords(worldPosition, currentTerrain);
        lastSampledPosition = ConvertAlphamapCoordsToWorld(coord, currentTerrain); // Debug sphere pos

        float[,,] splat = currentData.GetAlphamaps(coord.x, coord.y, 1, 1);

        int maxIndex = 0;
        float maxMix = 0f;

        for (int i = 0; i < numTextures; i++)
        {
            float value = splat[0, 0, i];
            if (value > maxMix)
            {
                maxMix = value;
                maxIndex = i;
            }
        }

        return maxIndex;
    }

    private Vector3 ConvertAlphamapCoordsToWorld(Vector2Int splatCoord, Terrain terrain)
    {
        TerrainData data = terrain.terrainData;
        float percentX = splatCoord.x / (float)data.alphamapWidth;
        float percentZ = splatCoord.y / (float)data.alphamapHeight;

        Vector3 terrainPos = terrain.transform.position;
        float worldX = percentX * data.size.x + terrainPos.x;
        float worldZ = percentZ * data.size.z + terrainPos.z;

        float height = terrain.SampleHeight(new Vector3(worldX, 0, worldZ)) + terrainPos.y;

        return new Vector3(worldX, height, worldZ);
    }

    public Vector3 GetLastSampledPosition()
    {
        return lastSampledPosition;
    }
}
