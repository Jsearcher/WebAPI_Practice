using BRSPRJ_A3_WebAPI.Models.ConfigScript;
using BRSPRJ_A3_WebAPI.Models.DAO.BRS;
using BRSPRJ_A3_WebAPI.Models.DBTables.A3;
using BRSPRJ_A3_WebAPI.Models.SrcMessage;
using Lib.DB;
using Lib.Log;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;

namespace BRSPRJ_A3_WebAPI.Controllers
{
    /// <summary>
    /// A3行李裝載資訊服務API
    /// </summary>
    [RoutePrefix("api/brs/boardingbags")]
    public class BoardingBagController : ApiController
    {
        #region =====[Private] Web Configurations=====
        /// <summary>
        /// T1BRSDB供此Web API使用之資料庫連線參數
        /// </summary>
        private static readonly string T1BRSDB_ConnStr = WebConfig.WebPropertySetting.Instance().GetProperty("T1BRSDB");
        /// <summary>
        /// T2BRSDB供此Web API使用之資料庫連線參數
        /// </summary>
        private static readonly string T2BRSDB_ConnStr = WebConfig.WebPropertySetting.Instance().GetProperty("T2BRSDB");
        /// <summary>
        /// 提供此Web API之服務系統名稱
        /// </summary>
        private static readonly string SYS_BRS = WebConfig.WebPropertySetting.Instance().GetProperty("SYS_BRS");
        /// <summary>
        /// 使用此Web API服務以完成行李謝載確認作業之PDA操作系統名稱
        /// </summary>
        private static readonly string SYS_PDA = WebConfig.WebPropertySetting.Instance().GetProperty("SYS_PDA");
        /// <summary>
        /// Json schema檔案之目錄字串
        /// </summary>
        private static readonly string JSDDirStr = HttpRuntime.BinDirectory + @"Models\SrcMessage\JSD\";
        /// <summary>
        /// Web Service執行模式
        /// </summary>
        /// <remarks>
        /// <para>0: 表示為測試模式</para>
        /// <para>1: 表示為正式運行模式</para>
        /// </remarks>
        private static readonly string Service_Mode = WebConfig.WebPropertySetting.Instance().GetProperty("MODE");
        /// <summary>
        /// Web Service執行模式為測試模式時，所使用的測試日期，格式為"yyyy-MM-dd"
        /// </summary>
        private static readonly string TestDate = WebConfig.WebPropertySetting.Instance().GetProperty("TestDate");
        /// <summary>
        /// 行李關聯所屬航班編號之預設值
        /// </summary>
        private static readonly string FlightNo_Default = WebConfig.WebPropertySetting.Instance().GetProperty("FlightNo_Default");
        /// <summary>
        /// 行李關聯所屬航班表訂出境時間之預設值
        /// </summary>
        private static readonly string STD_Default = WebConfig.WebPropertySetting.Instance().GetProperty("STD_Default");
        /// <summary>
        /// 用於異常行李輸入使用之PDA流水號
        /// </summary>
        private static readonly string PDASN_Abnormal = WebConfig.WebPropertySetting.Instance().GetProperty("PDASN_Abnormal");
        /// <summary>
        /// 異常行李輸入使用之配送車的車牌號碼
        /// </summary>
        private static readonly string T_Tag_Abnormal = WebConfig.WebPropertySetting.Instance().GetProperty("T_Tag_Abnormal");
        /// <summary>
        /// 異常行李輸入使用之配送車的封籤條碼編號
        /// </summary>
        private static readonly string T_STag_Abnormal = WebConfig.WebPropertySetting.Instance().GetProperty("T_STag_Abnormal");
        /// <summary>
        /// 異常行李輸入使用之容器(籠車)的條碼編號
        /// </summary>
        private static readonly string C_Tag_Abnormal = WebConfig.WebPropertySetting.Instance().GetProperty("C_Tag_Abnormal");
        /// <summary>
        /// 異常行李輸入使用之容器(籠車)的封籤條碼編號
        /// </summary>
        private static readonly string C_STag_Abnormal = WebConfig.WebPropertySetting.Instance().GetProperty("C_STag_Abnormal");
        #endregion

