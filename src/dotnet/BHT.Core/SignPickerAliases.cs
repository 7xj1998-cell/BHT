using System;
using System.Collections.Generic;
namespace BHT.Core {
 // Presentation aliases only. Original catalog/import codes remain supported.
 public static class SignPickerAliases {
  private static readonly Dictionary<string,string> aliases=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase){
   {"P.124A","P.124a1"},{"P.124A-1","P.124a1"},{"P.124A-2","P.124a2"},
   {"P.124B","P.124b1"},{"P.124B-1","P.124b1"},{"P.124B-2","P.124b2"}
  };
  public static string Canonical(string code){string value;return code!=null && aliases.TryGetValue(code.Trim(),out value)?value:code??"";}
  public static bool IsAlias(string code){return code!=null && aliases.ContainsKey(code);}
 }
}
