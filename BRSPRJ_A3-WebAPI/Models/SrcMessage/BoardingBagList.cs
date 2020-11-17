using Newtonsoft.Json;
using System.Collections.Generic;

namespace BRSPRJ_A3_WebAPI.Models.SrcMessage
{
    /// <summary>
    /// 傳遞訊息(BoardingBagList)之訊息內容種類之列舉項目
    /// </summary>
    public enum BodyChoice
    {
        /// <summary>
        /// 訊息種類為配送車之集合
        /// </summary>
        Truck,
        /// <summary>
        /// 訊息種類為容器(籠車)之集合
        /// </summary>
        Container,
        /// <summary>
        /// 訊息種類為裝載運送行李之集合
        /// </summary>
        Baggage
    }

    #region =====[Public] Class for Converting from Json Messages=====

    /// <summary>
    /// 運送行李卸載之基本資訊確認記錄類別
    /// </summary>
    public class Unloading_Records
    {
        /// <summary>
        /// 此件行李是否已從裝載之容器(籠車)取出並使用手持機掃瞄確認。
        /// </summary>
        [JsonProperty("ScanState", Required = Required.AllowNull, Order = 1)]
        public bool? ScanState { get; set; } = null;
        /// <summary>
        /// 將此件行李從裝載之容器(籠車)取出並使用手持機掃瞄確認，或是於裝載之容器(籠車)中未發現此件行李的作業人員
        /// </summary>
        [JsonProperty("ScanOper", Required = Required.Default, Order = 2)]
        public string ScanOper { get; set; }
        /// <summary>
        /// 將此件行李從裝載之容器(籠車)取出並使用手持機掃瞄確認，或是於裝載之容器(籠車)中未發現此件行李的時間
        /// </summary>
        /// <remarks>yyyyMMddHHmmss.fff (local time)</remarks>
        [JsonProperty("ScanTime", Required = Required.Default, Order = 3)]
        public string ScanTime { get; set; }
    }

    /// <summary>
    /// 裝載運送行李之基本資訊運送記錄類別
    /// </summary>
    public class Loading_Records
    {
        /// <summary>
        /// 將此件行李裝載至容器(籠車)的地點、位置
        /// </summary>
        [JsonProperty("LoadPlace", Required = Required.AllowNull, Order = 1)]
        public string LoadPlace { get; set; }
        /// <summary>
        /// 將此件行李裝載至容器(籠車)的時間
        /// </summary>
        [JsonProperty("LoadTime", Required = Required.AllowNull, Order = 2)]
        public string LoadTime { get; set; }
    }

    /// <summary>
    /// 裝載運送行李之基本資訊類別
    /// </summary>
    public class Records_Baggage
    {
        /// <summary>
        /// 裝載運送行李之基本資訊運送記錄
        /// </summary>
        [JsonProperty("Loading", Required = Required.Default, Order = 1)]
        public Loading_Records Loading { get; set; }
        /// <summary>
        /// 運送行李卸載之基本資訊確認記錄
        /// </summary>
        [JsonProperty("Unloading", Required = Required.Default, Order = 2)]
        public Unloading_Records Unloading { get; set; }
    }

    /// <summary>
    /// 裝載運送行李之基本資訊識別類別
    /// </summary>
    public class ID_Baggage
    {
        /// <summary>
        /// 行李條碼編號
        /// </summary>
        [JsonProperty("BagTag", Required = Required.Always, Order = 1)]
        public string BagTag { get; set; }
        /// <summary>
        /// 行李所屬航班編號
        /// </summary>
        [JsonProperty("FlightNo", Required = Required.Default, Order = 2)]
        public string FlightNo { get; set; }
        /// <summary>
        /// 行李所屬航班之表訂出(入)境時間
        /// </summary>
        [JsonProperty("STD", Required = Required.Default, Order = 3)]
        public string STD { get; set; }
    }

    /// <summary>
    /// 裝載運送行李之基本資訊與處理狀態類別
    /// </summary>
    public class Baggage
    {
        /// <summary>
        /// 裝載運送行李之基本資訊識別
        /// </summary>
        [JsonProperty("ID", Required = Required.Always, Order = 1)]
        public ID_Baggage ID { get; set; }
        /// <summary>
        /// 裝載運送行李之基本資訊運送記錄
        /// </summary>
        [JsonProperty("Records", Required = Required.DisallowNull, Order = 2)]
        public Records_Baggage Records { get; set; }
    }

    /// <summary>
    /// 特定容器(籠車)所裝載之行李類別
    /// </summary>
    public class LoadedBaggage
    {
        /// <summary>
        /// 此特定容器(籠車)裝載行李之基本資訊與處理狀態物件
        /// </summary>
        [JsonProperty("Baggage", Required = Required.Default, Order = 1)]
        public Baggage Baggage { get; set; }
    }

