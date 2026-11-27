using System;
using System.Collections.Generic;
using Freyja.NpcScheduling;
using Freyja.NpcScheduling.Execution;
using UnityEngine;

namespace Freyja.NpcScheduling.Unity
{
    [DisallowMultipleComponent]
    public sealed class NpcScheduleTextAssetProvider : MonoBehaviour
    {
        [SerializeField] private TextAsset activityCatalog;
        [SerializeField] private TextAsset[] templates = Array.Empty<TextAsset>();
        [SerializeField] private TextAsset[] npcSchedules = Array.Empty<TextAsset>();
        [SerializeField] private TextAsset[] overrides = Array.Empty<TextAsset>();
        [SerializeField] private TextAsset activityRecipes;

        public bool TryLoad(out ResolvedScheduleData scheduleData,
            out IDictionary<string,ActivityRecipe> recipes, out string error)
        {
            scheduleData=null; recipes=null; error=null;
            if(activityCatalog==null){error="missing_activity_catalog";return false;}
            if(activityRecipes==null){error="missing_activity_recipes";return false;}
            if(npcSchedules==null||npcSchedules.Length==0){error="missing_npc_schedule_assets";return false;}
            try
            {
                var documents=new ScheduleDataDocuments(activityCatalog.text,Documents(templates,"template"),
                    Documents(npcSchedules,"npc"),Documents(overrides,"override"));
                ScheduleDataLoadResult result=ScheduleDataLoader.Load(documents);
                scheduleData=result.Data;
                recipes=ActivityRecipeLoader.Load(activityRecipes.text,scheduleData.Activities,"TextAsset:"+activityRecipes.name);
                return true;
            }
            catch(Exception exception)
            {
                scheduleData=null; recipes=null; error="npc_data_invalid: "+exception.Message; return false;
            }
        }

        private static IList<ScheduleTextDocument> Documents(TextAsset[] assets,string kind)
        {
            var documents=new List<ScheduleTextDocument>();
            if(assets==null)return documents;
            for(int i=0;i<assets.Length;i++)
            {
                TextAsset asset=assets[i];
                if(asset==null)throw new InvalidOperationException("missing_"+kind+"_text_asset_"+i);
                documents.Add(new ScheduleTextDocument("TextAsset:"+asset.name+"["+i+"]",asset.text));
            }
            return documents;
        }
    }
}
