using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Video;


namespace BL_Grid
{
    public sealed class Tile
    {
        public readonly Vector2Int Coordinates;

        // fields (expand as needed)
        public byte type;   
        public byte flags; 

        public Tile(int x, int y)
        {
            this.Coordinates = new Vector2Int(x, y);
            type = 0;
            flags = 0;
        }

        public override string ToString() => $"Tile({Coordinates.x},{Coordinates.y}) type={type}";
    }


    public sealed class Grid : MonoBehaviour
    {
        public GameObject Drone, Hub;
        public static Grid I { get; private set; }

        public List<GridEntityData> entities;

        public List<Drone> Drones = new List<Drone>();

        [Header("Grid Size (Backend)")]
        [SerializeField, UnityEngine.Range(1, 200)] public int Width = 21;
        [SerializeField, UnityEngine.Range(1, 200)] public int Height = 21;

        [Header("Init Behavior")]
        [SerializeField] private bool initOnAwake = true;
        [SerializeField] private bool rebuildOnValidate = true;

    

        [SerializeField, HideInInspector] private Tile[] tiles;

        public bool IsReady => tiles != null && tiles.Length == Width * Height;

        private void Awake()
        {
            if (I != null && I != this)
            {
                Destroy(gameObject);
                return;
            }

            I = this;

            if (initOnAwake)
                Rebuild(Width, Height);
        }

        private void OnDestroy()
        {
            if (I == this) I = null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            Width = Mathf.Max(1, Width);
            Height = Mathf.Max(1, Height);

            if (rebuildOnValidate)
            {
                // In edit mode, keep it synced for debugging.
                Rebuild(Width, Height);
                GridView.I?.Rebuild(force: false);
            }
        }
#endif

        /// <summary>
        /// Rebuilds the grid and recreates all tiles.
        /// </summary>
        public void Rebuild(int newWidth, int newHeight)
        {
            Width = Mathf.Max(1, newWidth);
            Height = Mathf.Max(1, newHeight);

            tiles = new Tile[Width * Height];

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    tiles[ToIndex(x, y)] = new Tile(x, y);
        }

        // Optional: keep a static-style API for convenience
        public static void Init(int newWidth, int newHeight, bool overwrite = true)
        {
            if (I == null)
            {
                Debug.LogError("No Grid instance in scene. Add a Grid component to a GameObject first.");
                return;
            }

            if (!overwrite && I.IsReady)
            {
                Debug.LogWarning("Grid.Init called but grid already exists (overwrite=false).");
                return;
            }

            I.Rebuild(newWidth, newHeight);
        }

        public GameObject SpawnEntity(GameObject entity, Vector2Int position)
        {
            GameObject spawnIns = Instantiate(entity, transform.parent);
            spawnIns.GetComponent<GridEntity>().Data.Position = position;
            return spawnIns;
        }

        void Start()
        {
            Drones.Add(SpawnEntity(Drone, new Vector2Int(Width / 2, Height / 2)).GetComponent<Drone>());
            SpawnEntity(Hub, new Vector2Int(Width / 2, Height / 2));
        }






        public int ToIndex(int x, int y) => x + y * Width;

        public bool InBounds(int x, int y)
            => (uint)x < (uint)Width && (uint)y < (uint)Height;

        public Tile Get(int x, int y)
        {
            if (!IsReady) throw new System.InvalidOperationException("Grid not initialized. Call Rebuild/Init or enable initOnAwake.");
            if (!InBounds(x, y)) return null;
            return tiles[ToIndex(x, y)];
        }

        public void Set(int x, int y, Tile tile)
        {
            if (!IsReady) throw new System.InvalidOperationException("Grid not initialized. Call Rebuild/Init or enable initOnAwake.");
            if (!InBounds(x, y)) return;
            tiles[ToIndex(x, y)] = tile;
        }

        public Tile[] RawTiles => tiles;


        public Vector2Int WrapPosition(Vector2Int position)
        {
            int x = (position.x % Width + Width) % Width;
            int y = (position.y % Height + Height) % Height;
            return new Vector2Int(x, y);
        }
    }
}
