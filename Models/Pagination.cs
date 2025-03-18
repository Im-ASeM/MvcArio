public class Pagination<T>
{
    public List<T> Items { get; set; } // لیست آیتم‌ها برای نمایش
    public int TotalCount { get; set; } // تعداد کل آیتم‌ها
    public int PageSize { get; set; } // تعداد آیتم‌ها در هر صفحه
    public int CurrentPage { get; set; } // شماره صفحه فعلی
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize); // تعداد کل صفحات

    public Pagination(List<T> items, int totalCount, int pageSize, int currentPage)
    {
        Items = items;
        TotalCount = totalCount;
        PageSize = pageSize;
        CurrentPage = currentPage;
    }
}