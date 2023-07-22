using System.Collections.Generic;

namespace ChainyiBot
{
    class QueryResponse
    {
        public CategoryInfo Query { get; set; }

        public IEnumerator<PageInfo> GetEnumerator()
        {
            return Query.CategoryMembers.GetEnumerator();
        }
    }

    class CategoryInfo
    {
        public List<PageInfo> CategoryMembers { get; set; }
    }

    class PageInfo
    {
        public int PageId { get; set; }

        public int Ns { get; set; }

        public string Title { get; set; }
    }
}
