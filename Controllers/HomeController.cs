using Microsoft.AspNetCore.Mvc;
using Project3.Models;
using Project3.Data;
using System;

namespace Project3.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        // Constructor để lấy context từ DbContext
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult TestRegistration()
        {
            return View();
        }

        public IActionResult courses()
        {
            return View();
        }

        public IActionResult course_single()
        {
            return View();
        }

                // Action để xử lý việc gửi feedback
        [HttpPost]
        public IActionResult Index(Feedbacks feedback)
        {
            if (ModelState.IsValid)
            {
                // Lưu thông tin feedback vào database
                feedback.CreatedAt = DateTime.UtcNow;
                _context.Feedbacks.Add(feedback);
                _context.SaveChanges();

                // Thông báo thành công
                ViewBag.Message = "Feedback submitted successfully!";

                // Trả về lại view Contact để giữ người dùng ở lại trang liên hệ
                return View();
            }

            // Nếu model không hợp lệ, trả về lại trang Contact với thông báo lỗi
            ViewBag.Message = "There was an error submitting your feedback.";
            return View();
        }
        
    }
}
