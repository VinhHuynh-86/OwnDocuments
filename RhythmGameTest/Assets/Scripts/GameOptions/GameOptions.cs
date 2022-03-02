using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using gameoptions;

namespace gameoptions
{
    public class Effects
    {
        public int physicsMode = 0; //[0;2]
        public int lightingLimit = 0; //[0;3]
        public int particlesLimit = 0; //[0;3]
        public bool dof = false; //[;]
        public bool loadAllStadium = false; //[;]
        public bool texturesHightQuality = false; //[;]
        public Define.QUALITIES_LEVEL qualityLevel = Define.QUALITIES_LEVEL.ULTRA; //[0;5] : Very Low, Low, Medium, High, Very High, Ultra
    }
    public class GameOptions
    {
        public static string[] separatingStrings = { "$$", ">=", "<=", ">", "<", "="};
        public static string MODEL = "MODEL";
        public static string CORES = "CORES";
        public static string MEM_T = "MEM_T";
        public static string MANUF = "MANUF";
        public static string CPU = "CPU";
        public static string GPU = "GPU";
        public static string MEM = "MEM";
        public static JsonReader jsonReader = new JsonReader();
        //private IEnumerable<JSONNode> jsonHeader;
        //private IEnumerable<JSONNode> jsonProfile;
        public static JSONNode jsonHeader;
        public static JSONNode jsonProfile;
        public static CContentProvider contentProvider = new CContentProvider();
        public static Effects effects = new Effects();
        public static void UpdateValues()
        {
            //update values
            effects.physicsMode = contentProvider.GetValue("physics_Mode", 0);
            effects.lightingLimit = contentProvider.GetValue("lighting_maxQualityLimit", 0);
            effects.particlesLimit = contentProvider.GetValue("particles_maxQualityLimit", 0);
            effects.loadAllStadium = contentProvider.GetValue("load_allStadium", false);
            effects.dof = contentProvider.GetValue("dof", false);
        #if !UNITY_EDITOR
            effects.qualityLevel = (Define.QUALITIES_LEVEL)contentProvider.GetValue("qualityLevel", 2);
        #else
            effects.qualityLevel = (Define.QUALITIES_LEVEL)QualitySettings.GetQualityLevel();
        #endif

        #if UNITY_STANDALONE
            effects.qualityLevel = Define.QUALITIES_LEVEL.ULTRA;
            effects.dof = true;
        #endif
            effects.texturesHightQuality = contentProvider.GetValue("textures_hightQuality", false);
        }
        public static bool UseDOF()
        {
            return effects.dof;
        }
        public static void LoadProfiles()
        {
            // var header = N["header"].Children;
            // var cpu = N["profiles"]["CPU"].Children;
            // var gpu = N["profiles"]["GPU"].Children;
            // var mem = N["profiles"]["MEM"].Children;

            jsonReader.LoadDataJson("GameOptions/GameOptions");

            jsonHeader = jsonReader.jsonContent["header"];
            jsonProfile = jsonReader.jsonContent["profiles"];

            List<string> profileTypes = new List<string>();
            foreach(string key in jsonProfile.Keys)
            {
                // keys : CPU, GPU, MEM
                // each jsonProfile[key] contrain : defaults, selection, overrides, priority
                {
                    string profileName = GetOverrideProfile(jsonProfile[key], key);

                    LoadOptionDefaults(jsonProfile[key]);

                    if (profileName != "")
                    {
                        Debug.LogFormat(" *** profileName : {0} ***", profileName);
                        LoadOptionsOverrides(jsonProfile[key], profileName);
                    }
                }
            }

            UpdateValues();
            ApplyProfile();
        }

