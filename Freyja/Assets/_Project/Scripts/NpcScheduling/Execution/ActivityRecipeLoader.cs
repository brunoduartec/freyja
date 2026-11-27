using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Freyja.NpcScheduling;

namespace Freyja.NpcScheduling.Execution
{
    public static class ActivityRecipeLoader
    {
        private sealed class RecipeFile { public int SchemaVersion { get; set; } public List<ActivityRecipe> Recipes { get; set; } }
        private static readonly HashSet<string> KnownActions=new HashSet<string>(new[]
        { "find_target","find_waypoints","reserve_slot","move_to","align_to","occupy_slot","play_animation","wait","release_slot" },StringComparer.Ordinal);

        public static IDictionary<string,ActivityRecipe> Load(string json,IDictionary<string,ActivityDefinition> activities,string source="recipes.json")
        {
            if(String.IsNullOrWhiteSpace(json))throw new InvalidDataException(source+": conteúdo vazio");
            if(activities==null)throw new ArgumentNullException("activities");
            try
            {
                RecipeFile file=StrictJson.Deserialize<RecipeFile>(json);
                if(file.SchemaVersion!=1||file.Recipes==null)throw new InvalidDataException("schemaVersion 1 e recipes são obrigatórios");
                var result=new Dictionary<string,ActivityRecipe>(StringComparer.Ordinal);
                foreach(ActivityRecipe recipe in file.Recipes)
                {
                    if(recipe==null||String.IsNullOrWhiteSpace(recipe.ActivityId)||!Regex.IsMatch(recipe.ActivityId,"^[a-z][a-z0-9_]*$"))throw new InvalidDataException("activityId inválido");
                    if(!activities.ContainsKey(recipe.ActivityId))throw new InvalidDataException("atividade ausente do catálogo: "+recipe.ActivityId);
                    if(result.ContainsKey(recipe.ActivityId))throw new InvalidDataException("ID de receita duplicado: "+recipe.ActivityId);
                    if(recipe.Steps==null||recipe.Steps.Count==0)throw new InvalidDataException(recipe.ActivityId+": steps vazio");
                    for(int i=0;i<recipe.Steps.Count;i++)ValidateStep(recipe.ActivityId,recipe.Steps[i],i);
                    ValidateOrder(recipe);
                    result.Add(recipe.ActivityId,recipe);
                }
                foreach(string required in new[]{"idle","wander","sit"})if(!result.ContainsKey(required))throw new InvalidDataException("receita obrigatória ausente: "+required);
                return result;
            }
            catch(Exception ex) when(!(ex is InvalidDataException&&ex.Message.StartsWith(source+":",StringComparison.Ordinal)))
            { throw new InvalidDataException(source+": "+ex.Message,ex); }
        }

        private static void ValidateStep(string recipe,ActivityStep step,int index)
        {
            string prefix=recipe+" step["+index+"]: ";
            if(step==null)throw new InvalidDataException(prefix+"passo nulo");
            if(!KnownActions.Contains(step.Action))throw new InvalidDataException(prefix+"operação desconhecida: "+step.Action);
            if(step.Action=="find_target"&&String.IsNullOrWhiteSpace(step.Capability))throw new InvalidDataException(prefix+"capability obrigatória");
            if(step.Action=="find_target"&&String.IsNullOrWhiteSpace(step.Target))throw new InvalidDataException(prefix+"target obrigatório");
            if(step.Action=="move_to"&&String.IsNullOrWhiteSpace(step.Point))throw new InvalidDataException(prefix+"point obrigatório");
            if(step.Action=="play_animation"&&String.IsNullOrWhiteSpace(step.Animation))throw new InvalidDataException(prefix+"animation obrigatória");
            if(step.Action=="wait"&&(step.Until!="schedule_end"&&step.Until!="duration"))throw new InvalidDataException(prefix+"until deve ser schedule_end ou duration");
            if(step.Action=="wait"&&step.Until=="duration"&&step.DurationSeconds<=0)throw new InvalidDataException(prefix+"durationSeconds deve ser positivo");
        }
        private static void ValidateOrder(ActivityRecipe recipe)
        {
            var actions=recipe.Steps.Select(x=>x.Action).ToList();
            if(recipe.ActivityId=="sit")
            {
                string[] required={"find_target","reserve_slot","move_to","align_to","occupy_slot","play_animation","wait","play_animation","release_slot"};
                int cursor=0;foreach(string action in actions)if(cursor<required.Length&&action==required[cursor])cursor++;
                if(cursor!=required.Length)throw new InvalidDataException("sit: sequência precisa encontrar/reservar/mover/alinha/ocupar/entrar/esperar/sair/liberar");
            }
            if(recipe.ActivityId=="wander"&&(!actions.Contains("find_waypoints")||!actions.Contains("move_to")||!actions.Contains("wait")))throw new InvalidDataException("wander: find_waypoints, move_to e wait são obrigatórios");
            int reserve=actions.IndexOf("reserve_slot"),occupy=actions.IndexOf("occupy_slot"),move=actions.IndexOf("move_to"),align=actions.IndexOf("align_to"),release=actions.LastIndexOf("release_slot");
            if(reserve>=0&&(actions.IndexOf("find_target")<0||actions.IndexOf("find_target")>reserve||move<reserve||occupy>=0&&align>=0&&(align<move||occupy<align)||occupy>=0&&occupy<move))throw new InvalidDataException(recipe.ActivityId+": ordem de busca/reserva/movimento/alinhamento/ocupação inválida");
            if(occupy>=0&&reserve<0)throw new InvalidDataException(recipe.ActivityId+": occupy_slot sem reserva");
            if(release>=0&&reserve<0)throw new InvalidDataException(recipe.ActivityId+": release_slot sem reserva");
        }
    }
}
