using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler.Serializer;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace WebApp
{
    public class BlockedUserMessageHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var manager = request.GetDependencyScope().GetService(typeof(ApplicationUserManager)) as ApplicationUserManager;
            var accessTokenFormat = request.GetDependencyScope().GetService(typeof(ISecureDataFormat<AuthenticationTicket>)) as ISecureDataFormat<AuthenticationTicket>;

            var token = request.Headers.Authorization.ToString().Replace("Bearer ", "");
            AuthenticationTicket ticket = accessTokenFormat.Unprotect(token);
           
            if (ticket!=null && ticket.Identity != null) 
            {
                var userId = ticket.Identity.GetUserId();
                var user = manager.FindById(userId);
                if (user.IsBlocked)
                {
                    var response = request.CreateResponse(HttpStatusCode.Unauthorized, "User is blocked!", request.Content.Headers.ContentType);
                    var tsc = new TaskCompletionSource<HttpResponseMessage>();
                    tsc.SetResult(response);
                    return tsc.Task;
                }
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}