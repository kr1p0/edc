using endeCryptOnline.Models;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();



//For Ajax
builder.Services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");

//For Authentication
builder.Services.AddAuthentication("Ciastko").AddCookie("Ciastko", options =>
{
    options.Cookie.Name = "Ciastko";
    options.LoginPath = "/Login";
    //options.AccessDeniedPath =
    //options.ExpireTimeSpan = TimeSpan.FromSeconds(10); //closing the browser deletes the cookie

});


//for maximal upload file + web.config file
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = int.MaxValue;
});

builder.Services.Configure<FormOptions>(o =>
{
    o.ValueLengthLimit = int.MaxValue;
    o.MultipartBodyLengthLimit = int.MaxValue;
    o.MultipartBoundaryLengthLimit = int.MaxValue;
    o.MultipartHeadersCountLimit = int.MaxValue;
    o.MultipartHeadersLengthLimit = int.MaxValue;
    o.BufferBodyLengthLimit = int.MaxValue;
    o.BufferBody = true;
    o.ValueCountLimit = int.MaxValue;
});


//test
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// test for Razor.RuntimeCompilation
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();


//For background action
builder.Services.AddHostedService<BackgroundProcess>();


var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}




//test
//Because of icrosoft.AspNetCore.HttpsPolicy.HttpsRedirectionMiddleware => Failed to determine the https port for redirect.
app.UseForwardedHeaders();
//test


//test
/*
app.Use((context, next) =>
{
    context.Request.Scheme = "https";
    return next();
});
*/
//test

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
