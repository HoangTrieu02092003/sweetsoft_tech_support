using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Customer_sweetsoft_tech_support.Models;
using System.Security.Claims;
using Newtonsoft.Json;

namespace Customer_sweetsoft_tech_support.Controllers
{
    public class TblSupportRequestsController : Controller
    {
        private readonly RequestContext _context;
        private readonly IConfiguration _configuration;

        public TblSupportRequestsController(RequestContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private async Task<bool> Validate(string secretKey, string recaptchaResponse)
        {
            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("secret", secretKey),
                    new KeyValuePair<string, string>("response", recaptchaResponse)
                });

                var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
                var responseString = await response.Content.ReadAsStringAsync();

                dynamic jsonResponse = JsonConvert.DeserializeObject(responseString);
                return jsonResponse.success == "true"; // Kiểm tra xem reCAPTCHA có hợp lệ không
            }
        }

        // GET: TblSupportRequests/Create
        public IActionResult Create()
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ReturnUrl"] = Url.RouteUrl("addRequest");
                return RedirectToAction("Login", "Custommer");
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var customer = _context.TblCustomers.FirstOrDefault(c => c.CustomerId == int.Parse(userId));
            var departments = _context.TblDepartments.ToList();
            var siteKey = _configuration["ReCaptcha:SiteKey"];
            ViewBag.SiteKey = siteKey;
            // Truyền dữ liệu vàoViewData
            ViewData["Customer"] = customer;
            ViewData["Department"] = new SelectList(departments, "DepartmentId", "DepartmentName");
            return View();
        }

        // POST: TblSupportRequests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RequestId,CustomerId,DepartmentId,RequestTitle,Product,RequestDetails,Status,IsDelete,CreatedAt,ResolvedAt")] TblSupportRequest tblSupportRequest)
        {
            var siteKey = _configuration["ReCaptcha:SiteKey"];
            var recaptchaSecretKey = _configuration["ReCaptcha:SecretKey"];
            var recaptchaResponseValue = Request.Form["g-recaptcha-response"];
            var isCaptchaValid = await Validate(recaptchaSecretKey, recaptchaResponseValue);

            if (!isCaptchaValid)
            {
                ModelState.AddModelError("", "Mã xác thực không hợp lệ.");
                ViewBag.SiteKey = siteKey;
                return View();
            }
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var customer = _context.TblCustomers.FirstOrDefault(c => c.CustomerId == userId);
            var departments = _context.TblDepartments.ToList();
            if (ModelState.IsValid)
            {
                var requestTitles = Request.Form["RequestTitle[]"];
                var requestDetails = Request.Form["RequestDetails[]"];
                var departmentIds = Request.Form["DepartmentId[]"];

                for (int i = 0; i < requestTitles.Count; i++)
                {
                    var supportRequest = new TblSupportRequest
                    {
                        CustomerId = customer.CustomerId,
                        DepartmentId = int.Parse(departmentIds[i]),
                        RequestTitle = requestTitles[i],
                        RequestDetails = requestDetails[i],
                        Product = tblSupportRequest.Product, // Giữ thông tin sản phẩm từ yêu cầu ban đầu
                        Status = 0,
                        IsDelete = false,
                        CreatedAt = DateTime.Now,
                        ResolvedAt = null,
                    };
                    var departmentManager = _context.TblUsers
                    .FirstOrDefault(u => u.DepartmentId == int.Parse(departmentIds[i]) && u.Role.RoleName == "Trưởng phòng");

                    _context.Add(supportRequest);
                    await _context.SaveChangesAsync();

                    var logRequest = new
                    {
                        User = departmentManager.UserId.ToString(),
                        Content = "Có yêu cầu mới",
                        Status = "0", // 0: chưa xem, 1 đã xem
                        Id = GenerateUniqueId(),
                        Timestamp = DateTime.Now
                    };

                    string adminApiUrl = "http://tech.runasp.net/api/log/write-log";
                    using (var client = new HttpClient())
                    {
                        var response = await client.PostAsJsonAsync(adminApiUrl, logRequest);
                        if (!response.IsSuccessStatusCode)
                        {
                            TempData["ErrorMessage"] = "Gửi thông báo thất bại";
                        }
                        else
                        {
                            TempData["SuccessMessage"] = "Gửi thông báo thành công!";
                        }
                    }

                    var requestProcessing = new TblRequestsProcessing
                    {
                        RequestId = supportRequest.RequestId, // Lấy ID của TblSupportRequest vừa lưu
                        DepartmentId = int.Parse(departmentIds[i]),
                        IsCompleted = 0, // Đánh dấu là chưa xử lý
                        ProcessedAt = DateTime.Now,
                        Note = "Đang xử lý...."
                    };

                    _context.TblRequestsProcessings.Add(requestProcessing);
                    
                }
                await _context.SaveChangesAsync();
                
                

                //var logService = new LogNotificationService();
                //logService.LogNotificationAction(departmentManager?.FullName??"Khách hàng", "Khách hàng tạo yêu cầu mới");
                TempData["success"] = "thành công";
                
                return RedirectToAction(nameof(Index),controllerName: "TblRequestsProcessings");
            }
            ViewData["Customer"] = customer;
            ViewData["Department"] = new SelectList(departments, "DepartmentId", "DepartmentName",tblSupportRequest.DepartmentId);
            TempData["error"] = "Tạo yêu cầu thất bại";
            return View(tblSupportRequest);
        }

        public string GenerateUniqueId()
        {
            // Lấy thời gian hiện tại (UTC) tính theo mili giây
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");

            // Tạo một giá trị ngẫu nhiên từ 1000 đến 9999 để đảm bảo tính duy nhất
            var randomValue = new Random().Next(1000, 9999);

            // Kết hợp thời gian và giá trị ngẫu nhiên thành ID
            var uniqueId = $"{timestamp}{randomValue}";

            return uniqueId;
        }
    }
}
