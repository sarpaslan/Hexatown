using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapController : MonoBehaviour
{
    public Grid Grid;
    public Tilemap Background;
    public Tilemap Foreground;
    public Tilemap Placed;
    public Tilemap Map;
    private Camera m_camera;

    public Tile BackgroundShadowTile;
    public Tile BackgroundNullTile;
    /// <summary>
    /// Used for creating a tile with animations
    /// </summary>
    public SpriteRenderer RevealTile;
    /// <summary>
    /// Used for errors or extra visiualizations
    /// </summary>
    public static TileMapController Instance;

    public static List<Vector3Int> RevealedTiles = new List<Vector3Int>();

    public static List<Vector3Int> BlockedBackgroundPositions = new List<Vector3Int>();

    private UnityEngine.Pool.ObjectPool<SpriteRenderer> m_revealTilePool;

    static Vector3Int
        LEFT = new Vector3Int(-1, 0, 0),
        RIGHT = new Vector3Int(1, 0, 0),
        DOWN = new Vector3Int(0, -1, 0),
        DOWNLEFT = new Vector3Int(-1, -1, 0),
        DOWNRIGHT = new Vector3Int(1, -1, 0),
        UP = new Vector3Int(0, 1, 0),
        UPLEFT = new Vector3Int(-1, 1, 0),
        UPRIGHT = new Vector3Int(1, 1, 0);

    static Vector3Int[] DIRECTIONS_EVEN =
          { LEFT, RIGHT, DOWN, DOWNLEFT, UP, UPLEFT };
    static Vector3Int[] DIRECTIONS_ODD =
          { LEFT, RIGHT, DOWN, DOWNRIGHT, UP, UPRIGHT };

    public Action<Vector3Int> OnPlaced;

    public Action<Vector3Int, BiomeType> OnRevealed;

    public bool CanPlace(Vector3Int pos, Card card)
    {
        if (!card.CanPlace(pos))
            return false;
        if (!Background.HasTile(pos))
            return false;

        if (card.Type == PlaceType.CASTLE)
            return true;

        if (Placed.HasTile(pos))
        {
            return false;
        }
        if (!Foreground.HasTile(pos))
        {
            return false;
        }
        return Place.CanPlace(card, (Tile)Foreground.GetTile(pos));
    }
    void OnDestroy()
    {
        RevealedTiles.Clear();
    }
    private void Awake()
    {
        m_revealTilePool = new UnityEngine.Pool.ObjectPool<SpriteRenderer>(PoolOnCreateTile, actionOnRelease: PoolOnTileRelease, actionOnGet: PoolOnTileGet);
        m_camera = FindObjectOfType<Camera>();
        if (m_camera == null)
        {
            throw new NullReferenceException("Camera is null");
        }
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            throw new Exception("Multiple tilemap controller instance found. Only one allowed");
        }
        Instance = this;
        void PoolOnTileGet(SpriteRenderer renderer)
        {
            renderer.gameObject.SetActive(true);
        }
        void PoolOnTileRelease(SpriteRenderer renderer)
        {
            renderer.gameObject.SetActive(false);
        }
        SpriteRenderer PoolOnCreateTile()
        {
            return Instantiate(RevealTile);
        }
    }

    public void SetBackgroundColor(Vector3Int position, Color color)
    {
        Background.SetColor(position, color);
    }

    public async UniTask RevealAreaAsync(Vector3Int position, int depth)
    {
        if (depth > 6)
            throw new InvalidOperationException("Depth is to much to handle.");

        if (BlockedBackgroundPositions.Contains(position))
            return;


        List<Vector3Int> revealPositions = GetRevealPositions(position, depth).OrderBy(t => Vector3.Distance(position, t)).ToList();

        foreach (var rev in revealPositions)
        {
            if (BlockedBackgroundPositions.Contains(rev))
                continue;
            if (Foreground.HasTile(rev))
                continue;

            var mapTile = (Tile)Map.GetTile(rev);

            var revBiome = Biome.GetBiome(mapTile);
            Biome.UnlockBiome(revBiome);

            GameController.Resources.SoundPlayer.Play("reveal");
            RevealAsync(rev, revBiome).Forget();
            await UniTask.Delay(UnityEngine.Random.Range(50, 150), true);
        }
        OnPlaced?.Invoke(position);
    }

    private HashSet<Vector3Int> GetRevealPositions(Vector3Int position, int depth)
    {
        HashSet<Vector3Int> revealPositions = new HashSet<Vector3Int>();

        if (depth <= 0)
            return revealPositions;

        var neighbors = GetNeighbours(position);
        foreach (var neighbor in neighbors)
        {
            revealPositions.Add(neighbor);

            if (depth > 1)
            {
                revealPositions.AddRange(GetRevealPositions(neighbor, depth - 1));
            }
        }

        return revealPositions;
    }
    public async UniTask RevealAsync(Vector3Int position, BiomeType biome)
    {
        var gm = m_revealTilePool.Get();
        var center = Map.CellToWorld(position);
        gm.transform.position = new Vector3(center.x, center.y - GameConfig.MAGIC_Y + 0.03f, 1f);
        var sp = Map.GetSprite(position);
        var sprite = gm.GetComponent<SpriteRenderer>();
        sprite.sprite = sp;
        Background.SetTile(position, BackgroundShadowTile);
        var tile = Map.GetTile(position);
        await UniTask.Delay(400, true);
        Foreground.SetTile(position, tile);
        m_revealTilePool.Release(gm);
        OnRevealed?.Invoke(position, biome);
        RevealedTiles.Add(position);
        /*
        */
    }

    public void Reveal(Vector3Int r)
    {
        Background.SetTile(r, BackgroundShadowTile);
        var tile = Map.GetTile(r);
        Foreground.SetTile(r, tile);
        RevealedTiles.Add(r);
    }

    public Tile GetTileAt(Vector3Int position)
    {
        return Placed.GetTile(position) as Tile;
    }
    public void SetTile(Vector3Int position, TileBase tileBase)
    {
        Placed.SetTile(position, tileBase);
    }

    public IEnumerable<Vector3Int> GetNeighbours(Vector3Int node)
    {
        Vector3Int[] directions = (node.y % 2) == 0 ?
             DIRECTIONS_EVEN :
             DIRECTIONS_ODD;
        foreach (var direction in directions)
        {
            Vector3Int neighborPos = node + direction;
            yield return neighborPos;
        }
    }

    public Vector3Int GetCellScreen(Vector3 screenPos)
    {
        Vector3 mouseWorldPos = m_camera.ScreenToWorldPoint(screenPos);
        return Grid.WorldToCell(mouseWorldPos);
    }
    public Vector3Int GetCellWorld(Vector3 worldPos)
    {
        return Grid.WorldToCell(worldPos);
    }

    public void CheckRevealedTiles()
    {
        int minX = int.MaxValue;
        int minY = int.MaxValue;

        int maxX = int.MinValue;
        int maxY = int.MinValue;

        foreach (Place place in GameController.Places)
        {
            var pos = place.Position;

            Vector3Int[] directions = (pos.x % 2 == 0) ? DIRECTIONS_EVEN : DIRECTIONS_ODD;

            foreach (Vector3Int dir in directions)
            {
                Vector3Int targetPos = pos + dir * place.Card.RevealDepth;

                if (maxX < targetPos.x) maxX = targetPos.x;
                if (maxY < targetPos.y) maxY = targetPos.y;
                if (minX > targetPos.x) minX = targetPos.x;
                if (minY > targetPos.y) minY = targetPos.y;
            }
        }

        for (int i = 0; i < RevealedTiles.Count; i++)
        {
            Vector3Int pos = RevealedTiles[i];
            if (!(pos.x < minX || pos.x > maxX || pos.y < minY || pos.y > maxY))
            {
                continue;
            }

            bool shouldBeRevealed = false;

            if (Placed.HasTile(pos))
            {
                shouldBeRevealed = true;
            }
            else
            {
                foreach (var n in GetNeighbours(pos))
                {
                    if (Placed.HasTile(n))
                    {
                        shouldBeRevealed = true;
                        break;
                    }
                }
            }
            if (!shouldBeRevealed)
            {
                Foreground.SetTile(pos, null);
                Background.SetTile(pos, BackgroundNullTile);
                RevealedTiles.Remove(pos);
            }
        }
    }
}
