var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddSession();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles(new StaticFileOptions
{
    RequestPath = "/admin",
    ServeUnknownFileTypes = true
});

app.UseStaticFiles();

app.UseRouting();

//app.UseAuthentication();  // ✅ must come before Authorization

//app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=Templates}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();
