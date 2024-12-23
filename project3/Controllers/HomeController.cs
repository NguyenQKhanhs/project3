using Microsoft.AspNetCore.Mvc;
using Project3.Models;
using Project3.Data;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Project3.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        // Constructor để lấy context từ DbContext
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, EmailService emailService)
        {
            _logger = logger;
            _context = context;
            _emailService = emailService;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var courses = await _context.Courses.ToListAsync();
            var Courses = courses.Take(6).ToList(); // Lấy tối đa 6 khóa học
            return View(Courses);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult TestRegistration()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TestRegistration(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                ViewBag.Message = "Please enter an ID to search.";
                return View(); // Trả về trang hiện tại nếu không có ID
            }
            ViewBag.Courses = await _context.Courses.ToListAsync(); // Lấy danh sách khóa học để hiển thị lại
            // Tìm kiếm dữ liệu theo ID
            var result = await _context.CustomerInformations

                .Where(c => c.CustomerInformationId.Contains(id)) // Dùng Contains để tìm theo một phần của ID
                .ToListAsync();

            if (result.Count == 0)
            {
                ViewBag.Message = "No results found.";
            }

            // Trả kết quả tìm kiếm về view TestRegistration
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> courses()
        {
            // Fetch all courses from the database
            var courses = await _context.Courses.ToListAsync();

            // Return the list of courses to the view
            return View(courses); // Use 'courses' (plural) instead of 'Course'
        }
        [HttpGet]
        public async Task<IActionResult> Course_Single(int id)
        {
            // Tìm khóa học theo ID
            var course = await _context.Courses.FindAsync(id);

            if (course == null)
            {
                // Nếu không tìm thấy khóa học, có thể trả về một trang lỗi hoặc redirect
                return NotFound();
            }

            return View(course); // Trả về view với thông tin khóa học
        }

        // Action để xử lý việc gửi feedback
        [HttpPost]
        public IActionResult Index(Feedbacks feedback)
        {

            // Lưu thông tin feedback vào database
            feedback.CreatedAt = DateTime.UtcNow;
            _context.Feedbacks.Add(feedback);
            _context.SaveChanges();

            // Thông báo thành công thông qua TempData để hiển thị tại trang Index
            TempData["Message"] = "Feedback submitted successfully!";

            // Chuyển hướng về trang Index
            return RedirectToAction("Index");



        }



        [HttpGet]
        public async Task<ActionResult> registration(int courseId)
        {
            // Tìm thông tin khóa học dựa trên courseId
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
            {
                // Nếu không tìm thấy khóa học, chuyển hướng hoặc trả về thông báo lỗi
                TempData["ErrorMessage"] = "Khóa học không tồn tại.";
                return RedirectToAction("Courses", "Home");
            }

            // Truyền thông tin khóa học qua ViewBag hoặc ViewModel
            ViewBag.Course = course;

            // Trả về view đăng ký với thông tin khóa học
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> registration(CustomerInformation customerInformation, string Schedule, int CourseId)
        {
            if (string.IsNullOrEmpty(Schedule))
            {
                TempData["AlertMessage"] = "Please select a schedule!";
                TempData["AlertType"] = "warning"; // SweetAlert2 "warning" icon

                var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == CourseId);
                ViewBag.Course = course; // Assign course information back
                return View(customerInformation);
            }

            // Validate CourseId
            var courseExists = await _context.Courses.AnyAsync(c => c.CourseId == CourseId);
            if (!courseExists)
            {
                TempData["AlertMessage"] = "The course does not exist. Please try again.";
                TempData["AlertType"] = "error"; // SweetAlert2 "error" icon

                var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == CourseId);
                ViewBag.Course = course; // Assign course information back
                return View(customerInformation);
            }

            // Assign CourseId to the CustomerInformation object
            customerInformation.CourseId = CourseId;

            // Check if the user already exists
            var existingCustomer = await _context.CustomerInformations.FirstOrDefaultAsync(c =>
                c.Email == customerInformation.Email &&
                c.PhoneNumber == customerInformation.PhoneNumber &&
                c.FullName == customerInformation.FullName &&
                c.CourseId == customerInformation.CourseId);

            if (existingCustomer != null)
            {
                TempData["AlertMessage"] = "The user already exists! You can update the information.";
                TempData["AlertType"] = "warning"; // SweetAlert2 "warning" icon

                ViewBag.Course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == CourseId); // Assign course information back
                return View(customerInformation);
            }

            // Assign a unique ID
            customerInformation.CustomerInformationId = await GenerateUniqueCustomerInformationIdAsync();
            // Assign values to required fields
            customerInformation.Schedule = Schedule;
            customerInformation.Status = "Unattempted";
            customerInformation.CreatedAt = DateTime.UtcNow;

            // Kiểm tra và gán ClassesId nếu có lớp học tương ứng với CourseId
            var matchingClass = await _context.Classes.FirstOrDefaultAsync(c => c.CourseId == CourseId);
            if (matchingClass != null)
            {
                customerInformation.ClassesId = matchingClass.ClassesId;
            }

            try
            {
                await _context.CustomerInformations.AddAsync(customerInformation);
                await _context.SaveChangesAsync();

                //get current date
                DateTime now = DateTime.Now;
                //Date time after 5 day
                DateTime after5Days = now.AddDays(5);
                //convert date to String
                String dateSendMail = after5Days.ToString("yyyy/MM/dd");
                // Send notification email
                var subject = "Course Registration Successful Notification";
                var message = $@"
        <html>
        <head>
            <style>
                body {{
                    font-family: Arial, sans-serif;
                    line-height: 1.6;
                    color: #333;
                }}
                h2 {{
                    color: #007BFF;
                }}
                p {{
                    margin: 10px 0;
                }}
                .course-details {{
                    margin-top: 15px;
                    padding: 10px;
                    background-color: #f8f9fa;
                    border: 1px solid #ddd;
                }}
            </style>
        </head>
        <body>
            <h2>Hello {customerInformation.FullName},</h2>

            <p>Thank you for registering for the course.</p>
            <p>To join the class, you must take our entrance test.</p>
            <p>
                Please come to the center at <strong>14:00</strong> on <strong>{dateSendMail}</strong>.<br />
                Address: <strong>8A Ton That Thuyet, My Dinh, Ha Noi</strong>.
            </p>

            <p>Your customer ID is: <strong>{customerInformation.CustomerInformationId}</strong></p>

            <div class='course-details'>
                <p><strong>Course Details:</strong></p>
                <p>- <strong>Course name:</strong> {await GetCourseNameByIdAsync(CourseId)}</p>
                <p>- <strong>Schedule:</strong> {Schedule}</p>
            </div>

            <p>Best wishes for your studies!</p>
            <p>Sincerely,<br />Support Team</p>
        </body>
        </html>";

                await _emailService.SendEmailAsync(customerInformation.Email, subject, message);


                TempData["AlertMessage"] = "Registration successful! The information has been sent to your email.";
                TempData["AlertType"] = "success"; // SweetAlert2 "success" icon

                ViewBag.Course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == CourseId); // Assign course information back
                return View(customerInformation);
            }
            catch (Exception ex)
            {
                TempData["AlertMessage"] = $"An error occurred: {ex.Message}";
                TempData["AlertType"] = "error"; // SweetAlert2 "error" icon

                ViewBag.Course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == CourseId); // Assign course information back
                return View(customerInformation);
            }
        }

        // Hàm hỗ trợ lấy tên khóa học
        private async Task<string> GetCourseNameByIdAsync(int courseId)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
            return course?.Name ?? "N/A";
        }


        // Phương thức để tạo ID duy nhất
        private async Task<string> GenerateUniqueCustomerInformationIdAsync()
        {
            Random random = new Random();
            string id;
            bool isUnique;

            do
            {
                // Sinh một ID ngẫu nhiên gồm 8 chữ số
                id = random.Next(10000000, 99999999).ToString();

                // Kiểm tra tính duy nhất
                isUnique = !await _context.CustomerInformations.AnyAsync(c => c.CustomerInformationId == id);
            } while (!isUnique);

            return id;
        }

    }
}
