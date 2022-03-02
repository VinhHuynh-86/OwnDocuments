using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Malee;

namespace GraphicProfiler
{
    [CreateAssetMenu(menuName = "Game Configs/GraphicsProfilerConfig")]
    public class GraphicsProfilerConfig : ScriptableObject
    {
        [SerializeField, Reorderable(paginate = true, pageSize = 0, elementNameProperty = "type")]
        public GraphicsProfileList graphicsProfileList;

        [System.Serializable]
        public class GraphicsProfileList : ReorderableArray<GraphicsProfilerProfile>
        {
        }

        public GraphicsProfilerProfile GetProfile(QualityLevel level)
        {
            GraphicsProfilerProfile result = this.graphicsProfileList[0];
            for(int i =0; i < this.graphicsProfileList.Count; i++)
            {
                if(this.graphicsProfileList[i].qualityLevel == level)
                {
                    result = this.graphicsProfileList[i];
                }
            }

            return result;
        }
    }
}