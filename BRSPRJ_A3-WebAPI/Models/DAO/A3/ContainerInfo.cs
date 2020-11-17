using Lib.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Reflection;

namespace BRSPRJ_A3_WebAPI.Models.DBTables.A3
{
    /// <summary>
    /// "CONTAINER_INFO"資料表類別
    /// </summary>
    public class ContainerInfo : DBRecord
    {
        #region =====[Public] Class=====

        /// <summary>
        /// 資料表欄位物件
        /// </summary>
        public class Row
        {
            #region =====[Public] Getter & Setter=====
            /// <summary>
            /// 裝載行李之容器(籠車)的條碼編號
            /// </summary>
            /// <remarks>為A3SealContainerInfo資料表的[Containerbarcode]欄位</remarks>
            public string CONTAINER_TAG { get; set; }
            /// <summary>
            /// 裝載行李之容器(籠車)的封籤條碼編號
            /// </summary>
            /// <remarks>為A3SealContainerInfo資料表的[ContainerSealbarcode]欄位</remarks>
            public string CONTAINER_SEAL_TAG { get; set; }
            /// <summary>
            /// 將裝載行李之容器(籠車)貼上封籤條碼之時間，格式為yyyy-MM-dd HH:mm:ss.fff
            /// </summary>
            /// <remarks>為A3SealContainerInfo資料表的[SealContainer_TS]欄位</remarks>
            public DateTime CONTAINER_SEAL_TIME { get; set; }
            /// <summary>
            /// 裝載此已封籤容器(籠車)之配送車的車牌號碼
            /// </summary>
            /// <remarks>為A3SealCarInfo資料表的[LicensePlateNumber]欄位</remarks>
            public string TRUCK_TAG { get; set; }
            /// <summary>
            /// 裝載此已封籤容器(籠車)之配送車的封籤條碼編號
            /// </summary>
            /// <remarks>為A3SealCarInfo資料表的[CarSealbarcode]欄位</remarks>
            public string TRUCK_SEAL_TAG { get; set; }
            /// <summary>
            /// 此已封籤容器(籠車)所裝載之行李數量
            /// </summary>
            /// <remarks>為A3SealContainerInfo資料表的[intBagInContainer]欄位</remarks>
            public int BAG_LOADED { get; set; }
            /// <summary>
            /// 將此已封籤容器(籠車)裝載至配送車的時間，格式為yyyy-MM-dd HH:mm:ss.fff
            /// </summary>
            /// <remarks>為A3SealContainerInfo資料表的[LOADContainer_TS]欄位</remarks>
            public DateTime CONTAINER_LOAD_TIME { get; set; }
            /// <summary>
            /// 此已封籤容器(籠車)之全域唯一識別碼
            /// </summary>
            /// <remarks>為A3SealContainerInfo資料表的[SealGroupcode]欄位</remarks>
            public string SEAL_GROUP_CODE { get; set; }
            /// <summary>
            /// 掃描此已封籤容器(籠車)之手持機序列號碼
            /// </summary>
            /// <remarks>為A3SealContainerInfo資料表的[SealPDASN]欄位</remarks>
            public string SEAL_PDASN { get; set; }
            /// <summary>
            /// 未知
            /// </summary>
            /// <remarks>為A3SealContainerInfo資料表的[Location]欄位</remarks>
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
        public ContainerInfo(IDbConnection pConn) : base(pConn)
        {
            DBOwner = "dbo";
            TableName = "CONTAINER_INFO";
            FieldName = new string[] { "CONTAINER_TAG", "CONTAINER_SEAL_TAG", "CONTAINER_SEAL_TIME", "TRUCK_TAG", "TRUCK_SEAL_TAG", 
                                    "BAG_LOADED", "CONTAINER_LOAD_TIME", "SEAL_GROUP_CODE", "SEAL_PDASN", "LOCATION", "UPDATE_TIME" };
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
            pSqlStr += "'" + oRow.CONTAINER_TAG + "'";
            pSqlStr += ", '" + oRow.CONTAINER_SEAL_TAG + "'";
            pSqlStr += ", '" + oRow.CONTAINER_SEAL_TIME.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
            pSqlStr += ", '" + oRow.TRUCK_TAG + "'";
            pSqlStr += ", '" + oRow.TRUCK_SEAL_TAG + "'";
            pSqlStr += ", " + oRow.BAG_LOADED;
            pSqlStr += ", '" + oRow.CONTAINER_LOAD_TIME.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
            pSqlStr += ", '" + oRow.SEAL_GROUP_CODE + "'";
            pSqlStr += ", '" + oRow.SEAL_PDASN + "'";
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
            pSqlStr += ", [" + FieldName[2] + "] [datetime] NOT NULL";
            pSqlStr += ", [" + FieldName[3] + "] [varchar](30) NOT NULL";
            pSqlStr += ", [" + FieldName[4] + "] [varchar](30) NOT NULL";
            pSqlStr += ", [" + FieldName[5] + "] [int] NOT NULL";
            pSqlStr += ", [" + FieldName[6] + "] [datetime] NOT NULL";
            pSqlStr += ", [" + FieldName[7] + "] [varchar](256) NOT NULL";
            pSqlStr += ", [" + FieldName[8] + "] [varchar](30) NULL";
            pSqlStr += ", [" + FieldName[9] + "] [varchar](30) NOT NULL";
            pSqlStr += ", [" + FieldName[10] + "] [datetime] NOT NULL";
            pSqlStr += ")";

            return Execute(pSqlStr);
        }