    /// <summary>
    /// 裝載行李之容器(籠車)之基本資訊運送記錄類別
    /// </summary>
    public class Records_Container
    {
        /// <summary>
        /// 將裝載行李之容器(籠車)貼上封籤條碼之時間
        /// </summary>
        /// <remarks>yyyyMMddHHmmss.fff (local time)</remarks>
        [JsonProperty("SealTime", Required = Required.Default, Order = 1)]
        public string SealTime { get; set; }
        /// <summary>
        /// 將此已封籤容器(籠車)裝載至配送車的時間
        /// </summary>
        /// <remarks>yyyyMMddHHmmss.fff (local time)</remarks>
        [JsonProperty("LoadTime", Required = Required.Default, Order = 2)]
        public string LoadTime { get; set; }
        /// <summary>
        /// 此已封籤容器(籠車)所裝載之行李數量
        /// </summary>
        [JsonProperty("BagCount", Required = Required.Always, Order = 3)]
        public int BagCount { get; set; }
        /// <summary>
        /// 此已封籤容器(籠車)中已掃描確認並卸載之行李數量
        /// </summary>
        [JsonProperty("ScannedCount", Required = Required.DisallowNull, Order = 4)]
        public int ScannedCount { get; set; }
        /// <summary>
        /// 此已封籤容器(籠車)之全域唯一識別碼
        /// </summary>
        [JsonProperty("GUID", Required = Required.Default, Order = 5)]
        public string GUID { get; set; }
        /// <summary>
        /// 容器(籠車)裝載行李與封籤位置
        /// </summary>
        [JsonProperty("Location", Required = Required.Default, Order = 6)]
        public string Location { get; set; }
        /// <summary>
        /// 掃描此已封籤容器(籠車)之手持機序列號碼
        /// </summary>
        [JsonProperty("PDASN", Required = Required.Default, Order = 7)]
        public string PDASN { get; set; }
    }

    /// <summary>
    /// 裝載行李之容器(籠車)之基本資訊識別類別
    /// </summary>
    public class ID_Container
    {
        /// <summary>
        /// 裝載行李之容器(籠車)的條碼編號
        /// </summary>
        [JsonProperty("ContainerTag", Required = Required.Always, Order = 1)]
        public string ContainerTag { get; set; }
        /// <summary>
        /// 裝載行李之容器(籠車)的封籤條碼編號
        /// </summary>
        [JsonProperty("SealTag", Required = Required.Always, Order = 2)]
        public string SealTag { get; set; }
    }

    /// <summary>
    /// 裝載運送容器(籠車)之基本資訊與裝載、封籤狀態類別
    /// </summary>
    public class BasicInfo_Container
    {
        /// <summary>
        /// 裝載行李之容器(籠車)之基本資訊識別
        /// </summary>
        [JsonProperty("ID", Required = Required.Always, Order = 1)]
        public ID_Container ID { get; set; }
        /// <summary>
        /// 裝載行李之容器(籠車)之基本資訊運送記錄
        /// </summary>
        [JsonProperty("Records", Required = Required.DisallowNull, Order = 2)]
        public Records_Container Records { get; set; }
    }

    /// <summary>
    /// 特定裝載運送容器(籠車)之基本資訊與所裝載之行李類別
    /// </summary>
    public class Container
    {
        /// <summary>
        /// 裝載運送容器(籠車)之基本資訊與裝載、封籤狀態物件
        /// </summary>
        [JsonProperty("BasicInfo", Required = Required.Always, Order = 1)]
        public BasicInfo_Container BasicInfo { get; set; }
        /// <summary>
        /// 特定容器(籠車)所裝載之行李列表
        /// </summary>
        [JsonProperty("LoadedBaggage", Required = Required.Default, Order = 2)]
        public List<LoadedBaggage> LoadedBaggage { get; set; }
    }

    /// <summary>
    /// 特定配送車所裝載之已封籤容器(籠車)類別
    /// </summary>
    public class Containers
    {
        /// <summary>
        /// 已封籤容器(籠車)物件
        /// </summary>
        [JsonProperty("Container", Required = Required.AllowNull, Order = 1)]
        public Container Container { get; set; }
    }

