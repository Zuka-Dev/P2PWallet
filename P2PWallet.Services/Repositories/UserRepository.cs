using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using P2PWallet.Models.DTOs;
using P2PWallet.Models.Entities;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Metrics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace P2PWallet.Services.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly P2PWalletDbContext _context;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;
        private readonly IAccountRepository _accountRepository;

        public UserRepository(P2PWalletDbContext context, IConfiguration config,IHttpContextAccessor httpContextAccessor,IEmailService emailService, IAccountRepository accountRepository)
        {
            _context = context;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
            _accountRepository = accountRepository;
        }

        public async Task<bool> CheckUserExists(string email)
        {
            var exists = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (exists is not null) return true;
            else return false;

        }

        public async Task<BaseResponseDTO> GetUserDetails()
        {
            try
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.SerialNumber);
                if (userIdClaim == null)
                {
                    return new BaseResponseDTO
                    {
                        Status = false,
                        StatusMessage = "Missing or invalid user ID in JWT token",
                        Data = new { }
                    };
                }
                var userId = userIdClaim?.Value;
                var user = await _context.Users.Include(x => x.Accounts).FirstOrDefaultAsync(x => Convert.ToString(x.Id) == userId);
                if (user is null)
                {
                    return new BaseResponseDTO
                    {
                        Status = false,
                        StatusMessage = "User Does not exist",
                        Data = new { }
                    };
                }

                var accountsDTO = user.Accounts.Select(a => new AccountDTO
                {
                    AccountNumber = a.AccountNumber,
                    Balance = a.Balance,
                    Currency = "NGN"
                }).ToList();
                return new BaseResponseDTO
                {
                    Status = true,
                    StatusMessage = "User Details",
                    Data = new
                    {
                        Username = user.Username,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        Address = user.Address,
                        ImageBase64 = user.ImageBase64Byte is null ? "" : Convert.ToBase64String(user.ImageBase64Byte),
                        HasPin = user.PinHash is null || user.PinSalt is null ? false : true,
                        Accounts = accountsDTO
                    }
                };
            }catch(Exception ex)
            {
                throw;
            }
        }

        public async Task<BaseResponseDTO> LoginUser(LoginUserDTO userDTO)
        {
            try
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(x => x.Email == userDTO.Email);
                //Check if user exists
                if (existingUser is null)
                {
                    return new BaseResponseDTO
                    {
                        Status = false,
                        StatusMessage = "User Not Found",
                        Data = new { }
                    };
                }
                //Verify Password
                if (!VerifyPassword(userDTO.Password, existingUser.PasswordHash, existingUser.PasswordSalt))
                {
                    return new BaseResponseDTO
                    {
                        Status = false,
                        StatusMessage = "Wrong Credentials. User Email or Password is incorrect",
                        Data = new { }
                    };
                }
                if (existingUser.IsVerified != true)
                {
                    return new BaseResponseDTO
                    {
                        Status = false,
                        StatusMessage = "User Email not Verified",
                        Data = new { }
                    };
                }
                //Create Token
                var token =  CreateToken(existingUser);
                //Return Response 
                return new BaseResponseDTO
                {
                    Status = true,
                    StatusMessage = "Login Succusssful",
                    Data = new { JWTToken = token}
                };
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
            
        }

   
        public async Task<BaseResponseDTO> RegisterUser(CreateUserDTO userDTO)
        {
            try
            {
                // Validate email format using regular expression
                if (!Regex.IsMatch(userDTO.Email, @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$"))
                {
                    return new BaseResponseDTO
                    {
                        Status = false,
                        StatusMessage = "Invalid email format. Please provide a valid email ending in '.com'.",
                        Data = new { }
                    };
                }
                if (await _context.Users.FirstOrDefaultAsync(x => x.Email == userDTO.Email) is not null || 
                    await _context.Users.FirstOrDefaultAsync(x => x.Username == userDTO.Username) is not null)
                {
                    return new BaseResponseDTO
                    {
                        Status = false,
                        StatusMessage = "User Already Exists",
                        Data = new {}
                    };
                }
                HashPassword(userDTO.Password, out byte[] passwordHash, out byte[] passwordSalt);
                var newUser = new User
                {
                    Username = userDTO.Username,
                    Email = userDTO.Email,
                    FirstName = userDTO.FirstName,
                    LastName = userDTO.LastName,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    VerificationToken = Guid.NewGuid().ToString(),
                    Address = userDTO.Address,
                    PhoneNumber = userDTO.PhoneNumber,
                };


                // newUser.VerificationToken = Guid.NewGuid().ToString();
                // user.ResetTokenExpires = DateTime.Now.AddMinutes(30);
                var frontendUrl = _config.GetSection("Frontend:VerifyUrl").Value;
                var url = BuildResetUrl(frontendUrl!, newUser.VerificationToken, newUser.Email);
                await _emailService.SendVerificationEmail(newUser, url!);

                await _context.Users.AddAsync(newUser);
                await _context.SaveChangesAsync();
                var account = await _accountRepository.CreateAccount(newUser);
                if (!account) return new BaseResponseDTO
                {
                    Status= false,
                    StatusMessage= "Account not Created"
                };
                return new BaseResponseDTO{
                    Status=true,
                    StatusMessage="User Successfully Created",
                    Data = new {
                        Username = newUser.Username,
                        FirstName = newUser.FirstName,
                        LastName = newUser.LastName,
                        Email = newUser.Email,
                        PhoneNumber = newUser.PhoneNumber,
                        Address = newUser.Address
                    }
                };
            } catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<BaseResponseDTO> VerifyToken(VerifyEmailDTO verifyEmailDTO)
        {
            string token = verifyEmailDTO.Token;
            string email = verifyEmailDTO.Email;

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if(token != user.VerificationToken)
            {
                return new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage="Invalid Verification Token",
                };
            }
            user.IsVerified = true;
            user.VerificationToken = null;
            user.VerifiedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return new BaseResponseDTO
            {
                Status = true,
                StatusMessage = "Email Verification successful",
            };
        }
        
        public async Task<BaseResponseDTO> ChangePassword(PasswordDTO passwordDTO)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.SerialNumber);
            if (userIdClaim == null)
            {
                return new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = "Missing or invalid user ID in JWT token",
                    Data = new { }
                };
            }
            var userId = userIdClaim?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(x => Convert.ToString(x.Id) == userId);
            if (user is null)
            {
                return new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = "User Does not exist",
                    Data = new { }
                };
            }
            if (!VerifyPassword(passwordDTO.OldPassword, user.PasswordHash, user.PasswordSalt))
            {
                return new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = "Invalid Password",
                    Data = new { }
                };
            }
            HashPassword(passwordDTO.NewPassword, out byte[] newHash, out byte[] newSalt);
            user.PasswordHash = newHash;
            user.PasswordSalt = newSalt;
            await _context.SaveChangesAsync();
            return new BaseResponseDTO
            {
                Status = true,
                StatusMessage = "Pasword Successfully Updated",
                Data = new { }
            };
        }



        public async Task<BaseResponseDTO> CreatePin(CreatePinDTO pinDTO)
        {
            try
            {
            
            var pin = pinDTO.Pin;
            HashPin(pin, out byte[] pinHash, out byte[] pinSalt);
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.SerialNumber);
            if (userIdClaim == null)
            {
               return new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = "Missing or invalid user ID in JWT token",
                    Data = new { }
                };
            }
            var userId = userIdClaim?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(x => Convert.ToString(x.Id) == userId);
            if (user is null)
            {
                 return new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = "User Does not exist",
                    Data = new { }
                };
            }
            if (user.PinHash is not null || user.PinSalt is not null)
            {
                return new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = "User Pin Has Already Generated",
                    Data = new { }
                };
            }
            user.PinHash = pinHash;
            user.PinSalt = pinSalt;
            await _context.SaveChangesAsync();
            return new BaseResponseDTO
            {
                Status = true,
                StatusMessage = "PIN Successfully created",
                Data = new { }
            };
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<BaseResponseDTO> ChangePin(ChangePinDTO pinDTO)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.SerialNumber);
            if (userIdClaim == null)
            {
                return new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = "Missing or invalid user ID in JWT token",
                    Data = new { }
                };
            }
            var userId = userIdClaim?.Value;
            try { 
            var user = await _context.Users.Include(u=>u.SecurityQuestions).FirstOrDefaultAsync(x => Convert.ToString(x.Id) == userId);
            
            if (user!.PinHash is null||user.PinSalt is null)
            {
                return new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = "User has not generated a PIN",
                    Data = new { }
                }; 
            }
            //Verify OldPin
            if (!VerifyPin(pinDTO.OldPin, user.PinHash, user.PinSalt))
            {
                return new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = "Invalid Old PIN ",
                    Data = new { }
                };
            }
            foreach (var item in pinDTO.SecurityAnswers)
            {
                    var question = user.SecurityQuestions.FirstOrDefault(x => x.Id == item.QuestionId);
                    if(question!.Answer != question.Answer || question is null)
                    {
                        return new BaseResponseDTO
                        {
                            Status = false,
                            StatusMessage = "Wrong Answer to the questions"
                        };
                    }
            }
            HashPin(pinDTO.NewPin, out byte[] pinHash, out byte[] pinSalt);
            user.PinHash= pinHash;
            user.PinSalt = pinSalt;
            await _context.SaveChangesAsync();
                return new BaseResponseDTO
                {
                    Status = true,
                    StatusMessage = "Pin Successfully Changed"
                };
            }
            catch(Exception ex)
            {
                throw;
            }

        }

        public async Task<BaseResponseDTO> ForgotPassword(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user is null)
            {
                return new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = "User Not found",
                };
            }
            user.PasswordResetToken = Guid.NewGuid().ToString();
            user.ResetTokenExpires = DateTime.Now.AddMinutes(30);
            await _context.SaveChangesAsync();
            var frontendUrl = _config.GetSection("Frontend:ResetUrl").Value;
            var url = BuildResetUrl(frontendUrl!, user.PasswordResetToken, user.Email);
            await _emailService.SendResetTokenEmail(user, url!);
            return new BaseResponseDTO
            {
                Status = true,
                StatusMessage = "Reset token Changed"
            };

        }

        public async Task<BaseResponseDTO> PasswordReset(ResetPasswordDTO resetPasswordDTO)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == resetPasswordDTO.Email);
            if (user is null)
            {
                return new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = "User Not found",
                    Data = new { }
                };
            }
            if(resetPasswordDTO.ResetToken != user.PasswordResetToken)
            {
                return new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = "Invalid Reset token",
                    Data = new { }
                };
            }
            if(user.ResetTokenExpires < DateTime.Now)
            {
                return new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = "Reset Password Token Has Expired",
                    Data = new { }
                };
            }
            HashPassword(resetPasswordDTO.Password!, out byte[] PasswordHash, out byte[] PasswordSalt);
            user.PasswordHash = PasswordHash;
            user.PasswordSalt = PasswordSalt;
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;
            await _context.SaveChangesAsync();
            return new BaseResponseDTO
            {
                Status = true,
                StatusMessage = "Password Reset Successful",
                Data = new { }
            };

        }
        public async Task<BaseResponseDTO> UpdateUserById(UpdateUserDTO updateUserDTO)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.SerialNumber);
            if (userIdClaim == null)
            {
                return new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = "Missing or invalid user ID in JWT token",
                    Data = new { }
                };
            }
            var userId = userIdClaim?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(x => Convert.ToString(x.Id) == userId);
            if (user is null)
            {
                return new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = "User Does not exist",
                    Data = new { }
                };
            }
            user.LastName = updateUserDTO.LastName;
            user.PhoneNumber = updateUserDTO.Phone;
            user.Address = updateUserDTO.Address;
            user.ImageBase64Byte = Convert.FromBase64String(updateUserDTO.ImageBase64);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return new BaseResponseDTO
            {
                Status = true,
                StatusMessage = "Details successfully Updated"
            };
        }

        public async Task<BaseResponseDTO> CreateSecurityQuestions(SecurityQuestionDTO request)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.SerialNumber);
            if (userIdClaim == null)
            {
                return new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = "Missing or invalid user ID in JWT token",
                    Data = new { }
                };
            }
            var userId = userIdClaim?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(x => Convert.ToString(x.Id) == userId);
            if (user is null)
            {
                return new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = "User Does not exist",
                    Data = new { }
                };
            }
            try
            {
                foreach (var question in request.securityQuestions)
                {
                    var securityQuestion = new SecurityQuestion
                    {
                        Question = question.Question,
                        Answer = question.Answer,
                        UserId = Convert.ToInt32(userId)
                    };
                    await _context.SecurityQuestions.AddAsync(securityQuestion);
                }
                await _context.SaveChangesAsync();
                return new BaseResponseDTO
                {
                    Status = true,
                    StatusMessage = "Security Questions Created"
                };
            }
            catch (Exception ex)
            {
                throw; 
            }
        }





        private string BuildResetUrl(string url, string resetToken, string email)
        {
            var queryParams = new Dictionary<string, string>
            {
                {"token",resetToken},
                {"email",email }
            };
            var queryString = string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
            return $"{url}?{queryString}";
        }



        private string CreateToken(User user)
        {
            //Claims
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.SerialNumber, Convert.ToString(user.Id)),
                new Claim(ClaimTypes.Email, user.Email)
            };
            //Key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("JWT:Secret").Value));
            //Credentials
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            //Create Token
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
                );
            //Token Handler
            var handler = new JwtSecurityTokenHandler();
            //write token
            return handler.WriteToken(token);
        }
        private void HashPassword(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }
        private bool VerifyPassword(string password,  byte[] passwordHash,  byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var inputPassword = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return passwordHash.SequenceEqual(inputPassword);
            }
        }
        private void HashPin(string pin, out byte[] pinHash, out byte[] pinSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                pinSalt = hmac.Key;
                pinHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(pin));
            }
        }
        private bool VerifyPin(string pin, byte[] pinHash, byte[] pinSalt)
        {
            using (var hmac = new HMACSHA512(pinSalt))
            {
                var inputPassword = hmac.ComputeHash(Encoding.UTF8.GetBytes(pin));
                return pinHash.SequenceEqual(inputPassword);
            }
        }

       
    }
}
