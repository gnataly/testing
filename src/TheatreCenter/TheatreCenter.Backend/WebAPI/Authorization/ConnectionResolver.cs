//using System.Security.Claims;
//using TheatreCenter.Domain.Enums;

//namespace TheatreCenter.Backend.WebAPI.Authorization
//{
//    public interface IConnectionResolver
//    {
//        string GetConnectionString();
//    }

//    public class ConnectionResolver : IConnectionResolver
//    {
//        private readonly IHttpContextAccessor _httpContextAccessor;
//        private readonly IConfiguration _configuration;

//        public ConnectionResolver(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
//        {
//            _httpContextAccessor = httpContextAccessor;
//            _configuration = configuration;
//        }

//        public string GetConnectionString()
//        {
//            var user = _httpContextAccessor.HttpContext?.User;

//            if (user?.Identity?.IsAuthenticated ?? false)
//            {
//                var role = user.FindFirst(ClaimTypes.Role)?.Value;

//                return role switch
//                {
//                    nameof(AccessLevel.Admin) => _configuration.GetConnectionString("AdminConnection"),
//                    nameof(AccessLevel.User) => _configuration.GetConnectionString("UserConnection"),
//                    _ => _configuration.GetConnectionString("GuestConnection")
//                };
//            }

//            return _configuration.GetConnectionString("GuestConnection");
//        }
//    }
//}
