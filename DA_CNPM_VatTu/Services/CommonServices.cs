using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;

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
        public static string ConvertViewToString(ControllerContext controllerContext, PartialViewResult pvr, ICompositeViewEngine _viewEngine)
        {
            using (StringWriter writer = new StringWriter())
            {
                ViewEngineResult vResult = _viewEngine.FindView(controllerContext, pvr.ViewName, false);
                ViewContext viewContext = new ViewContext(controllerContext, vResult.View, pvr.ViewData, pvr.TempData, writer, new HtmlHelperOptions());

                vResult.View.RenderAsync(viewContext);

                return writer.GetStringBuilder().ToString();
            }
        }
    }
}
