using UnityEngine;
using UnityEditor;

/// <summary>  
/// 把Resource下的资源打包成.unity3d 到StreamingAssets目录下  
/// </summary>  
public class MapEditorTool : EditorWindow
{
    public float offsetX = 64f;
    public float offsetY = 34f;
    Vector2 scrollPos;
    Vector2 pos;
    string longString;

    [MenuItem("GOEditorTool/MapEditorTool %M")]
	public static void Init()
	{
        MapEditorTool window = (MapEditorTool)EditorWindow.GetWindow(typeof(MapEditorTool), false, "MapEditorTool", true);
        window.minSize = new Vector2(600.0f, 300.0f);
        window.wantsMouseMove = true;
        window.Show();
    }

    private const float kZoomMin = 0.1f;
    private const float kZoomMax = 10.0f;

    private readonly Rect _zoomArea = new Rect(0.0f, 75.0f, 1000.0f, 800f);//300.0f - 100.0f);
    private float _zoom = 1.0f;
    private Vector2 _zoomCoordsOrigin = Vector2.zero;

    TerrainDatas factionTerrainDatas;
    FactionBattleData battleData;
    MapNode[,] mapCellModels;
    public int[] directionFlag;
    int mapHeight;
    int mapWidth;

    private Vector2 ConvertScreenCoordsToZoomCoords(Vector2 screenCoords)
    {
        return (screenCoords - _zoomArea.TopLeft()) / _zoom + _zoomCoordsOrigin;
    }

    private void DrawZoomArea()
    {
        // Within the zoom area all coordinates are relative to the top left corner of the zoom area
        // with the width and height being scaled versions of the original/unzoomed area's width and height.
        EditorZoomArea.Begin(_zoom, _zoomArea);
        CreateMap();
        EditorZoomArea.End();
    }

    private void DrawNonZoomArea()
    {
        GUI.Box(new Rect(0.0f, 0.0f, 600.0f, 50.0f), "Adjust zoom of middle box with slider or mouse wheel.\nMove zoom area dragging with middle mouse button or Alt+left mouse button.");

        GUI.Label(new Rect(0.0f, 65.0f, 100, 20), "MapID");
        var mapID = GUI.TextArea(new Rect(60.0f, 65.0f, 100, 20), "1000");
        _zoom = EditorGUI.Slider(new Rect(0.0f, 90.0f, 600.0f, 25.0f), _zoom, kZoomMin, kZoomMax);
        if (GUI.Button(new Rect(180.0f, 65.0f, 200, 20), "Apply"))
        {
            factionTerrainDatas = DataManager.GetTerrainDatas();
            battleData = DataManager.GetFactionBattleData(1001);
            mapCellModels = DataManager.GetFactionMapData(1001);
            mapHeight = battleData.MapHeight;
            mapWidth = battleData.MapWidth;
        }
    }

    private void HandleEvents()
    {
        // Allow adjusting the zoom with the mouse wheel as well. In this case, use the mouse coordinates
        // as the zoom center instead of the top left corner of the zoom area. This is achieved by
        // maintaining an origin that is used as offset when drawing any GUI elements in the zoom area.
        if (Event.current.type == EventType.ScrollWheel)
        {
            Vector2 screenCoordsMousePos = Event.current.mousePosition;
            Vector2 delta = Event.current.delta;
            Vector2 zoomCoordsMousePos = ConvertScreenCoordsToZoomCoords(screenCoordsMousePos);
            float zoomDelta = -delta.y / 150.0f;
            float oldZoom = _zoom;
            _zoom += zoomDelta;
            _zoom = Mathf.Clamp(_zoom, kZoomMin, kZoomMax);
            _zoomCoordsOrigin += (zoomCoordsMousePos - _zoomCoordsOrigin) - (oldZoom / _zoom) * (zoomCoordsMousePos - _zoomCoordsOrigin);

            Event.current.Use();
        }

        // Allow moving the zoom area's origin by dragging with the middle mouse button or dragging
        // with the left mouse button with Alt pressed.
        if (Event.current.type == EventType.MouseDrag &&
            (Event.current.button == 0 && Event.current.modifiers == EventModifiers.Alt) ||
            Event.current.button == 2)
        {
            Debug.LogWarning(Event.current.pressure);
            //Vector2 delta = Event.current.delta;
            //delta /= _zoom;
            //_zoomCoordsOrigin += delta;

            //Event.current.Use();
        }
    }

    private void CreateMap()
    {
        if (battleData != null && mapCellModels != null && factionTerrainDatas != null)
        {
            for (int i = 0; i < battleData.MapHeight; i++)
            {
                for (int j = 0; j < battleData.MapWidth; j++)
                {
                    MapNode mapCell = mapCellModels[i, j];
                    if (mapCell.terrainId != 0)
                    {
                        TerrainData tmpTerrainData = factionTerrainDatas.dict[mapCell.terrainId];
                        mapCell.Init(tmpTerrainData);

                        pos = Vector2.zero;
                        pos.x = j * offsetX + i * offsetX + 100;
                        pos.y = -i * offsetY + j * offsetY + 1200;

                        var tmpName1 = tmpTerrainData.Image;
                        if (tmpName1 == null || tmpName1 == "")
                        {
                            tmpName1 = "terrain_1006";
                        }
                        Texture tex1 = (Texture)AssetDatabase.LoadAssetAtPath(string.Format("Assets/Editor/Textures/GridMapImage/{0}.png", tmpName1), typeof(Texture)); ;

                        Rect iconRect = new Rect(pos.x, 80 + pos.y, tex1.width, tex1.height);
                        GUI.DrawTexture(iconRect, tex1, ScaleMode.ScaleToFit);
                    }
                    else
                    {
                        pos = Vector2.zero;
                        pos.x = j * offsetX + i * offsetX + 100;
                        pos.y = -i * offsetY + j * offsetY + 1200;

                        var tmpName1 = "terrain_1006";

                        Texture tex1 = (Texture)AssetDatabase.LoadAssetAtPath(string.Format("Assets/Editor/Textures/GridMapImage/{0}.png", tmpName1), typeof(Texture)); ;

                        Rect iconRect = new Rect(pos.x, 80 + pos.y, tex1.width, tex1.height);
                        GUI.DrawTexture(iconRect, tex1, ScaleMode.ScaleToFit);
                    }
                }                
            }
        }
    }

    private void ConvertExcelToJson()
    {
        //SimpleJson.JsonArray data = new JsonArray();
        //FileStream stream = File.Open(GetExcelPath, FileMode.Open, FileAccess.Read);
        //IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
        //DataSet result = excelReader.AsDataSet();
        //int columns = result.Tables[0].Columns.Count;
        //int rows = result.Tables[0].Rows.Count;
    }


    public void OnGUI()
    {
        HandleEvents();
        // The zoom area clipping is sometimes not fully confined to the passed in rectangle. At certain
        // zoom levels you will get a line of pixels rendered outside of the passed in area because of
        // floating point imprecision in the scaling. Therefore, it is recommended to draw the zoom
        // area first and then draw everything else so that there is no undesired overlap.
        DrawZoomArea();
        DrawNonZoomArea();        
    }    
}
