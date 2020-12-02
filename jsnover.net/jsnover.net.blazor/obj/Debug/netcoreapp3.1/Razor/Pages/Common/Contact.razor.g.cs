#pragma checksum "C:\VS\jsnover.net\jsnover.net.blazor\Pages\Common\Contact.razor" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "e9d1e057634b43ad5c969be55b2d2d37486a8390"
// <auto-generated/>
#pragma warning disable 1591
namespace jsnover.net.blazor.Pages.Common
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
#line 2 "C:\VS\jsnover.net\jsnover.net.blazor\Pages\Common\Contact.razor"
using Microsoft.Extensions.Logging;

#line default
#line hidden
#nullable disable
    [Microsoft.AspNetCore.Components.RouteAttribute("/contact")]
    public partial class Contact : Microsoft.AspNetCore.Components.ComponentBase
    {
        #pragma warning disable 1998
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
            __builder.OpenElement(0, "div");
            __builder.AddAttribute(1, "class", "pad-25x3");
            __builder.OpenElement(2, "div");
            __builder.AddAttribute(3, "class", "col-lg-12 jsno-background rounded-lg shadow-border");
            __builder.AddMarkupContent(4, "<p><h3>Please reach out if you need to contact us to say hello, or for support. &#128578</h3></p>\r\n        ");
            __builder.AddMarkupContent(5, "<p><h3>\r\n                If you are an advertiser, or a sponsor and would like to reach out for a possible business venture,\r\n                please click the \"Business\" button before submitting your message to us.\r\n            </h3></p>\r\n        ");
            __builder.AddMarkupContent(6, "<p><h3>\r\n                If you have an issue with the website, please click the \"Issue\" button before submitting. Thank you for your help\r\n                with improving our website.\r\n            </h3></p>\r\n\r\n        ");
            __builder.OpenElement(7, "div");
            __builder.AddAttribute(8, "class", "container-fluid");
            __builder.OpenComponent<Microsoft.AspNetCore.Components.Forms.EditForm>(9);
            __builder.AddAttribute(10, "Model", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<System.Object>(
#nullable restore
#line 25 "C:\VS\jsnover.net\jsnover.net.blazor\Pages\Common\Contact.razor"
                             contactRequest

#line default
#line hidden
#nullable disable
            ));
            __builder.AddAttribute(11, "OnValidSubmit", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<Microsoft.AspNetCore.Components.EventCallback<Microsoft.AspNetCore.Components.Forms.EditContext>>(Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Forms.EditContext>(this, 
#nullable restore
#line 25 "C:\VS\jsnover.net\jsnover.net.blazor\Pages\Common\Contact.razor"
                                                            HandleValidSubmit

#line default
#line hidden
#nullable disable
            )));
            __builder.AddAttribute(12, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment<Microsoft.AspNetCore.Components.Forms.EditContext>)((context) => (__builder2) => {
                __builder2.OpenComponent<Microsoft.AspNetCore.Components.Forms.DataAnnotationsValidator>(13);
                __builder2.CloseComponent();
                __builder2.AddMarkupContent(14, "\r\n\r\n                ");
                __builder2.OpenElement(15, "span");
                __builder2.AddAttribute(16, "class", "text-danger comment-message");
                __builder2.OpenElement(17, "strong");
                __builder2.AddContent(18, 
#nullable restore
#line 28 "C:\VS\jsnover.net\jsnover.net.blazor\Pages\Common\Contact.razor"
                                                                   uploadError

#line default
#line hidden
#nullable disable
                );
                __builder2.CloseElement();
                __builder2.CloseElement();
                __builder2.AddMarkupContent(19, "\r\n                ");
                __builder2.OpenElement(20, "span");
                __builder2.AddAttribute(21, "class", "text-success comment-message");
                __builder2.OpenElement(22, "strong");
                __builder2.AddContent(23, 
#nullable restore
#line 29 "C:\VS\jsnover.net\jsnover.net.blazor\Pages\Common\Contact.razor"
                                                                    uploadSuccess

#line default
#line hidden
#nullable disable
                );
                __builder2.CloseElement();
                __builder2.CloseElement();
                __builder2.AddMarkupContent(24, "\r\n\r\n                ");
                __builder2.OpenElement(25, "div");
                __builder2.AddAttribute(26, "class", "form-group border border-dark px-4");
                __builder2.OpenElement(27, "div");
                __builder2.AddAttribute(28, "class", "form-group");
                __builder2.AddMarkupContent(29, "<label class=\"col-form-label\">Name (  Optional  )</label>");
                __Blazor.jsnover.net.blazor.Pages.Common.Contact.TypeInference.CreateValidationMessage_0(__builder2, 30, 31, 
#nullable restore
#line 33 "C:\VS\jsnover.net\jsnover.net.blazor\Pages\Common\Contact.razor"
                                                                                                           () => contactRequest.Name

#line default
#line hidden
#nullable disable
                );
                __builder2.AddMarkupContent(32, "\r\n                        ");
                __builder2.OpenComponent<Microsoft.AspNetCore.Components.Forms.InputText>(33);
                __builder2.AddAttribute(34, "class", "form-control shadow-border-sm");
                __builder2.AddAttribute(35, "Value", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<System.String>(
#nullable restore
#line 34 "C:\VS\jsnover.net\jsnover.net.blazor\Pages\Common\Contact.razor"
                                                contactRequest.Name

#line default
#line hidden
#nullable disable
                ));
                __builder2.AddAttribute(36, "ValueChanged", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<Microsoft.AspNetCore.Components.EventCallback<System.String>>(Microsoft.AspNetCore.Components.EventCallback.Factory.Create<System.String>(this, Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.CreateInferredEventCallback(this, __value => contactRequest.Name = __value, contactRequest.Name))));
                __builder2.AddAttribute(37, "ValueExpression", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<System.Linq.Expressions.Expression<System.Func<System.String>>>(() => contactRequest.Name));
                __builder2.CloseComponent();
                __builder2.CloseElement();
                __builder2.AddMarkupContent(38, "\r\n                    ");
                __builder2.OpenElement(39, "div");
                __builder2.AddAttribute(40, "class", "form-group");
                __builder2.AddMarkupContent(41, "<label class=\"col-form-label\">Email ( Required )</label>");
                __Blazor.jsnover.net.blazor.Pages.Common.Contact.TypeInference.CreateValidationMessage_1(__builder2, 42, 43, 
#nullable restore
#line 37 "C:\VS\jsnover.net\jsnover.net.blazor\Pages\Common\Contact.razor"
                                                                                                        (() => contactRequest.Email)

#line default
#line hidden
#nullable disable
                , 44, 
#nullable restore
#line 37 "C:\VS\jsnover.net\jsnover.net.blazor\Pages\Common\Contact.razor"
                                                                                                                                                            new Dictionary<string, object>() { { "class", "comment-message" } }

#line default
#line hidden
#nullable disable
                );
                __builder2.AddMarkupContent(45, "\r\n                        ");
                __builder2.OpenComponent<Microsoft.AspNetCore.Components.Forms.InputText>(46);
                __builder2.AddAttribute(47, "class", "form-control shadow-border-sm");
                __builder2.AddAttribute(48, "Value", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<System.String>(
#nullable restore
#line 38 "C:\VS\jsnover.net\jsnover.net.blazor\Pages\Common\Contact.razor"
                                                contactRequest.Email

#line default
#line hidden
#nullable disable
                ));
                __builder2.AddAttribute(49, "ValueChanged", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<Microsoft.AspNetCore.Components.EventCallback<System.String>>(Microsoft.AspNetCore.Components.EventCallback.Factory.Create<System.String>(this, Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.CreateInferredEventCallback(this, __value => contactRequest.Email = __value, contactRequest.Email))));
                __builder2.AddAttribute(50, "ValueExpression", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<System.Linq.Expressions.Expression<System.Func<System.String>>>(() => contactRequest.Email));
                __builder2.CloseComponent();
                __builder2.CloseElement();
                __builder2.AddMarkupContent(51, "\r\n                    ");
                __builder2.OpenElement(52, "div");
                __builder2.AddAttribute(53, "class", "form-group");
                __builder2.AddMarkupContent(54, "<label class=\"col-form-label\">Comment ( 500 character limit )</label>");
                __Blazor.jsnover.net.blazor.Pages.Common.Contact.TypeInference.CreateValidationMessage_2(__builder2, 55, 56, 
#nullable restore
#line 41 "C:\VS\jsnover.net\jsnover.net.blazor\Pages\Common\Contact.razor"
                                                                                                                     (() => contactRequest.Body)

#line default
#line hidden
#nullable disable
                );
                __builder2.AddMarkupContent(57, "\r\n                        ");
                __builder2.OpenComponent<Microsoft.AspNetCore.Components.Forms.InputTextArea>(58);
                __builder2.AddAttribute(59, "class", "form-control shadow-border-sm");
                __builder2.AddAttribute(60, "Value", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<System.String>(
#nullable restore
#line 42 "C:\VS\jsnover.net\jsnover.net.blazor\Pages\Common\Contact.razor"
                                                    contactRequest.Body

#line default
#line hidden
#nullable disable
                ));
                __builder2.AddAttribute(61, "ValueChanged", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<Microsoft.AspNetCore.Components.EventCallback<System.String>>(Microsoft.AspNetCore.Components.EventCallback.Factory.Create<System.String>(this, Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.CreateInferredEventCallback(this, __value => contactRequest.Body = __value, contactRequest.Body))));
                __builder2.AddAttribute(62, "ValueExpression", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<System.Linq.Expressions.Expression<System.Func<System.String>>>(() => contactRequest.Body));
                __builder2.CloseComponent();
                __builder2.CloseElement();
                __builder2.AddMarkupContent(63, "\r\n                    ");
                __builder2.OpenElement(64, "div");
                __builder2.AddAttribute(65, "class", "form-group");
                __builder2.OpenElement(66, "button");
                __builder2.AddAttribute(67, "class", "btn btn-info form-control shadow-border-sm");
                __builder2.AddAttribute(68, "onclick", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(this, 
#nullable restore
#line 45 "C:\VS\jsnover.net\jsnover.net.blazor\Pages\Common\Contact.razor"
                                                                                             IsBusiness

#line default
#line hidden
#nullable disable
                ));
                __builder2.AddAttribute(69, "type", "button");
                __builder2.AddContent(70, 
#nullable restore
#line 45 "C:\VS\jsnover.net\jsnover.net.blazor\Pages\Common\Contact.razor"
                                                                                                                        business

#line default
#line hidden
#nullable disable
                );
                __builder2.CloseElement();
                __builder2.AddMarkupContent(71, "\r\n\r\n                        ");
                __builder2.OpenElement(72, "span");
                __builder2.AddAttribute(73, "class", "form-group");
                __builder2.AddAttribute(74, "hidden", 
#nullable restore
#line 47 "C:\VS\jsnover.net\jsnover.net.blazor\Pages\Common\Contact.razor"
                                                           !isBusiness

#line default
#line hidden
#nullable disable
                );
                __builder2.AddMarkupContent(75, "<label class=\"col-form-label\">Comapny Name</label>");
                __Blazor.jsnover.net.blazor.Pages.Common.Contact.TypeInference.CreateValidationMessage_3(__builder2, 76, 77, 
#nullable restore
#line 48 "C:\VS\jsnover.net\jsnover.net.blazor\Pages\Common\Contact.razor"
                                                                                                        () => contactRequest.CompanyName

#line default
#line hidden
#nullable disable
                );
                __builder2.AddMarkupContent(78, "\r\n                            ");
                __builder2.OpenComponent<Microsoft.AspNetCore.Components.Forms.InputText>(79);
                __builder2.AddAttribute(80, "class", "form-control shadow-border-sm");
                __builder2.AddAttribute(81, "Value", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<System.String>(
#nullable restore
#line 49 "C:\VS\jsnover.net\jsnover.net.blazor\Pages\Common\Contact.razor"
                                                    contactRequest.CompanyName

#line default
#line hidden
#nullable disable
                ));
                __builder2.AddAttribute(82, "ValueChanged", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<Microsoft.AspNetCore.Components.EventCallback<System.String>>(Microsoft.AspNetCore.Components.EventCallback.Factory.Create<System.String>(this, Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.CreateInferredEventCallback(this, __value => contactRequest.CompanyName = __value, contactRequest.CompanyName))));
                __builder2.AddAttribute(83, "ValueExpression", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<System.Linq.Expressions.Expression<System.Func<System.String>>>(() => contactRequest.CompanyName));
                __builder2.CloseComponent();
                __builder2.CloseElement();
                __builder2.CloseElement();
                __builder2.AddMarkupContent(84, "\r\n                    ");
                __builder2.OpenElement(85, "div");
                __builder2.AddAttribute(86, "class", "form-group");
                __builder2.OpenElement(87, "button");
                __builder2.AddAttribute(88, "class", "btn btn-warning form-control shadow-border-sm");
                __builder2.AddAttribute(89, "onclick", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(this, 
#nullable restore
#line 53 "C:\VS\jsnover.net\jsnover.net.blazor\Pages\Common\Contact.razor"
                                                                                                IsIssue

#line default
#line hidden
#nullable disable
                ));
                __builder2.AddAttribute(90, "type", "button");
                __builder2.AddContent(91, 
#nullable restore
#line 53 "C:\VS\jsnover.net\jsnover.net.blazor\Pages\Common\Contact.razor"
                                                                                                                        issue

#line default
#line hidden
#nullable disable
                );
                __builder2.CloseElement();
                __builder2.CloseElement();
                __builder2.AddMarkupContent(92, "\r\n                    ");
                __builder2.OpenElement(93, "div");
                __builder2.AddAttribute(94, "class", "form-group");
                __builder2.AddMarkupContent(95, "<button class=\"btn btn-primary form-control shadow-border-sm\" type=\"submit\">Submit</button>\r\n                        ");
                __builder2.OpenElement(96, "span");
                __builder2.OpenElement(97, "strong");
                __builder2.AddContent(98, 
#nullable restore
#line 57 "C:\VS\jsnover.net\jsnover.net.blazor\Pages\Common\Contact.razor"
                                       saving

#line default
#line hidden
#nullable disable
                );
                __builder2.CloseElement();
                __builder2.CloseElement();
                __builder2.CloseElement();
                __builder2.CloseElement();
                __builder2.AddMarkupContent(99, "\r\n                ");
                __builder2.OpenElement(100, "span");
                __builder2.AddAttribute(101, "class", "text-danger comment-message");
                __builder2.OpenElement(102, "strong");
                __builder2.AddContent(103, 
#nullable restore
#line 60 "C:\VS\jsnover.net\jsnover.net.blazor\Pages\Common\Contact.razor"
                                                                   uploadError

#line default
#line hidden
#nullable disable
                );
                __builder2.CloseElement();
                __builder2.CloseElement();
                __builder2.AddMarkupContent(104, "\r\n                ");
                __builder2.OpenElement(105, "span");
                __builder2.AddAttribute(106, "class", "text-success comment-message");
                __builder2.OpenElement(107, "strong");
                __builder2.AddContent(108, 
#nullable restore
#line 61 "C:\VS\jsnover.net\jsnover.net.blazor\Pages\Common\Contact.razor"
                                                                    uploadSuccess

#line default
#line hidden
#nullable disable
                );
                __builder2.CloseElement();
                __builder2.CloseElement();
            }
            ));
            __builder.CloseComponent();
            __builder.CloseElement();
            __builder.CloseElement();
            __builder.CloseElement();
        }
        #pragma warning restore 1998
