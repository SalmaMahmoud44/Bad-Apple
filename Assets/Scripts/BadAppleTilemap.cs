using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Video;

public class BadAppleTilemap : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileBase lightTile;
    [SerializeField] private TileBase darkTile;

    [Header("Resolution (keep 4:3)")]
    [SerializeField] private int width = 64;
    [SerializeField] private int height = 48;

    [SerializeField, Range(0f, 1f)] private float threshold = 0.5f;

    private RenderTexture smallTexture;
    private Texture2D readTexture;
    private TileBase[] tiles;
    private BoundsInt area;

    private void Start()
    {
        if (videoPlayer == null || tilemap == null || lightTile == null || darkTile == null)
        {
            Debug.LogError("BadAppleTilemap: assign VideoPlayer, Tilemap, Light Tile and Dark Tile in the Inspector.");
            enabled = false;
            return;
        }

        smallTexture = new RenderTexture(width, height, 0);
        readTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tiles = new TileBase[width * height];
        area = new BoundsInt(0, 0, 0, width, height, 1);

        videoPlayer.sendFrameReadyEvents = true;
        videoPlayer.frameReady += OnFrameReady;
        videoPlayer.errorReceived += OnVideoError;
        videoPlayer.Play();
    }

    private void OnFrameReady(VideoPlayer source, long frameIndex)
    {
        Graphics.Blit(source.texture, smallTexture);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = smallTexture;
        readTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        RenderTexture.active = previous;

        Color32[] pixels = readTexture.GetPixels32();
        byte limit = (byte)(threshold * 255);

        for (int i = 0; i < pixels.Length; i++)
        {
            tiles[i] = pixels[i].r > limit ? lightTile : darkTile;
        }

        tilemap.SetTilesBlock(area, tiles);
    }

    private void OnVideoError(VideoPlayer source, string message)
    {
        Debug.LogError("Video error: " + message);
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.frameReady -= OnFrameReady;
            videoPlayer.errorReceived -= OnVideoError;
        }

        if (smallTexture != null) smallTexture.Release();
        if (readTexture != null) Destroy(readTexture);
    }
}