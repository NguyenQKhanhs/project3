using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging; // Import the namespace for ILogger
using System.Threading.Tasks; // Import for Task
using RestSharp;
using Microsoft.Extensions.Options;
using Project3.Data;
using System.Net;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using X.PagedList;




namespace Proj3.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;
        private readonly string _clientId;
        private readonly EmailService _emailService;

        



        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger, IOptions<ImgurSettings> options, EmailService emailService)
        {
            _context = context;
            _logger = logger; // Initialize logger if needed
            _clientId = options.Value.ClientId; // Lấy Client ID từ cấu hình
            _emailService = emailService;
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
            var passwordHasher = new PasswordHasher<Account>();
            account.Password = passwordHasher.HashPassword(account, account.Password);
            account.CreatedAt = DateTime.UtcNow;
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();
            return RedirectToAction("admin_account");
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
            var existingAccount = await _context.Accounts.FindAsync(account.AccountId);
            if (existingAccount == null)
            {
                return NotFound();
            }

            var passwordHasher = new PasswordHasher<Account>();
            existingAccount.Password = passwordHasher.HashPassword(existingAccount, account.Password);
            existingAccount.Username = account.Username;
            existingAccount.FullName = account.FullName;
            existingAccount.Email = account.Email;

            await _context.SaveChangesAsync();
            return RedirectToAction("admin_account");
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
            // Kiểm tra hình ảnh
            if (image != null && image.Length > 0)
            {
                try
                {
                    using var memoryStream = new MemoryStream();
                    await image.CopyToAsync(memoryStream);
                    var fileBytes = memoryStream.ToArray();
                    var base64Image = Convert.ToBase64String(fileBytes);

                    // Tạo client để upload ảnh lên Imgur
                    var client = new RestClient("https://api.imgur.com/3/upload");
                    var request = new RestRequest("/", Method.Post);
                    request.AddHeader("Authorization", $"Client-ID {_clientId}");
                    request.AddParameter("image", base64Image);

                    // Gửi yêu cầu và lấy phản hồi
                    var response = await client.ExecuteAsync(request);

                    if (response.IsSuccessful)
                    {
                        var imgurResponse = System.Text.Json.JsonDocument.Parse(response.Content);
                        var imgUrl = imgurResponse.RootElement.GetProperty("data").GetProperty("link").GetString();
                        course.ImageUrl = imgUrl; // Lưu URL ảnh vào đối tượng course
                    }
                    else
                    {
                        // Lỗi từ API Imgur
                        ViewBag.Message = $"Upload thất bại! Lỗi từ API: {response.StatusCode} - {response.Content}";
                        return View(course);
                    }
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi upload ảnh
                    ViewBag.Message = $"Lỗi trong quá trình upload ảnh: {ex.Message}";
                    return View(course);
                }
            }
            else
            {
                ViewBag.Message = "Vui lòng chọn một hình ảnh để tải lên.";
                return View(course);
            }

            // Đặt thông tin cho đối tượng course
            course.CreatedAt = DateTime.UtcNow;

            // Lưu thông tin khóa học vào cơ sở dữ liệu
            try
            {
                _context.Courses.Add(course);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                // Xử lý lỗi cụ thể liên quan đến cơ sở dữ liệu
                ViewBag.Message = $"Lỗi lưu cơ sở dữ liệu: {dbEx.InnerException?.Message ?? dbEx.Message}";
                return View(course);
            }
            catch (Exception ex)
            {
                // Xử lý lỗi chung
                ViewBag.Message = $"Lỗi không xác định: {ex.Message}";
                return View(course);
            }

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
                existingCourse.ExamDate = course.ExamDate;
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
        public async Task<IActionResult> admin_course_delete(int id)
        {
            // Tìm khóa học theo ID
            var course = await _context.Courses.FindAsync(id);

            // Kiểm tra xem khóa học có tồn tại không
            if (course == null)
            {
                // Xử lý khi không tìm thấy khóa học
                return NotFound($"Course with ID {id} not found.");
            }

            // Tìm tất cả Class có CourseId trùng với khóa học đang xóa
            var classesToDelete = await _context.Classes
                .Where(c => c.CourseId == id)
                .ToListAsync();

            // Xóa tất cả các lớp
            _context.Classes.RemoveRange(classesToDelete);

            // Tìm tất cả CustomerInformation có CourseId trùng với khóa học đang xóa
            var customerInformationsToDelete = await _context.CustomerInformations
                .Where(ci => ci.CourseId == id)
                .ToListAsync();

            // Xóa tất cả các CustomerInformation
            _context.CustomerInformations.RemoveRange(customerInformationsToDelete);

            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            // Xóa khóa học
            _context.Courses.Remove(course);

            // Lưu thay đổi sau khi xóa khóa học
            await _context.SaveChangesAsync();

            // Chuyển hướng về trang danh sách khóa học
            return RedirectToAction("admin_course"); // Thay đổi "admin_course" thành tên action bạn muốn chuyển hướng đến
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
        [HttpGet]
        public async Task<ActionResult> CourseSearch(string searchTerm)
        {
            // Tìm tất cả các khóa học có Name chứa từ khóa tìm kiếm
            var courses = await _context.Courses
                .Where(c => c.Name != null && c.Name.Contains(searchTerm)) // Kiểm tra Name không null
                .ToListAsync();

            return View(courses);
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
        [HttpGet]
        public async Task<ActionResult> ClassSearch(string searchTerm)
        {
            // Tìm tất cả các lớp học có tên khóa học chứa từ khóa tìm kiếm
            var classes = await _context.Classes
                .Include(c => c.Course) // Giả sử bạn có mối quan hệ với bảng Course
                .Include(c => c.Instructor)
                .Where(c => c.Course.Name.Contains(searchTerm))
                .ToListAsync();

            return View(classes);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> admin_class_delete(int classId)
        {
            // Kiểm tra classId hợp lệ
            if (classId <= 0)
            {
                ViewBag.Message = "Invalid class ID.";
                return View("Error");
            }

            // Tìm lớp theo ID
            var classToDelete = await _context.Classes.FindAsync(classId);

            // Kiểm tra xem lớp có tồn tại không
            if (classToDelete == null)
            {
                ViewBag.Message = "Class not found.";
                return View("Error");
            }

            // Cập nhật tất cả CustomerInformation có ClassesId trùng với classId
            var customerInformations = await _context.CustomerInformations
                .Where(ci => ci.ClassesId == classId)
                .ToListAsync();

            foreach (var customerInfo in customerInformations)
            {
                customerInfo.ClassesId = null; // Hoặc bạn có thể xóa bản ghi này nếu cần
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            // Xóa lớp
            _context.Classes.Remove(classToDelete);

            // Lưu thay đổi sau khi xóa lớp
            await _context.SaveChangesAsync();

            // Chuyển hướng về trang danh sách lớp
            return RedirectToAction("admin_class"); // Thay đổi "ClassList" thành tên action bạn muốn chuyển hướng đến
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> admin_class_student(int classesId)
        {
            // Kiểm tra classesId hợp lệ
            if (classesId <= 0)
            {
                ViewBag.Message = "Invalid class ID.";
                return View("Error");
            }

            // Lấy CourseId tương ứng với classesId
            var courseId = await _context.Classes
                .Where(c => c.ClassesId == classesId)
                .Select(c => c.CourseId) // Giả sử bạn có CourseId trong bảng Classes
                .FirstOrDefaultAsync();

            // Nếu không tìm thấy courseId, trả về thông báo lỗi
            if (courseId == null)
            {
                ViewBag.Message = "No course found for this class.";
                return View("Error");
            }

            // Lấy thông tin khách hàng có CourseId tương ứng
            var customerInformations = await _context.CustomerInformations
                .Include(ci => ci.Class) // Load thông tin lớp học liên quan
                .Include(ci => ci.Course) // Load thông tin khóa học
                .Where(ci => ci.ClassesId == classesId && ci.Mark > 40) // Lọc theo CourseId
                .ToListAsync();

            // Kiểm tra xem có thông tin khách hàng nào không
            if (customerInformations == null || !customerInformations.Any())
            {
                ViewBag.Message = "No customer information found for this course.";
            }

            // Debug log
            

            return View(customerInformations);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> admin_class_student_delete(string id, int classesId) // Thêm tham số classesId
        {
            // Tìm thông tin khách hàng theo ID
            var customerInformation = await _context.CustomerInformations.FindAsync(id);

            // Kiểm tra xem khách hàng có tồn tại không
            if (customerInformation != null)
            {
                // Chỉ xóa ClassesId
                customerInformation.ClassesId = null;

                // Cập nhật thông tin khách hàng
                _context.CustomerInformations.Update(customerInformation);
                await _context.SaveChangesAsync(); // Lưu thay đổi
            }

            // Chuyển hướng về trang admin_class_student với classesId
            return RedirectToAction("admin_class_student", new { classesId = classesId }); // Chuyển hướng với tham số classesId
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
        public async Task<IActionResult> admin_class_add(Class newClass)
        {
            
                newClass.CreatedAt = DateTime.UtcNow; // Gán thời gian tạo

                // Thêm lớp mới vào cơ sở dữ liệu
                await _context.Classes.AddAsync(newClass);
                await _context.SaveChangesAsync(); // Lưu thay đổi để có ClassesId

                // Cập nhật ClassesId cho tất cả CustomerInformation có cùng CourseId
                var customerInformations = await _context.CustomerInformations
                    .Where(ci => ci.CourseId == newClass.CourseId)
                    .ToListAsync();

                foreach (var customerInfo in customerInformations)
                {
                    customerInfo.ClassesId = newClass.ClassesId; // Cập nhật ClassesId
                }

                // Lưu thay đổi cho CustomerInformation
                await _context.SaveChangesAsync();

                return RedirectToAction("admin_class"); // Chuyển hướng đến danh sách lớp
            

            
        }
    

        [Authorize]
        // GET: admin_class_edit
        public async Task<IActionResult> admin_class_edit(int id)
        {
            // Tìm lớp học theo ClassesId
            var classToEdit = await _context.Classes.FindAsync(id);
            if (classToEdit == null)
            {
                return NotFound(); // Trả về lỗi 404 nếu không tìm thấy
            }

            // Lấy danh sách khóa học và giảng viên để hiển thị trong dropdown
            ViewBag.Courses = await _context.Courses.ToListAsync();
            ViewBag.Instructors = await _context.Instructors.ToListAsync();

            return View(classToEdit); // Trả về view với thông tin lớp học để chỉnh sửa
        }

        [Authorize]
        // POST: admin_class_edit
        [HttpPost]
        public async Task<IActionResult> admin_class_edit(Class updatedClass)
        {
            
                // Tìm lớp học hiện tại trong cơ sở dữ liệu
                var existingClass = await _context.Classes.FindAsync(updatedClass.ClassesId); // Sử dụng ClassesId
                if (existingClass == null)
                {
                    return NotFound(); // Trả về lỗi 404 nếu không tìm thấy
                }

                // Cập nhật các trường thông tin
                existingClass.CourseId = updatedClass.CourseId; // Cập nhật khóa học
                existingClass.InstructorId = updatedClass.InstructorId; // Cập nhật giảng viên
                existingClass.Description = updatedClass.Description; // Cập nhật mô tả lớp
                existingClass.UpdatedAt = DateTime.UtcNow; // Cập nhật thời gian

                await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu
                return RedirectToAction("admin_class"); // Chuyển hướng đến danh sách lớp
            

            
        }


        [Authorize]
        public async Task<ActionResult> admin_instructor()
        {
            var instructors = await _context.Instructors.ToListAsync();
            return View(instructors);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> InstructorSearch(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return View(new List<Instructor>()); // Trả về danh sách rỗng nếu không có từ khóa tìm kiếm
            }

            // Tìm kiếm trong CustomerInformation theo FullName hoặc Email
            var instructor = await _context.Instructors
                .Where(ci => ci.Name.Contains(searchTerm) || ci.Email.Contains(searchTerm))
                .ToListAsync();

            return View(instructor); // Trả về view với kết quả tìm kiếm
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
        [HttpPost]
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
            var customerInformation = await _context.CustomerInformations
                .Include(c => c.Course) // Load dữ liệu Course liên quan
                .ToListAsync();

            return View(customerInformation);
        }


        [Authorize]
        [HttpGet]
        public async Task<ActionResult> admin_student_view(string id) // Đổi type của id thành string nếu CustomerInformationId là string
        {
            // Tìm thông tin khách hàng theo ID
            var customerInformation = await _context.CustomerInformations
                .Include(ci => ci.Class) // Nếu cần thông tin về lớp, bao gồm dữ liệu liên quan
                .Include(ci => ci.Course) // Nếu cần thông tin về khóa học, bao gồm dữ liệu liên quan
                .FirstOrDefaultAsync(ci => ci.CustomerInformationId == id); // Sử dụng FirstOrDefaultAsync để tìm theo ID

            if (customerInformation == null)
            {
                return NotFound(); // Nếu không tìm thấy thông tin khách hàng
            }

            return View(customerInformation); // Trả về view với thông tin khách hàng
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> admin_student_search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return View(new List<CustomerInformation>()); // Trả về danh sách rỗng nếu không có từ khóa tìm kiếm
            }

            // Tìm kiếm trong CustomerInformation theo FullName hoặc Email
            var customerInformations = await _context.CustomerInformations
                .Where(ci => ci.FullName.Contains(searchTerm) || ci.Email.Contains(searchTerm) || ci.CustomerInformationId.Contains(searchTerm))
                .ToListAsync();

            return View(customerInformations); // Trả về view với kết quả tìm kiếm
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> admin_student_add()
        {
            // Lấy danh sách khóa học từ cơ sở dữ liệu
            var courses = await _context.Courses.ToListAsync();
            ViewBag.Courses = courses; // Gán danh sách khóa học cho ViewBag

            return View(new CustomerInformation()); // Truyền model rỗng để tránh lỗi null
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> admin_student_add(CustomerInformation customerInformation, string Schedule)
        {
            // Kiểm tra nếu Schedule chưa được chọn
            if (string.IsNullOrEmpty(Schedule))
            {
                ViewBag.Message = "Please select a schedule!";
                ViewBag.Courses = await _context.Courses.ToListAsync(); // Lấy danh sách khóa học để hiển thị lại
                return View(customerInformation);
            }

            // Kiểm tra xem người dùng đã tồn tại chưa
            bool userExists = await _context.CustomerInformations.AnyAsync(c =>
                c.Email == customerInformation.Email &&
                c.PhoneNumber == customerInformation.PhoneNumber &&
                c.FullName == customerInformation.FullName);

            if (userExists)
            {
                ViewBag.Message = "Users already exist!";
                ViewBag.Courses = await _context.Courses.ToListAsync(); // Lấy danh sách khóa học để hiển thị lại
                return View(customerInformation);
            }

            // Gán ID duy nhất cho sinh viên mới
            customerInformation.CustomerInformationId = await GenerateUniqueCustomerInformationIdAsync();

            // Gán giá trị cho các trường bắt buộc
            customerInformation.Schedule = Schedule;
            customerInformation.Status = "Unattempted"; // Giá trị mặc định cho Status
            customerInformation.CreatedAt = DateTime.UtcNow; // Thời gian tạo mới

            // Thêm sinh viên mới vào cơ sở dữ liệu
            await _context.CustomerInformations.AddAsync(customerInformation);
            await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu

            // So sánh CourseId và cập nhật ClassesId
            var matchingClasses = await _context.Classes
                .Where(cls => cls.CourseId == customerInformation.CourseId) // Giả sử CustomerInformation có thuộc tính CourseId
                .ToListAsync();

            // Nếu tìm thấy lớp học trùng khớp, cập nhật ClassesId cho CustomerInformation
            if (matchingClasses.Any())
            {
                // Chọn ClassesId của lớp đầu tiên tìm thấy (có thể tùy chỉnh theo nhu cầu)
                customerInformation.ClassesId = matchingClasses.First().ClassesId;

                // Cập nhật CustomerInformation đã thêm với ClassesId
                _context.CustomerInformations.Update(customerInformation);
                await _context.SaveChangesAsync(); // Lưu thay đổi
            }

            TempData["SuccessMessage"] = "More new students succeed!"; // Lưu thông báo thành công
            return RedirectToAction("adminstudent"); // Chuyển hướng đến danh sách sinh viên
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
        [Authorize]
        [HttpGet]
        public ActionResult AdminStudentDeleteConfirmation(string id)
        {
            var customerInformation = _context.CustomerInformations.Find(id);
            if (customerInformation == null)
            {
                return NotFound();
            }
            return View(customerInformation); // Trả về view xác nhận
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> AdminStudentDelete(string id)
        {
            var customerInformation = await _context.CustomerInformations.FindAsync(id);
            if (customerInformation != null)
            {
                _context.CustomerInformations.Remove(customerInformation);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("AdminStudent");
        }
        
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> adminstudentedit(string id)
        {
            var customerInformation = await _context.CustomerInformations.FindAsync(id);

            if (customerInformation == null)
            {
                return NotFound();
            }

            // Populate ViewBag with courses for the dropdown
            ViewBag.Courses = await _context.Courses.ToListAsync();

            return View(customerInformation);
        }

        [HttpPost]
        public async Task<ActionResult> adminstudentedit(CustomerInformation customerInformations, string Schedule)
        {
            if (string.IsNullOrEmpty(Schedule))
            {
                ViewBag.Message = "Vui lòng chọn ca học!";

                var courses = await _context.Courses.ToListAsync(); // Get the list of courses
                ViewBag.Courses = courses; // Assign the course list to ViewBag

                // Ensure that we keep the selected CourseId
                ViewBag.SelectedCourseId = customerInformations.CourseId; // Pass the current CourseId back to the view

                return View(customerInformations); // Return the view with the error message
            }

            // Find the current information in the database
            var existingCustomer = await _context.CustomerInformations.FindAsync(customerInformations.CustomerInformationId);
            if (existingCustomer == null)
            {
                return NotFound(); // Return a 404 error if the information is not found
            }

            // Update the fields with the new information
            existingCustomer.FullName = customerInformations.FullName;
            existingCustomer.Email = customerInformations.Email;
            existingCustomer.PhoneNumber = customerInformations.PhoneNumber;
            existingCustomer.Mark = customerInformations.Mark; // Update mark if provided in the model
            existingCustomer.Status = customerInformations.Status; // Update status
            existingCustomer.Schedule = customerInformations.Schedule; // Update schedule
            

            await _context.SaveChangesAsync(); // Save changes to the database

            return RedirectToAction("adminstudent"); // Redirect to the student list
        }




        [Authorize]
        public async Task<IActionResult> admin_feedback()
        {
            var feedbacks = await _context.Feedbacks
            .FromSqlRaw("SELECT * FROM Feedbacks")
            .ToListAsync();
            return View(feedbacks);
        }

        [Authorize]
        public async Task<IActionResult> admin_feedback_view(int id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                return NotFound(); // Nếu không tìm thấy phản hồi
            }
            return View(feedback); // Trả về view với dữ liệu phản hồi
        }


        [Authorize]
        public IActionResult admin()
        {
            // Lấy tên người dùng từ claims
            var username = User.Identity.Name;

            // Tìm kiếm thông tin tài khoản trong cơ sở dữ liệu
            var account = _context.Accounts.FirstOrDefault(a => a.Username == username);

            // Lấy dữ liệu từ bảng CustomerInformation
            var customerResults = _context.CustomerInformations.ToList();

            // Tính toán số lượng Pass và Fail cho các tháng
            var passCounts = new int[12]; // Mảng cho mỗi tháng
            var failCounts = new int[12];
            var newRegistrations = new int[12]; // Dữ liệu cho biểu đồ đường

            foreach (var customer in customerResults)
            {
                int month = customer.CreatedAt.Month - 1; // Tháng 0-11

                if (customer.Mark >= 50) // Giả sử điểm Pass >= 50
                {
                    passCounts[month]++;
                }
                else
                {
                    failCounts[month]++;
                }

                // Tính số lượng đăng ký mới theo tháng
                newRegistrations[month]++;
            }

            // Tạo mảng tháng
            var months = new[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

            // Lấy top 5 học sinh có điểm cao nhất
            var topStudents = _context.CustomerInformations
                .Where(c => c.Mark != null)
                .OrderByDescending(c => c.Mark)
                .Take(5)
                .Select(c => new StudentViewModel
                {
                    Name = c.FullName,
                    Score = c.Mark
                })
                .ToList();

            // Tạo mô hình để truyền dữ liệu vào view
            var viewModel = new
            {
                Months = months,
                PassCounts = passCounts,
                FailCounts = failCounts,
                NewRegistrations = newRegistrations,
                Account = account, // Thêm thông tin tài khoản
                TopStudents = topStudents // Thêm top 5 học sinh
            };

            return View(viewModel); // Trả về view cùng với dữ liệu
        }


        // [HttpPost]
        // public async Task<IActionResult> submit_feedback([FromForm] Feedbacks feedback)
        // {
        //     if (ModelState.IsValid)
        //     {
        //         feedback.CreatedAt = DateTime.UtcNow;

        //         // Lưu phản hồi vào cơ sở dữ liệu
        //         await _context.Feedbacks.AddAsync(feedback);
        //         await _context.SaveChangesAsync();

        //         // Giữ người dùng lại trên trang với thông báo thành công
        //         ViewBag.Message = "Thank you for your feedback!";
        //         return View("Contact"); // Hiển thị lại trang contact
        //     }

        //     // Nếu có lỗi, vẫn giữ người dùng ở lại trang với thông báo lỗi
        //     return View("Contact");
        // }

    }
}
