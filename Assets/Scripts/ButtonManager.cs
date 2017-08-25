using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    [SerializeField] GameObject NewGameOptions;
    [SerializeField] GameObject GraphicsOptions;
    [SerializeField] GameObject SoundOptions;
    [SerializeField] GameObject OptionsGM;
    [SerializeField] GameObject background;
    [SerializeField] GameObject loadingScreen;

    [SerializeField] Dropdown DropdownDifficulty;
    [SerializeField] Dropdown DropdownLevel;
    [SerializeField] Dropdown DropdownMode;
    [SerializeField] Dropdown dropdownResolution;

    [SerializeField] Button StartGameButton;
    [SerializeField] Button OptionsButton;
    [SerializeField] Button ExitButton;
    [SerializeField] GameObject Menu;

    [Header("Graphics Options")]
    [SerializeField] Toggle fullscreenToggle;
    [SerializeField] Toggle AmbientOcclusion;
    [SerializeField] Toggle ScreenSpaceReflections;
    [SerializeField] Toggle DepthOfField;
    [SerializeField] Toggle ColorGrading;
    [SerializeField] Toggle ChromaticAberration;
    [SerializeField] Toggle Grain;
    [SerializeField] Toggle MotionBlur;
    [SerializeField] Toggle Bloom;

    [Header("Sound Options")]
    [SerializeField] Toggle sound;

    List<string> res = new List<string>();

    void Start()
    {
        sound.isOn = LevelParameters.sound;
        
        Time.timeScale = 1;

        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
        }

        if (NewGameOptions != null)
        {
            NewGameOptions.SetActive(false);
        }
        else
        {
            Menu.SetActive(false);
            background.SetActive(false);
        }

        if (LevelParameters.resolutions.Count == 0)
        {
            LevelParameters.currentResolution = new LevelParameters.Resolution(Screen.currentResolution.width, Screen.currentResolution.height);

            List<string> res = new List<string>();
            foreach (Resolution item in Screen.resolutions)
            {
                string val = item.width.ToString() + "x" + item.height.ToString();

                if (!res.Contains(val))
                {
                    res.Add(val);
                    LevelParameters.resolutions.Add(new LevelParameters.Resolution(item.width, item.height));
                }
            }
        }

        res = new List<string>();

        foreach (var item in LevelParameters.resolutions)
        {
            res.Add(item.stringVal);
        }

        dropdownResolution.AddOptions(res);

        OptionsGM.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        VariablesSet();
    }

    void Update()
    {
       // Debug.Log("Button Manager: " + LevelParameters.sound);

        if (NewGameOptions == null)
        {
            if (Input.GetKey(KeyCode.Escape) && Time.timeScale == 1)
            {
                Time.timeScale = 0;
                Menu.SetActive(true);
                background.SetActive(true);
            }
        }
    }

    public void ChangeSound()
    {
        if (!LevelParameters.sound && sound.isOn)
        {
            LevelParameters.sound = true;
        }
        if (LevelParameters.sound && !sound.isOn)
        {
            LevelParameters.sound = false;
        }
    }

    public void StartGame()
    {
        NewGameOptions.SetActive(true);
        ButtonsInteractibleSwitch();
    }


    public void ExitNewGameOptions()
    {
        NewGameOptions.SetActive(false);
        ButtonsInteractibleSwitch();
    }

    public void Options()
    {
        OptionsGM.SetActive(true);
        ButtonsInteractibleSwitch();
    }

    public void ExitOptions()
    {
        OptionsGM.SetActive(false);
        ButtonsInteractibleSwitch();
        VariablesSet();
    }

    public void ToggleGraphicsOptions()
    {
        GraphicsOptions.SetActive(true);
        SoundOptions.SetActive(false);
    }

    void VariablesSet()
    {
        AmbientOcclusion.isOn = LevelParameters.AmbientOcclusion;
        ScreenSpaceReflections.isOn = LevelParameters.ScreenSpaceReflections;
        DepthOfField.isOn = LevelParameters.DepthOfField;
        ColorGrading.isOn = LevelParameters.ColorGrading;
        ChromaticAberration.isOn = LevelParameters.ChromaticAberration;
        Grain.isOn = LevelParameters.Grain;
        MotionBlur.isOn = LevelParameters.MotionBlur;
        Bloom.isOn = LevelParameters.Bloom;
        fullscreenToggle.isOn = LevelParameters.fullscreen;

        int ind = 0;

        for (int i = 0; i < LevelParameters.resolutions.Count; i++)
        {
            if (LevelParameters.resolutions[i].stringVal == LevelParameters.currentResolution.stringVal)
            {
                ind = i;
                break;
            }
        }

        dropdownResolution.value = ind;
    }

    public void ToggleSoundOptions()
    {
        GraphicsOptions.SetActive(false);
        SoundOptions.SetActive(true);
    }

    public void RestartLevel()
    {
        SceneManager.LoadSceneAsync("SkyCity");
    }

    public void LoadLevel()
    {
        loadingScreen.SetActive(true);
        LevelParameters.difficulty = DropdownDifficulty.value;
        if (DropdownMode.value == 1)
        {
            LevelParameters.mode = true;
        }
        else
        {
            LevelParameters.mode = false;
        }

        if (DropdownLevel.value == 0)
        {
            SceneManager.LoadSceneAsync("SkyCity");
        }
        else if (DropdownLevel.value == 1)
        {
            SceneManager.LoadSceneAsync("Scene1");
        }

    }

    void ButtonsInteractibleSwitch()
    {
        if (NewGameOptions != null)
        {
            StartGameButton.interactable = !StartGameButton.interactable;
            OptionsButton.interactable = !OptionsButton.interactable;
            ExitButton.interactable = !ExitButton.interactable;
        }
        else
        {
            Menu.SetActive(!Menu.activeSelf);
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void ContinueGame()
    {
        Time.timeScale = 1;
        Menu.SetActive(false);
        background.SetActive(false);
    }

    public void ExitMenu()
    {
        SceneManager.LoadScene("Menu");    
    }

    public void ApplyGraphicsChanges()
    {
        LevelParameters.AmbientOcclusion = AmbientOcclusion.isOn;
        LevelParameters.ScreenSpaceReflections = ScreenSpaceReflections.isOn;
        LevelParameters.DepthOfField = DepthOfField.isOn;
        LevelParameters.ColorGrading = ColorGrading.isOn;
        LevelParameters.ChromaticAberration = ChromaticAberration.isOn;
        LevelParameters.Grain = Grain.isOn;
        LevelParameters.MotionBlur = MotionBlur.isOn;
        LevelParameters.Bloom = Bloom.isOn;
        LevelParameters.fullscreen = fullscreenToggle.isOn;
        LevelParameters.currentResolution = LevelParameters.resolutions[dropdownResolution.value];
        Screen.SetResolution(LevelParameters.currentResolution.width, LevelParameters.currentResolution.height, true);
        Screen.SetResolution(LevelParameters.currentResolution.width, LevelParameters.currentResolution.height, LevelParameters.fullscreen);
    }
}
