using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Text;

namespace Expense_Tracker_App.Extensions
{
    public static class ViewExtensions
    {
        public static async Task<string> RenderViewAsync<TModel>(
            Controller controller,
            string viewPath,
            TModel model,
            bool partial = false)
        {
            controller.ViewData.Model = model;

            using var sw = new StringWriter();
            var engine = controller.HttpContext.RequestServices.GetService(typeof(IRazorViewEngine)) as IRazorViewEngine;
            var tempDataProvider = controller.HttpContext.RequestServices.GetService(typeof(ITempDataProvider)) as ITempDataProvider;
            var actionContext = new ActionContext(controller.HttpContext, controller.RouteData, controller.ControllerContext.ActionDescriptor);

            var viewResult = engine.GetView(null, viewPath, !partial);
            // Nếu không tìm thấy, thử tìm với đường dẫn tương đối
            if (!viewResult.Success)
            {
                // Thử tìm trong thư mục Views
                viewResult = engine.FindView(actionContext, $"~/Views/{viewPath}", !partial);
            }

            // Nếu vẫn không tìm thấy, thử tìm với các đường dẫn khác
            if (!viewResult.Success)
            {
                // Thử tìm trong thư mục Shared
                viewResult = engine.FindView(actionContext, $"~/Views/Shared/{viewPath}", !partial);
            }

            if (!viewResult.Success)
            {
                var searchedLocations = string.Join("\n", viewResult.SearchedLocations);
                throw new InvalidOperationException($"Không tìm thấy view: {viewPath}\nĐã tìm kiếm tại:\n{searchedLocations}");
            }

            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                controller.ViewData,
                controller.TempData,
                sw,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }
    }
}