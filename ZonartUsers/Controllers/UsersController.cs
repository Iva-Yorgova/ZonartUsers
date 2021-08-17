﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZonartUsers.Data;
using ZonartUsers.Data.Models;
using ZonartUsers.Models.Questions;
using ZonartUsers.Models.Users;

namespace ZonartUsers.Controllers
{
    using static WebConstants.Cache;

    public class UsersController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly ZonartUsersDbContext data;
        private readonly IMemoryCache cache;

        public UsersController(UserManager<User> userManager, 
            SignInManager<User> signInManager, 
            ZonartUsersDbContext data, 
            IMemoryCache cache)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.data = data;
            this.cache = cache;
        }


        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(RegisterUserModel user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            var registeredUser = new User
            {
                Email = user.Email,
                UserName = user.Email,
                FullName = user.FullName
            };

            var result = await this.userManager.CreateAsync(registeredUser, user.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            else
            {
                return RedirectToAction("Login", "Users");
            }

            return View(user);
        }


        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginUserModel user)
        {
            var loggedInUser = this.userManager.FindByEmailAsync(user.Email).Result;

            if (loggedInUser ==  null)
            {
                ModelState.AddModelError(string.Empty, "Credentials invalid!");
                return View(user);
            }
   
            var passwordIsValid = await this.userManager.CheckPasswordAsync(loggedInUser, user.Password);

            if (!passwordIsValid)
            {
                ModelState.AddModelError(string.Empty, "Credentials invalid!");
                return View(user);
            }

            else
            {
                await this.signInManager.SignInAsync(loggedInUser, true);
                return RedirectToAction("Welcome", "Users", loggedInUser);
            }
        }


        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }


        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await this.userManager.GetUserAsync(this.User);
            await this.userManager.ChangePasswordAsync(user, "", "");

            return View();
        }


        [Authorize(Roles = "Administrator")]
        public IActionResult EditQuestion()
        {
            // Logic here
            return View();
        }


        public IActionResult Welcome(LoginUserModel user)
        {
            return View(user);
        }


        //[ResponseCache(Duration = 3600)]
        public IActionResult Questions()
        {
            var latestQuestions = this.cache.Get<List<QuestionsListingViewModel>>(QuestionsCacheKey);

            if (latestQuestions == null)
            {
                latestQuestions = this.data.Questions
                .Select(t => new QuestionsListingViewModel
                {
                    Text = t.Text,
                    Answer = t.Answer
                })
                .ToList();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(60));

                this.cache.Set(QuestionsCacheKey, latestQuestions, cacheOptions);
            }

            return View(latestQuestions);
        }

    }
}
