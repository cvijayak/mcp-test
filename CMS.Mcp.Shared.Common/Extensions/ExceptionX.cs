namespace CMS.Mcp.Shared.Common.Extensions
{
    using System;
    using System.Diagnostics;

    public static class ExceptionX
    {
        public static string ToDemystifiedString(this Exception exception) => exception.ToStringDemystified();
    }
}