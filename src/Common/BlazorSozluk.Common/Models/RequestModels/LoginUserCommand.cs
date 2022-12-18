using BlazorSozluk.Common.ViewModels.Queries;
using MediatR;

namespace BlazorSozluk.Common.ViewModels.Models
{
    public class LoginUserCommand : IRequest<LoginUserViewModel>
    {
        public string EmailAddress { get;  set; }
        public string Password { get;  set; }

        public LoginUserCommand(string emailAddress, string password)
        {
            EmailAddress = emailAddress ?? throw new ArgumentNullException(nameof(emailAddress));
            Password = password ?? throw new ArgumentNullException(nameof(password));
        }
        public LoginUserCommand()
        {

        }
    }
}
