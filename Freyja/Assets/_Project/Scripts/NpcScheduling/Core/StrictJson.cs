using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Freyja.NpcScheduling
{
    // Parser pequeno compartilhado: só string em/objeto fora; sem Unity nem I/O.
    public static class StrictJson
    {
        public static T Deserialize<T>(string json) { return (T)ConvertValue(Parse(json),typeof(T),"$"); }
        private static object ConvertValue(object node,Type type,string path)
        {
            if(node==null)throw new FormatException(path+": null não permitido");
            if(type==typeof(string)){if(!(node is string))throw new FormatException(path+": esperado string");return node;}
            if(type==typeof(int)){if(!(node is double)||((double)node%1)!=0)throw new FormatException(path+": esperado inteiro");return checked((int)(double)node);}
            if(type==typeof(bool)){if(!(node is bool))throw new FormatException(path+": esperado booleano");return node;}
            if(type.IsGenericType&&type.GetGenericTypeDefinition()==typeof(List<>))
            {
                var source=node as List<object>;if(source==null)throw new FormatException(path+": esperado array");var target=(System.Collections.IList)Activator.CreateInstance(type);var itemType=type.GetGenericArguments()[0];for(int i=0;i<source.Count;i++)target.Add(ConvertValue(source[i],itemType,path+"["+i+"]"));return target;
            }
            if(type.IsGenericType&&type.GetGenericTypeDefinition()==typeof(Dictionary<,>)&&type.GetGenericArguments()[0]==typeof(string))
            {
                var source=node as Dictionary<string,object>;if(source==null)throw new FormatException(path+": esperado objeto");var target=(System.Collections.IDictionary)Activator.CreateInstance(type);var valueType=type.GetGenericArguments()[1];foreach(var pair in source)target.Add(pair.Key,ConvertValue(pair.Value,valueType,path+"."+pair.Key));return target;
            }
            var values=node as Dictionary<string,object>;if(values==null)throw new FormatException(path+": esperado objeto");var instance=Activator.CreateInstance(type);var properties=type.GetProperties(BindingFlags.Instance|BindingFlags.Public);var seen=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach(var pair in values){PropertyInfo match=null;foreach(var property in properties)if(String.Equals(property.Name,pair.Key,StringComparison.OrdinalIgnoreCase)){match=property;break;}if(match==null)throw new FormatException(path+": propriedade desconhecida '"+pair.Key+"'");if(!seen.Add(match.Name))throw new FormatException(path+": propriedade duplicada '"+pair.Key+"'");match.SetValue(instance,ConvertValue(pair.Value,match.PropertyType,path+"."+pair.Key),null);}
            return instance;
        }
        private static object Parse(string json){var parser=new Parser(json);var value=parser.Value();parser.Space();if(!parser.End)throw new FormatException("conteúdo após JSON na posição "+parser.Position);return value;}
        private sealed class Parser
        {
            private readonly string text;private int index;public Parser(string text){this.text=text??"";}public bool End{get{return index>=text.Length;}}public int Position{get{return index;}}
            public void Space(){while(!End&&Char.IsWhiteSpace(text[index]))index++;}
            public object Value(){Space();if(End)throw Error("JSON vazio");char c=text[index];if(c=='{')return Object();if(c=='[')return Array();if(c=='\"')return String();if(c=='t'){Word("true");return true;}if(c=='f'){Word("false");return false;}if(c=='n'){Word("null");return null;}if(c=='-'||Char.IsDigit(c))return Number();throw Error("valor inválido");}
            private Dictionary<string,object> Object(){index++;var result=new Dictionary<string,object>(StringComparer.Ordinal);Space();if(Take('}'))return result;while(true){Space();if(End||text[index]!='\"')throw Error("chave esperada");string key=String();if(result.ContainsKey(key))throw Error("chave duplicada '"+key+"'");Space();Need(':');result.Add(key,Value());Space();if(Take('}'))return result;Need(',');}}
            private List<object> Array(){index++;var result=new List<object>();Space();if(Take(']'))return result;while(true){result.Add(Value());Space();if(Take(']'))return result;Need(',');}}
            private string String(){Need('\"');var result=new StringBuilder();while(!End){char c=text[index++];if(c=='\"')return result.ToString();if(c<32)throw Error("controle inválido em string");if(c!='\\'){result.Append(c);continue;}if(End)throw Error("escape incompleto");c=text[index++];switch(c){case '\"':result.Append('\"');break;case '\\':result.Append('\\');break;case '/':result.Append('/');break;case 'b':result.Append('\b');break;case 'f':result.Append('\f');break;case 'n':result.Append('\n');break;case 'r':result.Append('\r');break;case 't':result.Append('\t');break;case 'u':if(index+4>text.Length)throw Error("unicode incompleto");int code; if(!Int32.TryParse(text.Substring(index,4),NumberStyles.HexNumber,CultureInfo.InvariantCulture,out code))throw Error("unicode inválido");result.Append((char)code);index+=4;break;default:throw Error("escape inválido");}}throw Error("string não terminada");}
            private double Number(){int start=index;if(Take('-')){}if(Take('0')){}else{Digits(true);}if(Take('.'))Digits(true);if(Take('e')||Take('E')){if(!Take('+'))Take('-');Digits(true);}double value;if(!Double.TryParse(text.Substring(start,index-start),NumberStyles.Float,CultureInfo.InvariantCulture,out value)||Double.IsInfinity(value)||Double.IsNaN(value))throw Error("número inválido");return value;}
            private void Digits(bool required){int start=index;while(!End&&Char.IsDigit(text[index]))index++;if(required&&start==index)throw Error("dígito esperado");}
            private void Word(string word){if(index+word.Length>text.Length||System.String.CompareOrdinal(text,index,word,0,word.Length)!=0)throw Error("token inválido");index+=word.Length;}
            private bool Take(char c){if(!End&&text[index]==c){index++;return true;}return false;}private void Need(char c){if(!Take(c))throw Error("esperado '"+c+"'");}private FormatException Error(string message){return new FormatException(message+" na posição "+index);}
        }
    }
}
