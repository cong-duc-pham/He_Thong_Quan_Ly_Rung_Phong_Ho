document.getElementById("searchInput")
    ?.addEventListener("keyup", function () {

        let keyword = this.value.toLowerCase();
        let rows = document.querySelectorAll("#sinhVatTable tbody tr");

        rows.forEach(row => {
            row.style.display =
                row.innerText.toLowerCase().includes(keyword)
                    ? ""
                    : "none";
        });
    });

