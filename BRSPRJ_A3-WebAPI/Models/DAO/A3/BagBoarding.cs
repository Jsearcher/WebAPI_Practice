using Lib.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Reflection;

namespace BRSPRJ_A3_WebAPI.Models.DBTables.A3
{
    /// <summary>
    /// "BAG_BOARDING"資料表類別
    /// </summary>
    public class BagBoarding : DBRecord
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
            /// <remarks>為A3BagPreBoarding資料表的[BagBarCode]欄位</remarks>
            public string BAG_TAG { get; set; }
            /// <summary>
            /// 行李所屬航班之作業日期
            /// </summary>
            /// <remarks>將以該筆資料接收之系統日期(Local)作記錄</remarks>
            public DateTime BSM_DATE { get; set; }
            /// <summary>
            /// 裝載此件行李之容器(籠車)的條碼編號
            /// </summary>
            /// <remarks>為A3SealContainerInfo資料表的[Containerbarcode]欄位</remarks>
            public string CONTAINER_TAG { get; set; }
            /// <summary>
            /// 裝載此件行李之容器(籠車)的封籤條碼編號
            /// </summary>
            /// <remarks>為A3SealContainerInfo資料表的[ContainerSealbarcode]欄位</remarks>
            public string CONTAINER_SEAL_TAG { get; set; }
            /// <summary>
            /// 將此件行李裝載至容器(籠車)的地點、位置，例如：A3
            /// </summary>
            public string BAG_LOAD_PLACE { get; set; }
            /// <summary>
            /// 將此件行李裝載至容器(籠車)的時間，格式為yyyy-MM-dd HH:mm:ss.fff
            /// </summary>
            /// <remarks>為A3BagPreBoarding資料表的[LOADBag_TS]欄位</remarks>
            public DateTime BAG_LOAD_TIME { get; set; }
            /// <summary>
            /// 此件行李是否已從裝載之容器(籠車)取出並使用手持機掃瞄確認
            /// </summary>
            /// <remarks>
            /// <para>'true'表示已掃描確認</para>
            /// <para>'false'表示尚未掃描確認或該件行李遺失、不在裝載之容器(籠車)中</para>
            /// </remarks>
            public bool? SCAN_STATE { get; set; } = null;
            /// <summary>
            /// 將此件行李從裝載之容器(籠車)取出並使用手持機掃瞄確認，或是於裝載之容器(籠車)中未發現此件行李的作業人員
            /// </summary>
            public string SCAN_OPERATOR { get; set; }
            /// <summary>
            /// 將此件行李從裝載之容器(籠車)取出並使用手持機掃瞄確認，或是於裝載之容器(籠車)中未發現此件行李的時間，格式為yyyyMMddHHmmss.fff
            /// </summary>
            public DateTime? SCAN_TIME { get; set; } = null;
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
        public BagBoarding(IDbConnection pConn) : base(pConn)
        {
            DBOwner = "dbo";
            TableName = "BAG_BOARDING";
            FieldName = new string[] { "BAG_TAG", "BSM_DATE", "CONTAINER_TAG", "CONTAINER_SEAL_TAG", 
                                    "BAG_LOAD_PLACE", "BAG_LOAD_TIME", "SCAN_STATE", "SCAN_OPERATOR", "SCAN_TIME", "UPDATE_TIME" };
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
            pSqlStr += ", '" + oRow.BSM_DATE.ToString("yyyy-MM-dd") + "'";
            pSqlStr += ", '" + oRow.CONTAINER_TAG + "'";
            pSqlStr += ", '" + oRow.CONTAINER_SEAL_TAG + "'";
            pSqlStr += ", '" + oRow.BAG_LOAD_PLACE + "'";
            pSqlStr += ", '" + oRow.BAG_LOAD_TIME.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
            pSqlStr += ", '" + oRow.SCAN_STATE + "'";
            pSqlStr += ", '" + oRow.SCAN_OPERATOR + "'";
            if (oRow.SCAN_TIME == null)
            {
                pSqlStr += ", NULL";
            }
            else
            {
                pSqlStr += ", '" + oRow.SCAN_TIME?.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
            }
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
            pSqlStr += "[" + FieldName[0] + "] [varchar](10) NOT NULL PRIMARY KEY";
            pSqlStr += ", [" + FieldName[1] + "] [datetime] NOT NULL PRIMARY KEY";
            pSqlStr += ", [" + FieldName[2] + "] [varchar](30) NOT NULL";
            pSqlStr += ", [" + FieldName[3] + "] [varchar](30) NOT NULL";
            pSqlStr += ", [" + FieldName[4] + "] [varchar](10) NOT NULL";
            pSqlStr += ", [" + FieldName[5] + "] [datetime] NOT NULL";
            pSqlStr += ", [" + FieldName[6] + "] [bit] NOT NULL";
            pSqlStr += ", [" + FieldName[7] + "] [varchar](30) NULL";
            pSqlStr += ", [" + FieldName[8] + "] [datetime] NULL";
            pSqlStr += ", [" + FieldName[9] + "] [datetime] NOT NULL";
            pSqlStr += ")";

            return Execute(pSqlStr);
        }