        public static string GetOverrideProfile(JSONNode jProfile, string profileType)
        {
            // Debug.Log("******** SystemInfo *******");
            // Debug.Log("deviceModel :" + SystemInfo.deviceModel); // cores number
            // Debug.Log("processorCount :" + SystemInfo.processorCount); // CPU cores number
            // Debug.Log("processorFrequency :" + SystemInfo.processorFrequency); // CPU Max Frequency
            // Debug.Log("systemMemorySize :" + SystemInfo.systemMemorySize); // RAM
            // Debug.Log("graphicsDeviceName :" + SystemInfo.graphicsDeviceName); // GPU's name
            // Debug.Log("graphicsMemorySize :" + SystemInfo.graphicsMemorySize); // GPU's MEM

            JSONNode jselection = jProfile["selection"];
            if(jselection == null)
            {
                return "";
            }
            string profileName = "";
            string profileName2 = "";
            foreach(JSONNode node in jselection.Children)
            {
                foreach(string key in node.Keys)
                {
                    // keys : CPU_X1, CPU_X2,...
                    //  or  : GPU_X1, GPU_X2,...
                    //  or  : MEM_X1, MEM_X2,...
                    JSONNode node2 = node[key];
                    if(node2 != null)
                    {
                        foreach(JSONNode node3 in node2.Children)  // It's an list of element list. One list of all is met
                        {
                            foreach(JSONNode node4 in node3.Children) // It's an element list. All conditions must be met
                            {
                                profileName2 = "";

                                if(node4.AsArray.Count > 0)
                                {
                                    string effect = node4.AsArray[0]; // It's just has one element
                                    string delimitedString = "";
                                    for(int i = 0; i < separatingStrings.Length; ++i)
                                    {
                                        if(effect.Contains(separatingStrings[i]))
                                        {
                                            delimitedString = separatingStrings[i];
                                            break;
                                        }
                                    }
                                    string[] words = effect.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);

                                    string modelUpper = SystemInfo.deviceModel.ToUpper();
                                    if(delimitedString == "$$")
                                    {
                                        if(words[0] == MODEL)
                                        {
                                            if(modelUpper.Contains(words[1].ToUpper()))
                                            {
                                                profileName2 = key;
                                            }
                                        }
                                        else if(words[0] == MANUF)
                                        {
                                            if(modelUpper.Contains(words[1].ToUpper()))
                                            {
                                                profileName2 = key;
                                            }
                                        }
                                        else if(words[0] == GPU)
                                        {
                                            if(SystemInfo.graphicsDeviceName.Contains(words[1]))
                                            {
                                                profileName2 = key;
                                            }
                                        }
                                    }
                                    else if(delimitedString == ">=")
                                    {
                                        if(words[0] == CORES)
                                        {
                                            if(SystemInfo.processorCount >= int.Parse(words[1]))
                                            {
                                                profileName2 = key;
                                            }
                                        }
                                        else if(words[0] == MEM_T)
                                        {
                                            if((profileType == GPU && SystemInfo.graphicsMemorySize >= int.Parse(words[1]))
                                            || (profileType == MEM && SystemInfo.systemMemorySize >= int.Parse(words[1])))
                                            {
                                                profileName2 = key;
                                            }
                                        }
                                    }
                                    else if(delimitedString == "<=")
                                    {
                                        if(words[0] == CORES)
                                        {
                                            if(SystemInfo.processorCount <= int.Parse(words[1]))
                                            {
                                                profileName2 = key;
                                            }
                                        }
                                        else if(words[0] == MEM_T)
                                        {
                                            if((profileType == GPU && SystemInfo.graphicsMemorySize <= int.Parse(words[1]))
                                            || (profileType == MEM && SystemInfo.systemMemorySize <= int.Parse(words[1])))
                                            {
                                                profileName2 = key;
                                            }
                                        }
                                    }
                                    else if(delimitedString == ">")
                                    {
                                        if(words[0] == CORES)
                                        {
                                            if(SystemInfo.processorCount > int.Parse(words[1]))
                                            {
                                                profileName2 = key;
                                            }
                                        }
                                        else if(words[0] == MEM_T)
                                        {
                                            if((profileType == GPU && SystemInfo.graphicsMemorySize > int.Parse(words[1]))
                                            || (profileType == MEM && SystemInfo.systemMemorySize > int.Parse(words[1])))
                                            {
                                                profileName2 = key;
                                            }
                                        }
                                    }
                                    else if(delimitedString == "<")
                                    {
                                        if(words[0] == CORES)
                                        {
                                            if(SystemInfo.processorCount < int.Parse(words[1]))
                                            {
                                                profileName2 = key;
                                            }
                                        }
                                        else if(words[0] == MEM_T)
                                        {
                                            if((profileType == GPU && SystemInfo.graphicsMemorySize < int.Parse(words[1]))
                                            || (profileType == MEM && SystemInfo.systemMemorySize < int.Parse(words[1])))
                                            {
                                                profileName2 = key;
                                            }
                                        }
                                    }
                                    else if(delimitedString == "=")
                                    {
                                        if(words[0] == CORES)
                                        {
                                            if(SystemInfo.processorCount == int.Parse(words[1]))
                                            {
                                                profileName2 = key;
                                            }
                                        }
                                        else if(words[0] == MEM_T)
                                        {
                                            if((profileType == GPU && SystemInfo.graphicsMemorySize == int.Parse(words[1]))
                                            || (profileType == MEM && SystemInfo.systemMemorySize == int.Parse(words[1])))
                                            {
                                                profileName2 = key;
                                            }
                                        }
                                    }

                                    if(profileName2 == "")
                                    {
                                        break;
                                    }
                                }
                            }

                            if(profileName2 != "")
                            {
                                profileName = profileName2;
                                return profileName;
                            }
                        }
                    }
                }
            }

