using AuthorizationServer.Entities;
using System.Collections.Concurrent;

namespace AuthorizationServer
{
    /// <summary>
    /// 资源服务器存储对象。
    /// </summary>
    public static class AudiencesStore
    {
        public static ConcurrentDictionary<string, Audience> Audiences =
            new ConcurrentDictionary<string, Audience>();

        /// <summary>
        /// 默认添加一些测试用的资源服务器对象。
        /// 生产环境中应该实现数据存储。
        /// </summary>
        static AudiencesStore()
        {
            Audiences.TryAdd("RS1001",
                new Audience
                {
                    Name = "ResourceServer 1#",
                    ClientId = "RS1001",
                    Base64Secret = "DAFDASEEREGAGAGAGDAFDAERWEAGRAGASDGADG",

                });
            Audiences.TryAdd("RS2001",
                new Audience
                {
                    Name = "ResourceServer 2#",
                    ClientId = "RS2001",
                    Base64Secret = "WEWLFLDSAJFIAWEWGEAEWTAWGERGEAGEAGEATE",
                });
        }

        public static Audience FindAudience(string clientId)
        {
            Audience audience = null;
            if (Audiences.TryGetValue(clientId, out audience)) return audience;
            return null;
        }
    }
}