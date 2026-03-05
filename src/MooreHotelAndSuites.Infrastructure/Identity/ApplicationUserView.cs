using MooreHotelAndSuites.Application.Interfaces.Identity;

namespace MooreHotelAndSuites.Infrastructure.Identity
{
    internal sealed class ApplicationUserView : IApplicationUser
    {
        public ApplicationUserView(ApplicationUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            Id = user.Id ?? throw new ArgumentNullException(nameof(user.Id));
            UserName = user.UserName ?? throw new ArgumentNullException(nameof(user.UserName));
            Email = user.Email ?? throw new ArgumentNullException(nameof(user.Email));
            FullName = user.FullName;
            EmailConfirmed = user.EmailConfirmed;
        }

        public string Id { get; }
        public string UserName { get; }
        public string Email { get; }
        public string? FullName { get; }
        public bool EmailConfirmed { get; }
    }
}