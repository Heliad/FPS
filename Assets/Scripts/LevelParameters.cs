using System.Collections.Generic;
using UnityEngine;

public class LevelParameters
{
    static public int difficulty = 1;
    static public bool AmbientOcclusion = true;
    static public bool ScreenSpaceReflections = true;
    static public bool DepthOfField = true;
    static public bool ColorGrading = true;
    static public bool ChromaticAberration = true;
    static public bool Grain = true;
    static public bool MotionBlur = true;
    static public bool Bloom = true;

    static public bool sound = true;

    static public bool fullscreen = true;
    static public bool mode = false;

    public class Resolution
    {
        public int width;
        public int height;
        public string stringVal;

        public Resolution(int w, int h)
        {
            width = w;
            height = h;
            stringVal = w.ToString() + "x" + h.ToString();
        }
    }

    static public List<Resolution> resolutions = new List<Resolution>();

    static public Resolution currentResolution;
}
