﻿using AutoMapper;
using BlazorSozluk.Api.Application.Interfaces.Repositories;
using BlazorSozluk.Common.Infastructure;
using BlazorSozluk.Common.Infastructure.Exceptions;
using BlazorSozluk.Common.ViewModels.Models;
using BlazorSozluk.Common.ViewModels.Queries;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlazorSozluk.Api.Application.Features.Commands.User
{
    public class LoginUserCommandHanler : IRequestHandler<LoginUserCommand, LoginUserViewModel>
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;

        public LoginUserCommandHanler(IUserRepository userRepository, IMapper mapper, IConfiguration configuration)
        {
            this.userRepository = userRepository;
            this.mapper = mapper;
            this.configuration = configuration;
        }

        public async Task<LoginUserViewModel> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var dbUser = await userRepository.GetSingleAsync(i => i.EmailAdress == request.EmailAddress);

            if (dbUser == null)
                throw new DatabaseValidationException("User not found!");

            // Veri tabanında şifrelenmiş olan pasworrd, çözülemeyeceğinden,
            // dışardan gelen şifreyi ona çevirip karsılastırıyoruz
            var pass = PasswordEncryptor.Encrpt(request.Password);
            if (dbUser.Password != pass)
                throw new DatabaseValidationException("Password is wrong!");

            if (!dbUser.EmailConfig)
                throw new DatabaseValidationException("Email address is not confirmed yet!");

            var result = mapper.Map<LoginUserViewModel>(dbUser);


            var claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier,dbUser.Id.ToString()),
                new Claim(ClaimTypes.Email,dbUser.EmailAdress),
                new Claim(ClaimTypes.Name,dbUser.UserName),
                new Claim(ClaimTypes.GivenName,dbUser.FirstName),
                new Claim(ClaimTypes.Surname,dbUser.LastName),
            };

            result.Token = GenerateToken(claims);

            return result;

        }

        private string GenerateToken(Claim[] claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["AuthConfig:Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.Now.AddDays(10);

            var token = new JwtSecurityToken(claims: claims,
                                             expires: expiry,
                                             signingCredentials: creds,
                                             notBefore: DateTime.Now);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
