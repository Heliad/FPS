using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.PostProcessing;

public class SceneInitializer : MonoBehaviour
{

    PostProcessingProfile profile;

    void Start ()
    {
        ApplyChanges();
	}

    public void ChangeOptions()
    {
        ApplyChanges();
    }

    void ApplyChanges()
    {
        profile = Camera.main.GetComponent<PostProcessingBehaviour>().profile;

        profile.motionBlur.enabled = LevelParameters.MotionBlur;
        profile.bloom.enabled = LevelParameters.Bloom;

        profile = GameObject.Find("Gun Camera").GetComponent<PostProcessingBehaviour>().profile;

        profile.ambientOcclusion.enabled = LevelParameters.AmbientOcclusion;
        profile.screenSpaceReflection.enabled = LevelParameters.ScreenSpaceReflections;
        profile.depthOfField.enabled = LevelParameters.DepthOfField;
        profile.colorGrading.enabled = LevelParameters.ColorGrading;
        profile.chromaticAberration.enabled = LevelParameters.ChromaticAberration;
        profile.grain.enabled = LevelParameters.Grain;

        if (!LevelParameters.sound && AudioListener.volume == 1)
        {
            AudioListener.volume = 0;
        }
        if (LevelParameters.sound && AudioListener.volume == 0)
        {
            AudioListener.volume = 1;
        }
    }
}
