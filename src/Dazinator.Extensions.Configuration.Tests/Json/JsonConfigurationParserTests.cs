namespace Dazinator.Extensions.Configuration.Tests.Json;

using Dazinator.Extensions.Configuration.Json;
using Xunit.Categories;

[UsesVerify]
[UnitTest]
public class JsonConfigurationParserTests
{

    [Theory]
    [InlineData(TestJson)]
    public Task CanAddAsyncDelegateProvider(string json)
    {      
        var dictionary = JsonConfigurationParser.Parse(json);
        return Verify(dictionary);
    }
   

    public const string TestJson = @"{""nervous"":[{""highway"":""pattern"",""necessary"":-763993287,""roar"":""bread"",""oil"":false,""brave"":-1044951554,""city"":1824557784.2769182},false,1052146982,true,false,1089327723],""note"":137599294,""quarter"":true,""clothes"":767666682.3025572,""forward"":""such"",""brief"":false}";

}
