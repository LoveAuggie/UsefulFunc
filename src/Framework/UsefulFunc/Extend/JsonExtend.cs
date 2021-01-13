using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auggie.Lib.JsonExtend
{
    public static class JsonExtend
    {
        /// <summary>
        /// 字符串转Json，（封装tryCatch）
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static JObject ToJObject(this string json)
        {
            try
            {
                return JObject.Parse(json);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 字符串转Json，（封装tryCatch）
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static JToken ToJToken(this string json)
        {
            try
            {
                return JToken.Parse(json);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 字符串转Json，（封装tryCatch）
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static JArray ToJArray(this string json)
        {
            try
            {
                return JArray.Parse(json);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// JArray的批量添加
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void AddRange(this JArray source, JArray target)
        {
            foreach (var token in target)
            {
                source.Add(token);
            }
        }

        /// <summary>
        /// 简写的转换方法
        /// </summary>
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// 简写的转换方法
        /// </summary>
        public static T ToObject<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
