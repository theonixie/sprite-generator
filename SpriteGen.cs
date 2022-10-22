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
        public ulong seed;
        /// <summary> How many shapes are part of this limb group.</summary>
        public float totalShapes;
        /// <summary> The smallest and largest radius a shape can have in this group.</summary>
        public float radMin, radMax;
        /// <summary> The left and right limits of where this limb group can be.</summary>
        public float xMin, xMax;
        /// <summary> The up and down limits of where this limb group can be.</summary>
        public float yMin, yMax;

        public LimbGroup(ulong s, float ts, float rMin, float rMax, float xMi, float xMa, float yMi, float yMa) {
            seed = s;
            totalShapes = ts;
            radMin = rMin;
            radMax = rMax;
            xMin = xMi;
            xMax = xMa;
            yMin = yMi;
            yMax = yMa;
        }
    }

    /// <summary> The overall size of the sprite. </summary>
	private int size = 64;

    /// <summary> A list of Vector2 arrays that define the points of each shape.<br/>
    /// Each individual Vector2 array represents a single unique shape. </summary>
    private List<Vector2[]> shapes;

    /// <summary> Used to randomly generate shapes and colors. </summary>
    private RandomNumberGenerator random;

    /// <summary> Stores each of the parameters that can be used by the generator.<br/>
    /// Each value is associated with a string key. </summary>
    [Export] public Dictionary<string, float> parameters;

    private Dictionary<string, LimbGroup> values;

    /// <summary> The seed a limb of the sprite.<br/>These seeds are randomized each time a new sprite is generated. </summary>
    private ulong colorSeed, legSeed, torsoSeed, armSeed, headSeed;

    /// <summary> The list of hexadecimal color codes loaded from a GPL file. </summary>
    private List<String> colorCodes;
    /// <summary> The palette of colors used for this sprite. </summary>
    private Color[] palette;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        // parameters = new Dictionary<string, float>();
        // parameters.Add("colors", 4f); // Total colors on the sprite.
        // parameters.Add("t_heightMin", -24);
        // parameters.Add("t_heightMax", 24);
        // parameters.Add("t_radMax", 32f);
        // parameters.Add("t_radMin", 8);
        // parameters.Add("t_shapeNum", 16f);
        // parameters.Add("t_width", -18);

        values = new Dictionary<string, LimbGroup>();
        values.Add("legs", new LimbGroup(300, 8, 12, 24, -24, -8, 12, 48));
        values.Add("torso", new LimbGroup(600, 16, 8, 32, -18, 0, -24, 24));
        values.Add("arms", new LimbGroup(900, 8, 12, 24, -64, -24, -24, 24));
        values.Add("head", new LimbGroup(1200, 8, 8, 16, -24, 0, -28, -40));

        palette = new Color[4];
        palette[0] = new Color(1,1,1);
        palette[1] = new Color(0, 1, 1);
        palette[2] = new Color(0, 0, 1);
        palette[3] = new Color(0, 0, 0);
		shapes = new List<Vector2[]>();
        random = new RandomNumberGenerator();
        //random.Seed = 204603460;
        colorSeed = 200;
        legSeed = 300;
        torsoSeed = 600;
        armSeed = 900;
        headSeed = 1200;
        //random.Seed = legSeed;
        //LoadColors("res://Palettes/jewel-tone-appalachia28.gpl");
        LoadColors("res://Palettes/sk-24.gpl");
	}

	public override void _Draw() {
        // // We want to start with big shapes and then continue to shrink down the shape size as we continue.
        // float totalShapes = 10;
        // for(float i = 0; i < totalShapes; i++) {
        //     int vertices = random.RandiRange(3, 8);
        //     float randAngle = random.RandfRange(0, Mathf.Pi * 2);
        //     float randOffset = random.RandfRange(Mathf.Lerp(0f, 4f, i / totalShapes), Mathf.Lerp(3f, 10f, i / totalShapes));
        //     Vector2 center = new Vector2();
        //     center.x = randOffset * Mathf.Cos(randAngle);
        //     center.y = randOffset * Mathf.Sin(randAngle);
        //     //center += new Vector2(50, 50);
        //     float radiusMin = random.RandfRange(Mathf.Lerp(5f, 2f, i / totalShapes),
        //                                             Mathf.Lerp(8f, 4f, i / totalShapes));
        //     float radiusMax = random.RandfRange(Mathf.Lerp(8f, 4f, i / totalShapes),
        //                                             Mathf.Lerp(10f, 6f, i / totalShapes));
        //     Color color = new Color(random.Randf(), random.Randf(), random.Randf());
        //     DrawShape(vertices, center, radiusMin, radiusMax, color);
        // }

        /**
         * TODO: Ideas for how to make the sprite generator support animation:
         * We already generate the individual shapes as groups, but the data doesn't *memorize* those groups.
         * If we can keep them in their separate categories, we should be able to animate and tweak them individually.
         */
        random.Seed = colorSeed;
        // TODO: make it so the amount of colors can be modified during runtime.
        palette = new Color[4]; // Stores each of the colors that the generated sprite can use.
        // Randomly pick a color from the color list for each slot of the palette array.
        for(int i = 0; i < palette.Length; i++) {
            palette[i] = new Color(colorCodes[random.RandiRange(0, colorCodes.Count - 1)]);
        }

		// Start big, then progressively go smaller. Or maybe not?
        
        // Generate each limb group for each individual sprite, then generate the next group, then the next one, then the next one.
        for(int k = 0; k < 4; k++) {
            // TODO: make it so the amount of colors can be modified during runtime.
            palette = new Color[4]; // Stores each of the colors that the generated sprite can use.
            // Randomly pick a color from the color list for each slot of the palette array.
            for(int i = 0; i < palette.Length; i++) {
                palette[i] = new Color(colorCodes[random.RandiRange(0, colorCodes.Count - 1)]);
            }

            foreach(KeyValuePair<string, LimbGroup> group in values) {
                for(float i = 0; i < group.Value.totalShapes; i++) {
                    int vertices = random.RandiRange(3, 6); // Randomly determine how many vertices this shape has.
                    Color color  = new Color(palette[random.RandiRange(0, palette.Length - 1)]); // Randomly pick a color for this shape from the palette.
                    Vector2 center = new Vector2(random.RandfRange(group.Value.xMin, group.Value.xMax), random.RandfRange(group.Value.yMin, group.Value.yMax) + (k * 128f));

                    DrawShape(vertices, center, group.Value.radMin, group.Value.radMax, color); // Use the DrawShape function to generate the shape.
                }
            }
        }
        // Start with Legs.
        // random.Seed = legSeed; // Set the randomizer's seed to the seed for this limb.
        // // TODO: make totalShapes a parameter in the dictionary.
		// float totalShapes = 8f; // This is how many shapes we want to generate for this limb.
		// for(float i = 0; i < totalShapes; i++) { // Generate one shape for each of the total shapes. Each new shape overlaps the previous.
		// 	int vertices = random.RandiRange(3, 6); // Randomly determine how many vertices this shape has.
		// 	float randAngle = random.RandfRange(0, Mathf.Pi * 2); // Randomly determine the angle of this shape.
        //     // TODO: Add dict parameters for radii.
		// 	float radiusMin = random.RandfRange(8f, 16f); // Randomly determine the smallest possible radius for this shape.
		// 	float radiusMax = random.RandfRange(18f, 28f);
		// 	Color color = new Color(palette[random.RandiRange(0, palette.Length - 1)]); // Randomly pick a color for this shape from the palette.
		// 	Vector2 center = new Vector2(random.RandfRange(-24f, -8f), random.RandfRange(12f, 48f)); // Randomly set a center.
        //     DrawShape(vertices, center, radiusMin, radiusMax, color); // Use the DrawShape function to generate the shape.
        //     //CreateShape(vertices, center, radiusMin, radiusMax, color);

		// }
		// // Then torso.
        // random.Seed = torsoSeed;
		// totalShapes = parameters["t_shapeNum"];
		// for(float i = 0; i < totalShapes; i++) {
		// 	int vertices = random.RandiRange(3, 6);
		// 	float randAngle = random.RandfRange(0, Mathf.Pi * 2);
		// 	float radiusMin = parameters["t_radMin"];
		// 	float radiusMax = parameters["t_radMax"];
		// 	Color color = new Color(palette[random.RandiRange(0, palette.Length - 1)]);
		// 	Vector2 center = new Vector2(random.RandfRange(parameters["t_width"], 0f), random.RandfRange(parameters["t_heightMin"], parameters["t_heightMax"]));
        //     DrawShape(vertices, center, radiusMin, radiusMax, color);
        //     //CreateShape(vertices, center, radiusMin, radiusMax, color);

		// }
		// // Arms are next.
        // random.Seed = armSeed;
		// totalShapes = 8f;
		// for(float i = 0; i < totalShapes; i++) {
		// 	int vertices = random.RandiRange(3, 6);
		// 	float randAngle = random.RandfRange(0, Mathf.Pi * 2);
		// 	float radiusMin = 12f; // REPLACE WITH DICTIONARY VALUE
		// 	float radiusMax = 24f; // REPLACE WITH DICT VALUE
		// 	Color color = new Color(palette[random.RandiRange(0, palette.Length - 1)]);
		// 	Vector2 center = new Vector2(random.RandfRange(-64f, -24f), random.RandfRange(-24f, 24f));
        //     DrawShape(vertices, center, radiusMin, radiusMax, color);
        //     //CreateShape(vertices, center, radiusMin, radiusMax, color);

		// }
        // // So, head?
        // random.Seed = headSeed;
		// totalShapes = 8f;
		// for(float i = 0; i < totalShapes; i++) {
		// 	int vertices = random.RandiRange(3, 6);
		// 	float randAngle = random.RandfRange(0, Mathf.Pi * 2);
		// 	float radiusMin = 8f; // REPLACE WITH DICTIONARY VALUE
		// 	float radiusMax = 16f; // REPLACE WITH DICT VALUE
		// 	Color color = new Color(palette[random.RandiRange(0, palette.Length - 1)]);
		// 	Vector2 center = new Vector2(random.RandfRange(-24f, 0f), random.RandfRange(-28f, -40f));
        //     DrawShape(vertices, center, radiusMin, radiusMax, color);
        //     //CreateShape(vertices, center, radiusMin, radiusMax, color);

		// }
        

        // foreach(Vector2[] points in shapes) {
        //     Color[] colors = new Color[] { new Color(0,0,0) };
        //     Vector2[] outline = new Vector2[Geometry.OffsetPolygon2d(points, 32f).Count];
        //     Geometry.OffsetPolygon2d(points, 32f).CopyTo(outline, 0);
        //     DrawPolygon(outline, colors);
        // }
        // foreach(Vector2[] points in shapes) {
        //     Color[] colors = new Color[] { new Color(random.Randf(), random.Randf(), random.Randf()) };
        //     //Vector2[] outline = new Vector2[points.Length];
        //     //Geometry.OffsetPolygon2d(points, 6f).CopyTo(outline, 0);
        //     DrawPolygon(points, colors);
        // }
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }

    /// <summary>
    /// Loads colors from a color file.
    /// </summary>
    /// <param name="filePath">The path leading to the color palette to load.</param>
    private void LoadColors(string filePath) {
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
    }

    /// <summary>
    /// Randomizes all shape parameters and then prompts the renderer update.
    /// </summary>
	public void DrawNewSprite() {
        random.Randomize();
        colorSeed = (ulong)random.RandiRange(0, int.MaxValue);
        legSeed = (ulong)random.RandiRange(0, int.MaxValue);
        torsoSeed = (ulong)random.RandiRange(0, int.MaxValue);
        armSeed = (ulong)random.RandiRange(0, int.MaxValue);
        headSeed = (ulong)random.RandiRange(0, int.MaxValue);
		Update();
	}

    /// <summary>
    /// Randomizes all shape parameters based on a given seed and then prompts the renderer update.
    /// </summary>
	public void DrawNewSprite(ulong randomizerSeed) {
        random.Seed = randomizerSeed;
        colorSeed = (ulong)random.RandiRange(0, int.MaxValue);
        legSeed = (ulong)random.RandiRange(0, int.MaxValue);
        torsoSeed = (ulong)random.RandiRange(0, int.MaxValue);
        armSeed = (ulong)random.RandiRange(0, int.MaxValue);
        headSeed = (ulong)random.RandiRange(0, int.MaxValue);
		Update();
	}

    /// <summary>
    /// Creates a randomized shape based on provided parameters.
    /// </summary>
    /// <param name="vertexCount">How many points make up this shape.</param>
    /// <param name="center">The center of this shape in 2D space.</param>
    /// <param name="radiusMin">The smallest distance a point can be from the shape's center.</param>
    /// <param name="radiusMax">The largest distance a point can be from the shape's center.</param>
    /// <param name="color">The color of this shape.</param>
	public void DrawShape(int vertexCount, Vector2 center, float radiusMin, float radiusMax, Color color) {
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
			points[i].x = Mathf.Clamp(points[i].x, -64f, 0f);
            //sidePoints[i].x = Mathf.Clamp(sidePoints[i].x, 0f, 165f);
            points[i].y = (radius * Mathf.Sin((((float)i / vertexCount) * Mathf.Pi * 2f) + angle)) + center.y;
            sidePoints[i].y = points[i].y;

			// Now make the mirrored version of this shape.
			mirrorPoints[i].x = -points[i].x;
			mirrorPoints[i].y = points[i].y; 
            //GD.Print(points[i].x + " | " + points[i].y);
        }

        Color[] colors = new Color[] { color };

        DrawPolygon(points, colors);
		DrawPolygon(mirrorPoints, colors);
        DrawPolygon(sidePoints, colors);
        // Rect2 position = new Rect2(new Vector2(8, 0), new Vector2(8, 16));
        // Rect2 sourcePosition = new Rect2(Vector2.Zero, new Vector2(8, 16));
        // await ToSignal(GetTree(), "idle_frame");
        // Texture.DrawRectRegion(Texture.GetRid(), position, sourcePosition);
        
    }

    public void CreateShape(int vertexCount, Vector2 center, float radiusMin, float radiusMax, Color color) {
        //RandomNumberGenerator random = new RandomNumberGenerator();
        //random.Randomize();
        // Vector2[] points = new Vector2[4];
        // points[0] = new Vector2(10, 10) + center;
        // points[1] = new Vector2(-10, 10) + center;
        // points[2] = new Vector2(-10, -10) + center;
        // points[3] = new Vector2(10, -10) + center;
        Vector2[] points = new Vector2[vertexCount];
		Vector2[] mirrorPoints = new Vector2[vertexCount];
        for(int i = 0; i < vertexCount; i++) {
            float angle = random.RandfRange(0, (1.0f / vertexCount) * Mathf.Pi * 2f);
            float radius = random.RandfRange(radiusMin, radiusMax);
            points[i].x = (radius * Mathf.Cos((((float)i / vertexCount) * Mathf.Pi * 2f) + angle)) + center.x;
			points[i].x = Mathf.Clamp(points[i].x, -64f, 0f);
            points[i].y = (radius * Mathf.Sin((((float)i / vertexCount) * Mathf.Pi * 2f) + angle)) + center.y;

			// Now make the mirrored version of this shape.
			mirrorPoints[i].x = -points[i].x;
			mirrorPoints[i].y = points[i].y; 
            //GD.Print(points[i].x + " | " + points[i].y);
        }

        Color[] colors = new Color[] { color };

        //DrawPolygon(points, colors);
		//DrawPolygon(mirrorPoints, colors);
        shapes.Add(points);
        shapes.Add(mirrorPoints);

        // Rect2 position = new Rect2(new Vector2(8, 0), new Vector2(8, 16));
        // Rect2 sourcePosition = new Rect2(Vector2.Zero, new Vector2(8, 16));
        // await ToSignal(GetTree(), "idle_frame");
        // Texture.DrawRectRegion(Texture.GetRid(), position, sourcePosition);
        
    }

    /// <summary>
    /// Changes the value of a specific parameter in the dictionary and then forces the renderer to redraw the frame.
    /// </summary>
    /// <param name="newValue">The new value this parameter should have.</param>
    /// <param name="paramName">The name of the parameter to change in the dictionary.</param>
    public void ChangeParameter(float newValue, string paramName) {
        parameters[paramName] = newValue;
        Update();
    }
}
