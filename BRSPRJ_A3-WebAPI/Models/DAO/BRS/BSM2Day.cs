using Lib.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Reflection;

namespace BRSPRJ_A3_WebAPI.Models.DAO.BRS
{
    /// <summary>
    /// "BAG_BOARDING"資料表類別
    /// </summary>
    public class BSM2Day : DBRecord
    {
        #region =====[Public] Class=====

        /// <summary>
        /// 資料表欄位物件
        /// </summary>
        public class Row
        {
            #region =====[Public] Getter & Setter=====
            /// <summary>
            /// 行李條碼編號
            /// </summary>
            public string BAG_TAG { get; set; }
            /// <summary>
            /// 行李所屬航班編號
            /// </summary>
            public string BSM_FLIGHT { get; set; }
            /// <summary>
            /// 行李所屬航班之作業日期，格式為yyyyMMdd
            /// </summary>
            public string BSM_DATE { get; set; }
            /// <summary>
            /// 行李所屬航班目的地
            /// </summary>
            public string DESTINATION { get; set; }
            /// <summary>
            /// 行李所屬航班艙等
            /// </summary>
            public string CABIN_CLASS { get; set; }
            /// <summary>
            /// 行李所屬旅客姓名
            /// </summary>
            public string PASSENGER { get; set; }
            /// <summary>
            /// 行李所屬旅客座位
            /// </summary>
            public string SEAT { get; set; }
            /// <summary>
            /// 行李裝載許可
            /// </summary>
            public string AUTH_LOAD { get; set; }
            /// <summary>
            /// 行李運送許可
            /// </summary>
            public string AUTH_TRANSPORT { get; set; }
            /// <summary>
            /// 行李處理狀態
            /// </summary>
            public int? BSM_STATE { get; set; } = null;
            /// <summary>
            /// 行李裝載航班
            /// </summary>
            public string BAG_FLIGHT { get; set; }
            /// <summary>
            /// 行李裝載日期，格式為yyyyMMdd
            /// </summary>
            public string BAG_DATE { get; set; }
            /// <summary>
            /// 行李裝載籠車編號
            /// </summary>
            public string CART_ID { get; set; }
            /// <summary>
            /// 行李裝載狀態
            /// </summary>
            public int? BAG_STATE { get; set; } = null;
            /// <summary>
            /// 資料列變動人員
            /// </summary>
            public string UPD_USER { get; set; }
            /// <summary>
            /// 資料列變動時間，格式為yyyyMMddHHmmss
            /// </summary>
            public string UPD_TIME { get; set; }
            /// <summary>
            /// 行李BSM訊息字串
            /// </summary>
            public string BSM_SOURCE { get; set; }
            /// <summary>
            /// 非目標欄位
            /// </summary>
            public int? OP_Attribute { get; set; } = null;
            /// <summary>
            /// 非目標欄位
            /// </summary>
            public string NEXT_FLIGHT { get; set; }
            /// <summary>
            /// 非目標欄位
            /// </summary>
            public string FINAL_DEST { get; set; }
            /// <summary>
            /// 非目標欄位
            /// </summary>
            public string PREV_FLIGHT { get; set; }
            /// <summary>
            /// 非目標欄位
            /// </summary>
            public int? SECURITY_NO { get; set; } = null;
            /// <summary>
            /// 非目標欄位
            /// </summary>
            public string TRAVELLER_ID { get; set; }
            /// <summary>
            /// 非目標欄位
            /// </summary>
            public string TIER_ID { get; set; }
            #endregion
        }

        #endregion

        #region =====[Public] Constructor & Destructor=====

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="pConn">資料庫連接物件</param>
        public BSM2Day(IDbConnection pConn) : base(pConn)
        {
            DBOwner = "brsdba";
            TableName = "BSM_2DAY";
            FieldName = new string[] { "BAG_TAG", "BSM_FLIGHT", "BSM_DATE", "DESTINATION", "CABIN_CLASS", "PASSENGER", "SEAT",
                                    "AUTH_LOAD", "AUTH_TRANSPORT", "BSM_STATE", "BAG_FLIGHT", "BAG_DATE", "CART_ID", "BAG_STATE",
                                    "UPD_USER", "UPD_TIME", "BSM_SOURCE",
                                    "OP_Attribute", "NEXT_FLIGHT", "FINAL_DEST", "PREV_FLIGHT", "SECURITY_NO", "TRAVELLER_ID", "TIER_ID" };
        }

        #endregion

        #region =====[Protected] Base Method for Each Table=====

