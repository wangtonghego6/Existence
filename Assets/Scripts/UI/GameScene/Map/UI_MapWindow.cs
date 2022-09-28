using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JKFrame;

/// <summary>
/// UI��ͼ����
/// </summary>
[UIElement(true, "UI/UI_MapWindow",4)]
public class UI_MapWindow : UI_WindowBase
{
    [SerializeField] private RectTransform content; // ���е�ͼ�顢Icon��ʾ�ĸ�����
    private float contentSize;

    [SerializeField] private GameObject mapItemPrefab; // ������ͼ����UI�е�Ԥ����
    [SerializeField] private GameObject mapIconPrefab; // ����Icon��UI�е�Ԥ����
    [SerializeField] private RectTransform playerIcon; // �������λ�õ�Icon

    private Dictionary<ulong, Image> mapObjectIconDic = new Dictionary<ulong, Image>(); // ���еĵ�ͼ����Icon�ֵ�
    private float mapChunkImageSize;  // UI��ͼ��ĳߴ�
    private int mapChunkSize;   // һ����ͼ���ж��ٸ�����   
    private float mapSizeOnWorld;// 3D��ͼ�������е�����
    private Sprite forestSprite;// ɭ�ֵؿ�ľ���

    private float minScale;         // ��С�ķŴ���
    private float maxScale  = 10;   // ���ķŴ���

    public override void Init()
    {
        transform.Find("Scroll View").GetComponent<ScrollRect>().onValueChanged.AddListener(UpdatePlayerIconPos);
    }
    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            float newScale = Mathf.Clamp(content.localScale.x + scroll, minScale, maxScale);
            content.localScale = new Vector3(newScale, newScale, 0);
        }
    }
    /// <summary>
    /// ��ʼ����ͼ
    /// </summary>
    /// <param name="mapSize">һ����ͼһ�л�һ���ж��ٸ�Image/Chunk</param>
    /// <param name="mapSizeOnWord">��ͼ��������һ�л�һ���ж��</param>
    /// <param name="forestTexture">ɭ�ֵ���ͼ</param>
    public void InitMap(float mapSize,int mapChunkSize, float mapSizeOnWord,Texture2D forestTexture)
    { 
        this.mapSizeOnWorld = mapSizeOnWord;
        forestSprite = CreateMapSprite(forestTexture);
        this.mapChunkSize = mapChunkSize;
        // ���ݳߴ�
        contentSize = mapSizeOnWord * 10;
        content.sizeDelta = new Vector2(contentSize, contentSize);
        content.localScale = new Vector3(maxScale, maxScale, 1);
        // һ��UI��ͼ��ĳߴ�
        mapChunkImageSize = contentSize / mapSize;
        minScale = 1050f / contentSize;
    }

    /// <summary>
    /// �������ĵ㣬Ϊ��������ŵ�ʱ�����ĵ���������ڵ�����
    /// </summary>
    /// <param name="viewerPosition"></param>
    public void UpdatePivot(Vector3 viewerPosition)
    {
        float x = viewerPosition.x / mapSizeOnWorld;
        float y = viewerPosition.z / mapSizeOnWorld;
        // �޸�Content��ᵼ��Scroll Rect ����� ��ֵ�޸��¼�=��UpdatePlayerIconPos
        content.pivot = new Vector2(x, y);
    }

    public void UpdatePlayerIconPos(Vector2 value)
    {
        // ��ҵ�Icon��ȫ����Content�����ĵ�
        playerIcon.anchoredPosition3D = content.anchoredPosition3D;
    }

    /// <summary>
    /// ���һ����ͼ��
    /// </summary>
    public void AddMapChunk(Vector2Int chunkIndex,Serialization_Dic<ulong, MapObjectData> mapObjectDic,Texture2D texture = null)
    { 
        RectTransform mapChunkRect = Instantiate(mapItemPrefab,content).GetComponent<RectTransform>();
        // ȷ����ͼ���Image������Ϳ��
        mapChunkRect.anchoredPosition = new Vector2(chunkIndex.x * mapChunkImageSize, chunkIndex.y * mapChunkImageSize);
        mapChunkRect.sizeDelta = new Vector2(mapChunkImageSize, mapChunkImageSize);

        Image mapChunkImage = mapChunkRect.GetComponent<Image>();
        // ɭ�ֵ����
        if (texture == null)
        {
            mapChunkImage.type = Image.Type.Tiled;
            // ��������ש�ı�����Ҫ��һ��Image����ʾ �����ͼ���������ĸ�������
            // ��ͼ��Image�ı���
            float ratio = forestSprite.texture.width / mapChunkImageSize;
            // һ����ͼ�����ж��ٸ�����
            mapChunkImage.pixelsPerUnitMultiplier = mapChunkSize * ratio;
            mapChunkImage.sprite = forestSprite;
        }
        else mapChunkImage.sprite = CreateMapSprite(texture);

        // ��������ICON
        foreach (var item in mapObjectDic.Dictionary.Values)
        {
            AddMapObjectIcon(item);
        }
    }

    /// <summary>
    /// ���ɵ�ͼ����
    /// </summary>
    private Sprite CreateMapSprite(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    /// <summary>
    /// �Ƴ���ͼ����Icon
    /// </summary>
    public void RemoveMapObjectIcon(ulong mapObjectID)
    {
        if (mapObjectIconDic.TryGetValue(mapObjectID,out Image iconImg))
        {
            // ��Icon��Ϸ�����ӻض����
            iconImg.JKGameObjectPushPool();
            mapObjectIconDic.Remove(mapObjectID);
       }
    }

    /// <summary>
    /// ���һ����ͼ����Icon
    /// </summary>
    public void AddMapObjectIcon(MapObjectData mapObjectData)
    {
        MapObjectConfig config = ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, mapObjectData.ConfigID);
        if (config.MapIconSprite == null || config.IconSize<=0) return;
        GameObject go = PoolManager.Instance.GetGameObject(mapIconPrefab, content);
        Image iconImg = go.GetComponent<Image>();
        iconImg.sprite = config.MapIconSprite;
        iconImg.transform.localScale = Vector3.one * config.IconSize;
        float x = mapObjectData.Position.x * 10;       // ��Ϊ����Content�ĳߴ��ڳ�ʼ���Ѿ�*10������IconҲ��Ҫ����ͬ����ϵ��
        float y = mapObjectData.Position.z * 10;
        go.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
        mapObjectIconDic.Add(mapObjectData.ID, iconImg);   // ����Icon�ֵ�
    }

}
