using Lib.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Reflection;

namespace BRSPRJ_A3_WebAPI.Models.DBTables.A3
{
    /// <summary>
    /// "TRUCK_INFO"資料表類別
    /// </summary>
    public class TruckInfo : DBRecord
    {
        #region =====[Public] Class=====

        /// <summary>
        /// 資料表欄位物件
        /// </summary>
        public class Row
        {
            #region =====[Public] Getter & Setter=====
            /// <summary>
            /// 裝載已封籤容器(籠車)之配送車的車牌號碼
            /// </summary>
            /// <remarks>為A3SealCarInfo資料表的[LicensePlateNumber]欄位</remarks>
            public string TRUCK_TAG { get; set; }
            /// <summary>
            /// 裝載已封籤容器(籠車)之配送車的封籤條碼編號
            /// </summary>
            /// <remarks>為A3SealCarInfo資料表的[CarSealbarcode]欄位</remarks>
            public string TRUCK_SEAL_TAG { get; set; }
            /// <summary>
            /// 此已封籤配送車所裝載之已封籤容器(籠車)數量
            /// </summary>
            /// <remarks>為A3SealCarInfo資料表的[intContainerInCart]欄位</remarks>
            public int CONTAINER_LOADED { get; set; }
            /// <summary>
            /// 裝載已封籤容器(籠車)之配送車出貨的時間，格式為yyyy-MM-dd HH:mm:ss.fff
            /// </summary>
            /// <remarks>為A3SealCarInfo資料表的[Delivery_TS]欄位</remarks>
            public DateTime DELIVERY_TIME { get; set; }
            /// <summary>
            /// 此已封籤配送車之全域唯一識別碼
            /// </summary>
            /// <remarks>為A3SealCarInfo資料表的[DeliveryGroupCode]欄位</remarks>
            public string DELIVERY_GROUP_CODE { get; set; }

            /// <summary>
            /// 掃描此已封籤配送車之手持機序列號碼
            /// </summary>
            /// <remarks>為A3SealCarInfo資料表的[DeliveryPDASN]欄位</remarks>
            public string DELIVERY_PDASN { get; set; }
            /// <summary>
            /// 未知
            /// </summary>
            /// <remarks>為A3SealCarInfo資料表的[Location]欄位</remarks>
            public string LOCATION { get; set; }
            /// <summary>
            /// 資料列變動時間，格式為yyyy-MM-dd HH:mm:ss.fff
            /// </summary>
            public DateTime UPDATE_TIME { get; set; }
            #endregion
        }

        #endregion

        #region =====[Public] Constructor & Destructor=====

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="pConn">資料庫連接物件</param>
        public TruckInfo(IDbConnection pConn) : base(pConn)
        {
            DBOwner = "dbo";
            TableName = "TRUCK_INFO";
            FieldName = new string[] { "TRUCK_TAG", "TRUCK_SEAL_TAG", "CONTAINER_LOADED", "DELIVERY_TIME", 
                                    "DELIVERY_GROUP_CODE", "DELIVERY_PDASN", "LOCATION", "UPDATE_TIME" };
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
            pSqlStr += "'" + oRow.TRUCK_TAG + "'";
            pSqlStr += ", '" + oRow.TRUCK_SEAL_TAG + "'";
            pSqlStr += ", " + oRow.CONTAINER_LOADED;
            pSqlStr += ", '" + oRow.DELIVERY_TIME.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
            pSqlStr += ", '" + oRow.DELIVERY_GROUP_CODE + "'";
            pSqlStr += ", '" + oRow.DELIVERY_PDASN + "'";
            pSqlStr += ", '" + oRow.LOCATION + "'";
            pSqlStr += ", '" + oRow.UPDATE_TIME.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
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
            pSqlStr += "[" + FieldName[0] + "] [varchar](30) NOT NULL PRIMARY KEY";
            pSqlStr += ", [" + FieldName[1] + "] [varchar](30) NOT NULL PRIMARY KEY";
            pSqlStr += ", [" + FieldName[2] + "] [int] NOT NULL";
            pSqlStr += ", [" + FieldName[3] + "] [datetime] NOT NULL";
            pSqlStr += ", [" + FieldName[4] + "] [varchar](256) NOT NULL";
            pSqlStr += ", [" + FieldName[5] + "] [varchar](30) NULL";
            pSqlStr += ", [" + FieldName[6] + "] [varchar](30) NOT NULL";
            pSqlStr += ", [" + FieldName[7] + "] [datetime] NOT NULL";
            pSqlStr += ")";

            return Execute(pSqlStr);
        }

        #endregion

        #region =====[Public] Method=====

        /// <summary>
        /// 依據[DELIVERY_TIME]篩選[dbo.TRUCK_INFO]資料表
        /// </summary>
        /// <param name="pLDate">配送車運送日期(預設為系統時間之當日)</param>
        /// <returns>
        /// <para> 0: 依條件搜尋的筆數</para>
        /// <para>-1: 例外錯誤</para>
        /// </returns>
        /// <remarks>使用"<c>RecordList</c>"取出所查詢的資料列</remarks>
        public int SelectByLoadDate(string pLDate)
        {
            DateTime dtLDate = string.IsNullOrEmpty(pLDate) ? DateTime.Now : DateTime.ParseExact(pLDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            return SelectByCondition(string.Format(" WHERE {0} >= '{1}' and {0} < '{2}'", FieldName[3], dtLDate.ToString("yyyy-MM-dd"), dtLDate.AddDays(1).ToString("yyyy-MM-dd")));
        }

        /// <summary>
        /// 依據[TRUCK_TAG], [TRUCK_SEAL_TAG], [DELIVERY_TIME]篩選[dbo.TRUCK_INFO]資料表
        /// </summary>
        /// <param name="pTag">配送車的車牌號碼</param>
        /// <param name="pSealTag">配送車的封籤條碼編號</param>
        /// <param name="pLDate">配送車運送日期(預設為系統時間之當日)</param>
        /// <returns>
        /// <para> 0: 依條件搜尋的筆數</para>
        /// <para>-1: 例外錯誤</para>
        /// </returns>
        /// <remarks>使用"<c>RecordList</c>"取出所查詢的資料列</remarks>
        public int SelectByKey(string pTag, string pSealTag, string pLDate)
        {
            DateTime dtLDate = string.IsNullOrEmpty(pLDate) ? DateTime.Now : DateTime.ParseExact(pLDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            return SelectByCondition(string.Format(" WHERE {0} >= '{1}' and {0} < '{2}' and {3} = '{4}' and {5} = '{6}'",
                                                        FieldName[3], dtLDate.ToString("yyyy-MM-dd"), dtLDate.AddDays(1).ToString("yyyy-MM-dd"),
                                                        FieldName[0], pTag, FieldName[1], pSealTag));
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
            sql += " SET " + FieldName[2] + " = " + oRow.CONTAINER_LOADED;
            sql += ", " + FieldName[3] + " = '" + oRow.DELIVERY_TIME.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
            sql += ", " + FieldName[4] + " = '" + oRow.DELIVERY_GROUP_CODE + "'";
            sql += ", " + FieldName[5] + " = '" + oRow.DELIVERY_PDASN + "'";
            sql += ", " + FieldName[6] + " = '" + oRow.LOCATION + "'";
            sql += ", " + FieldName[7] + " = '" + oRow.UPDATE_TIME.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
            sql += " WHERE " + FieldName[0] + " = '" + oRow.TRUCK_TAG + "'";
            sql += " and " + FieldName[1] + " = '" + oRow.TRUCK_SEAL_TAG + "'";

            return Execute(sql);
        }

        #endregion
    }
}