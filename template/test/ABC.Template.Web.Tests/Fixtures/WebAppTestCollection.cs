namespace ABC.Template.Web.Tests.Fixtures;

[CollectionDefinition(Name)]
//#if (UseAspire)
public class WebAppTestCollection : TestCollection<AspireHostAppFixture>
//#else
public class WebAppTestCollection : TestCollection<WebAppFixture>
//#endif
{
    public const string Name = nameof(WebAppTestCollection);
}
