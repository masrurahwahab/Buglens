
using System.Text.Json.Serialization;

namespace Buglens.Models
{
       public class GoogleUserInfo
        {
            [JsonPropertyName("id")] 
            public string Id { get; set; }
        
            [JsonPropertyName("email")]
            public string Email { get; set; }
        
            [JsonPropertyName("name")]
            public string Name { get; set; }
        
            [JsonPropertyName("picture")]
            public string Picture { get; set; }
        
            [JsonPropertyName("verified_email")]
            public bool VerifiedEmail { get; set; }
        
            [JsonPropertyName("given_name")]
            public string GivenName { get; set; }
        
            [JsonPropertyName("family_name")]
            public string FamilyName { get; set; }
        }
 
}