        /// <summary>
        /// 取得指定日期(系統時間)所有已裝載之A3行李資訊列表
        /// </summary>
        /// <param name="queryDate">資料查詢日期，需符合"yyyy-MM-dd"規則</param>
        /// <remarks>BoardingBagList訊息包含：
        /// <para>Header - 傳送來源、目的地與傳送時間</para>
        /// <para>Body - 配送車(列表) -> 運送哪些籠車(列表) -> 裝載哪些行李(列表)</para>
        /// </remarks>
        /// <returns>傳送訊息(BoardingBagList)</returns>
        [Route("all"), HttpGet]
        public string GetAllLoadingBag(string queryDate)
        {
            LogBase.FileDirectory = HttpContext.Current.Server.MapPath("~/Log/");

            if (!(new Regex(@"^\d{4}-((0\d)|(1[012]))-(([012]\d)|3[01])$")).IsMatch(queryDate))
            {
                ErrorLog.Log("ERROR", "GetAllLoadingBag", "ERROR Code: 201");
                return "ERROR|201"; // 資料查詢日期之規格不符
            }
            if (Service_Mode == "0")
            {
                queryDate = TestDate;
            }
            string tomorrow = DateTime.ParseExact(queryDate, "yyyy-MM-dd", CultureInfo.InvariantCulture).AddDays(1).ToString("yyyy-MM-dd");
            DataBase T1BRSDB = null, T2BRSDB = null;
            List<BSM2Day.Row> RowList_BSM = new List<BSM2Day.Row>();
            List<FIDS2Day.Row> RowList_FIDS = new List<FIDS2Day.Row>();
            List<BagBoarding.Row> RowList_Bag = new List<BagBoarding.Row>();
            List<ContainerInfo.Row> RowList_Container = new List<ContainerInfo.Row>();
            List<TruckInfo.Row> Rowlist_Truck = new List<TruckInfo.Row>();
            string JsonRtn = string.Empty;
            InfoLog.Log("BoardingBagController", "GetAllLoadingBag", string.Format("資料查詢日期驗證成功，開始資料搜尋，Service_Mode = '{0}'，, queryDate = '{1}'",
                                                                    Service_Mode, queryDate));

            try
            {
                // 從資料庫取得目標資料
                T1BRSDB = DataBase.Instance(T1BRSDB_ConnStr);
                T2BRSDB = DataBase.Instance(T2BRSDB_ConnStr);
                if (T2BRSDB.Conn == new DataBase(null).Conn || T1BRSDB.Conn == new DataBase(null).Conn)
                {
                    ErrorLog.Log("ERROR", "GetAllLoadingBag", "ERROR Code: 110");
                    return "ERROR|110"; // 資料庫連線失敗
                }
                BSM2Day TB_BSM2Day = new BSM2Day(T1BRSDB.Conn);
                FIDS2Day TB_FIDS2Day = new FIDS2Day(T1BRSDB.Conn);
                BagBoarding TB_BagBoarding = new BagBoarding(T2BRSDB.Conn);
                ContainerInfo TB_ContainerInfo = new ContainerInfo(T2BRSDB.Conn);
                TruckInfo TB_TruckInfo = new TruckInfo(T2BRSDB.Conn);
                if (TB_BagBoarding.SelectByLoadDate(queryDate) > 0
                    && TB_ContainerInfo.SelectByLoadDate(queryDate) > 0
                    && TB_TruckInfo.SelectByLoadDate(queryDate) > 0)
                {
                    // LEFT JOIN作資料關聯
                    TB_BagBoarding.RecordList.ForEach(obj => RowList_Bag.Add(obj as BagBoarding.Row));
                    TB_ContainerInfo.RecordList.ForEach(obj => RowList_Container.Add(obj as ContainerInfo.Row));
                    TB_TruckInfo.RecordList.ForEach(obj => Rowlist_Truck.Add(obj as TruckInfo.Row));
                    var query = from bRow in RowList_Bag
                                join cRow in RowList_Container on new { bRow.CONTAINER_TAG, bRow.CONTAINER_SEAL_TAG } equals new { cRow.CONTAINER_TAG, cRow.CONTAINER_SEAL_TAG } into bcList
                                from bcRow in bcList.DefaultIfEmpty()
                                join tRow in Rowlist_Truck on new { bcRow.TRUCK_TAG, bcRow.TRUCK_SEAL_TAG } equals new { tRow.TRUCK_TAG, tRow.TRUCK_SEAL_TAG } into bctList
                                from bctRow in bctList.DefaultIfEmpty()
                                where bRow.BAG_LOAD_TIME.CompareTo(DateTime.ParseExact(queryDate, "yyyy-MM-dd", CultureInfo.InvariantCulture)) >= 0
                                    && bRow.BAG_LOAD_TIME.CompareTo(DateTime.ParseExact(tomorrow, "yyyy-MM-dd", CultureInfo.InvariantCulture)) < 0
                                orderby bctRow.TRUCK_SEAL_TAG, bcRow.CONTAINER_SEAL_TAG, bRow.BAG_TAG
                                select new
                                {
                                    bctRow.TRUCK_TAG,
                                    bctRow.TRUCK_SEAL_TAG,
                                    bctRow.DELIVERY_TIME,
                                    bctRow.CONTAINER_LOADED,
                                    T_GUID = bctRow.DELIVERY_GROUP_CODE,
                                    T_LOCATION = bctRow.LOCATION,
                                    T_PDASN = bctRow.DELIVERY_PDASN,
                                    bcRow.CONTAINER_TAG,
                                    bcRow.CONTAINER_SEAL_TAG,
                                    bcRow.CONTAINER_SEAL_TIME,
                                    bcRow.CONTAINER_LOAD_TIME,
                                    bcRow.BAG_LOADED,
                                    C_GUID = bcRow.SEAL_GROUP_CODE,
                                    C_LOCATION = bcRow.LOCATION,
                                    C_PDASN = bcRow.SEAL_PDASN,
                                    bRow.BAG_TAG,
                                    bRow.BSM_DATE,
                                    bRow.BAG_LOAD_PLACE,
                                    bRow.BAG_LOAD_TIME,
                                    bRow.SCAN_STATE,
                                    bRow.SCAN_OPERATOR,
                                    bRow.SCAN_TIME
                                };
                    InfoLog.Log("BoardingBagController", "GetAllLoadingBag", "所需裝載行李資訊查詢成功");

                    // 組合傳送訊息(BoardingBagList)
                    BoardingBagList allLoadingBag = Init_BoardingBagList(SYS_BRS, SYS_PDA);
                    Body body;
                    Containers containers;
                    LoadedBaggage loadedBaggage;
                    string lastTruckTag = string.Empty, lastTruckSealTag = string.Empty, lastContainerTag = string.Empty, lastContainerSealTag = string.Empty;
                    foreach (var row in query)
                    {
                        // 關聯"T1BRSDB"之"BSM_2DAY"資料表的[FLIGHT_NO]及"FIDS_2DAY"資料表的[STD]欄位
                        if (TB_BSM2Day.SelectByKey(row.BAG_TAG, queryDate) > 0)
                        {
                            TB_BSM2Day.RecordList.ForEach(obj => RowList_BSM.Add(obj as BSM2Day.Row));
                            if (TB_FIDS2Day.SelectByKey(RowList_BSM[0].BSM_FLIGHT, queryDate) > 0)
                            {
                                TB_FIDS2Day.RecordList.ForEach(obj => RowList_FIDS.Add(obj as FIDS2Day.Row));
                            }
                        }

                        // 若目前處理的資料列之"配送車識別"與上一次之值不同，則表示有新的Truck資料需新增，之後更新上一次識別值
                        if (row.TRUCK_TAG != lastTruckTag && row.TRUCK_SEAL_TAG != lastTruckSealTag)
                        {
                            body = Init_Body(BodyChoice.Truck);
                            (body.SealObj as Truck).BasicInfo.ID.TruckTag = row.TRUCK_TAG;
                            (body.SealObj as Truck).BasicInfo.ID.SealTag = row.TRUCK_SEAL_TAG;
                            (body.SealObj as Truck).BasicInfo.Records.DeliveryTime = row.DELIVERY_TIME.ToString("yyyyMMddHHmmss.fff");
                            (body.SealObj as Truck).BasicInfo.Records.ContainerCount = row.CONTAINER_LOADED;
                            (body.SealObj as Truck).BasicInfo.Records.GUID = row.T_GUID;
                            (body.SealObj as Truck).BasicInfo.Records.Location = row.T_LOCATION;
                            (body.SealObj as Truck).BasicInfo.Records.PDASN = row.T_PDASN;
                            allLoadingBag.LoadingList.Body.Add(body);
                            lastTruckTag = row.TRUCK_TAG;
                            lastTruckSealTag = row.TRUCK_SEAL_TAG;
                        }
                        // 若目前處理的資料列之"容器(籠車)識別"與上一次之值不同，則表示有新的Containers資料需新增，之後更新上一次識別值
                        if (row.CONTAINER_TAG != lastContainerTag && row.CONTAINER_SEAL_TAG != lastContainerSealTag)
                        {
                            containers = Init_Containers();
                            containers.Container.BasicInfo.ID.ContainerTag = row.CONTAINER_TAG;
                            containers.Container.BasicInfo.ID.SealTag = row.CONTAINER_SEAL_TAG;
                            containers.Container.BasicInfo.Records.SealTime = row.CONTAINER_SEAL_TIME.ToString("yyyyMMddHHmmss.fff");
                            containers.Container.BasicInfo.Records.LoadTime = row.CONTAINER_LOAD_TIME.ToString("yyyyMMddHHmmss.fff");
                            containers.Container.BasicInfo.Records.BagCount = row.BAG_LOADED;
                            containers.Container.BasicInfo.Records.GUID = row.C_GUID;
                            containers.Container.BasicInfo.Records.Location = row.C_LOCATION;
                            containers.Container.BasicInfo.Records.PDASN = row.C_PDASN;
                            (allLoadingBag.LoadingList.Body.Last().SealObj as Truck).Containers.Add(containers);
                            lastContainerTag = row.CONTAINER_TAG;
                            lastContainerSealTag = row.CONTAINER_SEAL_TAG;
                        }
                        // 每次遞迴處理行李裝載的資料列
                        loadedBaggage = Init_LoadedBaggage();
                        loadedBaggage.Baggage.ID.BagTag = row.BAG_TAG;
                        loadedBaggage.Baggage.ID.FlightNo = RowList_BSM.Count > 0 ? RowList_BSM[0].BSM_FLIGHT : FlightNo_Default; // "XX9999"表示查無關聯的BSM之所屬航班
                        loadedBaggage.Baggage.ID.STD = RowList_FIDS.Count > 0 ? RowList_FIDS[0].STD : STD_Default; // "0000"表示查無關聯的BSM之所屬航班的表訂出境時間
                        loadedBaggage.Baggage.Records.Loading.LoadPlace = row.BAG_LOAD_PLACE;
                        loadedBaggage.Baggage.Records.Loading.LoadTime = row.BAG_LOAD_TIME.ToString("yyyyMMddHHmmss.fff");
                        loadedBaggage.Baggage.Records.Unloading.ScanState = row.SCAN_STATE;
                        if (row.SCAN_STATE == true)
                        {
                            loadedBaggage.Baggage.Records.Unloading.ScanOper = row.SCAN_OPERATOR;
                            loadedBaggage.Baggage.Records.Unloading.ScanTime = row.SCAN_TIME?.ToString("yyyyMMddHHmmss.fff");
                        }
                        (allLoadingBag.LoadingList.Body.Last().SealObj as Truck).Containers.Last().Container.LoadedBaggage.Add(loadedBaggage);
                    }
                    allLoadingBag.LoadingList.Header.SendTime = DateTime.Now.ToString("yyyyMMddHHmmss.fff");

                    // 資料物件轉換為Json字串
                    using (TextReader reader = File.OpenText(JSDDirStr + @"BoardingBagList.json"))
                    {
                        JSchemaPreloadedResolver resolver = new JSchemaPreloadedResolver();
                        resolver.Add(new Uri("http://json-schema.brs.a3.ctci.com/Common/Header.json"), File.Open(JSDDirStr + @"Common\Header.json", FileMode.Open));
                        resolver.Add(new Uri("http://json-schema.brs.a3.ctci.com/Common/BagInfo.json"), File.Open(JSDDirStr + @"Common\BagInfo.json", FileMode.Open));
                        resolver.Add(new Uri("http://json-schema.brs.a3.ctci.com/Common/ContainerInfo.json"), File.Open(JSDDirStr + @"Common\ContainerInfo.json", FileMode.Open));
                        resolver.Add(new Uri("http://json-schema.brs.a3.ctci.com/Common/TruckInfo.json"), File.Open(JSDDirStr + @"Common\TruckInfo.json", FileMode.Open));
                        JSchema schema = JSchema.Load(new JsonTextReader(reader), resolver);
                        JObject JAllLoadingBag = JObject.FromObject(allLoadingBag);
                        bool isValid = JAllLoadingBag.IsValid(schema, out IList<string> messages);
                        if (isValid)
                        {
                            InfoLog.Log("BoardingBagController", "GetAllLoadingBag", "傳送訊息組合完成並轉換Json字串");
                            JsonRtn = "SUCCESS|" + JsonConvert.SerializeObject(allLoadingBag, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        }
                        else
                        {
                            ErrorLog.Log("BoardingBagController", "GetAllLoadingBag", "資料物件所轉換之Json字串驗證錯誤");
                            JsonRtn = "ERROR|202"; // 訊息格式驗證錯誤
                        }
                    }
                }
                else
                {
                    ErrorLog.Log("ERROR", "GetAllLoadingBag", "ERROR Code: 111");
                    JsonRtn = "ERROR|111"; // 查詢裝載行李資訊發生錯誤
                }

            }
            catch (Exception ex)
            {
                ErrorLog.Log("GetAllLoadingBag", ex);
                JsonRtn = "ERROR|101"; // Web API發生例外錯誤
            }
            finally
            {
                if (T1BRSDB != null)
                {
                    T1BRSDB.Close();
                }
                if (T2BRSDB != null)
                {
                    T2BRSDB.Close();
                    T2BRSDB = null;
                }
            }

            InfoLog.Log("BoardingBagController", "GetAllLoadingBag", "回傳處理結果");
            return JsonRtn;
        }

        /// <summary>
        /// 依指定條件參數取得今日(系統時間)已裝載之A3行李資訊列表
        /// </summary>
        /// <remarks>BoardingBagList訊息包含：
        /// <para>Header - 傳送來源、目的地與傳送時間</para>
        /// <para>Body - 配送車(列表) -> 運送哪些籠車(列表) -> 裝載哪些行李(列表)</para>
        /// </remarks>
        /// <param name="queryDate">資料查詢日期，需符合"yyyy-MM-dd"規則</param>
        /// <param name="QueryType">取得資訊清單所需之指定對象類型，作為基本查詢條件，包含
        /// <para>truck: 指定對象為配送車</para>
        /// <para>container: 指定對象為容器(籠車)</para>
        /// <para>baggage: 指定對象為裝載行李</para>
        /// </param>
        /// <param name="id">是否以指定對象類型之所屬為查詢條件
        /// <para>0: 以指定對象類型(如container)作為查詢條件</para>
        /// <para>1: 以指定對象類型(如container)之所屬(truck)作為查詢條件，參數可為非0</para>
        /// </param>
        /// <param name="ListType">清單中所需取得的資訊類型，包含
        /// <para>baggage: 僅需裝載行李之資訊清單</para>
        /// <para>container: 僅需運送容器(籠車)之資訊清單</para>
        /// <para>both: 運送容器(籠車)及其裝載行李之資訊清單</para>
        /// <para>all: 配送車、所運送容器(籠車)及其裝載行李之資訊清單</para>
        /// </param>
        /// <param name="level">資訊清單的內容程度
        /// <para>0: 依取得的資訊類型(<c>ListType</c>)需求，簡列查詢結果清單</para>
        /// <para>1: 依取得的資訊類型(<c>ListType</c>)需求，詳列查詢結果清單，參數可為非0</para>
        /// </param>
        /// <param name="Tag">依指定對象類型之查詢條件(<c>QueryType</c>及<c>id</c>)而定的編碼條件，如ContainerTag</param>
        /// <param name="SealTag">依指定對象類型之查詢條件(<c>QueryType</c>及<c>id</c>)而定的編碼條件，如ContainerSealTag，也可能不使用(預設為null)</param>
        /// <returns>傳送訊息(BoardingBagList)</returns>
        [Route("{QueryType}/{id}/{ListType}/{level}"), HttpGet]
        public string GetLoadingBagByConstraint(string queryDate, string QueryType, string id, string ListType, string level, string Tag, string SealTag = null)
        {
            LogBase.FileDirectory = HttpContext.Current.Server.MapPath("~/Log/");

            if (!(new Regex(@"^\d{4}-((0\d)|(1[012]))-(([012]\d)|3[01])$")).IsMatch(queryDate))
            {
                ErrorLog.Log("ERROR", "GetLoadingBagByConstraint", "ERROR Code: 201");
                return "ERROR|201"; // 資料查詢日期之規格不符
            }
            if (Service_Mode == "0")
            {
                queryDate = TestDate;
            }
            string tomorrow = DateTime.ParseExact(queryDate, "yyyy-MM-dd", CultureInfo.InvariantCulture).AddDays(1).ToString("yyyy-MM-dd");
            DataBase T1BRSDB = null, T2BRSDB = null;
            List<BSM2Day.Row> RowList_BSM = new List<BSM2Day.Row>();
            List<FIDS2Day.Row> RowList_FIDS = new List<FIDS2Day.Row>();
            List<BagBoarding.Row> RowList_Bag = new List<BagBoarding.Row>();
            List<ContainerInfo.Row> RowList_Container = new List<ContainerInfo.Row>();
            List<TruckInfo.Row> RowList_Truck = new List<TruckInfo.Row>();
            string JsonRtn = string.Empty;
            InfoLog.Log("BoardingBagController", "GetLoadingBagByConstraint", string.Format("資料查詢日期驗證成功，開始資料搜尋，Service_Mode = '{0}'，, queryDate = '{1}'",
                                                                    Service_Mode, queryDate));

            try
            {
                // 從資料庫取得目標資料
                T1BRSDB = DataBase.Instance(T1BRSDB_ConnStr);
                T2BRSDB = DataBase.Instance(T2BRSDB_ConnStr);
                if (T2BRSDB.Conn == new DataBase(null).Conn || T1BRSDB.Conn == new DataBase(null).Conn)
                {
                    ErrorLog.Log("ERROR", "GetLoadingBagByConstraint", "ERROR Code: 110");
                    return "ERROR|110"; // 資料庫連線失敗
                }
                BSM2Day TB_BSM2Day = new BSM2Day(T1BRSDB.Conn);
                FIDS2Day TB_FIDS2Day = new FIDS2Day(T1BRSDB.Conn);
                BagBoarding TB_BagBoarding = new BagBoarding(T2BRSDB.Conn);
                ContainerInfo TB_ContainerInfo = new ContainerInfo(T2BRSDB.Conn);
                TruckInfo TB_TruckInfo = new TruckInfo(T2BRSDB.Conn);
                if (TB_BagBoarding.SelectByLoadDate(queryDate) > 0
                    && TB_ContainerInfo.SelectByLoadDate(queryDate) > 0
                    && TB_TruckInfo.SelectByLoadDate(queryDate) > 0)
                {
                    TB_BagBoarding.RecordList.ForEach(obj => RowList_Bag.Add(obj as BagBoarding.Row));
                    TB_ContainerInfo.RecordList.ForEach(obj => RowList_Container.Add(obj as ContainerInfo.Row));
                    TB_TruckInfo.RecordList.ForEach(obj => RowList_Truck.Add(obj as TruckInfo.Row));
                    var query = from bRow in RowList_Bag
                                join cRow in RowList_Container on new { bRow.CONTAINER_TAG, bRow.CONTAINER_SEAL_TAG } equals new { cRow.CONTAINER_TAG, cRow.CONTAINER_SEAL_TAG } into bcList
                                from bcRow in bcList.DefaultIfEmpty()
                                join tRow in RowList_Truck on new { bcRow.TRUCK_TAG, bcRow.TRUCK_SEAL_TAG } equals new { tRow.TRUCK_TAG, tRow.TRUCK_SEAL_TAG } into bctList
                                from bctRow in bctList.DefaultIfEmpty()
                                where bRow.BAG_LOAD_TIME.CompareTo(DateTime.ParseExact(queryDate, "yyyy-MM-dd", CultureInfo.InvariantCulture)) >= 0
                                    && bRow.BAG_LOAD_TIME.CompareTo(DateTime.ParseExact(tomorrow, "yyyy-MM-dd", CultureInfo.InvariantCulture)) < 0
                                orderby bctRow.TRUCK_SEAL_TAG, bcRow.CONTAINER_SEAL_TAG, bRow.BAG_TAG
                                select new
                                {
                                    bctRow.TRUCK_TAG,
                                    bctRow.TRUCK_SEAL_TAG,
                                    bctRow.DELIVERY_TIME,
                                    bctRow.CONTAINER_LOADED,
                                    T_GUID = bctRow.DELIVERY_GROUP_CODE,
                                    T_LOCATION = bctRow.LOCATION,
                                    T_PDASN = bctRow.DELIVERY_PDASN,
                                    bcRow.CONTAINER_TAG,
                                    bcRow.CONTAINER_SEAL_TAG,
                                    bcRow.CONTAINER_SEAL_TIME,
                                    bcRow.CONTAINER_LOAD_TIME,
                                    bcRow.BAG_LOADED,
                                    C_GUID = bcRow.SEAL_GROUP_CODE,
                                    C_LOCATION = bcRow.LOCATION,
                                    C_PDASN = bcRow.SEAL_PDASN,
                                    bRow.BAG_TAG,
                                    bRow.BSM_DATE,
                                    bRow.BAG_LOAD_PLACE,
                                    bRow.BAG_LOAD_TIME,
                                    bRow.SCAN_STATE,
                                    bRow.SCAN_OPERATOR,
                                    bRow.SCAN_TIME
                                };
                    InfoLog.Log("BoardingBagController", "GetLoadingBagByConstraint", "所需裝載行李資訊查詢成功");

                    // 依據指定對象類型決定資訊清單訊息的查詢條件
                    switch (QueryType.ToLower())
                    {
                        case "truck":
                            if (id == "0")
                            {
                                // "指定的配送車下"作為查詢條件
                                query = from bctRow in query
                                        where bctRow.TRUCK_TAG.Equals(Tag) && bctRow.TRUCK_SEAL_TAG.Equals(SealTag)
                                        select bctRow;
                            }
                            else
                            {
                                // 指定的配送車上無所屬物件，回傳指定對象類型錯誤
                                ErrorLog.Log("ERROR", "GetLoadingBagByConstraint", "ERROR Code: 122"); // "id"參數錯誤或不允許
                                return "ERROR|120";
                            }
                            break;
                        case "container":
                            if (id == "0")
                            {
                                // "指定的容器(籠車)下"作為查詢條件
                                query = from bctRow in query
                                        where bctRow.CONTAINER_TAG.Equals(Tag) && bctRow.CONTAINER_SEAL_TAG.Equals(SealTag)
                                        select bctRow;
                            }
                            else
                            {
                                // "指定的容器(籠車)上之所屬配送車"作為查詢條件
                                var queryT = query.Where(row => row.CONTAINER_TAG.Equals(Tag) && row.CONTAINER_SEAL_TAG.Equals(SealTag)).First();
                                query = from bctRow in query
                                        where bctRow.TRUCK_TAG.Equals(queryT.TRUCK_TAG) && bctRow.TRUCK_SEAL_TAG.Equals(queryT.TRUCK_SEAL_TAG)
                                        select bctRow;
                            }
                            break;
                        case "baggage":
                            if (id == "0")
                            {
                                // "指定的裝載行李"作為查詢條件
                                query = from bctRow in query
                                        where bctRow.BAG_TAG.Equals(Tag)
                                        select bctRow;
                            }
                            else
                            {
                                // "指定的裝載行李之所屬容器(籠車)"作為查詢條件
                                var queryC = query.Where(row => row.BAG_TAG.Equals(Tag)).First();
                                query = from bctRow in query
                                        where bctRow.CONTAINER_TAG.Equals(queryC.CONTAINER_TAG) && bctRow.CONTAINER_SEAL_TAG.Equals(queryC.CONTAINER_SEAL_TAG)
                                        select bctRow;
                            }
                            break;
                        default:
                            // 非許可的指定對象類型，回傳指定對象類型錯誤
                            ErrorLog.Log("ERROR", "GetLoadingBagByConstraint", "ERROR Code: 121");
                            return "ERROR|121"; // QueryType參數錯誤或不允許
                    }

                    // 依所需取得的資訊類型組合傳送訊息(BoardingBagList)
                    BoardingBagList allLoadingBag = Init_BoardingBagList(SYS_BRS, SYS_PDA);
                    string lastTruckTag = string.Empty, lastTruckSealTag = string.Empty, lastContainerTag = string.Empty, lastContainerSealTag = string.Empty;
                    BodyChoice choice = default;
                    foreach (var row in query)
                    {
                        // 關聯"T1BRSDB"之"BSM_2DAY"資料表的[FLIGHT_NO]及"FIDS_2DAY"資料表的[STD]欄位
                        if (TB_BSM2Day.SelectByKey(row.BAG_TAG, queryDate) > 0)
                        {
                            TB_BSM2Day.RecordList.ForEach(obj => RowList_BSM.Add(obj as BSM2Day.Row));
                            if (TB_FIDS2Day.SelectByKey(RowList_BSM[0].BSM_FLIGHT, queryDate) > 0)
                            {
                                TB_FIDS2Day.RecordList.ForEach(obj => RowList_FIDS.Add(obj as FIDS2Day.Row));
                            }
                        }

                        switch (ListType.ToLower())
                        {
                            case "baggage":
                                // 僅需裝載行李之資訊清單
                                (allLoadingBag.LoadingList.Body ?? (allLoadingBag.LoadingList.Body = Enumerable.Empty<Body>().ToList()))
                                    .Add(Init_Body(BodyChoice.Baggage));
                                (allLoadingBag.LoadingList.Body.Last().SealObj as Baggage).ID.BagTag = row.BAG_TAG;
                                (allLoadingBag.LoadingList.Body.Last().SealObj as Baggage).ID.FlightNo = RowList_BSM.Count > 0 ? RowList_BSM[0].BSM_FLIGHT : FlightNo_Default; // "XX9999"表示查無關聯的BSM之所屬航班
                                (allLoadingBag.LoadingList.Body.Last().SealObj as Baggage).ID.STD = RowList_FIDS.Count > 0 ? RowList_FIDS[0].STD : STD_Default; // "0000"表示查無關聯的BSM之所屬航班的表訂出境時間
                                (allLoadingBag.LoadingList.Body.Last().SealObj as Baggage).Records.Unloading.ScanState = row.SCAN_STATE;
                                if (level != "0")
                                {
                                    (allLoadingBag.LoadingList.Body.Last().SealObj as Baggage).Records.Loading.LoadPlace = row.BAG_LOAD_PLACE;
                                    (allLoadingBag.LoadingList.Body.Last().SealObj as Baggage).Records.Loading.LoadTime = row.BAG_LOAD_TIME.ToString("yyyyMMddHHmmss.fff");
                                    (allLoadingBag.LoadingList.Body.Last().SealObj as Baggage).Records.Unloading.ScanOper = string.IsNullOrEmpty(row.SCAN_OPERATOR) ? null : row.SCAN_OPERATOR;
                                    (allLoadingBag.LoadingList.Body.Last().SealObj as Baggage).Records.Unloading.ScanTime = row.SCAN_TIME?.ToString("yyyyMMddHHmmss.fff");
                                }
                                choice = BodyChoice.Baggage;
                                break;
                            case "container":
                                // 僅需運送容器(籠車)之資訊清單
                                // 若目前處理的資料列之"容器(籠車)識別"與上一次之值不同，則表示有新的Containers資料需新增，之後更新上一次識別值
                                if (row.CONTAINER_TAG != lastContainerTag && row.CONTAINER_SEAL_TAG != lastContainerSealTag)
                                {
                                    (allLoadingBag.LoadingList.Body ?? (allLoadingBag.LoadingList.Body = Enumerable.Empty<Body>().ToList()))
                                        .Add(Init_Body(BodyChoice.Container));
                                    (allLoadingBag.LoadingList.Body.Last().SealObj as Container).BasicInfo.ID.ContainerTag = row.CONTAINER_TAG;
                                    (allLoadingBag.LoadingList.Body.Last().SealObj as Container).BasicInfo.ID.SealTag = row.CONTAINER_SEAL_TAG;
                                    (allLoadingBag.LoadingList.Body.Last().SealObj as Container).BasicInfo.Records.BagCount = row.BAG_LOADED;
                                    (allLoadingBag.LoadingList.Body.Last().SealObj as Container).BasicInfo.Records.ScannedCount =
                                        query.Count(r => r.CONTAINER_TAG.Equals(row.CONTAINER_TAG) && r.CONTAINER_SEAL_TAG.Equals(row.CONTAINER_SEAL_TAG) && r.SCAN_STATE == true);
                                    if (level != "0")
                                    {
                                        (allLoadingBag.LoadingList.Body.Last().SealObj as Container).BasicInfo.Records.SealTime = row.CONTAINER_SEAL_TIME.ToString("yyyyMMddHHmmss.fff");
                                        (allLoadingBag.LoadingList.Body.Last().SealObj as Container).BasicInfo.Records.LoadTime = row.CONTAINER_LOAD_TIME.ToString("yyyyMMddHHmmss.fff");
                                        (allLoadingBag.LoadingList.Body.Last().SealObj as Container).BasicInfo.Records.GUID = row.C_GUID;
                                        (allLoadingBag.LoadingList.Body.Last().SealObj as Container).BasicInfo.Records.Location = row.C_LOCATION;
                                        (allLoadingBag.LoadingList.Body.Last().SealObj as Container).BasicInfo.Records.PDASN = string.IsNullOrEmpty(row.C_PDASN) ? null : row.C_PDASN;
                                    }
                                    lastContainerTag = row.CONTAINER_TAG;
                                    lastContainerSealTag = row.CONTAINER_SEAL_TAG;
                                }
                                choice = BodyChoice.Container;
                                break;
                            case "both":
                                // 運送容器(籠車)及其裝載行李之資訊清單
                                // 若目前處理的資料列之"容器(籠車)識別"與上一次之值不同，則表示有新的Containers資料需新增，之後更新上一次識別值
                                if (row.CONTAINER_TAG != lastContainerTag && row.CONTAINER_SEAL_TAG != lastContainerSealTag)
                                {
                                    (allLoadingBag.LoadingList.Body ?? (allLoadingBag.LoadingList.Body = Enumerable.Empty<Body>().ToList()))
                                        .Add(Init_Body(BodyChoice.Container));
                                    (allLoadingBag.LoadingList.Body.Last().SealObj as Container).BasicInfo.ID.ContainerTag = row.CONTAINER_TAG;
                                    (allLoadingBag.LoadingList.Body.Last().SealObj as Container).BasicInfo.ID.SealTag = row.CONTAINER_SEAL_TAG;
                                    (allLoadingBag.LoadingList.Body.Last().SealObj as Container).BasicInfo.Records.BagCount = row.BAG_LOADED;
                                    (allLoadingBag.LoadingList.Body.Last().SealObj as Container).BasicInfo.Records.ScannedCount =
                                        query.Count(r => r.CONTAINER_TAG.Equals(row.CONTAINER_TAG) && r.CONTAINER_SEAL_TAG.Equals(row.CONTAINER_SEAL_TAG) && r.SCAN_STATE == true);
                                    if (level != "0")
                                    {
                                        (allLoadingBag.LoadingList.Body.Last().SealObj as Container).BasicInfo.Records.SealTime = row.CONTAINER_SEAL_TIME.ToString("yyyyMMddHHmmss.fff");
                                        (allLoadingBag.LoadingList.Body.Last().SealObj as Container).BasicInfo.Records.LoadTime = row.CONTAINER_LOAD_TIME.ToString("yyyyMMddHHmmss.fff");
                                        (allLoadingBag.LoadingList.Body.Last().SealObj as Container).BasicInfo.Records.GUID = row.C_GUID;
                                        (allLoadingBag.LoadingList.Body.Last().SealObj as Container).BasicInfo.Records.Location = row.C_LOCATION;
                                        (allLoadingBag.LoadingList.Body.Last().SealObj as Container).BasicInfo.Records.PDASN = string.IsNullOrEmpty(row.C_PDASN) ? null : row.C_PDASN;
                                    }
                                    lastContainerTag = row.CONTAINER_TAG;
                                    lastContainerSealTag = row.CONTAINER_SEAL_TAG;
                                }
                                // 每次遞迴處理行李裝載的資料列
                                ((allLoadingBag.LoadingList.Body.Last().SealObj as Container).LoadedBaggage ?? ((allLoadingBag.LoadingList.Body.Last().SealObj as Container).LoadedBaggage = Enumerable.Empty<LoadedBaggage>().ToList()))
                                    .Add(Init_LoadedBaggage());
                                (allLoadingBag.LoadingList.Body.Last().SealObj as Container).LoadedBaggage.Last().Baggage.ID.BagTag = row.BAG_TAG;
                                (allLoadingBag.LoadingList.Body.Last().SealObj as Container).LoadedBaggage.Last().Baggage.ID.FlightNo =
                                    RowList_BSM.Count > 0 ? RowList_BSM[0].BSM_FLIGHT : FlightNo_Default; // "XX9999"表示查無關聯的BSM之所屬航班
                                (allLoadingBag.LoadingList.Body.Last().SealObj as Container).LoadedBaggage.Last().Baggage.ID.STD =
                                    RowList_FIDS.Count > 0 ? RowList_FIDS[0].STD : STD_Default; // "0000"表示查無關聯的BSM之所屬航班的表訂出境時間
                                (allLoadingBag.LoadingList.Body.Last().SealObj as Container).LoadedBaggage.Last().Baggage.Records.Unloading.ScanState = row.SCAN_STATE;
                                if (level != "0")
                                {
                                    (allLoadingBag.LoadingList.Body.Last().SealObj as Container).LoadedBaggage.Last().Baggage.Records.Loading.LoadPlace = row.BAG_LOAD_PLACE;
                                    (allLoadingBag.LoadingList.Body.Last().SealObj as Container).LoadedBaggage.Last().Baggage.Records.Loading.LoadTime = row.BAG_LOAD_TIME.ToString("yyyyMMddHHmmss.fff");
                                    (allLoadingBag.LoadingList.Body.Last().SealObj as Container).LoadedBaggage.Last().Baggage.Records.Unloading.ScanOper = string.IsNullOrEmpty(row.SCAN_OPERATOR) ? null : row.SCAN_OPERATOR;
                                    (allLoadingBag.LoadingList.Body.Last().SealObj as Container).LoadedBaggage.Last().Baggage.Records.Unloading.ScanTime = row.SCAN_TIME?.ToString("yyyyMMddHHmmss.fff");
                                }
                                choice = BodyChoice.Container;
                                break;
                            case "all":
                                // 配送車、所運送容器(籠車)及其裝載行李之資訊清單
                                // 若目前處理的資料列之"配送車識別"與上一次之值不同，則表示有新的Truck資料需新增，之後更新上一次識別值
                                if (row.TRUCK_TAG != lastTruckTag && row.TRUCK_SEAL_TAG != lastTruckSealTag)
                                {
                                    (allLoadingBag.LoadingList.Body ?? (allLoadingBag.LoadingList.Body = Enumerable.Empty<Body>().ToList()))
                                        .Add(Init_Body(BodyChoice.Truck));
                                    (allLoadingBag.LoadingList.Body.Last().SealObj as Truck).BasicInfo.ID.TruckTag = row.TRUCK_TAG;
                                    (allLoadingBag.LoadingList.Body.Last().SealObj as Truck).BasicInfo.ID.SealTag = row.TRUCK_SEAL_TAG;
                                    (allLoadingBag.LoadingList.Body.Last().SealObj as Truck).BasicInfo.Records.ContainerCount = row.CONTAINER_LOADED;
                                    if (level != "0")
                                    {
                                        (allLoadingBag.LoadingList.Body.Last().SealObj as Truck).BasicInfo.Records.DeliveryTime = row.DELIVERY_TIME.ToString("yyyyMMddHHmmss.fff");
                                        (allLoadingBag.LoadingList.Body.Last().SealObj as Truck).BasicInfo.Records.GUID = row.T_GUID;
                                        (allLoadingBag.LoadingList.Body.Last().SealObj as Truck).BasicInfo.Records.Location = row.T_LOCATION;
                                        (allLoadingBag.LoadingList.Body.Last().SealObj as Truck).BasicInfo.Records.PDASN = string.IsNullOrEmpty(row.T_PDASN) ? null : row.T_PDASN;
                                    }
                                    lastTruckTag = row.TRUCK_TAG;
                                    lastTruckSealTag = row.TRUCK_SEAL_TAG;
                                }
                                // 若目前處理的資料列之"容器(籠車)識別"與上一次之值不同，則表示有新的Containers資料需新增，之後更新上一次識別值
                                if (row.CONTAINER_TAG != lastContainerTag && row.CONTAINER_SEAL_TAG != lastContainerSealTag)
                                {
                                    ((allLoadingBag.LoadingList.Body.Last().SealObj as Truck).Containers ?? ((allLoadingBag.LoadingList.Body.Last().SealObj as Truck).Containers = Enumerable.Empty<Containers>().ToList()))
                                        .Add(Init_Containers());
                                    (allLoadingBag.LoadingList.Body.Last().SealObj as Truck).Containers.Last().Container.BasicInfo.ID.ContainerTag = row.CONTAINER_TAG;
                                    (allLoadingBag.LoadingList.Body.Last().SealObj as Truck).Containers.Last().Container.BasicInfo.ID.SealTag = row.CONTAINER_SEAL_TAG;
                                    (allLoadingBag.LoadingList.Body.Last().SealObj as Truck).Containers.Last().Container.BasicInfo.Records.BagCount =
                                        query.Count(r => r.CONTAINER_TAG.Equals(row.CONTAINER_TAG) && r.CONTAINER_SEAL_TAG.Equals(row.CONTAINER_SEAL_TAG));
                                    (allLoadingBag.LoadingList.Body.Last().SealObj as Truck).Containers.Last().Container.BasicInfo.Records.ScannedCount =
                                        query.Count(r => r.CONTAINER_TAG.Equals(row.CONTAINER_TAG) && r.CONTAINER_SEAL_TAG.Equals(row.CONTAINER_SEAL_TAG) && r.SCAN_STATE == true);
                                    if (level != "0")
                                    {
                                        (allLoadingBag.LoadingList.Body.Last().SealObj as Truck).Containers.Last().Container.BasicInfo.Records.SealTime = row.CONTAINER_SEAL_TIME.ToString("yyyyMMddHHmmss.fff");
                                        (allLoadingBag.LoadingList.Body.Last().SealObj as Truck).Containers.Last().Container.BasicInfo.Records.LoadTime = row.CONTAINER_LOAD_TIME.ToString("yyyyMMddHHmmss.fff");
                                        (allLoadingBag.LoadingList.Body.Last().SealObj as Truck).Containers.Last().Container.BasicInfo.Records.GUID = row.C_GUID;
                                        (allLoadingBag.LoadingList.Body.Last().SealObj as Truck).Containers.Last().Container.BasicInfo.Records.Location = row.C_LOCATION;
                                        (allLoadingBag.LoadingList.Body.Last().SealObj as Truck).Containers.Last().Container.BasicInfo.Records.PDASN = string.IsNullOrEmpty(row.C_PDASN) ? null : row.C_PDASN;
                                    }
                                    lastContainerTag = row.CONTAINER_TAG;
                                    lastContainerSealTag = row.CONTAINER_SEAL_TAG;
                                }
                                // 每次遞迴處理行李裝載的資料列
                                ((allLoadingBag.LoadingList.Body.Last().SealObj as Truck).Containers.Last().Container.LoadedBaggage ?? ((allLoadingBag.LoadingList.Body.Last().SealObj as Truck).Containers.Last().Container.LoadedBaggage = Enumerable.Empty<LoadedBaggage>().ToList()))
                                    .Add(Init_LoadedBaggage());
                                (allLoadingBag.LoadingList.Body.Last().SealObj as Truck).Containers.Last().Container.LoadedBaggage.Last().Baggage.ID.BagTag = row.BAG_TAG;
                                (allLoadingBag.LoadingList.Body.Last().SealObj as Truck).Containers.Last().Container.LoadedBaggage.Last().Baggage.ID.FlightNo =
                                    RowList_BSM.Count > 0 ? RowList_BSM[0].BSM_FLIGHT : FlightNo_Default; // "XX9999"表示查無關聯的BSM之所屬航班
                                (allLoadingBag.LoadingList.Body.Last().SealObj as Truck).Containers.Last().Container.LoadedBaggage.Last().Baggage.ID.STD =
                                    RowList_FIDS.Count > 0 ? RowList_FIDS[0].STD : STD_Default; // "0000"表示查無關聯的BSM之所屬航班的表訂出境時間
                                (allLoadingBag.LoadingList.Body.Last().SealObj as Truck).Containers.Last().Container.LoadedBaggage.Last().Baggage.Records.Unloading.ScanState = row.SCAN_STATE;
                                if (level != "0")
                                {
                                    (allLoadingBag.LoadingList.Body.Last().SealObj as Truck).Containers.Last().Container.LoadedBaggage.Last().Baggage.Records.Loading.LoadPlace = row.BAG_LOAD_PLACE;
                                    (allLoadingBag.LoadingList.Body.Last().SealObj as Truck).Containers.Last().Container.LoadedBaggage.Last().Baggage.Records.Loading.LoadTime = row.BAG_LOAD_TIME.ToString("yyyyMMddHHmmss.fff");
                                    (allLoadingBag.LoadingList.Body.Last().SealObj as Truck).Containers.Last().Container.LoadedBaggage.Last().Baggage.Records.Unloading.ScanOper = string.IsNullOrEmpty(row.SCAN_OPERATOR) ? null : row.SCAN_OPERATOR;
                                    (allLoadingBag.LoadingList.Body.Last().SealObj as Truck).Containers.Last().Container.LoadedBaggage.Last().Baggage.Records.Unloading.ScanTime = row.SCAN_TIME?.ToString("yyyyMMddHHmmss.fff");
                                }
                                choice = BodyChoice.Truck;
                                break;
                            default:
                                // 非許可的所需取得資訊類型，回傳取得資訊類型錯誤
                                ErrorLog.Log("ERROR", "GetLoadingBagByConstraint", "ERROR Code: 123");
                                return "ERROR|123"; // ListType參數錯誤或不允許
                        }
                    }
                    allLoadingBag.LoadingList.Header.SendTime = DateTime.Now.ToString("yyyyMMddHHmmss.fff");

                    // 資料物件轉換為Json字串
                    using (TextReader reader = File.OpenText(JSDDirStr + @"BoardingBagList.json"))
                    {
                        JSchemaPreloadedResolver resolver = new JSchemaPreloadedResolver();
                        resolver.Add(new Uri("http://json-schema.brs.a3.ctci.com/Common/Header.json"), File.Open(JSDDirStr + @"Common\Header.json", FileMode.Open));
                        resolver.Add(new Uri("http://json-schema.brs.a3.ctci.com/Common/BagInfo.json"), File.Open(JSDDirStr + @"Common\BagInfo.json", FileMode.Open));
                        resolver.Add(new Uri("http://json-schema.brs.a3.ctci.com/Common/ContainerInfo.json"), File.Open(JSDDirStr + @"Common\ContainerInfo.json", FileMode.Open));
                        resolver.Add(new Uri("http://json-schema.brs.a3.ctci.com/Common/TruckInfo.json"), File.Open(JSDDirStr + @"Common\TruckInfo.json", FileMode.Open));
                        JSchema schema = JSchema.Load(new JsonTextReader(reader), resolver);
                        JObject JAllLoadingBag = JObject.FromObject(allLoadingBag, new JsonSerializer() { ContractResolver = new CustomResolver(choice) });
                        bool isValid = JAllLoadingBag.IsValid(schema, out IList<string> messages);
                        if (isValid)
                        {
                            InfoLog.Log("BoardingBagController", "GetLoadingBagByConstraint", "傳送訊息組合完成並轉換Json字串");
                            JsonSerializerSettings settings = new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore,
                                ContractResolver = new CustomResolver(choice)
                            };
                            JsonRtn = "SUCCESS|" + JsonConvert.SerializeObject(allLoadingBag, Formatting.None, settings);
                        }
                        else
                        {
                            ErrorLog.Log("BoardingBagController", "GetLoadingBagByConstraint", "ERROR Code: 202");
                            JsonRtn = "ERROR|202"; // 訊息格式驗證錯誤
                        }
                    }
                }
                else
                {
                    ErrorLog.Log("ERROR", "GetLoadingBagByConstraint", "ERROR Code: 111");
                    JsonRtn = "ERROR|111"; // 查詢裝載行李資訊發生錯誤
                }
            }
            catch (Exception ex)
            {
                ErrorLog.Log("GetLoadingBagByConstraint", ex);
                JsonRtn = "ERROR|101"; // Web API發生例外錯誤
            }
            finally
            {
                if (T1BRSDB != null)
                {
                    T1BRSDB.Close();
                }
                if (T2BRSDB != null)
                {
                    T2BRSDB.Close();
                }
            }

            InfoLog.Log("BoardingBagController", "GetLoadingBagByConstraint", "回傳處理結果");
            return JsonRtn;
        }