            return profileName;
        }
        public static void LoadOptionDefaults(JSONNode jProfile)
        {
            JSONNode jdefaults = jProfile["defaults"];
            if(jdefaults == null)
            {
                return;
            }

            foreach(JSONNode node in jdefaults.Children)
            {
                JSONNode jDefault = node["Default"];
                if(jDefault != null)
                {
                    foreach(JSONNode node2 in jDefault.Children)// Default's children
                    {
                        if(node2.AsArray.Count > 2)
                        {
                            SetGenericValue(node2.AsArray[0], node2.AsArray[1]);
                        }
                    }
                    break;
                }
            }
        }
        public static void LoadOptionsOverrides(JSONNode jProfile, string profileName)
        {
            JSONNode joverrides = jProfile["overrides"];
            if(joverrides == null)
            {
                return;
            }
            foreach(string key in joverrides.Keys)
            {
                if(key == profileName)
                {
                    // keys : CPU_X1, CPU_X2,...
                    //  or  : GPU_X1, GPU_X2,...
                    //  or  : MEM_X1, MEM_X2,...
                    JSONNode over_profile = joverrides[key];
                    if(over_profile != null)
                    {
                        foreach(string effectKey in over_profile.Keys)
                        {
                            SetGenericValue(effectKey, over_profile[effectKey]);
                        }
                    }

                    return;
                }
            }
        }

        public static void ApplyProfile()
        {
            //.. apply all effect at here...
            Debug.Log("effects.qualityLevel: "+effects.qualityLevel);
            QualitySettings.SetQualityLevel((int)effects.qualityLevel, true);

            //Apply FPS limitation
            QualitySettings.vSyncCount = 0;
		    Application.targetFrameRate = IsHighProfile() ? 60 : 30;

            GraphicProfiler.GraphicsProfiler.Init();
        }

        public static Define.QUALITIES_LEVEL GetQualityLevel()
        {
            return effects.qualityLevel;
        }

        public static bool IsLowProfile()
        {
            return (effects.qualityLevel == Define.QUALITIES_LEVEL.VERY_LOW || effects.qualityLevel == Define.QUALITIES_LEVEL.LOW);
        }
        public static bool IsHighProfile()
        {
            return (effects.qualityLevel == Define.QUALITIES_LEVEL.VERY_HIGH || effects.qualityLevel == Define.QUALITIES_LEVEL.ULTRA);
        }

        public static void SetGenericValue(string optionName, JSONNode note)
        {
            if(note.IsBoolean)
            {
                contentProvider.SetValue(optionName, note.AsBool);
            }
            else if(note.IsNumber)
            {
                contentProvider.SetValue(optionName, note.AsInt);
            }
            else if(note.IsString)
            {
                contentProvider.SetValue(optionName, note.Value);
            }
        }
    }
}
