using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Xml;
using System.IO;
using Newtonsoft.Json.Linq;
using Auggie.Lib.JsonExtend;

namespace Auggie.Lib.Config
{
    /// <summary>
    /// 统一配置管理文件
    /// 同时支持多种配置
    /// 1. 上级文件夹中的Common.config（xml格式配置, 仅支持AppSettings节点）
    /// 2. 同级文件夹中的config.json(json格式配置)
    /// 3. 程序app.config的配置
    /// 4. 指定的xml或者json格式的配置
    /// 注1： 多级之间，使用 : 进行连接
    /// 注2： AppSettings主节点，省略该多级前缀
    /// </summary>
    class ConfigSetting
    {
        public static ConfigSetting Value { get; private set; } = new ConfigSetting();

        private Dictionary<string, string> kvMap = new Dictionary<string, string>();

        public string this[string name]
        {
            get
            {
                if (kvMap.ContainsKey(name))
                    return kvMap[name];
                return "";
            }
        }
            
        private ConfigSetting()
        {
            this.pLoadXml("../Common.config");
            this.pLoadJson("config.json");

            // AppSettings信息的读取
            foreach (string a in ConfigurationManager.AppSettings.Keys)
            {
                kvMap[a] = ConfigurationManager.AppSettings[a];
            }
        }

        public static void LoadXml(string fileName)
        {
            Value.pLoadXml(fileName);
        }

        public static void LoadJson(string fileName)
        {
            Value.pLoadJson(fileName);
        }

        private void pLoadXml(string fileName)
        {
            if (!File.Exists(fileName)) return;
            ConfigXmlDocument doc = new ConfigXmlDocument();
            doc.Load(fileName);
            var AsNode = doc.SelectSingleNode("AppSettings");
            foreach (XmlNode ac in AsNode.ChildNodes)
            {
                var key = ac.Attributes["key"].Value;
                var value = ac.Attributes["value"].Value;

                kvMap[key] = value;
            }
        }

        private void pLoadJson(string fileName)
        {
            if (!File.Exists(fileName)) return;
            var jStr = File.ReadAllText(fileName);
            JObject json = jStr.ToJObject();
            if (json == null) return;

            ReadJson(json, "");
        }

        private void ReadJson(JObject json, string pre)
        {
            foreach (KeyValuePair<string,JToken> child in json)
            {
                switch (child.Value.Type)
                {
                    case JTokenType.Object:
                        ReadJson(child.Value as JObject, GetCurKey(pre, child.Key));
                        break;
                    case JTokenType.Array:                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             
                        {
                            var arr = child.Value as JArray;
                            for (int i = 0; i < arr.Count; i++)
                            {
                                var jt = arr[i] as JObject ;
                                if (jt != null)
                                    ReadJson(jt, GetCurKey(pre, child.Key, i.ToString()));
                            }
                        }
                        break;
                    default:
                        kvMap[child.Key] = child.Value.ToString();
                        break;
                }
            }
        }

        private string GetCurKey(params string[] args)
        {
            return string.Join(":", args.ToList().RemoveAll(t => string.IsNullOrEmpty(t)));
        }
    }
}