        #endregion

        #region =====[Public] Method=====

        /// <summary>
        /// 依據[CONTAINER_LOAD_TIME]篩選[dbo.CONTAINER_INFO]資料表
        /// </summary>
        /// <param name="pLDate">容器(籠車)裝載至配送車日期(預設為系統時間之當日)</param>
        /// <returns>
        /// <para> 0: 依條件搜尋的筆數</para>
        /// <para>-1: 例外錯誤</para>
        /// </returns>
        /// <remarks>使用"<c>RecordList</c>"取出所查詢的資料列</remarks>
        public int SelectByLoadDate(string pLDate)
        {
            DateTime dtLDate = string.IsNullOrEmpty(pLDate) ? DateTime.Now : DateTime.ParseExact(pLDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            return SelectByCondition(string.Format(" WHERE {0} >= '{1}' and {0} < '{2}'", FieldName[6], dtLDate.ToString("yyyy-MM-dd"), dtLDate.AddDays(1).ToString("yyyy-MM-dd")));
        }

        /// <summary>
        /// 依據[CONTAINER_TAG], [CONTAINER_SEAL_TAG], [CONTAINER_LOAD_TIME]篩選[dbo.CONTAINER_INFO]資料表
        /// </summary>
        /// <param name="pTag">容器(籠車)的條碼編號</param>
        /// <param name="pSealTag">容器(籠車)的封籤條碼編號</param>
        /// <param name="pLDate">容器(籠車)裝載至配送車日期(預設為系統時間之當日)</param>
        /// <returns>
        /// <para> 0: 依條件搜尋的筆數</para>
        /// <para>-1: 例外錯誤</para>
        /// </returns>
        /// <remarks>使用"<c>RecordList</c>"取出所查詢的資料列</remarks>
        public int SelectByKey(string pTag, string pSealTag, string pLDate)
        {
            DateTime dtLDate = string.IsNullOrEmpty(pLDate) ? DateTime.Now : DateTime.ParseExact(pLDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            return SelectByCondition(string.Format(" WHERE {0} >= '{1}' and {0} < '{2}' and {3} = '{4}' and {5} = '{6}'",
                                                    FieldName[6], dtLDate.ToString("yyyy-MM-dd"), dtLDate.AddDays(1).ToString("yyyy-MM-dd"),
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
            sql += " SET " + FieldName[2] + " = '" + oRow.CONTAINER_SEAL_TIME.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
            sql += ", " + FieldName[3] + " = '" + oRow.TRUCK_TAG + "'";
            sql += ", " + FieldName[4] + " = '" + oRow.TRUCK_SEAL_TAG + "'";
            sql += ", " + FieldName[5] + " = " + oRow.BAG_LOADED;
            sql += ", " + FieldName[6] + " = '" + oRow.CONTAINER_LOAD_TIME.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
            sql += ", " + FieldName[7] + " = '" + oRow.SEAL_GROUP_CODE + "'";
            sql += ", " + FieldName[8] + " = '" + oRow.SEAL_PDASN + "'";
            sql += ", " + FieldName[9] + " = '" + oRow.LOCATION + "'";
            sql += ", " + FieldName[10] + " = '" + oRow.UPDATE_TIME.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
            sql += " WHERE " + FieldName[0] + " = '" + oRow.CONTAINER_TAG + "'";
            sql += " and " + FieldName[1] + " = '" + oRow.CONTAINER_SEAL_TAG + "'";

            return Execute(sql);
        }

        #endregion
    }
}