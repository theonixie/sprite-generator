using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// This class generates new sprites based on a given set of parameters.
/// </summary>
public class SpriteGen : Node2D
{
	// Define the character as a collection of shapes, then draw the shapes.
	// Need to set up some variables or something so that it's not a pain in the ass to adjust the size of the character.
	// Current ideal is hopefully a character that fits within a 64x64 shape. This would be a 32x64 image mirrored on the x-axis.
    // Figuring out a way to make the character have an outline would be great. Maybe define a collection of Vector2 arrays with the shapes in them,
    //      then make outlines of them?

    private struct LimbGroup {
        public ulong[] seeds;

        public Dictionary<string, float> parameters;

        public LimbGroup(int seedCount, float ts, float rMin, float rMax, float xMi, float xMa, float yMi, float yMa, float symm) {
            parameters = new Dictionary<string, float>();
            
            seeds = new ulong[seedCount];
            parameters.Add("totalShapes", ts);
            parameters.Add("radMin", rMin);
            parameters.Add("radMax", rMax);
            parameters.Add("xMin", xMi);
            parameters.Add("xMax", xMa);
            parameters.Add("yMin", yMi);
            parameters.Add("yMax", yMa);
            parameters.Add("xSymmetry", symm);
        }
    }

    /// <summary> The overall size of the sprite. </summary>
	private int size = 64;

    /// <summary> How many sprites to generate. MUST BE A NUMBER THAT CAN BE SQUARE-ROOTED. </summary>
    private int totalSprites = 16;
    /// <summary> A list of Vector2 arrays that define the points of each shape.<br/>
    /// Each individual Vector2 array represents a single unique shape. </summary>
    private List<Vector2[]> shapes;
    /// <summary> Used to randomly generate shapes and colors. </summary>
    private RandomNumberGenerator random;
    /// <summary> Contains each of the parameters for each limb group. </summary>
    private Dictionary<string, LimbGroup> parameters;
    /// <summary> The randomizer seeds for the colors of each sprite in a batch. </summary>
    private ulong[] colorSeeds;
    /// <summary> How many colors each sprite should have. </summary>
    private int totalColors = 4;
    /// <summary> The list of hexadecimal color codes loaded from a GPL file. </summary>
    private List<String> colorCodes;
    /// <summary> The palette of colors used for this sprite. </summary>
    private Color[] palette;
    /// <summary> The node containing the parameter list. LimbGroup editors are added to this node.</summary>
    private Node menuListNode;

    private Label paletteNameLabel;
    private FileDialog loadPaletteDialog;

    private AcceptDialog presetDialog;
    private TextEdit presetTextEdit;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        // We add menu panels to this object whenever we create a limb group.
        menuListNode = GetNode<Node>("UI/Menus/ScrollPanel/ParamList");

        parameters = new Dictionary<string, LimbGroup>();
        // Note: int seedCount, float ts, float rMin, float rMax, float xMi, float xMa, float yMi, float yMa, float symm
        parameters.Add("legs", new LimbGroup(totalSprites, 8, 12, 24, -24, -8, 24, 48, 1));
        parameters.Add("torso", new LimbGroup(totalSprites, 12, 20, 21, -27, 0, -24, 24, 1));
        parameters.Add("arms", new LimbGroup(totalSprites, 8, 12, 24, -44, -34, -24, 24, 1));
        parameters.Add("head", new LimbGroup(totalSprites, 8, 21, 34, -19, 9, -28, -40, 1));
        parameters.Add("face", new LimbGroup(totalSprites, 6, 9, 17, -32, 6, -45, -25, 1));
        CreateLimbEditorPanel("torso");
        CreateLimbEditorPanel("head");
        CreateLimbEditorPanel("face");
        CreateLimbEditorPanel("arms");
        CreateLimbEditorPanel("legs");

		shapes = new List<Vector2[]>();
        random = new RandomNumberGenerator();

        // This loop randomizes the seeds for every sprite that is to be generated.
        for(int k = 0; k < totalSprites; k++) {
            foreach(KeyValuePair<string, LimbGroup> group in parameters) {
                group.Value.seeds[k] = (ulong)random.RandiRange(0, int.MaxValue); // Set the seed for this sprite to a random value.
            }
        }

        colorSeeds = new ulong[totalSprites];

        // Create a random collection of color seeds.
        for(int i = 0; i < totalSprites; i++) {
            colorSeeds[i] = (ulong)random.RandiRange(0, int.MaxValue);
        }

        paletteNameLabel = GetNode<Label>("UI/Menus/ScrollPanel/ParamList/ColorSelect/paletteLabel");
        //LoadColors("res://Palettes/jewel-tone-appalachia28.gpl");
        LoadColors("res://Palettes/sk-24.gpl");

