﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ZonartUsers.Data;
using ZonartUsers.Data.Models;
using ZonartUsers.Models.Orders;

namespace Zonart.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ZonartUsersDbContext data;
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;

        public OrdersController(ZonartUsersDbContext data, 
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            this.data = data;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public IActionResult Create([FromQuery] int templateId)
        {

            return View(new OrderTemplateModel
            {
                TemplateId = templateId,
       
            });
        }


        [HttpPost]
        public IActionResult Create(OrderTemplateModel info)
        {

            if (!ModelState.IsValid)
            {
                return View(info);
            }

            var order = new Order
            {
                TemplateId = info.TemplateId,
                Name = info.Name,
                Email = info.Email,
            };

            this.data.Orders.Add(order);
            this.data.SaveChanges();

            return RedirectToAction("All", "Templates");
        }
    }
}
