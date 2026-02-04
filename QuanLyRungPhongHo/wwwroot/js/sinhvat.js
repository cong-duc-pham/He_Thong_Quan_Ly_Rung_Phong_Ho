// Ajax Search
let searchTimeout;
const searchInput = document.getElementById("searchInput");
const tableBody = document.querySelector("#sinhVatTable tbody");

// Hàm thực hiện tìm kiếm
function performSearch() {
    const keyword = searchInput.value.trim();
    
    // Hiển thị loading
    tableBody.innerHTML = '<tr><td colspan="5" class="text-center"><i class="fas fa-spinner fa-spin"></i> Đang tìm kiếm...</td></tr>';

    // Gọi Ajax
    fetch(`/SinhVat/Search?keyword=${encodeURIComponent(keyword)}`, {
        method: 'GET',
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
    .then(response => {
        if (!response.ok) throw new Error('Network response was not ok');
        return response.text();
    })
    .then(html => {
        tableBody.innerHTML = html;
    })
    .catch(error => {
        console.error('Error:', error);
        tableBody.innerHTML = '<tr><td colspan="5" class="text-center text-danger"><i class="fas fa-exclamation-triangle"></i> Có lỗi xảy ra khi tìm kiếm</td></tr>';
    });
}

if (searchInput && tableBody) {
    // Tìm kiếm khi gõ (debounce)
    searchInput.addEventListener("keyup", function (e) {
        // Nếu nhấn Enter, tìm ngay lập tức
        if (e.key === "Enter") {
            clearTimeout(searchTimeout);
            performSearch();
            return;
        }

        // Nếu không phải Enter, debounce
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
            performSearch();
        }, 300); // Debounce 300ms
    });

    // Tìm kiếm khi nhấn Enter
    searchInput.addEventListener("keypress", function (e) {
        if (e.key === "Enter") {
            e.preventDefault(); // Ngăn form submit nếu có
            clearTimeout(searchTimeout);
            performSearch();
        }
    });
}