        #endregion

        #region =====[Public] Method=====

        /// <summary>
        /// 依據[BAG_LOAD_TIME]篩選[dbo.BAG_BOARDING]資料表
        /// </summary>
        /// <param name="pLDate">行李裝載日期(預設為系統時間之當日)</param>
        /// <returns>
        /// <para> 0: 依條件搜尋的筆數</para>
        /// <para>-1: 例外錯誤</para>
        /// </returns>
        /// <remarks>使用"<c>RecordList</c>"取出所查詢的資料列</remarks>
        public int SelectByLoadDate(string pLDate)
        {
            DateTime dtLDate = string.IsNullOrEmpty(pLDate) ? DateTime.Now : DateTime.ParseExact(pLDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            return SelectByCondition(string.Format(" WHERE {0} >= '{1}' and {0} < '{2}'", FieldName[5], dtLDate.ToString("yyyy-MM-dd"), dtLDate.AddDays(1).ToString("yyyy-MM-dd")));
        }

        /// <summary>
        /// 依據[BAG_TAG], [BAG_LOAD_TIME]篩選[dbo.BAG_BOARDING]資料表
        /// </summary>
        /// <param name="pBagTag">行李條碼編號</param>
        /// <param name="pLDate">行李裝載日期(預設為系統時間之當日)</param>
        /// <returns>
        /// <para> 0: 依條件搜尋的筆數</para>
        /// <para>-1: 例外錯誤</para>
        /// </returns>
        /// <remarks>使用"<c>RecordList</c>"取出所查詢的資料列</remarks>
        public int SelectByKey(string pBagTag, string pLDate)
        {
            DateTime dtLDate = string.IsNullOrEmpty(pLDate) ? DateTime.Now : DateTime.ParseExact(pLDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            return SelectByCondition(string.Format(" WHERE {0} >= '{1}' and {0} < '{2}' and {3} = '{4}'",
                                                    FieldName[5], dtLDate.ToString("yyyy-MM-dd"), dtLDate.AddDays(1).ToString("yyyy-MM-dd"), FieldName[0], pBagTag));
        }

        /// <summary>
        /// 依據[CONTAINER_TAG], [CONTAINER_SEAL_TAG], [BAG_LOAD_TIME]篩選[dbo.BAG_BOARDING]資料表
        /// </summary>
        /// <param name="pContainerTag">裝載行李之容器(籠車)的條碼編號</param>
        /// <param name="pSealTag">裝載行李之容器(籠車)的封籤條碼編號</param>
        /// <param name="pLDate">行李裝載日期(預設為系統時間之當日)</param>
        /// <returns>
        /// <para> 0: 依條件搜尋的筆數</para>
        /// <para>-1: 例外錯誤</para>
        /// </returns>
        /// <remarks>使用"<c>RecordList</c>"取出所查詢的資料列</remarks>
        public int SelectByContainer(string pContainerTag, string pSealTag, string pLDate)
        {
            DateTime dtLDate = string.IsNullOrEmpty(pLDate) ? DateTime.Now : DateTime.ParseExact(pLDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            return SelectByCondition(string.Format(" WHERE {0} >= '{1}' and {0} < '{2}' and {3} = '{4}' and {5} = '{6}'",
                                                    FieldName[5], dtLDate.ToString("yyyy-MM-dd"), dtLDate.AddDays(1).ToString("yyyy-MM-dd"),
                                                    FieldName[2], pContainerTag, FieldName[3], pSealTag));
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
            sql += " SET " + FieldName[2] + " = '" + oRow.CONTAINER_TAG + "'";
            sql += ", " + FieldName[3] + " = '" + oRow.CONTAINER_SEAL_TAG + "'";
            sql += ", " + FieldName[4] + " = '" + oRow.BAG_LOAD_PLACE + "'";
            sql += ", " + FieldName[5] + " = '" + oRow.BAG_LOAD_TIME.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
            sql += ", " + FieldName[6] + " = '" + oRow.SCAN_STATE + "'";
            sql += ", " + FieldName[7] + " = '" + oRow.SCAN_OPERATOR + "'";
            if (oRow.SCAN_TIME == null)
            {
                sql += ", " + FieldName[8] + " = NULL";
            }
            else
            {
                sql += ", " + FieldName[8] + " = '" + oRow.SCAN_TIME?.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
            }

            sql += ", " + FieldName[9] + " = '" + oRow.UPDATE_TIME.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
            sql += " WHERE " + FieldName[0] + " = '" + oRow.BAG_TAG + "'";
            sql += " and " + FieldName[1] + " = '" + oRow.BSM_DATE.ToString("yyyy-MM-dd") + "'";

            return Execute(sql);
        }

        #endregion
    }
}