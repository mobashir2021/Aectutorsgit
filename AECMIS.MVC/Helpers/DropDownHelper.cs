using System;
using System.Linq;
using System.Web.Mvc;
using AECMIS.DAL.Domain.Extensions;

namespace AECMIS.MVC.Helpers
{
    public static class DropDownHelper
    {
        public static SelectList ToSelectList<TEnum>(this TEnum enumObj)
        {
            var values = from TEnum e in Enum.GetValues(typeof(TEnum))
                         select new { Value = (int)Enum.Parse(typeof(TEnum),e.ToString()), Text = e.GetDescription() };
            
            return new SelectList(values, "Value", "Text", enumObj);
        }
    }
}