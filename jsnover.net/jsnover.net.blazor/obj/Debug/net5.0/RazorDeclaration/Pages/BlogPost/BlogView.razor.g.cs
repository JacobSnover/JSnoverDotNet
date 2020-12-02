// <auto-generated/>
#pragma warning disable 1591
#pragma warning disable 0414
#pragma warning disable 0649
#pragma warning disable 0169

namespace jsnover.net.blazor.Pages.BlogPost
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
#nullable restore
#line 2 "C:\VS\jsnover.net\jsnover.net.blazor\Pages\BlogPost\BlogView.razor"
using DataTransferObjects.BlogModels;

#line default
#line hidden
#nullable disable
#nullable restore
#line 3 "C:\VS\jsnover.net\jsnover.net.blazor\Pages\BlogPost\BlogView.razor"
using Infrastructure.Services;

#line default
#line hidden
#nullable disable
    [Microsoft.AspNetCore.Components.RouteAttribute("/blogview/{id:int}")]
    public partial class BlogView : Microsoft.AspNetCore.Components.ComponentBase
    {
        #pragma warning disable 1998
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
        }
        #pragma warning restore 1998
#nullable restore
#line 96 "C:\VS\jsnover.net\jsnover.net.blazor\Pages\BlogPost\BlogView.razor"
      
    private BlogCommentModel commentor = new BlogCommentModel();
    private EditContext editContext;
    private string uploadError = string.Empty;
    private string uploadSuccess = string.Empty;
    private string saving = string.Empty;
    private bool liked = false;
    private string likeUnlike = "Like";
    private bool viewed = false;
    private bool visit;

    [Parameter]
    public int id { get; set; }

    protected override void OnInitialized()
    {
        if(BlogViewModel.blog is null)
        {
            BlogViewModel.blog = new BlogDisplayModel() { Body = "Loading" };
        }
        editContext = new EditContext(commentor);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        visit = await sessionStorage.GetItemAsync<bool>("countedVisit");
        viewed = await sessionStorage.GetItemAsync<bool>("BlogViewCounted");
        var blogId = await sessionStorage.GetItemAsync<int>("ViewedBlogId");

        if (firstRender)
        {
            BlogViewModel.blog = await BlogService.GetBlog(id);

            this.StateHasChanged();
        }
        else
        {
            visit = await sessionStorage.GetItemAsync<bool>("countedVisit");
            viewed = await sessionStorage.GetItemAsync<bool>("BlogViewCounted");
        }

        // check if this blog has been viewed
        if (!viewed && blogId != BlogViewModel.blog.BlogId)
        {
            await sessionStorage.SetItemAsync("BlogViewCounted", true);
            await sessionStorage.SetItemAsync("ViewedBlogId", BlogViewModel.blog.BlogId);
            await BlogService.CountBlogView(BlogViewModel.blog.BlogId);
        }
        else if (viewed && blogId != BlogViewModel.blog.BlogId)
        {
            await sessionStorage.SetItemAsync("ViewedBlogId", BlogViewModel.blog.BlogId);
            await BlogService.CountBlogView(BlogViewModel.blog.BlogId);
        }

        if (!visit)
        {
            await sessionStorage.SetItemAsync("countedVisit", true);
            await BlogService.CountVisitor();
        }
    }

    private async Task HandleValidSubmit()
    {
        var isValid = editContext.Validate();

        if (isValid)
        {
            saving = "Saving . . .";
            commentor.BlogId = BlogViewModel.blog.BlogId;
            commentor.Liked = liked;
            var uploaded = await BlogService.SubmitComment(commentor);
            if (uploaded)
            {
                await BlogService.NotifySnoverAboutComment(commentor, BlogViewModel.blog);
                saving = string.Empty;
                commentor = new BlogCommentModel();
                uploadError = string.Empty;
                uploadSuccess = "Your comment has been saved! Thank you for reaching out to us";
                this.StateHasChanged();
            }
            else
            {
                saving = string.Empty;
                uploadSuccess = string.Empty;
                uploadError = "There was an ERROR when saving, if problem continues contact site, or try again later. We greatly apologize and truly appreciate your time today";
                this.StateHasChanged();
            }

        }
    }

    private void IsLiked()
    {
        if (liked)
        {
            likeUnlike = "Like";
            liked = false;
        }
        else
        {
            likeUnlike = "Unlike";
            liked = true;
        }
    }

    private void BlogSearch(string type, string parameter)
    {
        nav.NavigateTo($"blogs/{type}/{parameter}");
    }

#line default
#line hidden
#nullable disable
        [global::Microsoft.AspNetCore.Components.InjectAttribute] private NavigationManager nav { get; set; }
        [global::Microsoft.AspNetCore.Components.InjectAttribute] private Blazored.SessionStorage.ISessionStorageService sessionStorage { get; set; }
        [global::Microsoft.AspNetCore.Components.InjectAttribute] private Infrastructure.Services.BlogService BlogService { get; set; }
        [global::Microsoft.AspNetCore.Components.InjectAttribute] private DataTransferObjects.BlogModels.BlogListModel BlogModels { get; set; }
        [global::Microsoft.AspNetCore.Components.InjectAttribute] private DataTransferObjects.BlogModels.BlogViewModel BlogViewModel { get; set; }
    }
}
#pragma warning restore 1591