using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace GameLogic
{
    public sealed class GridView : MonoBehaviour
    {
        public static GridView I { get; private set; }

        [Header("Grid Settings")]
        [SerializeField] private float positionMultiplier = 1f;
        [SerializeField] private float tileScale = 1f;
        [SerializeField] private Vector3 tileRotation = Vector3.zero; // Euler angles added to all tiles
        [SerializeField] private float animDuration = 0.5f;
        [SerializeField] private float waveDelay = 0.1f; // delay per unit distance from center
        [SerializeField] private Ease animEase = Ease.OutBack;

        [Header("Main Tiles")]
        [SerializeField] private Mesh tileMesh;
        [SerializeField] private Material[] tileMaterials; // Must have GPU Instancing enabled!

        [Header("Wall Tiles (edges)")]
        [SerializeField] private Mesh wallMesh;
        [SerializeField] private Material[] wallMaterials;
        [SerializeField] private Vector2 wallOffset = Vector2.zero; // x=height, y=distance from grid

        [Header("Corner Tiles")]
        [SerializeField] private Mesh cornerMesh;
        [SerializeField] private Material[] cornerMaterials;
        [SerializeField] private Vector2 cornerOffset = Vector2.zero; // x=height, y=distance from grid

        [Header("Runtime (View-layer occupancy)")]
        public List<GameObject>[,] gridArray;      // objects on each tile (view-side)
        
        // GPU Instancing data
        private Matrix4x4[] tileMatrices, wallMatrices, cornerMatrices;
        private float[] tileAnim, wallAnim, cornerAnim;
        private Vector3[] tilePos;
        private int tileCount, wallCount, cornerCount, prevSx, prevSz;
        private bool tileAnimDone, wallAnimDone, cornerAnimDone;
        
        // Reusable draw buffer (growable, replaces old 1023-limit batch buffer)
        private Matrix4x4[] drawBuffer = new Matrix4x4[1024];

        // Visible grid range (computed from camera each frame)
        private int visMinX, visMaxX, visMinZ, visMaxZ;
        // Tile index lookup: tileIndex[x,z] gives index into tileMatrices
        private int[,] tileIndex;

        // Entity rendering cache
        private bool entitiesDirty = true;
        private Dictionary<int, List<Matrix4x4>> entityGroups = new Dictionary<int, List<Matrix4x4>>();
        private Vector3 lastCamPos;
        private Quaternion lastCamRot;
        
        private MaterialPropertyBlock propertyBlock;
        private Camera cachedCam;
        private float prevTileScale;
        private Vector3 prevTileRotation;



        public void MarkEntitiesDirty() => entitiesDirty = true;

        public int SizeX => GameGrid.I != null ? GameGrid.I.Width : 0;
        public int SizeZ => GameGrid.I != null ? GameGrid.I.Height : 0; // backend Height maps to view Z

        private void Awake()
        {
            if (I != null && I != this)
            {
                Destroy(gameObject);
                return;
            }
            I = this;
        }

        private void OnDestroy()
        {
            if (I == this) I = null;
        }

        private void Start()
        {
            if (GameGrid.I == null || !GameGrid.I.IsReady)
            {
                Debug.LogError("GridView requires Grid.I to exist and be initialized before GridView.Start().");
                return;
            }

            propertyBlock = new MaterialPropertyBlock();
            cachedCam = Camera.main;
            if (cachedCam == null) cachedCam = FindAnyObjectByType<Camera>();

            Rebuild(force: true);
        }

        private void Update()
        {
            float dt = Time.deltaTime, speed = animDuration > 0 ? 1f / animDuration : 100f;
            Vector3 bs = Vector3.one * tileScale * positionMultiplier;
            Quaternion br = Quaternion.Euler(tileRotation);

            // Detect Inspector changes to tileScale / tileRotation and rebuild matrices
            if (tileScale != prevTileScale || tileRotation != prevTileRotation)
            {
                prevTileScale = tileScale;
                prevTileRotation = tileRotation;
                if (tileAnimDone && tileMatrices != null)
                {
                    for (int i = 0; i < tileCount; i++)
                        tileMatrices[i] = Matrix4x4.TRS(tilePos[i], br, bs);
                    entitiesDirty = true;
                    wallAnimDone = false;
                    cornerAnimDone = false;
                }
            }

            // Animate tiles
            if (tileMesh != null && tileMaterials?.Length > 0 && tileMatrices != null)
            {
                ComputeVisibleRange();

                if (!tileAnimDone)
                {
                    // Only animate visible tiles — off-screen tiles snap to final state
                    bool allDone = true;
                    float step = dt * speed;
                    for (int i = 0; i < tileCount; i++)
                    {
                        float a = tileAnim[i];
                        if (a >= 1f) continue;
                        a = Mathf.MoveTowards(a, 1f, step);
                        tileAnim[i] = a;
                        if (a < 1f)
                        {
                            allDone = false;
                            // Only compute TRS for visible tiles
                            int x = i / prevSz, z = i % prevSz;
                            if (x >= visMinX && x <= visMaxX && z >= visMinZ && z <= visMaxZ)
                            {
                                float t = Mathf.Clamp01(a);
                                tileMatrices[i] = Matrix4x4.TRS(tilePos[i], br, bs * DOVirtual.EasedValue(0f, 1f, t, animEase));
                            }
                        }
                        else
                        {
                            tileMatrices[i] = Matrix4x4.TRS(tilePos[i], br, bs);
                        }
                    }
                    if (allDone) { tileAnimDone = true; entitiesDirty = true; }
                }

                // Draw only visible tiles
                DrawVisibleTiles(tileMesh, tileMaterials);
            }

            // Animate walls (only when visible — walls are at grid edges)
            if (wallMesh != null && wallMaterials?.Length > 0 && wallMatrices != null)
            {
                if (!wallAnimDone) UpdateWalls(dt, speed, bs, br);
                // Only draw walls if camera can see the grid edges
                if (visMinX <= 0 || visMaxX >= SizeX - 1 || visMinZ <= 0 || visMaxZ >= SizeZ - 1)
                    DrawInstanced(wallMesh, wallMaterials, wallMatrices, wallCount);
            }

            // Animate corners (only when visible)
            if (cornerMesh != null && cornerMaterials?.Length > 0 && cornerMatrices != null)
            {
                if (!cornerAnimDone) UpdateCorners(dt, speed, bs, br);
                if (visMinX <= 0 || visMaxX >= SizeX - 1 || visMinZ <= 0 || visMaxZ >= SizeZ - 1)
                    DrawInstanced(cornerMesh, cornerMaterials, cornerMatrices, cornerCount);
            }

            // Draw entities from backend
            DrawEntities();
        }

        private void EnsureDrawBuffer(int minSize)
        {
            if (drawBuffer.Length < minSize)
                drawBuffer = new Matrix4x4[Mathf.NextPowerOfTwo(minSize)];
        }

        private void DrawInstanced(Mesh mesh, Material[] mats, Matrix4x4[] matrices, int count,
            UnityEngine.Rendering.ShadowCastingMode shadows = UnityEngine.Rendering.ShadowCastingMode.Off)
        {
            if (count == 0 || mesh == null) return;
            for (int sub = 0; sub < mesh.subMeshCount && sub < mats.Length; sub++)
            {
                if (mats[sub] == null) continue;
                var rp = new RenderParams(mats[sub])
                {
                    shadowCastingMode = shadows,
                    matProps = propertyBlock
                };
                Graphics.RenderMeshInstanced(rp, mesh, sub, matrices, count);
            }
        }

        private void ComputeVisibleRange()
        {
            Camera cam = cachedCam;
            if (cam == null) { visMinX = 0; visMaxX = SizeX - 1; visMinZ = 0; visMaxZ = SizeZ - 1; return; }

            int sx = SizeX, sz = SizeZ;
            float minGx = float.MaxValue, maxGx = float.MinValue;
            float minGz = float.MaxValue, maxGz = float.MinValue;

            // Cast rays from viewport corners + edges onto the grid plane (y=0)
            // Use 8 sample points for better coverage with angled cameras
            Vector3[] viewpoints = new Vector3[8];
            viewpoints[0] = new Vector3(0, 0, 0);
            viewpoints[1] = new Vector3(1, 0, 0);
            viewpoints[2] = new Vector3(0, 1, 0);
            viewpoints[3] = new Vector3(1, 1, 0);
            viewpoints[4] = new Vector3(0.5f, 0, 0);
            viewpoints[5] = new Vector3(0.5f, 1, 0);
            viewpoints[6] = new Vector3(0, 0.5f, 0);
            viewpoints[7] = new Vector3(1, 0.5f, 0);

            int validHits = 0;
            for (int i = 0; i < viewpoints.Length; i++)
            {
                Ray ray = cam.ViewportPointToRay(viewpoints[i]);
                // Intersect with y=0 plane
                if (Mathf.Abs(ray.direction.y) < 0.0001f) continue; // parallel, skip
                
                float t = -ray.origin.y / ray.direction.y;
                if (t < 0)
                {
                    // Ray points away from ground — clamp to a reasonable far distance
                    // Project along the ray's XZ direction by the far clip plane distance
                    float farDist = cam.farClipPlane;
                    Vector3 farPoint = ray.origin + ray.direction.normalized * farDist;
                    float gx = farPoint.x / positionMultiplier + sx / 2f;
                    float gz = farPoint.z / positionMultiplier + sz / 2f;
                    // Clamp to grid bounds so it doesn't blow out the range
                    gx = Mathf.Clamp(gx, -1, sx + 1);
                    gz = Mathf.Clamp(gz, -1, sz + 1);
                    minGx = Mathf.Min(minGx, gx); maxGx = Mathf.Max(maxGx, gx);
                    minGz = Mathf.Min(minGz, gz); maxGz = Mathf.Max(maxGz, gz);
                    validHits++;
                    continue;
                }

                {
                    Vector3 hit = ray.origin + ray.direction * t;
                    float gx = hit.x / positionMultiplier + sx / 2f;
                    float gz = hit.z / positionMultiplier + sz / 2f;
                    minGx = Mathf.Min(minGx, gx); maxGx = Mathf.Max(maxGx, gx);
                    minGz = Mathf.Min(minGz, gz); maxGz = Mathf.Max(maxGz, gz);
                    validHits++;
                }
            }

            if (validHits == 0) { visMinX = 0; visMaxX = sx - 1; visMinZ = 0; visMaxZ = sz - 1; return; }

            // Pad by 2 tiles to avoid popping at edges
            visMinX = Mathf.Max(0, Mathf.FloorToInt(minGx) - 2);
            visMaxX = Mathf.Min(sx - 1, Mathf.CeilToInt(maxGx) + 2);
            visMinZ = Mathf.Max(0, Mathf.FloorToInt(minGz) - 2);
            visMaxZ = Mathf.Min(sz - 1, Mathf.CeilToInt(maxGz) + 2);
        }

        private void DrawVisibleTiles(Mesh mesh, Material[] mats)
        {
            int maxVisible = (visMaxX - visMinX + 1) * (visMaxZ - visMinZ + 1);
            if (maxVisible <= 0) return;

            EnsureDrawBuffer(maxVisible);

            int count = 0;
            for (int x = visMinX; x <= visMaxX; x++)
                for (int z = visMinZ; z <= visMaxZ; z++)
                    drawBuffer[count++] = tileMatrices[tileIndex[x, z]];

            DrawInstanced(mesh, mats, drawBuffer, count);
        }

        private void DrawEntities()
        {
            if (GameGrid.I == null || GameGrid.I.entities == null) return;

            // Check if camera moved
            Camera cam = cachedCam;
            if (cam != null && (cam.transform.position != lastCamPos || cam.transform.rotation != lastCamRot))
            {
                lastCamPos = cam.transform.position;
                lastCamRot = cam.transform.rotation;
                entitiesDirty = true;
            }

            // Rebuild cache only when entities change or animation is still running
            if (entitiesDirty || !tileAnimDone)
            {
                int sx = SizeX, sz = SizeZ;

                // Clear lists but keep the dictionary to avoid re-allocating
                foreach (var list in entityGroups.Values) list.Clear();

                foreach (var entity in GameGrid.I.entities)
                {
                    SharedEntityVisualData data = sharedDataDictionary.dataArray[entity.ID].sharedVisualData;
                    if (data.mesh == null || data.materials == null || data.materials.Length == 0) continue;

                    // Skip entities outside visible range
                    int ex = entity.Position.x, ez = entity.Position.y;
                    if (ex < visMinX || ex > visMaxX || ez < visMinZ || ez > visMaxZ) continue;

                    Vector3 worldPos = GridToWorld(new Vector3(entity.Position.x, data.Height, entity.Position.y));

                    // Match entity to its tile's animation progress
                    float animT = 1f;
                    if (!tileAnimDone && tileAnim != null && ex >= 0 && ex < sx && ez >= 0 && ez < sz)
                    {
                        int tileIdx = tileIndex[ex, ez];
                        animT = DOVirtual.EasedValue(0f, 1f, Mathf.Clamp01(tileAnim[tileIdx]), animEase);
                    }

                    Vector3 scale = Vector3.one * data.scale * animT;
                    float hash = Mathf.Abs((Mathf.Sin(entity.Position.x * 127.1f + entity.Position.y * 311.7f) * 43758.5453f) % 1f);
                    Quaternion rot = Quaternion.Euler(data.rotation) * (data.randomRotation ? Quaternion.Euler(0, 0, hash * 360f) : Quaternion.identity);
                    Matrix4x4 matrix = Matrix4x4.TRS(worldPos, rot, scale * positionMultiplier);

                    if (!entityGroups.TryGetValue(entity.ID, out var list))
                    {
                        list = new List<Matrix4x4>();
                        entityGroups[entity.ID] = list;
                    }
                    list.Add(matrix);
                }

                entitiesDirty = false;
            }

            // Draw each group with its own materials
            foreach (var kvp in entityGroups)
            {
                if (kvp.Value.Count == 0) continue;
                SharedEntityVisualData data = sharedDataDictionary.dataArray[kvp.Key].sharedVisualData;
                var matrices = kvp.Value;

                EnsureDrawBuffer(matrices.Count);
                matrices.CopyTo(drawBuffer);

                DrawInstanced(data.mesh, data.materials, drawBuffer, matrices.Count,
                    UnityEngine.Rendering.ShadowCastingMode.On);
            }
        }

        public void Rebuild(bool force = false)
        {
            if (GameGrid.I == null || !GameGrid.I.IsReady) { Debug.LogError("Cannot build GridView: backend Grid is missing or not ready."); return; }

            int sx = GameGrid.I.Width, sz = GameGrid.I.Height, n = sx * sz;
            if (!force && tileMatrices != null && tileCount == n && prevSx == sx && prevSz == sz) return;

            bool sizeChanged = prevSx != sx || prevSz != sz;
            Vector3 sv = Vector3.one * tileScale * positionMultiplier;
            Quaternion br = Quaternion.Euler(tileRotation);

            // Main grid tiles
            gridArray = new List<GameObject>[sx, sz];
            float[] oldAnim = tileAnim;
            tileMatrices = new Matrix4x4[n];
            tilePos = new Vector3[n];
            tileAnim = new float[n];
            tileIndex = new int[sx, sz];
            tileCount = n;
            tileAnimDone = false;
            wallAnimDone = false;
            cornerAnimDone = false;
            entitiesDirty = true;

            float cx = (sx - 1) / 2f, cz = (sz - 1) / 2f;
            float maxDist = Mathf.Sqrt(cx * cx + cz * cz);

            int idx = 0;
            for (int x = 0; x < sx; x++)
            {
                for (int z = 0; z < sz; z++)
                {
                    gridArray[x, z] = new List<GameObject>();
                    tileIndex[x, z] = idx;
                    Vector3 p = GridToWorld(new Vector3Int(x, 0, z));
                    tilePos[idx] = p;
                    float dist = Mathf.Sqrt((x - cx) * (x - cx) + (z - cz) * (z - cz));
                    float delay = dist * waveDelay / (animDuration > 0 ? animDuration : 1f);
                    tileAnim[idx] = (!sizeChanged && oldAnim != null && x < prevSx && z < prevSz) ? oldAnim[x * prevSz + z] : -delay;
                    tileMatrices[idx] = Matrix4x4.TRS(p, br, sv * DOVirtual.EasedValue(0f, 1f, Mathf.Clamp01(tileAnim[idx]), animEase));
                    idx++;
                }
            }
            prevSx = sx; prevSz = sz;

            // Walls & Corners - continue wave outward
            int wn = 2 * (sx + sz);
            float[] oldWall = wallAnim, oldCorner = cornerAnim;
            wallMatrices = new Matrix4x4[wn]; wallAnim = new float[wn]; wallCount = wn;
            cornerMatrices = new Matrix4x4[4]; cornerAnim = new float[4]; cornerCount = 4;
            float edgeDelay = (maxDist + 1) * waveDelay / (animDuration > 0 ? animDuration : 1f);
            float cornerDelay = (maxDist + 2) * waveDelay / (animDuration > 0 ? animDuration : 1f);
            for (int i = 0; i < wn; i++) wallAnim[i] = (!sizeChanged && oldWall != null && i < oldWall.Length) ? oldWall[i] : -edgeDelay;
            for (int i = 0; i < 4; i++) cornerAnim[i] = (!sizeChanged && oldCorner != null && i < oldCorner.Length) ? oldCorner[i] : -cornerDelay;
        }

        private void UpdateWalls(float dt, float speed, Vector3 bs, Quaternion br)
        {
            int sx = SizeX, sz = SizeZ;
            float hx = sx / 2f, hz = sz / 2f, pm = positionMultiplier, wd = wallOffset.y;
            Quaternion r180 = br * Quaternion.Euler(0, 0, 180);
            Quaternion rN90 = br * Quaternion.Euler(0, 0, -90);
            Quaternion rP90 = br * Quaternion.Euler(0, 0, 90);
            bool allDone = true;
            int w = 0;
            for (int i = 0; i < sx; i++)
            {
                float x = (i - hx + 0.5f) * pm;
                wallAnim[w] = Mathf.MoveTowards(wallAnim[w], 1f, dt * speed);
                float t0 = Mathf.Clamp01(wallAnim[w]);
                wallMatrices[w++] = Matrix4x4.TRS(new Vector3(x, wallOffset.x, (-hz - 0.5f - wd) * pm), r180, bs * DOVirtual.EasedValue(0f, 1f, t0, animEase));
                if (t0 < 1f) allDone = false;
                wallAnim[w] = Mathf.MoveTowards(wallAnim[w], 1f, dt * speed);
                float t1 = Mathf.Clamp01(wallAnim[w]);
                wallMatrices[w++] = Matrix4x4.TRS(new Vector3(x, wallOffset.x, (hz + 0.5f + wd) * pm), br, bs * DOVirtual.EasedValue(0f, 1f, t1, animEase));
                if (t1 < 1f) allDone = false;
            }
            for (int i = 0; i < sz; i++)
            {
                float z = (i - hz + 0.5f) * pm;
                wallAnim[w] = Mathf.MoveTowards(wallAnim[w], 1f, dt * speed);
                float t0 = Mathf.Clamp01(wallAnim[w]);
                wallMatrices[w++] = Matrix4x4.TRS(new Vector3((-hx - 0.5f - wd) * pm, wallOffset.x, z), rN90, bs * DOVirtual.EasedValue(0f, 1f, t0, animEase));
                if (t0 < 1f) allDone = false;
                wallAnim[w] = Mathf.MoveTowards(wallAnim[w], 1f, dt * speed);
                float t1 = Mathf.Clamp01(wallAnim[w]);
                wallMatrices[w++] = Matrix4x4.TRS(new Vector3((hx + 0.5f + wd) * pm, wallOffset.x, z), rP90, bs * DOVirtual.EasedValue(0f, 1f, t1, animEase));
                if (t1 < 1f) allDone = false;
            }
            wallAnimDone = allDone;
        }

        private void UpdateCorners(float dt, float speed, Vector3 bs, Quaternion br)
        {
            float hx = SizeX / 2f, hz = SizeZ / 2f, pm = positionMultiplier, cd = cornerOffset.y;
            float cx = (hx + 0.5f + cd) * pm, cz = (hz + 0.5f + cd) * pm;
            (Vector3 p, float r)[] c = { (new(-cx, cornerOffset.x, -cz), 180), (new(cx, cornerOffset.x, -cz), 90), (new(-cx, cornerOffset.x, cz), -90), (new(cx, cornerOffset.x, cz), 0) };
            bool allDone = true;
            for (int i = 0; i < 4; i++)
            {
                cornerAnim[i] = Mathf.MoveTowards(cornerAnim[i], 1f, dt * speed);
                float t = Mathf.Clamp01(cornerAnim[i]);
                cornerMatrices[i] = Matrix4x4.TRS(c[i].p, br * Quaternion.Euler(0, 0, c[i].r), bs * DOVirtual.EasedValue(0f, 1f, t, animEase));
                if (t < 1f) allDone = false;
            }
            cornerAnimDone = allDone;
        }

        // --- Coordinate conversions (view works in XZ, backend works in XY) ---

        public Vector3Int WorldToGrid(Vector3 position)
        {
            int sx = SizeX;
            int sz = SizeZ;
            if (sx <= 0 || sz <= 0) return Vector3Int.zero;

            int x = Mathf.RoundToInt(position.x / positionMultiplier) + sx / 2;
            int z = Mathf.RoundToInt(position.z / positionMultiplier) + sz / 2;

            return LoopGridPosition(new Vector3Int(x, 0, z));
        }

        public Vector3Int LoopGridPosition(Vector3Int gridPos)
        {
            int sx = SizeX;
            int sz = SizeZ;
            if (sx <= 0 || sz <= 0) return Vector3Int.zero;

            int x = ((gridPos.x % sx) + sx) % sx;
            int z = ((gridPos.z % sz) + sz) % sz;
            return new Vector3Int(x, 0, z);
        }

        public Vector3 GridToWorld(Vector3 gridPos)
        {
            int sx = SizeX;
            int sz = SizeZ;
            if (sx <= 0 || sz <= 0) return Vector3.zero;

            float x = (gridPos.x - sx / 2) * positionMultiplier;
            float z = (gridPos.z - sz / 2) * positionMultiplier;
            return new Vector3(x, gridPos.y, z);
        }


        private bool InBoundsView(Vector3Int p)
        {
            int sx = SizeX;
            int sz = SizeZ;
            return (uint)p.x < (uint)sx && (uint)p.z < (uint)sz;
        }

        // --- Backend tile access using XZ coords (z -> y) ---

        public Tile GetBackendTile(Vector3Int gridPos)
        {
            if (GameGrid.I == null || !GameGrid.I.IsReady) return null;
            gridPos = LoopGridPosition(gridPos);
            return GameGrid.I.Get(gridPos.x, gridPos.z);
        }

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) return;
            if (GameGrid.I == null || !GameGrid.I.IsReady) return;

            int sx = GameGrid.I.Width;
            int sz = GameGrid.I.Height;
            if (sx * sz > 2500) return; // skip gizmos for large grids

            Gizmos.color = Color.white;
            for (int x = 0; x < sx; x++)
                for (int z = 0; z < sz; z++)
                    Gizmos.DrawWireCube(GridToWorld(new Vector3Int(x, 0, z)), Vector3.one * positionMultiplier);
        }
    }
}
