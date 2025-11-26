using FluentValidation;

namespace OrderFlowClase.API.Identity.Dto.Auth
{
    public class ResponseLogin
    {
        public required string Token { get; set; }
        public DateTime ExpirationAtUtc { get; set; }
    }

}
