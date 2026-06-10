using System.Net;

namespace AndreasReitberger.API.REST.Extensions
{
    [Obsolete("Use from the core library once available")]
    public static class CookieCollectionExtension
    {
        extension(CookieCollection collection)
        {
            public CookieContainer ToContainer()
            {
                CookieContainer container = new();
                container.Add(collection);
                return container;
            }
        }
    }
}
