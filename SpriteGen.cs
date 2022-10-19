using Godot;
using System;
using System.Collections.Generic;

public class SpriteGen : Node2D
{
	// Define the character as a collection of shapes, then draw the shapes.
	// Need to set up some variables or something so that it's not a pain in the ass to adjust the size of the character.
	// Current ideal is hopefully a character that fits within a 64x64 shape. This would be a 32x64 image mirrored on the x-axis.
    // Figuring out a way to make the character have an outline would be great. Maybe define a collection of Vector2 arrays with the shapes in them,
    //      then make outlines of them?

	private int size = 64;

    private List<Vector2[]> shapes;

    private RandomNumberGenerator random;

    [Export]
    private Dictionary<string, float> parameters;

    private ulong legSeed, torsoSeed, armSeed, headSeed;

    private Color[] palette;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        palette = new Color[4];
        palette[0] = new Color(1,1,1);
        palette[1] = new Color(0, 1, 1);
        palette[2] = new Color(0, 0, 1);
        palette[3] = new Color(0, 0, 0);
		shapes = new List<Vector2[]>();
        random = new RandomNumberGenerator();
        //random.Seed = 204603460;
        legSeed = 300;
        torsoSeed = 600;
        armSeed = 900;
        headSeed = 1200;
        //random.Seed = legSeed;
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

        File colors = new Godot.File();
        colors.Open("res://Palettes/bot16.gpl", File.ModeFlags.Read);
        // Skip the first three lines of text, to skip the palette description.
        colors.GetLine();
        colors.GetLine();
        colors.GetLine();
        colors.GetLine();
        List<String> colorCodes = new List<string>();

        while(true) {
            string nextLine = colors.GetLine();
            if(nextLine == "") break;
            //GD.Print(nextLine.Split(new string[] {"\t"}, StringSplitOptions.None)[3]);
            colorCodes.Add(nextLine.Split(new string[] {"\t"}, StringSplitOptions.None)[3]);
        }

        colors.Close();

        palette = new Color[4];
        for(int i = 0; i < palette.Length; i++) {
            palette[i] = new Color(colorCodes[random.RandiRange(0, colorCodes.Count - 1)]);
        }

		// Start big, then progressively go smaller. Or maybe not?

        // Start with Legs.
        random.Seed = legSeed;
		float totalShapes = 8f;
		for(float i = 0; i < totalShapes; i++) {
			int vertices = random.RandiRange(3, 6);
			float randAngle = random.RandfRange(0, Mathf.Pi * 2);
            float randOffset = random.RandfRange(Mathf.Lerp(0f, 4f, i / totalShapes), Mathf.Lerp(3f, 10f, i / totalShapes));

			float lerpVal = i / totalShapes;
			float radiusMin = random.RandfRange(8f, 16f);
			float radiusMax = random.RandfRange(18f, 28f);
			Color color = new Color(palette[random.RandiRange(0, palette.Length - 1)]);
			Vector2 center = new Vector2(random.RandfRange(-24f, -8f), random.RandfRange(12f, 48f));
            DrawShape(vertices, center, radiusMin, radiusMax, color);
            //CreateShape(vertices, center, radiusMin, radiusMax, color);

		}
		// Then torso.
        random.Seed = torsoSeed;
		totalShapes = parameters["t_shapeNum"];
		for(float i = 0; i < totalShapes; i++) {
			int vertices = random.RandiRange(3, 6);
			float randAngle = random.RandfRange(0, Mathf.Pi * 2);
            float randOffset = random.RandfRange(Mathf.Lerp(0f, 4f, i / totalShapes), Mathf.Lerp(3f, 10f, i / totalShapes));

			float lerpVal = i / totalShapes;
			float radiusMin = parameters["t_radMin"];
			float radiusMax = parameters["t_radMax"];
			Color color = new Color(palette[random.RandiRange(0, palette.Length - 1)]);
			Vector2 center = new Vector2(random.RandfRange(parameters["t_width"], 0f), random.RandfRange(parameters["t_heightMin"], parameters["t_heightMax"]));
            DrawShape(vertices, center, radiusMin, radiusMax, color);
            //CreateShape(vertices, center, radiusMin, radiusMax, color);

		}
		// Arms are next.
        random.Seed = armSeed;
		totalShapes = 8f;
		for(float i = 0; i < totalShapes; i++) {
			int vertices = random.RandiRange(3, 6);
			float randAngle = random.RandfRange(0, Mathf.Pi * 2);
            float randOffset = random.RandfRange(Mathf.Lerp(0f, 4f, i / totalShapes), Mathf.Lerp(3f, 10f, i / totalShapes));

			float lerpVal = i / totalShapes;
			float radiusMin = 12f; // REPLACE WITH DICTIONARY VALUE
			float radiusMax = 24f; // REPLACE WITH DICT VALUE
			Color color = new Color(palette[random.RandiRange(0, palette.Length - 1)]);
			Vector2 center = new Vector2(random.RandfRange(-64f, -24f), random.RandfRange(-24f, 24f));
            DrawShape(vertices, center, radiusMin, radiusMax, color);
            //CreateShape(vertices, center, radiusMin, radiusMax, color);

		}
        // So, head?
        random.Seed = headSeed;
		totalShapes = 8f;
		for(float i = 0; i < totalShapes; i++) {
			int vertices = random.RandiRange(3, 6);
			float randAngle = random.RandfRange(0, Mathf.Pi * 2);
            //float randOffset = random.RandfRange(Mathf.Lerp(0f, 4f, i / totalShapes), Mathf.Lerp(3f, 10f, i / totalShapes));

			//float lerpVal = i / totalShapes;
			float radiusMin = 8f; // REPLACE WITH DICTIONARY VALUE
			float radiusMax = 16f; // REPLACE WITH DICT VALUE
			Color color = new Color(palette[random.RandiRange(0, palette.Length - 1)]);
			Vector2 center = new Vector2(random.RandfRange(-24f, 0f), random.RandfRange(-28f, -40f));
            DrawShape(vertices, center, radiusMin, radiusMax, color);
            //CreateShape(vertices, center, radiusMin, radiusMax, color);

		}
        

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

	public void DrawNewSprite() {
        random.Randomize();
        legSeed = (ulong)random.RandiRange(0, int.MaxValue);
        torsoSeed = (ulong)random.RandiRange(0, int.MaxValue);
        armSeed = (ulong)random.RandiRange(0, int.MaxValue);
        headSeed = (ulong)random.RandiRange(0, int.MaxValue);
		Update();
	}

	public void DrawShape(int vertexCount, Vector2 center, float radiusMin, float radiusMax, Color color) {
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

        DrawPolygon(points, colors);
		DrawPolygon(mirrorPoints, colors);
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

    public void ChangeParameter(float newValue, string paramName) {
        parameters[paramName] = newValue;
        Update();
    }
}
