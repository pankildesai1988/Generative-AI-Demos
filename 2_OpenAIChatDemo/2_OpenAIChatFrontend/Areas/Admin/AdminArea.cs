using Microsoft.AspNetCore.Mvc;

namespace _2_OpenAIChatFrontend.Areas.Admin
{
    public class AdminArea : AreaAttribute
    {
        public AdminArea() : base("Admin") { }
    }
}