#nullable restore
#line 69 "C:\VS\jsnover.net\jsnover.net.blazor\Pages\Common\Contact.razor"
      
    private DataTransferObjects.Common.ContactModel contactRequest = new DataTransferObjects.Common.ContactModel();
    private EditContext editContext;
    private string uploadError = string.Empty;
    private string uploadSuccess = string.Empty;
    private string saving = string.Empty;
    private bool isIssue = false;
    private string issue = "Issue";
    private bool isBusiness = false;
    private string business = "Business";


    protected override void OnInitialized()
    {
        editContext = new EditContext(contactRequest);
    }

    private async Task HandleValidSubmit()
    {
        var isValid = editContext.Validate();

        if (isValid)
        {
            saving = "Saving . . .";
            contactRequest.Issue = isIssue;
            contactRequest.Business = isBusiness;
            var uploaded = await Infrastructure.Services.Submit.SubmitContactRequest(contactRequest);
            if (uploaded)
            {
                saving = string.Empty;
                contactRequest = new DataTransferObjects.Common.ContactModel();
                uploadError = string.Empty;
                uploadSuccess = "It has been saved! Thank you for reaching out to us";
                this.StateHasChanged();
            }
            else
            {
                log.LogError("There was an ERROR when saving contact request");
                saving = string.Empty;
                uploadSuccess = string.Empty;
                uploadError = "There was an ERROR when saving, if problem continues please try again later. We greatly apologize and truly appreciate your time today";
                this.StateHasChanged();
            }

        }
    }

    private void IsIssue()
    {
        if (isIssue)
        {
            issue = "Issue";
            isIssue = false;
        }
        else
        {
            issue = "Non Issue";
            isIssue = true;
        }
    }

    private void IsBusiness()
    {
        if (isBusiness)
        {
            business = "Business";
            isBusiness = false;
        }
        else
        {
            business = "Non Business";
            isBusiness = true;
        }
    }

