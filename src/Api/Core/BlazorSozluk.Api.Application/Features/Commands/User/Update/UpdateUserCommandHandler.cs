using AutoMapper;
using BlazorSozluk.Api.Application.Interfaces.Repositories;
using BlazorSozluk.Common.Events.User;
using BlazorSozluk.Common.Infastructure;
using BlazorSozluk.Common;
using BlazorSozluk.Common.Models.RequestModels;
using MediatR;

namespace BlazorSozluk.Api.Application.Features.Commands.User.Update
{
    internal class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Guid>
    {
        private readonly IMapper mapper;
        private readonly IUserRepository userRepository;

        public UpdateUserCommandHandler(IMapper mapper, IUserRepository userRepository)
        {
            this.mapper = mapper;
            this.userRepository = userRepository;
        }

        public async Task<Guid> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var dbUser = await userRepository.GetByIdAsync(request.Id);

            if (dbUser is null)
                throw new DataMisalignedException("User not found!");

            var dbEmailAddress = dbUser.EmailAddress;
            var emailChanged = string.CompareOrdinal(dbEmailAddress, request.EmailAddress) != 0;

            mapper.Map(request, dbUser);

            var rows = await userRepository.UpdateAsync(dbUser);

            // check if email changed -> RabbitMQ

            if (emailChanged && rows > 0)
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

                dbUser.EmailConfig = false;
                await userRepository.UpdateAsync(dbUser);
            }

            return dbUser.Id;
        }
    }
}
