using System.Net;

namespace AndreasReitberger.API.REST.Extensions
{
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
