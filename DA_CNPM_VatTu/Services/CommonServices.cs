namespace DA_CNPM_VatTu.Services
{
    public static class CommonServices
    {
        private static IWebHostEnvironment _hostingEnvironment;
        public static IHttpContextAccessor _httpContextAccessor;

        public static void Configure(IWebHostEnvironment hostingEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _hostingEnvironment = hostingEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public static string ConvertImageToBase64(IWebHostEnvironment h, string imagePath)
        {
            byte[] imageBytes = System.IO.File.ReadAllBytes(h.WebRootPath + imagePath);
            string base64String = Convert.ToBase64String(imageBytes);

            return base64String;
        }
    }
}