#line default
#line hidden
#nullable disable
        [global::Microsoft.AspNetCore.Components.InjectAttribute] private ILogger<jsnover.net.blazor.DataTransferObjects.Common.ContactModel> log { get; set; }
    }
}
namespace __Blazor.jsnover.net.blazor.Pages.Common.Contact
{
    #line hidden
    internal static class TypeInference
    {
        public static void CreateValidationMessage_0<TValue>(global::Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder, int seq, int __seq0, global::System.Linq.Expressions.Expression<global::System.Func<TValue>> __arg0)
        {
        __builder.OpenComponent<global::Microsoft.AspNetCore.Components.Forms.ValidationMessage<TValue>>(seq);
        __builder.AddAttribute(__seq0, "For", __arg0);
        __builder.CloseComponent();
        }
        public static void CreateValidationMessage_1<TValue>(global::Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder, int seq, int __seq0, global::System.Linq.Expressions.Expression<global::System.Func<TValue>> __arg0, int __seq1, global::System.Collections.Generic.IReadOnlyDictionary<global::System.String, global::System.Object> __arg1)
        {
        __builder.OpenComponent<global::Microsoft.AspNetCore.Components.Forms.ValidationMessage<TValue>>(seq);
        __builder.AddAttribute(__seq0, "For", __arg0);
        __builder.AddAttribute(__seq1, "AdditionalAttributes", __arg1);
        __builder.CloseComponent();
        }
        public static void CreateValidationMessage_2<TValue>(global::Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder, int seq, int __seq0, global::System.Linq.Expressions.Expression<global::System.Func<TValue>> __arg0)
        {
        __builder.OpenComponent<global::Microsoft.AspNetCore.Components.Forms.ValidationMessage<TValue>>(seq);
        __builder.AddAttribute(__seq0, "For", __arg0);
        __builder.CloseComponent();
        }
        public static void CreateValidationMessage_3<TValue>(global::Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder, int seq, int __seq0, global::System.Linq.Expressions.Expression<global::System.Func<TValue>> __arg0)
        {
        __builder.OpenComponent<global::Microsoft.AspNetCore.Components.Forms.ValidationMessage<TValue>>(seq);
        __builder.AddAttribute(__seq0, "For", __arg0);
        __builder.CloseComponent();
        }
    }
}
#pragma warning restore 1591
