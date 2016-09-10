using System.ComponentModel.DataAnnotations;

namespace AuthorizationServer.Entities
{
    /// <summary>
    /// 代表资源服务器。
    /// </summary>
    public class Audience
    {
        /// <summary>
        /// 客户ID，每一个资源服务器有一个唯一的ID.
        /// </summary>
        [Key]
        [MaxLength(32)]
        public string ClientId { get; set; }

        /// <summary>
        /// 资源服务器和授权服务器同时使用的加密JWT的密码。
        /// </summary>
        [MaxLength(80)]
        [Required]
        public string Base64Secret { get; set; }

        /// <summary>
        /// 资源服务器名称。
        /// </summary>
        [MaxLength(100)]
        [Required]
        public string Name { get; set; }
    }
}
