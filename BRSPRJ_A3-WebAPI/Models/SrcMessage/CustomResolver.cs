using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BRSPRJ_A3_WebAPI.Models.SrcMessage
{
    /// <summary>
    /// 客製Json序列化物件器類別
    /// </summary>
    public class CustomResolver : DefaultContractResolver
    {
        /// <summary>
        /// 傳遞訊息(BoardingBagList)之訊息內容種類之列舉項目
        /// </summary>
        public BodyChoice Choice { get; set; }

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="choice">傳遞訊息(BoardingBagList)之訊息內容種類之列舉項目</param>
        public CustomResolver(BodyChoice choice)
        {
            Choice = choice;
        }

        /// <summary>
        /// Create a <see cref="JsonProperty"/> for the given <see cref="MemberInfo"/>
        /// <param name="member">The member to create a <see cref="JsonProperty"/> for</param>
        /// <param name="memberSerialization">The member parent's <see cref="MemberSerialization"/></param>
        /// <returns>A created <see cref="JsonProperty"/> for the given <see cref="MemberInfo"/></returns>
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty prop = base.CreateProperty(member, memberSerialization);
            if (member.GetCustomAttribute<JsonPropertyOnSealObjAttribute>() != null)
            {
                prop.PropertyName = ChooseOneOf(prop.PropertyName);
            }

            return prop;
        }

        /// <summary>
        /// 客製化Json序列化物件轉換中SealObj物件名稱選擇
        /// </summary>
        /// <param name="name">原SealObj物件名稱</param>
        /// <returns>依<c>BodyChoice</c>轉換後名稱</returns>
        protected string ChooseOneOf(string name)
        {
            switch (Choice)
            {
                case BodyChoice.Truck:
                    name = typeof(Truck).Name;
                    break;
                case BodyChoice.Container:
                    name = typeof(Container).Name;
                    break;
                case BodyChoice.Baggage:
                    name = typeof(Baggage).Name;
                    break;
                default:
                    name = typeof(Container).Name;
                    break;
            }

            return name;
        }
    }
}