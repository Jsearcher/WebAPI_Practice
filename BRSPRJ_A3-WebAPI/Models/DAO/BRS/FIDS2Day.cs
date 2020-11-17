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
    public class FIDS2Day : DBRecord
    {
        #region =====[Public] Class=====

        /// <summary>
        /// 資料表欄位物件
        /// </summary>
        public class Row
        {
            #region =====[Public] Getter & Setter=====
            /// <summary>
            /// 航班編號
            /// </summary>
            public string FLIGHT_NO { get; set; }
            /// <summary>
            /// 航班之作業日期，格式為yyyyMMdd
            /// </summary>
            public string DATE_TIME { get; set; }
            /// <summary>
            /// 航班出入境狀態
            /// </summary>
            public string TYPE { get; set; }
            /// <summary>
            /// 航班表定出(入)境時間
            /// </summary>
            public string STD { get; set; }
            /// <summary>
            /// 航班預計出(入)境時間
            /// </summary>
            public string ETD { get; set; }
            /// <summary>
            /// 航班實際出(入)境時間
            /// </summary>
            public string ATD { get; set; }
            /// <summary>
            /// 航班目的地(來源地)
            /// </summary>
            public string DESTINATION { get; set; }
            /// <summary>
            /// 航班機型
            /// </summary>
            public string PLANE { get; set; }
            /// <summary>
            /// 航班作業狀態
            /// </summary>
            public string STATUS { get; set; }
            /// <summary>
            /// 資料列變動人員
            /// </summary>
            public string UPD_USER { get; set; }
            /// <summary>
            /// 資料列變動時間，格式為yyyyMMddHHmmss
            /// </summary>
            public string UPD_TIME { get; set; }
            /// <summary>
            /// 資料列更異動狀態
            /// </summary>
            public int? UPD_TYPE { get; set; } = null;
            #endregion
        }

        #endregion

        #region =====[Public] Constructor & Destructor=====

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="pConn">資料庫連接物件</param>
        public FIDS2Day(IDbConnection pConn) : base(pConn)
        {
            DBOwner = "brsdba";
            TableName = "FIDS_2DAY";
            FieldName = new string[] { "FLIGHT_NO", "DATE_TIME", "TYPE", "STD", "ETD", "ATD",
                                        "DESTINATION", "PLANE", "STATUS", "UPD_USER", "UPD_TIME", "UPD_TYPE" };
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
            pSqlStr += "'" + oRow.FLIGHT_NO + "'";
            pSqlStr += ", '" + oRow.DATE_TIME + "'";
            pSqlStr += ", '" + oRow.TYPE + "'";
            pSqlStr += ", '" + oRow.STD + "'";
            pSqlStr += ", '" + oRow.ETD + "'";
            pSqlStr += ", '" + oRow.ATD + "'";
            pSqlStr += ", '" + oRow.DESTINATION + "'";
            pSqlStr += ", '" + oRow.PLANE + "'";
            pSqlStr += ", '" + oRow.STATUS + "'";
            pSqlStr += ", '" + oRow.UPD_USER + "'";
            pSqlStr += ", '" + oRow.UPD_TIME + "'";
            pSqlStr += ", " + oRow.UPD_TYPE;
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
            pSqlStr += ", [" + FieldName[1] + "] [varchar] NOT NULL PRIMARY KEY";
            pSqlStr += ", [" + FieldName[2] + "] [varchar] NOT NULL";
            pSqlStr += ", [" + FieldName[3] + "] [varchar] NOT NULL";
            pSqlStr += ", [" + FieldName[4] + "] [varchar] NULL";
            pSqlStr += ", [" + FieldName[5] + "] [varchar] NULL";
            pSqlStr += ", [" + FieldName[6] + "] [varchar] NOT NULL";
            pSqlStr += ", [" + FieldName[7] + "] [varchar] NULL";
            pSqlStr += ", [" + FieldName[8] + "] [varchar] NULL";
            pSqlStr += ", [" + FieldName[9] + "] [varchar] NULL";
            pSqlStr += ", [" + FieldName[10] + "] [varchar] NULL";
            pSqlStr += ", [" + FieldName[11] + "] [int] NULL";
            pSqlStr += ")";

            return Execute(pSqlStr);
        }

        #endregion

        #region =====[Public] Method=====

        /// <summary>
        /// 依據[FLIGHT_NO], [DATE_TIME]篩選[dbo.FIDS_2DAY]資料表，[FLIGHT_NO]包含"XX"的航空公司代碼
        /// </summary>
        /// <param name="pDate">航班之作業日期(預設為系統時間之當日)</param>
        /// <returns>
        /// <para> 0: 依條件搜尋的筆數</para>
        /// <para>-1: 例外錯誤</para>
        /// </returns>
        /// <remarks>使用"<c>RecordList</c>"取出所查詢的資料列</remarks>
        public int SelectXXByDate(string pDate)
        {
            pDate = string.IsNullOrEmpty(pDate) ?
                DateTime.Now.ToString("yyyyMMdd") :
                DateTime.ParseExact(pDate, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("yyyyMMdd");
            return SelectByCondition(string.Format(" WHERE {0} = '{1}' and {2} like 'XX%'" +
                                                    " ORDER BY {2} desc",
                                                    FieldName[1], pDate, FieldName[0]));
        }

        /// <summary>
        /// 依據[FLIGHT_NO], [DATE_TIME]篩選[dbo.FIDS_2DAY]資料表
        /// </summary>
        /// <param name="pFlightNo">航班編號</param>
        /// <param name="pDate">航班之作業日期(預設為系統時間之當日)</param>
        /// <returns>
        /// <para> 0: 依條件搜尋的筆數</para>
        /// <para>-1: 例外錯誤</para>
        /// </returns>
        /// <remarks>使用"<c>RecordList</c>"取出所查詢的資料列</remarks>
        public int SelectByKey(string pFlightNo, string pDate)
        {
            pDate = string.IsNullOrEmpty(pDate) ?
                DateTime.Now.ToString("yyyyMMdd") :
                DateTime.ParseExact(pDate, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("yyyyMMdd");
            return SelectByCondition(string.Format(" WHERE {0} = '{1}' and {2} = '{3}'" +
                                                    " ORDER BY {0} desc",
                                                    FieldName[1], pDate, FieldName[0], pFlightNo));
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
            sql += " SET " + FieldName[2] + " = '" + oRow.DATE_TIME + "'";
            sql += ", " + FieldName[3] + " = '" + oRow.STD + "'";
            sql += ", " + FieldName[4] + " = '" + oRow.ETD + "'";
            sql += ", " + FieldName[5] + " = '" + oRow.ATD + "'";
            sql += ", " + FieldName[6] + " = '" + oRow.DESTINATION + "'";
            sql += ", " + FieldName[7] + " = " + oRow.PLANE;
            sql += ", " + FieldName[8] + " = '" + oRow.STATUS + "'";
            sql += ", " + FieldName[9] + " = '" + oRow.UPD_USER + "'";
            sql += ", " + FieldName[10] + " = '" + oRow.UPD_TIME + "'";
            sql += ", " + FieldName[11] + " = " + oRow.UPD_TYPE;
            sql += " WHERE " + FieldName[0] + " = '" + oRow.FLIGHT_NO + "'";
            sql += " and " + FieldName[1] + " = '" + oRow.DATE_TIME + "'";

            return Execute(sql);
        }

        #endregion
    }
}