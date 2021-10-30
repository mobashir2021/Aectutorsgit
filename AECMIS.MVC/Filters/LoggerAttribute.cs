using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http.Routing;
using System.Web.Mvc;
using System.Web.Routing;
using Newtonsoft.Json;
using log4net;
using Microsoft.ApplicationInsights;
using System.Threading.Tasks;
using Newtonsoft.Json.Schema;
using System.IO;

namespace AECMIS.MVC.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class LoggerFilterAttribute : ActionFilterAttribute
    {
        

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if(context.HttpContext.Request.HttpMethod.ToLower() == "post" )
            {
                var telemetry = context.HttpContext.Items["Microsoft.ApplicationInsights.RequestTelemetry"] as Microsoft.ApplicationInsights.DataContracts.RequestTelemetry;

                if (telemetry != null)
                {
                    if (context.HttpContext.Request["model"] != null)
                    {
                        telemetry.Properties.Add("body", context.HttpContext.Request["model"]);
                    }
                    else
                    {
                        if (context.HttpContext.Request.InputStream.Position > 0)
                            context.HttpContext.Request.InputStream.Position = 0;

                        using (var reader = new StreamReader(context.HttpContext.Request.InputStream))
                        {
                            string requestBody = reader.ReadToEnd();
                            telemetry.Properties.Add("body", requestBody);
                        }
                    }
                }


            }
            //var ai = new TelemetryClient();
            //ai.TrackEvent(context.HttpContext.Request.HttpMethod, "This is a test event", "" );

            //base.OnActionExecuting(filterContext);
        }

    

    //public override void OnActionExecuting(ActionExecutingContext filterContext)
    //{
    //    var jsonData = filterContext.HttpContext.Request["model"];
    //    var headers = SerializeHeaders(filterContext.HttpContext.Request.Headers);
    //    var routeData = SerializeRouteData(filterContext.RouteData);
    //    var ipAddress = filterContext.HttpContext.Request.UserHostAddress;
    //    var requestMethod = filterContext.HttpContext.Request.HttpMethod;
    //    var user = filterContext.RequestContext.HttpContext.User.Identity.Name;
    //    var requestUri = filterContext.HttpContext.Request.RawUrl;
    //    var str = string.Format("{0} - {1} - {2} - {3} - User: {4} - {5}", routeData, ipAddress, headers,requestMethod, user, jsonData);

    //    Log.Debug(str);

    //    //var context = ((HttpContextBase)request.Properties["MS_HttpContext"]);
    //    //var routeData = request.GetRouteData();

    //    //return new ApiLogEntry
    //    //{
    //    //    Application = "[insert-calling-app-here]",
    //    //    User = context.User.Identity.Name,
    //    //    Machine = Environment.MachineName,
    //    //    RequestContentType = context.Request.ContentType,
    //    //    RequestRouteTemplate = routeData.Route.RouteTemplate,
    //    //    RequestRouteData = SerializeRouteData(routeData),
    //    //    RequestIpAddress = context.Request.UserHostAddress,
    //    //    RequestMethod = request.Method.Method,
    //    //    RequestHeaders = SerializeHeaders(request.Headers),
    //    //    RequestTimestamp = DateTime.Now,
    //    //    RequestUri = request.RequestUri.ToString()
    //    //};

    //}


    private string SerializeRouteData(RouteData routeData)
        {
            return JsonConvert.SerializeObject(routeData, Formatting.Indented);
        }

        private string SerializeHeaders(NameValueCollection headers)
        {
            var dict = new Dictionary<string, string>();

            foreach (string item in headers.Keys)
            {
                if (headers[item] != null)
                {
                    var header = String.Empty;
                    header = headers[item];
                    //foreach (var value in item.Value)
                    //{
                    //    header += value + " ";
                    //}

                    // Trim the trailing space and add item to the dictionary
                    header = header.TrimEnd(" ".ToCharArray());
                    dict.Add(item, header);
                }
            }

            return JsonConvert.SerializeObject(dict, Formatting.Indented);
        }
       

        private string GetLogLevel()
        {
            return ConfigurationManager.AppSettings["LOGLEVEL"].ToLower().ToString();
        }

    }
}