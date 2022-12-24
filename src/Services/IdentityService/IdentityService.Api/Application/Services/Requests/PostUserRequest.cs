namespace IdentityService.Api.Application.Services.Requests
{
    public class PostUserRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordConfirmation { get; set; }
    }
}
