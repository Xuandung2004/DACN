
document.addEventListener("DOMContentLoaded", () => {
    let currentParams = {
        search: "",
        minPrice: "",
        maxPrice: "",
        page: 1,
        pageSize: 8
    };

    async function loadProducts(params = {}) {
        currentParams = { ...currentParams, ...params };
        const query = new URLSearchParams(currentParams).toString();
        const grid = document.querySelector(".isotope-grid");
        grid.innerHTML = '<p class="text-center">Đang tải...</p>';

        try {
            const res = await fetch(`http://localhost:5150/api/DanhSachSanPham?${query}`);
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            const data = await res.json();
            renderProducts(data.items || []);
            renderPagination(data.totalItems, data.page, data.pageSize);
            history.replaceState(currentParams, "", `?${query}`);
        } catch (err) {
            console.error("Lỗi khi tải sản phẩm:", err);
            grid.innerHTML = '<p class="text-danger">Không thể tải sản phẩm.</p>';
        }
    }


    function renderProducts(products) {
        const grid = document.querySelector(".isotope-grid");
        grid.innerHTML = "";
        if (!products || products.length === 0) {
            grid.innerHTML = '<p class="text-muted">Không tìm thấy sản phẩm phù hợp.</p>';
            return;
        }
        products.forEach(sp => {
            const imgSrc = sp.anh[0];
            const price = (parseFloat(sp.gia) || 0).toLocaleString('vi-VN');
            const html = `
                <div class="col-sm-6 col-md-4 col-lg-3 p-b-35 isotope-item">
                    <div class="block2">
                        <div  class="block2-pic hov-img0">
                            <img class="img-product" src="/FrontendWeb/${imgSrc}" alt="${sp.tenSp}" >
                            <a href="#" class="block2-btn flex-c-m stext-103 cl2 size-102 bg0 bor2 hov-btn1 p-lr-15 trans-04 js-show-modal1" data-id="${sp.id}">
                                Xem nhanh
                            </a>
                        </div>
                        <div class="block2-txt flex-w flex-t p-t-14">
                            <div class="block2-txt-child1 flex-col-l">
                                <a href="product-detail.html?id=${sp.id}" class="stext-104 cl4 hov-cl1 trans-04 js-name-b2 p-b-6">
                                    ${sp.tenSp}
                                </a>
                                <span class="stext-105 cl3">${price} ₫</span>
                            </div>
                        </div>
                    </div>
                </div>
            `;
            grid.insertAdjacentHTML("beforeend", html);
        });
        if (grid) {
            grid.style.removeProperty('height');
        }
    }

    function renderPagination(totalItems, currentPage, pageSize) {
        const container = document.getElementById("pagination");
        if (!container) return;

        const totalPages = Math.ceil(totalItems / pageSize);

        // XÓA CHỈ NỘI DUNG CON, GIỮ LẠI CONTAINER
        container.innerHTML = '';

        if (totalPages <= 1) return;

        // Tạo wrapper (chỉ 1 lần)
        const wrapper = container;

        // === Prev ===
        if (currentPage > 1) {
            wrapper.appendChild(createPageLink("«", currentPage - 1));
        }

        // === Các trang số ===
        for (let i = 1; i <= totalPages; i++) {
            const link = createPageLink(i, i);
            if (i === currentPage) {
                link.classList.add("active-pagination1");
            }
            wrapper.appendChild(link);
        }

        // === Next ===
        if (currentPage < totalPages) {
            wrapper.appendChild(createPageLink("»", currentPage + 1));
        }



        // Hàm tạo <a>
        function createPageLink(text, page) {
            const a = document.createElement("a");
            a.href = "#";
            a.className = "flex-c-m how-pagination1 trans-04 m-all-7";
            a.textContent = text;
            a.addEventListener("click", (e) => {
                e.preventDefault();
                loadProducts({ page });
                document.querySelector(".isotope-grid")?.scrollIntoView({ behavior: "smooth" });
            });
            return a;
        }
    }

    // Tìm kiếm
    const searchBtn = document.getElementById("searchBtn");
    const searchInput = document.getElementById("searchInput");
    searchBtn?.addEventListener("click", () => {
        const keyword = searchInput.value.trim();
        loadProducts({ search: keyword, minPrice: "", maxPrice: "", page: 1 });
    });
    searchInput?.addEventListener("keypress", (e) => {
        if (e.key === "Enter") searchBtn?.click();
    });

    // Bộ lọc giá
    document.getElementById("filterBtn")?.addEventListener("click", () => {
        const min = document.getElementById("minPrice").value || "";
        const max = document.getElementById("maxPrice").value || "";
        // Reset ô tìm kiếm nếu có
        if (searchInput) searchInput.value = "";
        loadProducts({ minPrice: min, maxPrice: max, search: "", page: 1 });
    });

    // Load ban đầu
    const urlParams = new URLSearchParams(window.location.search);
    if (urlParams.toString()) {
        const params = Object.fromEntries(urlParams.entries());
        Object.keys(params).forEach(key => {
            if (["page", "pageSize"].includes(key)) params[key] = parseInt(params[key]);
        });
        loadProducts(params);
    } else {
        loadProducts();
    }

    // Xử lý back/forward
    window.addEventListener("popstate", (e) => {
        if (e.state) loadProducts(e.state);
    });


    // LẤy danh sách các danh mục và load vào filter
    // URL API trả về danh mục
    const apiUrl = 'http://localhost:5150/api/DanhSachSanPham/DanhMuc';

    // Lấy thẻ div container
    const dsDanhMuc = document.getElementById('category-container');
    dsDanhMuc.innerHTML = '';
    // Gọi API và render danh mục
    fetch(apiUrl)
        .then(res => res.json())
        .then(categories => {
            categories.forEach(cat => {
                const a = document.createElement('a');
                a.href = '#';
                a.className = 'flex-c-m stext-107 cl6 size-301 bor7 p-lr-15 hov-tag1 trans-04 m-r-5 m-b-5';
                a.textContent = cat.tenDm; // tên danh mục từ API
                dsDanhMuc.appendChild(a);
            });
        })
        .catch(err => console.error('Lỗi khi load danh mục:', err));
});