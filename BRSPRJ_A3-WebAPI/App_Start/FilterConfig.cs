﻿using System.Web;
using System.Web.Mvc;

namespace BRSPRJ_A3_WebAPI
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
