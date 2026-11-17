using System.Collections.Generic;
using UnityEngine;

public class CellularAutomataCave : MonoBehaviour
{
    [Header("Seed")]
    [Range(0, 10000)] public int seed;
    [Range(0, 10)] public int simulationStep = 3;
    public int width = 80;
    public int height = 80;
    public int tileSize = 1;
    [Range(0, 1)] public float wallChance = 0.59f;
    [Range(0, 8)] public int wallThreshold = 4;

    [Header("Tunnel")]
    [Range(1, 3)] public int tunnelRadius = 2;

    [Header("Lakes")]
    [Range(0, 15)] public int lakeCount = 7;
    [Range(5, 100)] public int minLakeSize = 15;
    [Range(5, 150)] public int maxLakeSize = 40;
    [Range(0, 8)] public int minFloorNeighboursForLake = 6;

    [Header("Enemies")]
    [Range(0f, 1f)] public float minEnemyChance = 0.002f;
    [Range(0f, 1f)] public float maxEnemyChance = 0.02f;

    [Header("Crystals")]
    [Range(0f, 1f)] public float minCrystalChance = 0.001f;
    [Range(0f, 1f)] public float maxCrystalChance = 0.004f;

    [Header("UI")]
    public GameObject deathScreen;
    public GameObject winScreen;

    private GameObject spawnedPlayer;
    private bool playerDead = false;
    private bool playerWon = false;

    public int maxCrystals = 8;
    public int minCrystalSpacing = 8;
    public int minCrystalDistanceFromStartEnd = 6;
    public int minFloorNeighboursForCrystal = 5;
    public float enemyHeight = 1f;

    private Vector2Int startPosition;
    private Vector2Int endPosition;

    private int[,] distanceMap;
    private int maxDistance;

    public GameObject crystalPrefab;
    public GameObject enemyPrefab;
    public GameObject playerPrefab;

    private List<GameObject> spawnedEnemies = new List<GameObject>();

    private bool[,] lakeGrid;

    private bool[,] grid;

    private System.Random rng;
    void Start()
    {
        seed = Random.Range(0, 10001);
        Generate();
    }

    void Update()
    {
        if (playerDead && Input.GetKeyDown(KeyCode.Space))
        {
            RespawnPlayer();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            seed = Random.Range(0, 10001);
            Generate();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Generate();
        }
    }

