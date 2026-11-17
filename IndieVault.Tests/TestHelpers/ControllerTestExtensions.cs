using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace IndieVault.Tests.TestHelpers
{
    public static class ControllerTestExtensions
    {
        // Gives a controller a working TempData backed by an in-memory dictionary.
        public static T WithTempData<T>(this T controller) where T : Controller
        {
            controller.TempData = new TempDataDictionary(controller.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());
            return controller;
        }
    }
}