        loadPaletteDialog = GetNode<FileDialog>("UI/Menus/PaletteLoadDialog");

        presetDialog = GetNode<AcceptDialog>("UI/Menus/PresetDialog");
        presetTextEdit = GetNode<TextEdit>("UI/Menus/PresetDialog/PresetText");
	}

	public override void _Draw() {
		// Start big, then progressively go smaller. Or maybe not?
        int rowWidth = (int)Mathf.Sqrt(totalSprites);
        // Generate each limb group for each individual sprite, then generate the next group, then the next one, then the next one.
        for(int k = 0; k < totalSprites; k++) {
            random.Seed = colorSeeds[k];
            // TODO: make it so the amount of colors can be modified during runtime.
            palette = new Color[totalColors]; // Stores each of the colors that the generated sprite can use.
            // Randomly pick a color from the color list for each slot of the palette array.
            for(int i = 0; i < palette.Length; i++) {
                palette[i] = new Color(colorCodes[random.RandiRange(0, colorCodes.Count - 1)]);
            }

            foreach(KeyValuePair<string, LimbGroup> group in parameters) {
                random.Seed = group.Value.seeds[k]; // Set the randomizer to this limb group's seed.
                for(float i = 0; i < group.Value.parameters["totalShapes"]; i++) {
                    int vertices = random.RandiRange(3, 6); // Randomly determine how many vertices this shape has.
                    Color color  = new Color(palette[random.RandiRange(0, palette.Length - 1)]); // Randomly pick a color for this shape from the palette.
                    Vector2 center = new Vector2(random.RandfRange(group.Value.parameters["xMin"], group.Value.parameters["xMax"]), random.RandfRange(group.Value.parameters["yMin"], group.Value.parameters["yMax"]));
                    center += new Vector2((k % rowWidth) * 144f, (k / rowWidth) * 144f);
                    DrawShape(vertices, center, (k % rowWidth) * 144f, (k / rowWidth) * 144f, group.Value.parameters["radMin"], group.Value.parameters["radMax"], color, group.Value.parameters["xSymmetry"] == 1); // Use the DrawShape function to generate the shape.
                }
            }
        }
	}

    /// <summary>
    /// Loads colors from a color file.
    /// </summary>
    /// <param name="filePath">The path leading to the color palette to load.</param>
    private void LoadColors(string filePath) {
        if(filePath.Substring(filePath.Length() - 3) != "gpl") return;

        File colors = new Godot.File(); // Create a file object to store the colors that are from the palette file.
        colors.Open(filePath, File.ModeFlags.Read); // Load a palette file.
        // Skip the first four lines of text, to skip the palette description.
        colors.GetLine();
        colors.GetLine();
        colors.GetLine();
        colors.GetLine();
        // Make a list that stores each of the hexadecimal color codes in the file.
        colorCodes = new List<string>();

        // Loop through the palette file, grabbing each of the hex color codes on each line.
        while(true) {
            string nextLine = colors.GetLine();
            if(nextLine == "") break; // If we grabbed an empty line, we reached the end of the file and should exit immediately.
            // The hex color code is the fourth entry on each line, with each item being separated by tabs.
            // Grab the hex code and then add it to the color code list.
            colorCodes.Add(nextLine.Split(new string[] {"\t"}, StringSplitOptions.None)[3]);
        }

        colors.Close(); // Close the palette file to prevent memory leaks.
        paletteNameLabel.Text = "Palette: " + filePath.Split(new String[] {"/"}, StringSplitOptions.None)[3];
        Update();
    }

    public void OpenPaletteLoadMenu() {
        loadPaletteDialog.CurrentDir = "res://Palettes/";
        loadPaletteDialog.PopupCentered();
    }

    public void OpenPresetMenu() {
        presetDialog.PopupCentered();
        string presetData = "";
        foreach(KeyValuePair<string, LimbGroup> group in parameters) {
            if(presetData != "") presetData += "\n";
            presetData += group.Key;
            foreach(KeyValuePair<string, float> value in group.Value.parameters) {
                presetData += "\n" + value.Value;
            }
        }

        presetTextEdit.Text = presetData;
    }

    public void LoadPresetData() {
        string[] importText = presetTextEdit.Text.Split(new String[] {"\n"}, StringSplitOptions.None);
        Dictionary<string, LimbGroup> newParameters = new Dictionary<string, LimbGroup>();
        try {
            for(int i = 0; i < importText.Length; i += 9) {
                newParameters.Add(importText[i], new LimbGroup(totalSprites, importText[i+1].ToFloat(), importText[i+2].ToFloat(), importText[i+3].ToFloat(), importText[i+4].ToFloat(), importText[i+5].ToFloat(), importText[i+6].ToFloat(), importText[i+7].ToFloat(), importText[i+8].ToFloat()));
            }
            parameters.Clear();
            parameters = newParameters;
            DrawNewSprite();
        } catch(Exception e) {
            GD.Print("Attempted to load faulty data.");
        }

        // parameters.Clear();
        // parameters = newParameters;
        // DrawNewSprite();
        
    }

    public void LoadPresetData(string inText) {
        string[] importText = inText.Split(new String[] {"\n"}, StringSplitOptions.None);
        Dictionary<string, LimbGroup> newParameters = new Dictionary<string, LimbGroup>();
        try {
            for(int i = 0; i < importText.Length; i += 9) {
                newParameters.Add(importText[i], new LimbGroup(totalSprites, importText[i+1].ToFloat(), importText[i+2].ToFloat(), importText[i+3].ToFloat(), importText[i+4].ToFloat(), importText[i+5].ToFloat(), importText[i+6].ToFloat(), importText[i+7].ToFloat(), importText[i+8].ToFloat()));
            }
            parameters.Clear();
            parameters = newParameters;
            DrawNewSprite();
        } catch(Exception e) {
            GD.Print("Attempted to load faulty data.");
        }

        // parameters.Clear();
        // parameters = newParameters;
        // DrawNewSprite();
        
    }

    public void SetBackgroundColor(Color newColor) {
        VisualServer.SetDefaultClearColor(newColor);
    }

    /// <summary>
    /// Randomizes all shape parameters and then prompts the renderer update.
    /// </summary>
	public void DrawNewSprite() {
        random.Randomize();

        // Randomize the seed for each sprite to be drawn.
        for(int k = 0; k < totalSprites; k++) {
            foreach(KeyValuePair<string, LimbGroup> group in parameters) {
                group.Value.seeds[k] = (ulong)random.RandiRange(0, int.MaxValue); // Set the seed for this sprite to a random value.
            }
            colorSeeds[k] = (ulong)random.RandiRange(0, int.MaxValue);
        }
		Update();
	}

    public void RandomizeColors() {

        random.Randomize();
        // Randomize the color seed for each sprite to be drawn.
        for(int k = 0; k < totalSprites; k++) {
            colorSeeds[k] = (ulong)random.RandiRange(0, int.MaxValue);
        }
		Update();
    }

    /// <summary>
    /// Creates a randomized shape based on provided parameters.
    /// </summary>
    /// <param name="vertexCount">How many points make up this shape.</param>
    /// <param name="center">The center of this shape in 2D space.</param>
    /// <param name="mirrorCenter">Where the x-axis mirror is located.</param>
    /// <param name="radiusMin">The smallest distance a point can be from the shape's center.</param>
    /// <param name="radiusMax">The largest distance a point can be from the shape's center.</param>
    /// <param name="color">The color of this shape.</param>
	public void DrawShape(int vertexCount, Vector2 center, float mirrorCenter, float verticalCenter, float radiusMin, float radiusMax, Color color, bool isSymmetrical) {
        Vector2[] points = new Vector2[vertexCount]; // The points of this shape.
		Vector2[] mirrorPoints = new Vector2[vertexCount]; // The reflected version of this shape on the opposite side.
        Vector2[] sidePoints = new Vector2[vertexCount]; // The side profile of this shape.
        for(int i = 0; i < vertexCount; i++) {
            float angle = random.RandfRange(0, (1.0f / vertexCount) * Mathf.Pi * 2f);
            float radius = random.RandfRange(radiusMin, radiusMax);
            points[i].x = (radius * Mathf.Cos((((float)i / vertexCount) * Mathf.Pi * 2f) + angle)) + center.x;
            sidePoints[i].x = points[i].x + 160f - (center.x / 2f); // Add the side view off to the side, before doing the mirror effect.
            // If the side points is past the "center" of the sprite, divide it's distance by half.
            // Helps emulate having an actual curve to the body so it's not a direct clone of the front view.
            if(sidePoints[i].x > 160f) sidePoints[i].x -= (sidePoints[i].x - 160) / 2f;
            
			points[i].x = Mathf.Clamp(points[i].x, mirrorCenter - 64f, (isSymmetrical ? mirrorCenter : (mirrorCenter + 64f)));
            //sidePoints[i].x = Mathf.Clamp(sidePoints[i].x, 0f, 165f);
            points[i].y = (radius * Mathf.Sin((((float)i / vertexCount) * Mathf.Pi * 2f) + angle)) + center.y;
            points[i].y = Mathf.Clamp(points[i].y, verticalCenter - 64f, verticalCenter + 64f);
            sidePoints[i].y = points[i].y;

			// Now make the mirrored version of this shape.
            if(isSymmetrical) {
                mirrorPoints[i].x = mirrorCenter + ((points[i].x - mirrorCenter) * -1 );
                mirrorPoints[i].y = points[i].y; 
            }
            //GD.Print(points[i].x + " | " + points[i].y);
        }

        Color[] colors = new Color[] { color };

        DrawPolygon(points, colors);
        if(isSymmetrical)
		    DrawPolygon(mirrorPoints, colors);
        //DrawPolygon(sidePoints, colors);
        // Rect2 position = new Rect2(new Vector2(8, 0), new Vector2(8, 16));
        // Rect2 sourcePosition = new Rect2(Vector2.Zero, new Vector2(8, 16));
        // await ToSignal(GetTree(), "idle_frame");
        // Texture.DrawRectRegion(Texture.GetRid(), position, sourcePosition);
        
    }

    /// <summary>
    /// Changes a parameter of a specified LimbGroup.
    /// </summary>
    /// <param name="newValue">The new value of the parameter.</param>
    /// <param name="groupName">The LimbGroup to edit.</param>
    /// <param name="paramName">The name of the parameter to change.</param>
    public void ChangeParameter(float newValue, string groupName, string paramName) {
        parameters[groupName].parameters[paramName] = newValue;
        Update();
    }

    public void ToggleSymmetry(bool newValue, string groupName, string paramName) {
        parameters[groupName].parameters[paramName] = newValue ? 1 : 0;
        Update();
    }

    /// <summary>
    /// Changes how many colors each sprite has.
    /// </summary>
    /// <param name="newValue">The new total amount of colors.</param>
    public void ChangeColorCount(float newValue) {
        totalColors = (int) newValue;
        Update();
    }

    /// <summary>
    /// Creates a new category in the limb editor panel for this limb group.<br/>
    /// WARNING: Requires that the LimbEditor.tscn is formatted with specific names.
    /// </summary>
    /// <param name="groupName">The name of the group we are making an editor for.</param>
    public void CreateLimbEditorPanel(string groupName) {
        var menuPrefab = GD.Load<PackedScene>("res://Scenes/LimbEditor.tscn");
        var menuInstance = menuPrefab.Instance();
        menuListNode.AddChild(menuInstance);
        menuInstance.GetNode<Label>("LimbName").Text = groupName;

        menuInstance.GetNode<CheckBox>("XSym").Connect("toggled", this, "ToggleSymmetry", new Godot.Collections.Array() {groupName, "xSymmetry"});
        menuInstance.GetNode<CheckBox>("XSym").Pressed = parameters[groupName].parameters["xSymmetry"] == 1;
        menuInstance.GetNode<HSlider>("ShapeNum").Connect("value_changed", this, "ChangeParameter", new Godot.Collections.Array() {groupName, "totalShapes"});
        menuInstance.GetNode<HSlider>("ShapeNum").Value = parameters[groupName].parameters["totalShapes"];
        menuInstance.GetNode<HSlider>("RadMin").Connect("value_changed", this, "ChangeParameter", new Godot.Collections.Array() {groupName, "radMin"});
        menuInstance.GetNode<HSlider>("RadMin").Value = parameters[groupName].parameters["radMin"];
        menuInstance.GetNode<HSlider>("RadMax").Connect("value_changed", this, "ChangeParameter", new Godot.Collections.Array() {groupName, "radMax"});
        menuInstance.GetNode<HSlider>("RadMax").Value = parameters[groupName].parameters["radMax"];
        menuInstance.GetNode<HSlider>("xMin").Connect("value_changed", this, "ChangeParameter", new Godot.Collections.Array() {groupName, "xMin"});
        menuInstance.GetNode<HSlider>("xMin").Value = parameters[groupName].parameters["xMin"];
        menuInstance.GetNode<HSlider>("xMax").Connect("value_changed", this, "ChangeParameter", new Godot.Collections.Array() {groupName, "xMax"});
        menuInstance.GetNode<HSlider>("xMax").Value = parameters[groupName].parameters["xMax"];
        menuInstance.GetNode<HSlider>("yMin").Connect("value_changed", this, "ChangeParameter", new Godot.Collections.Array() {groupName, "yMin"});
        menuInstance.GetNode<HSlider>("yMin").Value = parameters[groupName].parameters["yMin"];
        menuInstance.GetNode<HSlider>("yMax").Connect("value_changed", this, "ChangeParameter", new Godot.Collections.Array() {groupName, "yMax"});
        menuInstance.GetNode<HSlider>("yMax").Value = parameters[groupName].parameters["yMax"];
    }
}