    /// <summary>
    /// 裝載運送配送車之基本資訊運送記錄類別
    /// </summary>
    public class Records_Truck
    {
        /// <summary>
        /// 裝載已封籤容器(籠車)之配送車出貨的時間
        /// </summary>
        /// <remarks>yyyyMMddHHmmss.fff (local time)</remarks>
        [JsonProperty("DeliveryTime", Required = Required.Default, Order = 1)]
        public string DeliveryTime { get; set; }
        /// <summary>
        /// 此已封籤配送車所裝載之已封籤容器(籠車)數量
        /// </summary>
        [JsonProperty("ContainerCount", Required = Required.Always, Order = 2)]
        public int ContainerCount { get; set; }
        /// <summary>
        /// 此已封籤配送車之全域唯一識別碼
        /// </summary>
        [JsonProperty("GUID", Required = Required.Default, Order = 3)]
        public string GUID { get; set; }
        /// <summary>
        /// 配送車出貨位置
        /// </summary>
        [JsonProperty("Location", Required = Required.Default, Order = 4)]
        public string Location { get; set; }
        /// <summary>
        /// 掃描此已封籤配送車之手持機序列號碼
        /// </summary>
        [JsonProperty("PDASN", Required = Required.Default, Order = 5)]
        public string PDASN { get; set; }
    }

    /// <summary>
    /// 裝載運送配送車之基本資訊識別類別
    /// </summary>
    public class ID_Truck
    {
        /// <summary>
        /// 裝載已封籤容器(籠車)之配送車的車牌號碼
        /// </summary>
        [JsonProperty("TruckTag", Required = Required.Always, Order = 1)]
        public string TruckTag { get; set; }
        /// <summary>
        /// 裝載已封籤容器(籠車)之配送車的封籤條碼編號
        /// </summary>
        [JsonProperty("SealTag", Required = Required.Always, Order = 2)]
        public string SealTag { get; set; }
    }

    /// <summary>
    /// 裝載運送配送車之基本資訊與封籤、運送狀態類別
    /// </summary>
    public class BasicInfo_Truck
    {
        /// <summary>
        /// 裝載運送配送車之基本資訊識別
        /// </summary>
        [JsonProperty("ID", Required = Required.Always, Order = 1)]
        public ID_Truck ID { get; set; }
        /// <summary>
        /// 裝載運送配送車之基本資訊運送記錄
        /// </summary>
        [JsonProperty("Records", Required = Required.DisallowNull, Order = 2)]
        public Records_Truck Records { get; set; }
    }

    /// <summary>
    /// 特定配送車之基本資訊與所運送之容器(籠車)類別
    /// </summary>
    public class Truck
    {
        /// <summary>
        /// 配送車之基本資訊與封籤、運送狀態物件
        /// </summary>
        [JsonProperty("BasicInfo", Required = Required.Always, Order = 1)]
        public BasicInfo_Truck BasicInfo { get; set; }
        /// <summary>
        /// 特定配送車所所運送之容器(籠車)列表
        /// </summary>
        [JsonProperty("Containers", Required = Required.Always, Order = 2)]
        public List<Containers> Containers { get; set; }
    }

    /// <summary>
    /// 傳遞訊息BoardingBagList之內容
    /// </summary>
    /// <remarks><c>Truck</c>或<c>Container</c>擇一使用</remarks>
    public class Body
    {
        /// <summary>
        /// 已封籤配送車或容器(籠車)物件
        /// </summary>
        [JsonPropertyOnSealObj]
        public dynamic SealObj { get; set; }
    }

    /// <summary>
    /// 傳遞訊息BoardingBagList之標頭
    /// </summary>
    public class Header
    {
        /// <summary>
        /// 訊息傳送來源
        /// </summary>
        [JsonProperty("Source", Required = Required.Always, Order = 1)]
        public string Source { get; set; }
        /// <summary>
        /// 訊息傳送目的地
        /// </summary>
        [JsonProperty("Destination", Required = Required.Always, Order = 2)]
        public string Destination { get; set; }
        /// <summary>
        /// 訊息傳送時間
        /// </summary>
        /// <remarks>yyyyMMddHHmmss.fff (local time)</remarks>
        [JsonProperty("SendTime", Required = Required.Always, Order = 3)]
        public string SendTime { get; set; }
    }

    /// <summary>
    /// 傳遞訊息BoardingBagList之最上層標籤類別
    /// </summary>
    public class LoadingList
    {
        /// <summary>
        /// 傳遞訊息BoardingBagList之標頭物件
        /// </summary>
        [JsonProperty("Header", Required = Required.Always, Order = 1)]
        public Header Header { get; set; }
        /// <summary>
        /// 傳遞訊息BoardingBagList之內容列表
        /// </summary>
        /// <remarks>所有配送車資訊列表 或 指定已封籤容器(籠車)物件</remarks>
        [JsonProperty("Body", Required = Required.Always, Order = 2)]
        public List<Body> Body { get; set; }
    }

    /// <summary>
    /// 傳遞訊息BoardingBagList類別
    /// </summary>
    public class BoardingBagList
    {
        /// <summary>
        /// 傳遞訊息BoardingBagList之最上層標籤物件
        /// </summary>
        [JsonProperty("LoadingList")]
        public LoadingList LoadingList { get; set; }
    }
    #endregion
}