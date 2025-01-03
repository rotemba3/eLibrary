
const searchInput = document.getElementById('search-input');
const searchResults = document.getElementById('search-results');
const searchForm = document.getElementById('search-form');

// מנוע ההשלמה האוטומטית
searchInput.addEventListener('input', function () {
    const query = searchInput.value.trim();

    if (query.length === 0) {
        searchResults.innerHTML = ''; // מנקה את התוצאות אם השדה ריק
        return;
    }

    fetch(`/Books/SearchAutocomplete?query=${encodeURIComponent(query)}`)
        .then(response => response.json())
        .then(data => {
            searchResults.innerHTML = ''; // מנקה תוצאות קודמות
            if (data.length > 0) {
                data.forEach(book => {
                    const resultItem = document.createElement('div');
                    resultItem.className = 'result-item';
                    resultItem.innerHTML = `
                        <img src="${book.CoverImage}" alt="${book.Title}" />
                        <div>
                            <h4>${book.Title}</h4>
                            <p>By: ${book.Author}</p>
                        </div>
                    `;
                    searchResults.appendChild(resultItem);
                });
            } else {
                searchResults.innerHTML = '<p>No results found.</p>';
            }
        })
        .catch(error => console.error('Error fetching search results:', error));
});

