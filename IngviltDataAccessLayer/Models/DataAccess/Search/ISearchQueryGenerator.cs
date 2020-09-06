namespace Ingvilt.Models.DataAccess.Search {
    public interface ISearchQueryGenerator {
        string GetSearchQuery();

        string GetValueAsString();

        string GetName();

        string GetDescription();
    }
}
