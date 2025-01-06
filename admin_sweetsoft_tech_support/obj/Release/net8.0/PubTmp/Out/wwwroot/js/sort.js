document.addEventListener("DOMContentLoaded", () => {
        const sortLinks = document.querySelectorAll("th a");
        sortLinks.forEach(link => {
            const sortColumn = link.getAttribute("href").match(/sortColumn=(\w+)/)?.[1];
    const currentSortColumn = "@ViewData["SortColumn"]";
    const currentSortOrder = "@ViewData["SortOrder"]";

    if (sortColumn === currentSortColumn) {
        link.classList.add("sort-arrow", currentSortOrder);
            }
        });
});