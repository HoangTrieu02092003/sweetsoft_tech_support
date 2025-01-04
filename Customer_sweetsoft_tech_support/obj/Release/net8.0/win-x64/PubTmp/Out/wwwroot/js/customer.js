const searchIcon = document.querySelector('.search-icon');
const searchForm = document.querySelector('.formsearch');
const search = document.querySelector('.search');
const searchInput = document.querySelector('.searchInput');


searchIcon.addEventListener('click', () => {
    searchIcon.style.display = 'none';
    search.classList.toggle('show');
    searchForm.style.display = 'block';
});

searchInput.addEventListener('blur', () => {
    if (searchInput.value.trim() === '') {
        searchForm.style.display = 'none';
        search.classList.remove('show');
        searchIcon.style.display = 'block';
    }
});


