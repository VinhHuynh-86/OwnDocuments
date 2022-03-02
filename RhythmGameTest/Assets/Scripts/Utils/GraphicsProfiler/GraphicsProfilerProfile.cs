using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Malee;

namespace GraphicProfiler
{
    [System.Serializable]
    public class GraphicsProfilerProfile
    {
        public string profileName;
        public QualityLevel qualityLevel;
        public int benchmarkPoint = 0;

        [Space]

        [SerializeField, Reorderable(paginate = true, pageSize = 0, elementNameProperty = "type")]
        public GameOptionValueList gameOptionValue;

        [System.Serializable]
        public class StringList : ReorderableArray<string>
        {
        }

        [System.Serializable]
        public class GameOptionValueList : ReorderableArray<GameOptionValue>
        {
        }

        public bool GetOptionValue(GameOption valueType, out GameOptionValue outValue)
        {
            for(int i = 0; i < this.gameOptionValue.Count; i++)
            {
                if(this.gameOptionValue[i].gameOption == valueType)
                {
                    outValue = this.gameOptionValue[i];
                    return true;
                }
            }
            outValue = null;
            return false;
        }
    }

    [System.Serializable]
    public class GameOptionValue
    {
        public GameOption gameOption;
        public int value;
    }
}