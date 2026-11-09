
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ButterScott.Filters
{
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(
            ActionExecutingContext context)
        {
            var clienteId = context.HttpContext.Session
                .GetString("ClienteId");

            var ehAdmin = context.HttpContext.Session
                .GetString("EhAdmin");

            if (string.IsNullOrEmpty(clienteId))
            {
                context.Result = new RedirectToActionResult(
                    "Login",
                    "Conta",
                    null);

                return;
            }

            if (ehAdmin != "true")
            {
                context.Result = new RedirectToActionResult(
                    "Index",
                    "Home",
                    null);
            }
        }
    }
}
