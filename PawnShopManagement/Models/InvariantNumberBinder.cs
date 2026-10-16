using static PawnShopManagement.Localization.UiText;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
namespace PawnShopManagement.Models;
// HTML number controls submit an invariant decimal point even under vi-VN.
public sealed class InvariantNumberBinder : IModelBinder {
    public Task BindModelAsync(ModelBindingContext context) {
        var v=context.ValueProvider.GetValue(context.ModelName);
        if(v==ValueProviderResult.None) return Task.CompletedTask;
        context.ModelState.SetModelValue(context.ModelName,v);
        object? number=null;
        if(context.ModelType==typeof(decimal)&&decimal.TryParse(v.FirstValue,NumberStyles.AllowLeadingSign|NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out var d)) number=d;
        else if(context.ModelType==typeof(float)&&float.TryParse(v.FirstValue,NumberStyles.AllowLeadingSign|NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out var f)&&float.IsFinite(f)) number=f;
        if(number==null) context.ModelState.TryAddModelError(context.ModelName,T("Vui lòng nhập số hợp lệ."));
        else context.Result=ModelBindingResult.Success(number);
        return Task.CompletedTask;
    }
}
public sealed class InvariantNumberBinderProvider : IModelBinderProvider {
    public IModelBinder? GetBinder(ModelBinderProviderContext context) => context.Metadata.ModelType==typeof(decimal)||context.Metadata.ModelType==typeof(float)?new InvariantNumberBinder():null;
}
