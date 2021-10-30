using System;
using System.Text;
using log4net;

namespace AECMIS.MVC.Helpers
{
    public class ErrorLogger 
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static void LogError(Exception exception)
        {
            // You could use any logging approach here

            var builder = new StringBuilder();
            builder
                .AppendLine("----------")
                .AppendLine(DateTime.Now.ToString());
            AppendException(builder, exception);

            if (exception.InnerException != null)
            {
                builder
                    .AppendLine("----------INNEREXCEPTION----------");
                AppendException(builder, exception.InnerException);
            }

            Log.Error(builder.ToString());

        }

        private static void AppendException(StringBuilder strBuilder, Exception e)
        {
            strBuilder.AppendFormat("Source:\t{0}", e.Source)
                .AppendLine()
                .AppendFormat("Target:\t{0}", e.TargetSite)
                .AppendLine()
                .AppendFormat("Type:\t{0}", e.GetType().Name)
                .AppendLine()
                .AppendFormat("Message:\t{0}", e.Message)
                .AppendLine()
                .AppendFormat("Stack:\t{0}", e.StackTrace)
                .AppendLine();
        }
    }
}