        /// <summary>
        /// 依行李清單設定今日(系統時間)已裝載之A3行李掃描確認狀態
        /// </summary>
        /// <param name="bagList">Json格式之行李裝載資訊列表字串(BoardingBagList)</param>
        /// <returns>回應設定狀態</returns>
        [Route("unloading"), HttpPut]
        public string SetUnloadingBagState([FromBody] JObject bagList)
        {
            LogBase.FileDirectory = HttpContext.Current.Server.MapPath("~/Log/");

            DateTime setDate = Service_Mode == "0" ?
                DateTime.ParseExact(TestDate, "yyyy-MM-dd", CultureInfo.InvariantCulture) :
                DateTime.Now;
            DataBase T2BRSDB = null;
            DBTransaction Transaction_T2BRSDB = new DBTransaction();
            string JsonRtn = string.Empty;
            InfoLog.Log("BoardingBagController", "SetUnloadingBagState", string.Format("所接收的行李裝載資料列表字串 = \n{0}\n", bagList.ToString()));
            InfoLog.Log("BoardingBagController", "SetUnloadingBagState", string.Format("開始解析行李裝載資料列表字串，Service_Mode = '{0}'，, setDate = '{1}'",
                                                                                    Service_Mode, setDate));

            try
            {
                // 驗證Json字串並作序列轉換
                using (TextReader reader = File.OpenText(JSDDirStr + @"BoardingBagList.json"))
                {
                    JSchemaPreloadedResolver resolver = new JSchemaPreloadedResolver();
                    resolver.Add(new Uri("http://json-schema.brs.a3.ctci.com/Common/Header.json"), File.Open(JSDDirStr + @"Common\Header.json", FileMode.Open));
                    resolver.Add(new Uri("http://json-schema.brs.a3.ctci.com/Common/BagInfo.json"), File.Open(JSDDirStr + @"Common\BagInfo.json", FileMode.Open));
                    resolver.Add(new Uri("http://json-schema.brs.a3.ctci.com/Common/ContainerInfo.json"), File.Open(JSDDirStr + @"Common\ContainerInfo.json", FileMode.Open));
                    resolver.Add(new Uri("http://json-schema.brs.a3.ctci.com/Common/TruckInfo.json"), File.Open(JSDDirStr + @"Common\TruckInfo.json", FileMode.Open));
                    JSchema schema = JSchema.Load(new JsonTextReader(reader), resolver);
                    bool isValid = bagList.IsValid(schema, out IList<string> messages);
                    if (isValid)
                    {
                        JsonSerializerSettings settings = new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            ContractResolver = new CustomResolver(BodyChoice.Baggage)
                        };
                        BoardingBagList JsonObj = JsonConvert.DeserializeObject<BoardingBagList>(bagList.ToString(), settings);

                        // 彙整資料並存入資料庫
                        T2BRSDB = DataBase.Instance(T2BRSDB_ConnStr);
                        if (T2BRSDB.Conn == new DataBase(null).Conn)
                        {
                            ErrorLog.Log("ERROR", "SetUnloadingBagState", "ERROR Code: 110");
                            return "ERROR|110"; // 資料庫連線失敗
                        }
                        BagBoarding TB_BagBoarding = new BagBoarding(T2BRSDB.Conn);
                        Transaction_T2BRSDB.BeginTransaction(TB_BagBoarding);

                        foreach (Body body in JsonObj.LoadingList.Body)
                        {
                            Baggage bag = JsonConvert.DeserializeObject<Baggage>((body.SealObj as JObject).ToString(), settings);
                            if (TB_BagBoarding.SelectByKey(bag.ID.BagTag, setDate.ToString("yyyy-MM-dd")) > 0)
                            {
                                // 今日已加入的裝載行李資訊進行掃描狀態設定，<BagScanState>為false會被忽略
                                BagBoarding.Row Row_Bag = TB_BagBoarding.RecordList[0] as BagBoarding.Row;
                                if (Row_Bag.SCAN_STATE == false && bag.Records.Unloading.ScanState == true)
                                {
                                    Row_Bag.SCAN_STATE = true;
                                    Row_Bag.SCAN_OPERATOR = bag.Records.Unloading.ScanOper;
                                    Row_Bag.SCAN_TIME = DateTime.ParseExact(bag.Records.Unloading.ScanTime, "yyyyMMddHHmmss.fff", CultureInfo.InvariantCulture);
                                    Row_Bag.UPDATE_TIME = DateTime.Now;
                                    Transaction_T2BRSDB.SetTransactionResult(TB_BagBoarding, TB_BagBoarding.Update(Row_Bag));
                                }
                            }
                        }

                        if (Transaction_T2BRSDB.EndTransaction()[TB_BagBoarding.SqlCmd.Connection])
                        {
                            InfoLog.Log("BoardingBagController", "SetUnloadingBagState", "行李裝載資料列表Json字串解析驗證成功，並完成行李掃描確認資訊之修改");
                            JsonRtn = "SUCCESS";
                        }
                        else
                        {
                            ErrorLog.Log("BoardingBagController", "SetUnloadingBagState", "ERROR Code: 113");
                            JsonRtn = "ERROR|113"; // 行李掃描確認資訊之修改失敗回捲，未進行任何設定儲存
                        }
                    }
                    else
                    {
                        ErrorLog.Log("BoardingBagController", "SetUnloadingBagState", "ERROR Code: 202");
                        JsonRtn = "ERROR|202"; // 訊息格式驗證錯誤
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog.Log("SetUnloadingBagState", ex);
                JsonRtn = "ERROR|101"; // Web API發生例外錯誤
            }
            finally
            {
                if (T2BRSDB != null)
                {
                    T2BRSDB.Close();
                }
            }

            return JsonRtn;
        }

        /// <summary>
        /// 依行李清單新增今日(系統時間)已裝載之A3行李資訊(包含異常行李)
        /// </summary>
        /// <param name="bagList">Json格式之行李裝載資訊列表字串(BoardingBagList)</param>
        /// <param name="ProcType">新的A3裝載行李資訊新增之處理方式，包含
        /// <para>source: 行李資訊是由A3站即行李裝載源頭所產生，"BoardingBagList"需具備完整的"Body - 配送車(列表) -> 運送哪些籠車(列表) -> 裝載哪些行李(列表)"</para>
        /// <para>informal: 非正規(上述由源頭所產生)方式所裝載運送之行李，"BoardingBagList"僅需簡單的"Baggage"裝載運送行李之基本資訊，將以指定值設定其作資料儲存所需關聯之容器(籠車)與配送車</para>
        /// </param>
        /// <returns>回應新增狀態</returns>
        /// <remarks>[ContainerTag]=X9999X,[ContainerSealTag]=CX9999X,[TruckTag]=A3-999,[TruckSealTag]=TX9999X,[PDASN]=PDA_X9，強制裝載用</remarks>
        [Route("new-loading/{ProcType}"), HttpPost]
        public string SetNewLoadingBagList([FromBody] JObject bagList, string ProcType)
        {
            LogBase.FileDirectory = HttpContext.Current.Server.MapPath("~/Log/");

            DateTime setDate = Service_Mode == "0" ?
                DateTime.ParseExact(TestDate, "yyyy-MM-dd", CultureInfo.InvariantCulture) :
                DateTime.Now;
            DataBase T2BRSDB = null;
            DBTransaction Transaction_T2BRSDB = new DBTransaction();
            string JsonRtn = string.Empty;
            InfoLog.Log("BoardingBagController", "SetNewLoadingBagList", string.Format("所接收的行李裝載資料列表字串 = \n{0}\n", bagList.ToString()));
            InfoLog.Log("BoardingBagController", "SetNewLoadingBagList", string.Format("開始解析行李裝載資料列表字串，Service_Mode = '{0}'，, setDate = '{1}'",
                                                                                    Service_Mode, setDate));

            try
            {
                // 驗證Json字串並作序列轉換
                using (TextReader reader = File.OpenText(JSDDirStr + @"BoardingBagList.json"))
                {
                    JSchemaPreloadedResolver resolver = new JSchemaPreloadedResolver();
                    resolver.Add(new Uri("http://json-schema.brs.a3.ctci.com/Common/Header.json"), File.Open(JSDDirStr + @"Common\Header.json", FileMode.Open));
                    resolver.Add(new Uri("http://json-schema.brs.a3.ctci.com/Common/BagInfo.json"), File.Open(JSDDirStr + @"Common\BagInfo.json", FileMode.Open));
                    resolver.Add(new Uri("http://json-schema.brs.a3.ctci.com/Common/ContainerInfo.json"), File.Open(JSDDirStr + @"Common\ContainerInfo.json", FileMode.Open));
                    resolver.Add(new Uri("http://json-schema.brs.a3.ctci.com/Common/TruckInfo.json"), File.Open(JSDDirStr + @"Common\TruckInfo.json", FileMode.Open));
                    JSchema schema = JSchema.Load(new JsonTextReader(reader), resolver);
                    bool isValid = bagList.IsValid(schema, out IList<string> messages);
                    if (isValid)
                    {
                        // 彙整資料並存入資料庫
                        T2BRSDB = DataBase.Instance(T2BRSDB_ConnStr);
                        if (T2BRSDB.Conn == new DataBase(null).Conn)
                        {
                            ErrorLog.Log("ERROR", "SetUnloadingBagState", "ERROR Code: 110");
                            return "ERROR|110"; // 資料庫連線失敗
                        }
                        BagBoarding TB_BagBoarding = new BagBoarding(T2BRSDB.Conn);
                        ContainerInfo TB_ContainerInfo = new ContainerInfo(T2BRSDB.Conn);
                        TruckInfo TB_TruckInfo = new TruckInfo(T2BRSDB.Conn);
                        Transaction_T2BRSDB.BeginTransaction(TB_BagBoarding);
                        Transaction_T2BRSDB.BeginTransaction(TB_ContainerInfo);
                        Transaction_T2BRSDB.BeginTransaction(TB_TruckInfo);
                        JsonSerializerSettings settings = default;
                        BoardingBagList JsonObj = default;
                        switch (ProcType)
                        {
                            case "source":
                                // 行李資訊是由A3站即行李裝載源頭所產生
                                settings = new JsonSerializerSettings
                                {
                                    NullValueHandling = NullValueHandling.Ignore,
                                    ContractResolver = new CustomResolver(BodyChoice.Truck)
                                };
                                JsonObj = JsonConvert.DeserializeObject<BoardingBagList>(bagList.ToString(), settings);

                                foreach (Body body in JsonObj.LoadingList.Body)
                                {
                                    Truck truck = JsonConvert.DeserializeObject<Truck>((body.SealObj as JObject).ToString(), settings);
                                    DateTime nowTime = DateTime.Now;
                                    if (TB_TruckInfo.SelectByKey(truck.BasicInfo.ID.TruckTag, truck.BasicInfo.ID.SealTag, setDate.ToString("yyyy-MM-dd")) <= 0)
                                    {
                                        TruckInfo.Row Row_Truck = new TruckInfo.Row()
                                        {
                                            TRUCK_TAG = truck.BasicInfo.ID.TruckTag,
                                            TRUCK_SEAL_TAG = truck.BasicInfo.ID.SealTag,
                                            CONTAINER_LOADED = truck.BasicInfo.Records.ContainerCount,
                                            DELIVERY_TIME = DateTime.ParseExact(truck.BasicInfo.Records.DeliveryTime, "yyyyMMddHHmmss.fff", CultureInfo.InvariantCulture),
                                            DELIVERY_GROUP_CODE = truck.BasicInfo.Records.GUID,
                                            DELIVERY_PDASN = truck.BasicInfo.Records.PDASN,
                                            LOCATION = truck.BasicInfo.Records.Location,
                                            UPDATE_TIME = nowTime
                                        };
                                        Transaction_T2BRSDB.SetTransactionResult(TB_TruckInfo, TB_TruckInfo.Insert(Row_Truck));
                                    }

                                    foreach (Containers containers in truck.Containers)
                                    {
                                        if (TB_ContainerInfo.SelectByKey(containers.Container.BasicInfo.ID.ContainerTag, containers.Container.BasicInfo.ID.SealTag, setDate.ToString("yyyy-MM-dd")) <= 0)
                                        {
                                            ContainerInfo.Row Row_Container = new ContainerInfo.Row()
                                            {
                                                CONTAINER_TAG = containers.Container.BasicInfo.ID.ContainerTag,
                                                CONTAINER_SEAL_TAG = containers.Container.BasicInfo.ID.SealTag,
                                                CONTAINER_SEAL_TIME = DateTime.ParseExact(containers.Container.BasicInfo.Records.SealTime, "yyyyMMddHHmmss.fff", CultureInfo.InvariantCulture),
                                                TRUCK_TAG = truck.BasicInfo.ID.TruckTag,
                                                TRUCK_SEAL_TAG = truck.BasicInfo.ID.SealTag,
                                                BAG_LOADED = containers.Container.BasicInfo.Records.BagCount,
                                                CONTAINER_LOAD_TIME = DateTime.ParseExact(containers.Container.BasicInfo.Records.LoadTime, "yyyyMMddHHmmss.fff", CultureInfo.InvariantCulture),
                                                SEAL_GROUP_CODE = containers.Container.BasicInfo.Records.GUID,
                                                SEAL_PDASN = containers.Container.BasicInfo.Records.PDASN,
                                                LOCATION = containers.Container.BasicInfo.Records.Location,
                                                UPDATE_TIME = nowTime
                                            };
                                            Transaction_T2BRSDB.SetTransactionResult(TB_ContainerInfo, TB_ContainerInfo.Insert(Row_Container));
                                        }

                                        foreach (LoadedBaggage loaded in containers.Container.LoadedBaggage)
                                        {
                                            if (TB_BagBoarding.SelectByKey(loaded.Baggage.ID.BagTag, setDate.ToString("yyyy-MM-dd")) <= 0)
                                            {
                                                BagBoarding.Row Row_Bag = new BagBoarding.Row()
                                                {
                                                    BAG_TAG = loaded.Baggage.ID.BagTag,
                                                    BSM_DATE = DateTime.ParseExact(setDate.ToString("yyyy-MM-dd"), "yyyy-MM-dd", CultureInfo.InvariantCulture), // 以今日之系統日期存入
                                                    CONTAINER_TAG = containers.Container.BasicInfo.ID.ContainerTag,
                                                    CONTAINER_SEAL_TAG = containers.Container.BasicInfo.ID.SealTag,
                                                    BAG_LOAD_PLACE = loaded.Baggage.Records.Loading.LoadPlace,
                                                    BAG_LOAD_TIME = DateTime.ParseExact(loaded.Baggage.Records.Loading.LoadTime, "yyyyMMddHHmmss.fff", CultureInfo.InvariantCulture),
                                                    SCAN_STATE = false, // 預設為尚未掃描確認
                                                    SCAN_OPERATOR = null,
                                                    SCAN_TIME = null,
                                                    UPDATE_TIME = nowTime
                                                };
                                                Transaction_T2BRSDB.SetTransactionResult(TB_BagBoarding, TB_BagBoarding.Insert(Row_Bag));
                                            }
                                            else
                                            {
                                                ErrorLog.Log("ERROR", "SetNewLoadingBagList", "ERROR Code: 115");
                                                JsonRtn = "ERROR|115"; // 欲新增的行李編號已存在，未進行任何設定儲存
                                                break;
                                            }
                                        }
                                    }
                                }
                                break;
                            case "informal":
                                // 非正規(上述由源頭所產生)方式所裝載運送之行李
                                settings = new JsonSerializerSettings
                                {
                                    NullValueHandling = NullValueHandling.Ignore,
                                    ContractResolver = new CustomResolver(BodyChoice.Baggage)
                                };
                                JsonObj = JsonConvert.DeserializeObject<BoardingBagList>(bagList.ToString(), settings);

                                foreach (Body body in JsonObj.LoadingList.Body)
                                {
                                    Baggage bag = JsonConvert.DeserializeObject<Baggage>((body.SealObj as JObject).ToString(), settings);
                                    DateTime nowTime = DateTime.Now;
                                    if (TB_TruckInfo.SelectByKey(T_Tag_Abnormal, T_STag_Abnormal, setDate.ToString("yyyy-MM-dd")) <= 0)
                                    {
                                        TruckInfo.Row Row_Truck = new TruckInfo.Row()
                                        {
                                            TRUCK_TAG = T_Tag_Abnormal,
                                            TRUCK_SEAL_TAG = T_STag_Abnormal,
                                            CONTAINER_LOADED = 1, // 表示強制裝載用，預設裝載數量為1
                                            DELIVERY_TIME = DateTime.ParseExact(setDate.ToString("yyyy-MM-dd") + nowTime.ToString("yyyy-MM-dd HH:mm:ss.fff").Substring(10), "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                                            DELIVERY_GROUP_CODE = "Car_" + T_STag_Abnormal + "_" + nowTime.ToString("yyyyMMddHHmmss") + "_1",
                                            DELIVERY_PDASN = PDASN_Abnormal,
                                            LOCATION = "X9", // 直接預設為X9表示非正規的行李作業
                                            UPDATE_TIME = nowTime
                                        };
                                        Transaction_T2BRSDB.SetTransactionResult(TB_TruckInfo, TB_TruckInfo.Insert(Row_Truck));
                                    }

                                    if (TB_ContainerInfo.SelectByKey(C_Tag_Abnormal, C_STag_Abnormal, setDate.ToString("yyyy-MM-dd")) > 0)
                                    {
                                        ContainerInfo.Row Row_Container = TB_ContainerInfo.RecordList[0] as ContainerInfo.Row;
                                        Row_Container.CONTAINER_SEAL_TIME = DateTime.ParseExact(setDate.ToString("yyyy-MM-dd") + nowTime.ToString("yyyy-MM-dd HH:mm:ss.fff").Substring(10), "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                                        Row_Container.CONTAINER_LOAD_TIME = DateTime.ParseExact(setDate.ToString("yyyy-MM-dd") + nowTime.ToString("yyyy-MM-dd HH:mm:ss.fff").Substring(10), "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                                        Row_Container.BAG_LOADED++;
                                        Row_Container.SEAL_GROUP_CODE = "Container_" + C_STag_Abnormal + "_" + nowTime.ToString("yyyyMMddHHmmss") + "_" + Row_Container.BAG_LOADED.ToString();
                                        Row_Container.UPDATE_TIME = nowTime;
                                        Transaction_T2BRSDB.SetTransactionResult(TB_ContainerInfo, TB_ContainerInfo.Update(Row_Container));
                                    }
                                    else
                                    {
                                        ContainerInfo.Row Row_Container = new ContainerInfo.Row()
                                        {
                                            CONTAINER_TAG = C_Tag_Abnormal,
                                            CONTAINER_SEAL_TAG = C_STag_Abnormal,
                                            CONTAINER_SEAL_TIME = DateTime.ParseExact(setDate.ToString("yyyy-MM-dd") + nowTime.ToString("yyyy-MM-dd HH:mm:ss.fff").Substring(10), "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                                            TRUCK_TAG = T_Tag_Abnormal,
                                            TRUCK_SEAL_TAG = T_STag_Abnormal,
                                            BAG_LOADED = 1, // 表示強制裝載用，預設裝載數量為1
                                            CONTAINER_LOAD_TIME = DateTime.ParseExact(setDate.ToString("yyyy-MM-dd") + nowTime.ToString("yyyy-MM-dd HH:mm:ss.fff").Substring(10), "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                                            SEAL_GROUP_CODE = "Container_" + C_STag_Abnormal + "_" + nowTime.ToString("yyyyMMddHHmmss") + "_1",
                                            SEAL_PDASN = PDASN_Abnormal,
                                            LOCATION = "X9", // 直接預設為X9表示非正規的行李作業
                                            UPDATE_TIME = nowTime
                                        };
                                        Transaction_T2BRSDB.SetTransactionResult(TB_ContainerInfo, TB_ContainerInfo.Insert(Row_Container));
                                    }

                                    if (TB_BagBoarding.SelectByKey(bag.ID.BagTag, setDate.ToString("yyyy-MM-dd")) <= 0)
                                    {
                                        string bagLoadTime = Service_Mode == "0" ? setDate.ToString("yyyyMMdd") + bag.Records.Loading.LoadTime.Substring(8) : bag.Records.Loading.LoadTime;
                                        BagBoarding.Row Row_Bag = new BagBoarding.Row()
                                        {
                                            BAG_TAG = bag.ID.BagTag,
                                            BSM_DATE = DateTime.ParseExact(setDate.ToString("yyyy-MM-dd"), "yyyy-MM-dd", CultureInfo.InvariantCulture), // 以今日之系統日期存入
                                            CONTAINER_TAG = C_Tag_Abnormal,
                                            CONTAINER_SEAL_TAG = C_STag_Abnormal,
                                            BAG_LOAD_PLACE = "X9", // 直接預設為X9表示非正規的行李作業
                                            BAG_LOAD_TIME = DateTime.ParseExact(bagLoadTime, "yyyyMMddHHmmss.fff", CultureInfo.InvariantCulture),
                                            SCAN_STATE = false, // 預設為尚未掃描確認
                                            SCAN_OPERATOR = null,
                                            SCAN_TIME = null,
                                            UPDATE_TIME = nowTime
                                        };
                                        Transaction_T2BRSDB.SetTransactionResult(TB_BagBoarding, TB_BagBoarding.Insert(Row_Bag));
                                    }
                                    else
                                    {
                                        ErrorLog.Log("ERROR", "SetNewLoadingBagList", "ERROR Code: 114");
                                        JsonRtn = "ERROR|114"; // 欲設定的非正規行李編號已存在，未進行任何設定儲存
                                        break;
                                    }
                                }
                                break;
                            default:
                                // 非許可的裝載行李資訊新增之處理方式，回傳資訊新增處理方式錯誤
                                ErrorLog.Log("ERROR", "SetNewLoadingBagList", "ERROR Code: 123");
                                JsonRtn = "ERROR|123"; // ProcType參數錯誤或不允許
                                break;
                        }

                        if (string.IsNullOrEmpty(JsonRtn))
                        {
                            if (Transaction_T2BRSDB.EndTransaction()[TB_BagBoarding.SqlCmd.Connection])
                            {
                                InfoLog.Log("BoardingBagController", "SetNewLoadingBagList", "行李裝載資料列表Json字串解析驗證成功，並完成新的或非正規裝載之行李資訊輸入");
                                JsonRtn = "SUCCESS";
                            }
                            else
                            {
                                ErrorLog.Log("ERROR", "SetNewLoadingBagList", "ERROR Code: 113");
                                JsonRtn = "ERROR|113"; // 新的或非正規裝載之行李資訊輸入失敗回捲，未進行任何設定儲存
                            }
                        }
                    }
                    else
                    {
                        ErrorLog.Log("BoardingBagController", "SetNewLoadingBagList", "ERROR Code: 202");
                        JsonRtn = "ERROR|202"; // 訊息格式驗證錯誤
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog.Log("SetNewLoadingBagList", ex);
                JsonRtn = "ERROR|101"; // Web API發生例外錯誤
            }
            finally
            {
                if (T2BRSDB != null)
                {
                    T2BRSDB.Close();
                }
            }

            return JsonRtn;
        }

        #region =====[Private] Function=====
        /// <summary>
        /// 初始化傳遞訊息BoardingBagList物件
        /// </summary>
        /// <param name="pSource">訊息傳送來源</param>
        /// <param name="pDestination">訊息傳送目的地</param>
        /// <returns>傳遞訊息BoardingBagList物件</returns>
        /// <remarks>另填入<c>Body</c>內容列表</remarks>
        private BoardingBagList Init_BoardingBagList(string pSource, string pDestination)
        {
            return new BoardingBagList
            {
                LoadingList = new LoadingList
                {
                    Header = new Header()
                    {
                        Source = pSource,
                        Destination = pDestination
                    },
                    Body = new List<Body>()
                }
            };
        }
        /// <summary>
        /// 初始化傳遞訊息BoardingBagList之內容物件
        /// </summary>
        /// <param name="choice">訊息內容種類之列舉項目</param>
        /// <returns>傳遞訊息BoardingBagList之內容物件</returns>
        /// <remarks>另填入<c>Containers</c>已封籤容器(籠車)物件列表 或 填入<c>LoadedBaggage</c>所裝載之行李物件列表</remarks>
        private Body Init_Body(BodyChoice choice)
        {
            Body body;
            switch (choice)
            {
                case BodyChoice.Truck:
                    body = new Body()
                    {
                        SealObj = new Truck()
                        {
                            BasicInfo = new BasicInfo_Truck()
                            {
                                ID = new ID_Truck(),
                                Records = new Records_Truck()
                            }
                        }
                    };
                    break;
                case BodyChoice.Container:
                    body = new Body()
                    {
                        SealObj = new Container()
                        {
                            BasicInfo = new BasicInfo_Container()
                            {
                                ID = new ID_Container(),
                                Records = new Records_Container()
                            }
                        }
                    };
                    break;
                case BodyChoice.Baggage:
                    body = new Body()
                    {
                        SealObj = new Baggage()
                        {
                            ID = new ID_Baggage(),
                            Records = new Records_Baggage()
                            {
                                Loading = new Loading_Records(),
                                Unloading = new Unloading_Records()
                            }
                        }
                    };
                    break;
                default:
                    body = new Body()
                    {
                        SealObj = new Container()
                        {
                            BasicInfo = new BasicInfo_Container()
                            {
                                ID = new ID_Container(),
                                Records = new Records_Container()
                            }
                        }
                    };
                    break;
            }

            return body;
        }
        /// <summary>
        /// 初始化已封籤容器(籠車)物件
        /// </summary>
        /// <returns>已封籤容器(籠車)物件</returns>
        private Containers Init_Containers()
        {
            return new Containers()
            {
                Container = new Container()
                {
                    BasicInfo = new BasicInfo_Container()
                    {
                        ID = new ID_Container(),
                        Records = new Records_Container()
                    }
                }
            };
        }
        /// <summary>
        /// 初始化所裝載之行李物件
        /// </summary>
        /// <returns>所裝載之行李物件</returns>
        private LoadedBaggage Init_LoadedBaggage()
        {
            return new LoadedBaggage()
            {
                Baggage = new Baggage()
                {
                    ID = new ID_Baggage(),
                    Records = new Records_Baggage()
                    {
                        Loading = new Loading_Records(),
                        Unloading = new Unloading_Records()
                    }
                }
            };
        }
        #endregion
    }
}
