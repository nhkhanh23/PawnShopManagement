using System.Globalization;
using System.Resources;
namespace PawnShopManagement.Localization;
public static class UiText {
    private static readonly ResourceManager Resources = new("PawnShopManagement.Resources.UiResources", typeof(UiText).Assembly);
    public static string T(string key, params object[] args) {
        var value=Resources.GetString(key,CultureInfo.CurrentUICulture)??key;
        return args.Length==0?value:string.Format(CultureInfo.CurrentCulture,value,args);
    }
}
