using AutoMapper;
using BlazorSozluk.Api.Application.Interfaces.Repositories;
using BlazorSozluk.Common;
using BlazorSozluk.Common.Events.User;
using BlazorSozluk.Common.Infastructure;
using BlazorSozluk.Common.Infastructure.Exceptions;
using BlazorSozluk.Common.ViewModels.Models;
using MediatR;

namespace BlazorSozluk.Api.Application.Features.Commands.User.Create
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
    {
        private readonly IMapper mapper;
        private readonly IUserRepository userRepository;

        public CreateUserCommandHandler(IMapper mapper, IUserRepository userRepository)
        {
            this.mapper = mapper;
            this.userRepository = userRepository;
        }

        public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {

            var existsUser = await userRepository.GetSingleAsync(i => i.EmailAddress == request.EmailAddress);

            if (existsUser is not null)
                throw new DatabaseValidationException("User already exists!");

            var dbUser = mapper.Map<Domain.Models.User>(request);

            var rows = await userRepository.AddAsync(dbUser);

            // RMQ
            if (rows > 0)
            {
                var @event = new UserEmailChangeEvent()
                {
                    OldEmailAddress = null,
                    NewEmailAddress = request.EmailAddress
                };

                QueueFactory.SendMessageToExchange(exchangeName: SozlukConstants.UserExchangeName,
                                                   exchangeType: SozlukConstants.DefaultExcangeType,
                                                   queuName: SozlukConstants.UserEmailChangedQueueName,
                                                   obj: @event);
            }

            return dbUser.Id;
        }
    }
}
