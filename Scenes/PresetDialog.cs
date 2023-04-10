using Godot;
using System;

public class PresetDialog : WindowDialog
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    private TextEdit presetText;

    private JavaScriptObject presetLoadCallback;
    private JavaScriptObject presetCallbacks;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        presetText = GetNode<TextEdit>("PresetText");

        presetLoadCallback = JavaScript.CreateCallback(this, "OnPresetLoaded");
        presetCallbacks = JavaScript.GetInterface("gd_callbacks");

        
    }

    public void CopyToClipboard() {
        OS.Clipboard = presetText.Text;
    }

    public void PasteFromClipboard() {
        presetText.Text = OS.Clipboard;
    }

    public void SavePresetToFile() {
        JavaScript.DownloadBuffer(presetText.Text.ToUTF8(), "preset.txt", "text/plain");
    }

    public void LoadPresetFromFile() {
        JavaScript.Eval("loadData()");
    }

    public void OnPresetLoaded(Godot.Collections.Array parameters) {

    }
}
