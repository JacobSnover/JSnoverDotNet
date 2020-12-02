#pragma checksum "C:\VS\jsnover.net\jsnover.net.blazor\Pages\Index.razor" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "8cc6724e3db621a76280759e497a70ff376e0f42"
// <auto-generated/>
#pragma warning disable 1591
namespace jsnover.net.blazor.Pages
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
#nullable restore
#line 1 "C:\VS\jsnover.net\jsnover.net.blazor\_Imports.razor"
using System.Net.Http;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "C:\VS\jsnover.net\jsnover.net.blazor\_Imports.razor"
using Microsoft.AspNetCore.Authorization;

#line default
#line hidden
#nullable disable
#nullable restore
#line 3 "C:\VS\jsnover.net\jsnover.net.blazor\_Imports.razor"
using Microsoft.AspNetCore.Components.Authorization;

#line default
#line hidden
#nullable disable
#nullable restore
#line 4 "C:\VS\jsnover.net\jsnover.net.blazor\_Imports.razor"
using Microsoft.AspNetCore.Components.Forms;

#line default
#line hidden
#nullable disable
#nullable restore
#line 5 "C:\VS\jsnover.net\jsnover.net.blazor\_Imports.razor"
using Microsoft.AspNetCore.Components.Routing;

#line default
#line hidden
#nullable disable
#nullable restore
#line 6 "C:\VS\jsnover.net\jsnover.net.blazor\_Imports.razor"
using Microsoft.AspNetCore.Components.Web;

#line default
#line hidden
#nullable disable
#nullable restore
#line 7 "C:\VS\jsnover.net\jsnover.net.blazor\_Imports.razor"
using Microsoft.JSInterop;

#line default
#line hidden
#nullable disable
#nullable restore
#line 8 "C:\VS\jsnover.net\jsnover.net.blazor\_Imports.razor"
using jsnover.net.blazor;

#line default
#line hidden
#nullable disable
#nullable restore
#line 9 "C:\VS\jsnover.net\jsnover.net.blazor\_Imports.razor"
using jsnover.net.blazor.Shared;

#line default
#line hidden
#nullable disable
    [Microsoft.AspNetCore.Components.RouteAttribute("/")]
    public partial class Index : Microsoft.AspNetCore.Components.ComponentBase
    {
        #pragma warning disable 1998
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
            __builder.AddMarkupContent(0, @"<div class=""jumbotron translucent-text-box shadow-border jsno-pad-20 background-color-darkcyan""><h1>JSNOVER.NET</h1>
    <p><h4>
            Welcome to jsnover.net! This is my idea of a portfolio, but it is also much more then that.
            Please check out some of my projects and try them out for yourself! Let's open up a discussion
            about what interests you in Computer Science, and if you enjoy teaching as well then we can discuss that.
            The community around this field is amazing and my goal is to help that community become the best it can be.
        </h4></p></div>


");
            __builder.AddMarkupContent(1, "<div class=\"row\" style=\"margin-bottom:20px\"><div class=\"col-md-4 translucent-text-box margin-bttm-20\"><div class=\"homePTag rounded shadow-border pad-20\"><h2>As an Engineer</h2>\r\n            <p>\r\n                I have been learning as much as I can about Computer Science. First taking a .NET C# boot camp in Detroit, MI,\r\n                And using my prevoius teaching experience I  returned years later to teach the same course for the Grand Circus bootcamp. \r\n                I have been working on open source projects, as well as personal projects\r\n                such as this site and others like it, so that I can keep pushing the limits of my knowledge. I have a full time position as a software engineer, \r\n                reach out for consultation or a possible business oppurtunity.\r\n            </p></div></div>\r\n    <div class=\"col-md-4 translucent-text-box margin-bttm-20\"><div class=\"homePTag rounded shadow-border pad-20\"><h2>As a Musician</h2>\r\n            <p>\r\n                I have been a percussionist for 20 years. I played in Extreme Metal groups, Church groups, Rock groups, Jazz bands, and even\r\n                Hip-Hop. I was not only a performing artist, but also a percussion instructor for 5 years. I taught from ages 6 and up,\r\n                and through introspection I learned a lot about patience and dedication during those years. \r\n                I still practice and keep up on current music, and would consider any contract gigs as a percussionist.\r\n            </p></div></div>\r\n    <div class=\"col-md-4 translucent-text-box margin-bttm-20\"><div class=\"homePTag rounded shadow-border pad-20\"><h2>Next Steps</h2>\r\n            <p>\r\n                I have been living in Detroit for 5 years at this point and have started to enjoy calling it home. I come from\r\n                a diverse part of the country not just in race but also class, and that gave me a lot of perspective on the world, something I didn\'t notice as much\r\n                while living there. This became very apparent once moving to Detroit though, as I noticed a lot of the social tensions here\r\n                as there were in my hometown. I won\'t say that this is comforting, but I do find it feels very familiar to me. I have been doing\r\n                my best to learn about the local community, and have been enjoying meeting locals while being a part of a city on the rise.\r\n            </p></div></div></div>\r\n\r\n");
            __builder.OpenComponent<jsnover.net.blazor.Components.Footer>(2);
            __builder.CloseComponent();
        }
        #pragma warning restore 1998
#nullable restore
#line 58 "C:\VS\jsnover.net\jsnover.net.blazor\Pages\Index.razor"
       
    private bool visit;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        visit = await sessionStorage.GetItemAsync<bool>("countedVisit");
        if(firstRender)
        {

        }
        else
        {
            visit = await sessionStorage.GetItemAsync<bool>("countedVisit");
        }

        if(!visit)
        {
            await sessionStorage.SetItemAsync("countedVisit", true);
            await ToolService.CountVisitor();
        }
    }

#line default
#line hidden
#nullable disable
        [global::Microsoft.AspNetCore.Components.InjectAttribute] private jsnover.net.blazor.Infrastructure.Services.ToolService ToolService { get; set; }
        [global::Microsoft.AspNetCore.Components.InjectAttribute] private Blazored.SessionStorage.ISessionStorageService sessionStorage { get; set; }
    }
}
#pragma warning restore 1591
