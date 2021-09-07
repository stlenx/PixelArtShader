//----------------------------------||
//          Made by sariku          ||
//                                  ||
//          Copyright: Me           ||   
//          Licence: GPL2           ||
//                                  ||
//             For maker.           ||
//----------------------------------||

//For writing files
using System.IO;
//Use unity duh
using UnityEngine;
//For creating the custom inspector
using UnityEditor;

//Run inside the editor without needing to run the game
//Script must be attached to a camera
[ExecuteInEditMode, RequireComponent(typeof(Camera))]
public class ShaderHandler : MonoBehaviour
{
    //Folder to export new pallet to
    public string export = string.Empty;
    //Name for exported file
    public new string name = string.Empty;
    
    //Amount of pixels to show on screen
    public int pixelDensity = 256;
    //Texture to use for the color pallet
    public Texture2D colorPallet;
    
    //Internal color array generated from the given color pallet
    public Color[] pallet;
    //Internal material where the shader is loaded
    private Material _material;

    //Load the shader on load
    private void Awake()
    {
        //Load the stuff m8
        Load();
    }

    //Method to load everything
    public void Load()
    {
        //Initialize array with correct size
        pallet = new Color[colorPallet.width];

        //For each color in color pallet, save it to the array
        for (var x = 0; x < colorPallet.width; x++)
        {
            pallet[x] = colorPallet.GetPixel(x, 0);
        }
        
        //Load the shader
        _material = new Material(Shader.Find("Hidden/PixelArt"));
    }

    //Method used for extracting pallet into png file
    public void Extract()
    {
        //Create texture to write to
        var tex = new Texture2D(pallet.Length, 1, TextureFormat.RGB24, false);

        //Draw colors to each pixel
        for (var x = 0; x < colorPallet.width; x++)
        {
            tex.SetPixel(x, 0, pallet[x]);
        }
        //Apply changes
        tex.Apply();

        // Encode texture into PNG
        var bytes = tex.EncodeToPNG();

        //Save as .png file in the specified export location
        File.WriteAllBytes($"{Application.dataPath}{export}/{name}.png", bytes);
    }

    //Run shader on image render
    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        //Get screen aspect ratio for pixelation effect
        var aspectRatioData = Screen.height > Screen.width ? new Vector2((float)Screen.width / Screen.height, 1) : new Vector2(1, (float)Screen.height / Screen.width);
        
        //Set calculated aspect ration and desired pixel density
        _material.SetVector("_AspectRatioMultiplier", aspectRatioData);
        _material.SetInt("_PixelDensity", pixelDensity);

        //_ColorCount tells the shader the amount of colors to use (1 - 256)
        _material.SetInt("_ColorCount", pallet.Length);
        //Give the shader the color array
        _material.SetColorArray("_Colors", pallet);

        //Show image in screen with shader applied
        Graphics.Blit(src, dest, _material);
    }
}

//This is a custom inspector :O
[CustomEditor(typeof(ShaderHandler))]
public class CustomInspector : Editor
{
    //Color pallet array editor toggle
    private bool _showColors;
    //Color pallet toggle text
    private string _displayText = "Edit pallet";
    
    //Main boi
    public override void OnInspectorGUI()
    {
        //Get the juice
        var gui = (ShaderHandler)target;

        //Set the normal stuff
        gui.pixelDensity = EditorGUILayout.IntField("Pixel density", gui.pixelDensity);
        gui.colorPallet = (Texture2D) EditorGUILayout.ObjectField("Color Pallet", gui.colorPallet, typeof(Texture2D), false);

        //Add button for reloading shader and color pallet
        if(GUILayout.Button("Reload shader"))
        {
            gui.Load();
        }
        
        //Ew
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        
        //If the pallet editor button was pressed
        if(GUILayout.Button(_displayText))
        {
            //Toggle
            _displayText = _showColors ? "Edit pallet" : "Close editor";
            _showColors = !_showColors;
        }

        //If the pallet editor must be showned
        if (_showColors)
        {
            //Do magic
            serializedObject.Update();
            var myIterator = serializedObject.FindProperty("pallet");
            while (true)
            {
                var myRect = GUILayoutUtility.GetRect(0f, 16f);
                var showChildren = EditorGUI.PropertyField(myRect, myIterator);

                if (!myIterator.NextVisible(showChildren))
                {
                    break;
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
        
        EditorGUILayout.Space();
        
        //Horizontal stuff no way
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Path for exporting:");
        gui.export = EditorGUILayout.TextField(gui.export);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Name for exporting:");
        gui.name = EditorGUILayout.TextField(gui.name);
        EditorGUILayout.EndHorizontal();
        
        //Add button for extracting png
        if(GUILayout.Button("Extract PNG color pallet"))
        {
            //Lol JUST EXTRACT MATE
            gui.Extract();
        }
    }
}