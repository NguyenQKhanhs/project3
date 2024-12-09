using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging; // Import the namespace for ILogger
using System.Threading.Tasks; // Import for Task
using RestSharp;
using Microsoft.Extensions.Options;
using Project3.Data;


namespace Proj3.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;
        private readonly string _clientId;

        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger, IOptions<ImgurSettings> options)
        {
            _context = context;
            _logger = logger; // Initialize logger if needed
            _clientId = options.Value.ClientId; // Lấy Client ID từ cấu hình
        }
        [Authorize]
        public async Task<ActionResult> admin_account()
        {
            var accounts = await _context.Accounts.ToListAsync(); // Use asynchronous call
            return View(accounts);
        }
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> admin_account_delete(int id)
        {
            var account = await _context.Accounts.FindAsync(id); // Use asynchronous call
            if (account != null)
            {
                _context.Accounts.Remove(account);
                await _context.SaveChangesAsync(); // Use async save
            }
            return RedirectToAction("admin_account");
        }
        [Authorize]
        public ActionResult admin_account_add()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> admin_account_add(Account account)
        {
            
            {
                account.CreatedAt = DateTime.UtcNow;
                await _context.Accounts.AddAsync(account); // Use async add
                await _context.SaveChangesAsync();
                return RedirectToAction("admin_account");
            }
            
        }

        public IActionResult login()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> admin_account_edit(int id)
        {
            var existingAccount = await _context.Accounts.FindAsync(id);
            if (existingAccount == null)
            {
                return NotFound(); // Return 404 if not found
            }
            return View(existingAccount);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> admin_account_edit(Account account)
        {
           
            {
                var existingAccount = await _context.Accounts.FindAsync(account.AccountId);
                if (existingAccount == null)
                {
                    return NotFound();
                }

                existingAccount.Username = account.Username;
                existingAccount.FullName = account.FullName;
                existingAccount.Email = account.Email;
                existingAccount.Password = account.Password;

                await _context.SaveChangesAsync();
                return RedirectToAction("admin_account");
            }
            
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> admin_course()
        {
            var courses = await _context.Courses.ToListAsync(); // Lấy danh sách khóa học từ cơ sở dữ liệu
            return View(courses); // Trả về view với danh sách khóa học
        }



        [Authorize]
        public ActionResult admin_course_add()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> admin_course_add(Course course, IFormFile image)
        {
            if (image != null && image.Length > 0)
            {
                // Đọc file ảnh thành byte[]
                using var memoryStream = new MemoryStream();
                await image.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();
                var base64Image = Convert.ToBase64String(fileBytes);

                // Upload ảnh lên Imgur
                var client = new RestClient("https://api.imgur.com/3/upload");
                var request = new RestRequest("/", Method.Post);
                request.AddHeader("Authorization", $"Client-ID {_clientId}");
                request.AddParameter("image", base64Image);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    var imgurResponse = System.Text.Json.JsonDocument.Parse(response.Content);
                    var imgUrl = imgurResponse.RootElement.GetProperty("data").GetProperty("link").GetString();
                    course.ImageUrl = imgUrl; // Lưu link ảnh vào course
                }
                else
                {
                    ViewBag.Message = "Upload ảnh thất bại!";
                    return View(course);
                }
            }

            // Lưu thông tin khóa học vào cơ sở dữ liệu
            course.CreatedAt = DateTime.UtcNow;
            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();

            return RedirectToAction("admin_course");
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> admin_course_edit(int id)
        {
            var existingCourse = await _context.Courses.FindAsync(id);
            if (existingCourse == null)
            {
                return NotFound();
            }
            return View(existingCourse);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> admin_course_edit(Course course, IFormFile image)
        {
            
            {
                var existingCourse = await _context.Courses.FindAsync(course.CourseId);
                if (existingCourse == null)
                {
                    return NotFound();
                }

                // Cập nhật thông tin khóa học
                existingCourse.Name = course.Name;
                existingCourse.Description = course.Description;
                existingCourse.Price = course.Price;
                existingCourse.StartDate = course.StartDate;
                existingCourse.StudyLevel = course.StudyLevel;
                existingCourse.Duration = course.Duration;
                existingCourse.CategoryId = course.CategoryId;
                existingCourse.Content = course.Content;

                if (image != null && image.Length > 0)
                {
                    // Đọc file ảnh thành byte[]
                    using var memoryStream = new MemoryStream();
                    await image.CopyToAsync(memoryStream);
                    var fileBytes = memoryStream.ToArray();
                    var base64Image = Convert.ToBase64String(fileBytes);

                    // Upload ảnh lên Imgur
                    var client = new RestClient("https://api.imgur.com/3/upload");
                    var request = new RestRequest("/", Method.Post);
                    request.AddHeader("Authorization", $"Client-ID {_clientId}");
                    request.AddParameter("image", base64Image);

                    var response = await client.ExecuteAsync(request);

                    if (response.IsSuccessful)
                    {
                        var imgurResponse = System.Text.Json.JsonDocument.Parse(response.Content);
                        var imgUrl = imgurResponse.RootElement.GetProperty("data").GetProperty("link").GetString();
                        existingCourse.ImageUrl = imgUrl; // Cập nhật URL ảnh mới
                    }
                    else
                    {
                        ViewBag.Message = "Upload ảnh thất bại!";
                        return View(course);
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("admin_course");
            }
            
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> admin_course_delete(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("admin_course");
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> admin_course_view(int id)
        {
            var course = await _context.Courses.FindAsync(id); // Tìm khóa học theo ID
            if (course == null)
            {
                return NotFound(); // Nếu không tìm thấy khóa học
            }
            return View(course); // Trả về view với thông tin khóa học
        }

        [Authorize]
        public async Task<ActionResult> admin_class()
        {
            var classes = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Instructor)
                .ToListAsync();
            return View(classes);
        }

        [Authorize]
        // GET: admin_class_add
        public IActionResult admin_class_add()
        {
            ViewBag.Courses = _context.Courses.ToList(); // Lấy danh sách khóa học
            ViewBag.Instructors = _context.Instructors.ToList(); // Lấy danh sách giảng viên
            return View();
        }

        // POST: admin_class_add
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> admin_class_add(Class newClass)
        {
           
            
                newClass.CreatedAt = DateTime.UtcNow; // Gán thời gian tạo
                await _context.Classes.AddAsync(newClass); // Thêm lớp vào cơ sở dữ liệu
                await _context.SaveChangesAsync(); // Lưu thay đổi
                return RedirectToAction("admin_class"); // Chuyển hướng đến danh sách lớp
            

            
        }
    


        [Authorize]
        public async Task<ActionResult> admin_instructor()
        {
            var instructors = await _context.Instructors.ToListAsync();
            return View(instructors);
        }

        [Authorize]
        public ActionResult admin_instructor_add()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> admin_instructor_add(Instructor instructor, IFormFile image)
        {
            if (image != null && image.Length > 0)
            {
                // Đọc file ảnh thành byte[]
                using var memoryStream = new MemoryStream();
                await image.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();
                var base64Image = Convert.ToBase64String(fileBytes);

                // Upload ảnh lên Imgur
                var client = new RestClient("https://api.imgur.com/3/upload");
                var request = new RestRequest("/", Method.Post);
                request.AddHeader("Authorization", $"Client-ID {_clientId}"); // Thay _clientId bằng Client ID của bạn
                request.AddParameter("image", base64Image);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    var imgurResponse = System.Text.Json.JsonDocument.Parse(response.Content);
                    var imgUrl = imgurResponse.RootElement.GetProperty("data").GetProperty("link").GetString();
                    instructor.ImageLink = imgUrl; // Lưu URL ảnh vào instructor
                }
                else
                {
                    ViewBag.Message = "Upload ảnh thất bại!";
                    return View(instructor);
                }
            }

            // Lưu thông tin giảng viên
            instructor.CreatedAt = DateTime.UtcNow;
            await _context.Instructors.AddAsync(instructor);
            await _context.SaveChangesAsync();
            return RedirectToAction("admin_instructor");
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> admin_instructor_edit(int id)
        {
            var instructor = await _context.Instructors.FindAsync(id);
            if (instructor == null)
            {
                return NotFound();
            }
            return View(instructor);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> admin_instructor_edit(Instructor instructor, IFormFile image)
        {
            var existingInstructor = await _context.Instructors.FindAsync(instructor.InstructorId);
            if (existingInstructor == null)
            {
                return NotFound();
            }

            // Cập nhật thông tin
            existingInstructor.Name = instructor.Name;
            existingInstructor.Gender = instructor.Gender;
            existingInstructor.Bio = instructor.Bio;
            existingInstructor.PhoneNumber = instructor.PhoneNumber;
            existingInstructor.Email = instructor.Email;

            // Nếu có ảnh mới được tải lên
            if (image != null && image.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await image.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();
                var base64Image = Convert.ToBase64String(fileBytes);

                // Upload ảnh lên Imgur (hoặc lưu ảnh khác)
                var client = new RestClient("https://api.imgur.com/3/upload");
                var request = new RestRequest("/", Method.Post);
                request.AddHeader("Authorization", $"Client-ID {_clientId}");
                request.AddParameter("image", base64Image);

                var response = await client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    var imgurResponse = System.Text.Json.JsonDocument.Parse(response.Content);
                    var imgUrl = imgurResponse.RootElement.GetProperty("data").GetProperty("link").GetString();
                    existingInstructor.ImageLink = imgUrl; // Cập nhật URL ảnh
                }
                else
                {
                    ViewBag.Message = "Failed to upload the image.";
                    return View(instructor);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("admin_instructor");
        }

        [Authorize]
        public async Task<ActionResult> admin_instructor_delete(int id)
        {
            var instructor = await _context.Instructors.FindAsync(id); // Use asynchronous call
            if (instructor != null)
            {
                _context.Instructors.Remove(instructor);
                await _context.SaveChangesAsync(); // Use async save
            }
            return RedirectToAction("admin_instructor");
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> adminstudent()
        {
            var customerInformation = await _context.CustomerInformations.ToListAsync();
            return View(customerInformation);
        }

        public ActionResult admin_student_add()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> admin_student_add(CustomerInformation customerInformation, IFormFile Image)
        {
            if (Image != null && Image.Length > 0)
            {
                // Đọc file ảnh thành byte[]
                using var memoryStream = new MemoryStream();
                await Image.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();
                var base64Image = Convert.ToBase64String(fileBytes);

                // Upload ảnh lên Imgur
                var client = new RestClient("https://api.imgur.com/3/upload");
                var request = new RestRequest("/", Method.Post);
                request.AddHeader("Authorization", $"Client-ID {_clientId}"); // Thay _clientId bằng Client ID của bạn
                request.AddParameter("image", base64Image);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    // Parse JSON response để lấy link ảnh
                    var imgurResponse = System.Text.Json.JsonDocument.Parse(response.Content);
                    if (imgurResponse.RootElement.TryGetProperty("data", out var dataElement) &&
                        dataElement.TryGetProperty("link", out var linkElement))
                    {
                        customerInformation.ImageLink = linkElement.GetString(); // Lưu URL ảnh vào customerInformation
                    }
                    else
                    {
                        ViewBag.Message = "Không lấy được đường dẫn ảnh từ Imgur.";
                        return View(customerInformation);
                    }
                }
                else
                {
                    ViewBag.Message = "Upload ảnh thất bại! Đáp ứng từ Imgur không thành công.";
                    return View(customerInformation);
                }
            }

            // Xử lý lưu dữ liệu sinh viên
            customerInformation.CreatedAt = DateTime.UtcNow; // Đặt ngày tạo
            _context.CustomerInformations.Add(customerInformation); // Thêm sinh viên vào DbContext
            await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu
            return RedirectToAction("admin_student"); // Chuyển hướng về danh sách sinh viên
        }

        [Authorize]
        public ActionResult admin()
        {
            return View();
        }
    }
}