        /// <summary>
        /// 擷取一列資料表記錄
        /// </summary>
        /// <param name="pRs"><c>IDataReader</c>資料擷取物件</param>
        /// <returns>一列資料表記錄</returns>
        protected override object FetchRecord(IDataReader pRs)
        {
            Row oRow = new Row();

            try
            {
                List<PropertyInfo> props = new List<PropertyInfo>(oRow.GetType().GetProperties());
                for (int i = 0; i < pRs.FieldCount; i++)
                {
                    string readerName = pRs.GetName(i);
                    foreach (PropertyInfo prop in props)
                    {
                        if (readerName == prop.Name)
                        {
                            if (prop.PropertyType == typeof(string))
                            {
                                prop.SetValue(oRow, GetValueOrDefault<string>(pRs, i));
                            }
                            else
                            {
                                prop.SetValue(oRow, GetValueOrDefault<object>(pRs, i));
                            }
                            break;
                        }
                    }
                }

                return oRow;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 新增一列資料表記錄之對應值
        /// </summary>
        /// <param name="pSqlStr">"INSERT INTO" SQL指令</param>
        /// <param name="pObj">單筆資料列物件</param>
        /// <returns>指令執行狀態</returns>
        protected override int SetRecord(string pSqlStr, object pObj)
        {
            Row oRow = pObj as Row;

            pSqlStr += " VALUES(";
            pSqlStr += "'" + oRow.BAG_TAG + "'";
            pSqlStr += ", '" + oRow.BSM_FLIGHT + "'";
            pSqlStr += ", '" + oRow.BSM_DATE + "'";
            pSqlStr += ", '" + oRow.DESTINATION + "'";
            pSqlStr += ", '" + oRow.CABIN_CLASS + "'";
            pSqlStr += ", '" + oRow.PASSENGER + "'";
            pSqlStr += ", '" + oRow.SEAT + "'";
            pSqlStr += ", '" + oRow.AUTH_LOAD + "'";
            pSqlStr += ", '" + oRow.AUTH_TRANSPORT + "'";
            pSqlStr += ", " + oRow.BSM_STATE;
            pSqlStr += ", '" + oRow.BAG_FLIGHT + "'";
            pSqlStr += ", '" + oRow.BAG_DATE + "'";
            pSqlStr += ", '" + oRow.CART_ID + "'";
            pSqlStr += ", " + oRow.BAG_STATE;
            pSqlStr += ", '" + oRow.UPD_USER + "'";
            pSqlStr += ", '" + oRow.UPD_TIME + "'";
            pSqlStr += ", '" + oRow.BSM_SOURCE + "'";
            pSqlStr += ", " + oRow.OP_Attribute;
            pSqlStr += ", '" + oRow.NEXT_FLIGHT + "'";
            pSqlStr += ", '" + oRow.FINAL_DEST + "'";
            pSqlStr += ", '" + oRow.PREV_FLIGHT + "'";
            pSqlStr += ", " + oRow.SECURITY_NO;
            pSqlStr += ", '" + oRow.TRAVELLER_ID + "'";
            pSqlStr += ", '" + oRow.TIER_ID + "'";
            pSqlStr += ")";

            return Execute(pSqlStr);
        }

        /// <summary>
        /// 設定資料表建立所需之欄位與資料型態
        /// </summary>
        /// <param name="pSqlStr">"CREATE TABLE" SQL指令</param>
        /// <returns>指令執行狀態</returns>
        protected override int SetField(string pSqlStr)
        {
            pSqlStr += " (";
            pSqlStr += "[" + FieldName[0] + "] [varchar] NOT NULL PRIMARY KEY";
            pSqlStr += ", [" + FieldName[1] + "] [varchar] NULL";
            pSqlStr += ", [" + FieldName[2] + "] [varchar] NOT NULL PRIMARY KEY";
            pSqlStr += ", [" + FieldName[3] + "] [varchar] NULL";
            pSqlStr += ", [" + FieldName[4] + "] [varchar] NULL";
            pSqlStr += ", [" + FieldName[5] + "] [varchar] NULL";
            pSqlStr += ", [" + FieldName[6] + "] [varchar] NULL";
            pSqlStr += ", [" + FieldName[7] + "] [varchar] NULL";
            pSqlStr += ", [" + FieldName[8] + "] [varchar] NULL";
            pSqlStr += ", [" + FieldName[9] + "] [int] NULL";
            pSqlStr += ", [" + FieldName[10] + "] [varchar] NULL";
            pSqlStr += ", [" + FieldName[11] + "] [varchar] NULL";
            pSqlStr += ", [" + FieldName[12] + "] [varchar] NULL";
            pSqlStr += ", [" + FieldName[13] + "] [int] NULL";
            pSqlStr += ", [" + FieldName[14] + "] [varchar] NULL";
            pSqlStr += ", [" + FieldName[15] + "] [varchar] NOT NULL";
            pSqlStr += ", [" + FieldName[16] + "] [varchar] NULL";
            pSqlStr += ", [" + FieldName[17] + "] [int] NULL";
            pSqlStr += ", [" + FieldName[18] + "] [varchar] NULL";
            pSqlStr += ", [" + FieldName[19] + "] [varchar] NULL";
            pSqlStr += ", [" + FieldName[20] + "] [varchar] NULL";
            pSqlStr += ", [" + FieldName[21] + "] [int] NULL";
            pSqlStr += ", [" + FieldName[22] + "] [varchar] NULL";
            pSqlStr += ", [" + FieldName[23] + "] [varchar] NULL";
            pSqlStr += ")";

            return Execute(pSqlStr);
        }

        #endregion

        #region =====[Public] Method=====

        /// <summary>
        /// 依據[BAG_TAG], [BSM_DATE]篩選[dbo.BSM_2DAY]資料表
        /// </summary>
        /// <param name="pBagTag">行李條碼編號</param>
        /// <param name="pDate">行李所屬航班作業日期(預設為系統時間之當日)</param>
        /// <returns>
        /// <para> 0: 依條件搜尋的筆數</para>
        /// <para>-1: 例外錯誤</para>
        /// </returns>
        /// <remarks>使用"<c>RecordList</c>"取出所查詢的資料列</remarks>
        public int SelectByKey(string pBagTag, string pDate)
        {
            pDate = string.IsNullOrEmpty(pDate) ?
                DateTime.Now.ToString("yyyyMMdd") :
                DateTime.ParseExact(pDate, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("yyyyMMdd");
            return SelectByCondition(string.Format(" WHERE {0} = '{1}' and {2} = '{3}'" +
                                                    " ORDER BY {0} desc",
                                                    FieldName[2], pDate, FieldName[0], pBagTag));
        }

        /// <summary>
        /// 更新一筆資料表記錄
        /// </summary>
        /// <param name="pObj">單筆資料列物件</param>
        /// <returns>"UPDATE SET" SQL指令執行狀態</returns>
        public int Update(object pObj)
        {
            Row oRow = pObj as Row;

            string sql = "UPDATE " + FullTableName;
            sql += " SET " + FieldName[1] + " = '" + oRow.BSM_FLIGHT + "'";
            sql += ", " + FieldName[3] + " = '" + oRow.DESTINATION + "'";
            sql += ", " + FieldName[4] + " = '" + oRow.CABIN_CLASS + "'";
            sql += ", " + FieldName[5] + " = '" + oRow.PASSENGER + "'";
            sql += ", " + FieldName[6] + " = '" + oRow.SEAT + "'";
            sql += ", " + FieldName[7] + " = " + oRow.AUTH_LOAD;
            sql += ", " + FieldName[8] + " = '" + oRow.AUTH_TRANSPORT + "'";
            sql += ", " + FieldName[9] + " = " + oRow.BSM_STATE;
            sql += ", " + FieldName[10] + " = '" + oRow.BAG_FLIGHT + "'";
            sql += ", " + FieldName[11] + " = '" + oRow.BAG_DATE + "'";
            sql += ", " + FieldName[12] + " = '" + oRow.CART_ID + "'";
            sql += ", " + FieldName[13] + " = " + oRow.BAG_STATE;
            sql += ", " + FieldName[14] + " = '" + oRow.UPD_USER + "'";
            sql += ", " + FieldName[15] + " = '" + oRow.UPD_TIME + "'";
            sql += ", " + FieldName[16] + " = '" + oRow.BSM_SOURCE + "'";
            sql += ", " + FieldName[17] + " = " + oRow.OP_Attribute;
            sql += ", " + FieldName[18] + " = '" + oRow.NEXT_FLIGHT + "'";
            sql += ", " + FieldName[19] + " = '" + oRow.FINAL_DEST + "'";
            sql += ", " + FieldName[20] + " = '" + oRow.PREV_FLIGHT + "'";
            sql += ", " + FieldName[21] + " = " + oRow.SECURITY_NO;
            sql += ", " + FieldName[22] + " = '" + oRow.TRAVELLER_ID + "'";
            sql += ", " + FieldName[23] + " = '" + oRow.TIER_ID + "'";
            sql += " WHERE " + FieldName[0] + " = '" + oRow.BAG_TAG + "'";
            sql += " and " + FieldName[2] + " = '" + oRow.BSM_DATE + "'";

            return Execute(sql);
        }

        #endregion
    }
}