    public void Generate()
    {
        playerDead = false;
        playerWon = false;

        deathScreen.SetActive(false);
        winScreen.SetActive(false);

        if (spawnedPlayer != null)
        {
            Destroy(spawnedPlayer);
            spawnedPlayer = null;
        }

        CrystalCheckpoint.ResetCheckpoint();

        ClearPrevious();
        rng = new System.Random(seed);
        InitializeRandomGrid();
        for (int i = 0; i < simulationStep; i++)
        {
            grid = StepSimulation();
        }

        // Create entrance and exit as isolated floor tiles
        GenerateStartAndEnd();

        // Connect everything, including entrance and exit
        ConnectDisconnectedAreas();

        // Calculate actual cave distances from entrance
        CalculateDistances();

        GenerateLakes();

        PlaceEnemies();

        PlaceCrystals();

        RenderGrid();

        SpawnPlayer();
    }
    public void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            return;
        }

        CrystalCheckpoint.ResetCheckpoint();

        if (spawnedPlayer != null)
        {
            Destroy(spawnedPlayer);
        }

        Vector3 localSpawnPosition = new Vector3(startPosition.x * tileSize, 1f, startPosition.y * tileSize);
        Vector3 spawnPosition = transform.TransformPoint(localSpawnPosition);

        Instantiate(playerPrefab, spawnPosition, Quaternion.identity, transform);
    }


    public void InitializeRandomGrid()
    {
        grid = new bool[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    grid[x, y] = true;
                    continue;
                }
                grid[x, y] = rng.NextDouble() < wallChance;
            }
        }
    }

    public bool[,] StepSimulation()
    {
        bool[,] newGrid = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbourCount = CountNeighbours(x, y);

                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    newGrid[x, y] = true;
                    continue;
                }

                newGrid[x, y] = neighbourCount > wallThreshold;
            }
        }

        return newGrid;
    }

    public int CountNeighbours(int x, int y)
    {
        int count = 0;
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                {
                    continue;
                }

                int cx = x + dx;
                int cy = y + dy;

                if (cx < 0 || cx >= width || cy < 0 || cy >= height)
                {
                    count++;
                }
                else
                {
                    if (grid[cx, cy])
                    {
                        count++;
                    }
                }
            }
        }
        return count;
    }

    public void GenerateStartAndEnd()
    {
        List<Vector2Int> edgeTiles = new List<Vector2Int>();

        // Bottom and top edges
        for (int x = 1; x < width - 1; x++)
        {
            edgeTiles.Add(new Vector2Int(x, 0));
            edgeTiles.Add(new Vector2Int(x, height - 1));
        }

        // Left and right edges
        for (int y = 1; y < height - 1; y++)
        {
            edgeTiles.Add(new Vector2Int(0, y));
            edgeTiles.Add(new Vector2Int(width - 1, y));
        }

        // Random entrance
        startPosition = edgeTiles[rng.Next(edgeTiles.Count)];

        int longestDistance = -1;

        // Find geometrically furthest edge tile
        foreach (Vector2Int tile in edgeTiles)
        {
            int distance =
                Mathf.Abs(tile.x - startPosition.x) +
                Mathf.Abs(tile.y - startPosition.y);

            if (distance > longestDistance)
            {
                longestDistance = distance;
                endPosition = tile;
            }
        }

        // Turn entrance and exit walls into floor
        grid[startPosition.x, startPosition.y] = false;
        grid[endPosition.x, endPosition.y] = false;
    }

    public void CalculateDistances()
    {
        distanceMap = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                distanceMap[x, y] = -1;
            }
        }

        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        queue.Enqueue(startPosition);
        distanceMap[startPosition.x, startPosition.y] = 0;

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            foreach (Vector2Int direction in directions)
            {
                Vector2Int next = current + direction;

                if (next.x < 0 || next.x >= width || next.y < 0 || next.y >= height)
                {
                    continue;
                }

                if (grid[next.x, next.y])
                {
                    continue;
                }

                if (distanceMap[next.x, next.y] != -1)
                {
                    continue;
                }

                distanceMap[next.x, next.y] = distanceMap[current.x, current.y] + 1;

                queue.Enqueue(next);
            }
        }

        maxDistance = distanceMap[endPosition.x, endPosition.y];
    }

    public void PlaceEnemies()
    {
        if (enemyPrefab == null)
        {
            return;
        }

        for (int x = 1; x < width - 1; x += 3)
        {
            for (int y = 1; y < height - 1; y += 3)
            {
                if (grid[x, y])
                {
                    continue;
                }

                if (distanceMap[x, y] < 0)
                {
                    continue;
                }

                float distanceRatio = Mathf.Clamp01(distanceMap[x, y] / (float)maxDistance);

                float spawnChance = Mathf.Lerp(minEnemyChance, maxEnemyChance, distanceRatio);

                if (rng.NextDouble() < spawnChance)
                {
                    Vector3 position = new Vector3(x * tileSize, enemyHeight, y * tileSize);

                    GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity, transform);
                    spawnedEnemies.Add(enemy);
                }
            }
        }
    }

    public void PlaceCrystals()
    {
        if (crystalPrefab == null)
            return;

        List<Vector2Int> placedCrystals = new List<Vector2Int>();

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (placedCrystals.Count >= maxCrystals)
                    return;

                // Must be traversable floor
                if (grid[x, y])
                    continue;

                // Don't place crystals inside lakes
                if (lakeGrid[x, y])
                    continue;

                // Needs enough open space around it
                if (CountFloorNeighbours(x, y) < minFloorNeighboursForCrystal)
                    continue;

                Vector2Int position = new Vector2Int(x, y);

                // Keep away from entrance and exit
                if (Vector2Int.Distance(position, startPosition) < minCrystalDistanceFromStartEnd)
                    continue;

                if (Vector2Int.Distance(position, endPosition) < minCrystalDistanceFromStartEnd)
                    continue;

                // Keep crystals apart
                bool tooClose = false;

                foreach (Vector2Int other in placedCrystals)
                {
                    if (Vector2Int.Distance(position, other) < minCrystalSpacing)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (tooClose)
                    continue;

                float distanceRatio = Mathf.Clamp01(distanceMap[x, y] / (float)maxDistance);

                float spawnChance = Mathf.Lerp(minCrystalChance, maxCrystalChance, distanceRatio);

                if (rng.NextDouble() < spawnChance)
                {
                    GameObject crystal = Instantiate(crystalPrefab, new Vector3(x * tileSize, 0f, y * tileSize), Quaternion.identity, transform);

                    // Give each procedural crystal its own deterministic seed
                    LSystemsCrystal lSystem = crystal.GetComponent<LSystemsCrystal>();

                    if (lSystem != null)
                    {
                        lSystem.seed = rng.Next(0, 10001);
                    }

                    placedCrystals.Add(position);
                }
            }
        }
    }

    public void RenderGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == true)
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.parent = transform;
                    cube.transform.position = new Vector3(x * tileSize, 0.5f * tileSize, y * tileSize);
                    cube.transform.localScale = new Vector3(tileSize, 3f * tileSize, tileSize);
                    cube.GetComponent<Renderer>().material.color = new Color(0.93f, 0.89f, 0.89f);
                }
                else
                {
                    GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    floor.transform.parent = transform;
                    floor.transform.position = new Vector3(x * tileSize, 0, y * tileSize);
                    floor.transform.localScale = new Vector3(tileSize / 10f, 1, tileSize / 10f);

                    Vector2Int currentPosition = new Vector2Int(x, y);

                    if (currentPosition == startPosition)
                    {
                        // Entrance
                        floor.GetComponent<Renderer>().material.color = Color.green;
                    }
                    else if (currentPosition == endPosition)
                    {
                        // Exit
                        floor.tag = "End";
                        floor.GetComponent<Renderer>().material.color = Color.red;
                    }
                    else if (lakeGrid != null && lakeGrid[x, y])
                    {
                        // Lake tile
                        floor.tag = "Lake";

                        floor.GetComponent<Renderer>().material.color = new Color(0.15f, 0.45f, 0.70f);
                    }
                    else
                    {
                        // Normal cave floor
                        floor.GetComponent<Renderer>().material.color = new Color(0.79f, 0.65f, 0.48f);
                    }
                }
            }
        }
    }

    // Finds separate floor regions and connects them together
    public void ConnectDisconnectedAreas()
    {
        bool[,] visited = new bool[width, height];

        // Stores every disconnected floor area
        List<List<Vector2Int>> areas = new List<List<Vector2Int>>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Starts a flood fill from every unvisited floor cell
                if (!grid[x, y] && !visited[x, y])
                {
                    List<Vector2Int> area = new List<Vector2Int>();
                    FloodFill(x, y, visited, area);
                    areas.Add(area);
                }
            }
        }

        if (areas.Count <= 1)
        {
            return;
        }

        // Uses the largest floor region as the initially connected area
        areas.Sort((a, b) => b.Count.CompareTo(a.Count));

        List<Vector2Int> connectedArea = new List<Vector2Int>(areas[0]);

        for (int i = 1; i < areas.Count; i++)
        {
            Vector2Int pointA = areas[i][0];
            Vector2Int pointB = connectedArea[0];
            int shortestDistance = int.MaxValue;

            // Finds the closest pair of cells between both areas
            foreach (Vector2Int a in areas[i])
            {
                foreach (Vector2Int b in connectedArea)
                {
                    int distance = Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        pointA = a;
                        pointB = b;
                    }
                }
            }

            // Digs a tunnel between the disconnected area and connected cave
            DigTunnel(pointA, pointB);
            connectedArea.AddRange(areas[i]);
        }
    }

    // Finds all connected floor cells starting from one position
    public void FloodFill(int startX, int startY, bool[,] visited, List<Vector2Int> area)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            area.Add(current);

            foreach (Vector2Int direction in directions)
            {
                int x = current.x + direction.x;
                int y = current.y + direction.y;

                // Adds neighbouring unvisited floor cells to the same area
                if (x >= 0 && x < width && y >= 0 && y < height && !grid[x, y] && !visited[x, y])
                {
                    visited[x, y] = true;
                    queue.Enqueue(new Vector2Int(x, y));
                }
            }
        }
    }

    // Carves a wider tunnel between two floor regions
    public void DigTunnel(Vector2Int pointA, Vector2Int pointB)
    {
        int x = pointA.x;
        int y = pointA.y;

        while (x != pointB.x)
        {
            int radius = rng.Next(1, tunnelRadius + 1);
            CarveCircle(x, y, radius);

            if (x < pointB.x)
            {
                x++;
            }
            else
            {
                x--;
            }
        }

        while (y != pointB.y)
        {
            CarveCircle(x, y, tunnelRadius);

            if (y < pointB.y)
            {
                y++;
            }
            else
            {
                y--;
            }
        }

        CarveCircle(x, y, tunnelRadius);
    }

    public void CarveCircle(int centerX, int centerY, int radius)
    {
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                // Keeps the carved shape approximately circular
                if (dx * dx + dy * dy > radius * radius)
                {
                    continue;
                }

                int x = centerX + dx;
                int y = centerY + dy;

                // Avoid carving outside the cave or through the outer border
                if (x <= 0 || x >= width - 1 || y <= 0 || y >= height - 1)
                {
                    continue;
                }

                grid[x, y] = false;
            }
        }
    }

    public void GenerateLakes()
    {
        lakeGrid = new bool[width, height];

        for (int i = 0; i < lakeCount; i++)
        {
            Vector2Int? seedPosition = FindLakeSeed();

            if (seedPosition.HasValue)
            {
                int targetSize = rng.Next(minLakeSize, maxLakeSize + 1);
                GrowLake(seedPosition.Value, targetSize);
            }
        }
    }

    public Vector2Int? FindLakeSeed()
    {
        List<Vector2Int> candidates = new List<Vector2Int>();

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                // Must be floor
                if (grid[x, y])
                {
                    continue;
                }

                // Must not already be part of another lake
                if (lakeGrid[x, y])
                {
                    continue;
                }

                int floorNeighbours = CountFloorNeighbours(x, y);

                // Prefer relatively open areas instead of corridors
                if (floorNeighbours >= minFloorNeighboursForLake)
                {
                    candidates.Add(new Vector2Int(x, y));
                }
            }
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        return candidates[rng.Next(candidates.Count)];
    }

    public int CountFloorNeighbours(int x, int y)
    {
        int count = 0;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                {
                    continue;
                }

                int nx = x + dx;
                int ny = y + dy;

                if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                {
                    continue;
                }

                if (!grid[nx, ny])
                {
                    count++;
                }
            }
        }

        return count;
    }

    public void GrowLake(Vector2Int start, int targetSize)
    {
        List<Vector2Int> frontier = new List<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        frontier.Add(start);
        visited.Add(start);

        int currentSize = 0;

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (frontier.Count > 0 && currentSize < targetSize)
        {
            int index = rng.Next(frontier.Count);
            Vector2Int current = frontier[index];
            frontier.RemoveAt(index);

            // Don't place lakes on walls
            if (grid[current.x, current.y])
            {
                continue;
            }

            // Avoid spreading into narrow corridors
            if (CountFloorNeighbours(current.x, current.y) < minFloorNeighboursForLake)
            {
                continue;
            }

            lakeGrid[current.x, current.y] = true;
            currentSize++;

            foreach (Vector2Int direction in directions)
            {
                Vector2Int next = current + direction;

                if (next.x <= 0 || next.x >= width - 1 || next.y <= 0 || next.y >= height - 1)
                {
                    continue;
                }

                if (visited.Contains(next))
                {
                    continue;
                }

                if (grid[next.x, next.y])
                {
                    continue;
                }

                visited.Add(next);
                frontier.Add(next);
            }
        }
    }

    public void PlayerDied(GameObject player)
    {
        if (playerDead || playerWon)
            return;

        playerDead = true;
        spawnedPlayer = player;

        player.GetComponent<PlayerMovement>().enabled = false;

        deathScreen.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RespawnPlayer()
    {
        Vector3 respawnPosition;

        if (CrystalCheckpoint.HasCheckpoint())
        {
            respawnPosition = CrystalCheckpoint.GetRespawnPosition();
        }
        else
        {
            respawnPosition = transform.TransformPoint(new Vector3(startPosition.x * tileSize, 1f, startPosition.y * tileSize));
        }

        CharacterController controller = spawnedPlayer.GetComponent<CharacterController>();

        controller.enabled = false;
        spawnedPlayer.transform.position = respawnPosition;
        controller.enabled = true;

        spawnedPlayer.GetComponent<PlayerMovement>().enabled = true;

        RegenerateEnemies();

        deathScreen.SetActive(false);
        playerDead = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void RegenerateEnemies()
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }

        spawnedEnemies.Clear();
        PlaceEnemies();
    }

    public void PlayerWon(GameObject player)
    {
        if (playerWon || playerDead)
            return;

        playerWon = true;

        player.GetComponent<PlayerMovement>().enabled = false;

        winScreen.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ClearPrevious()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}