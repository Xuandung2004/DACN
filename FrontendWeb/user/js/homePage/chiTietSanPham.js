document.addEventListener('DOMContentLoaded', function () {
    // Nếu người dùng login, lấy userId từ localStorage
    const raw = localStorage.getItem("currentUser");
    let user = null;
    try {
        user = raw ? JSON.parse(raw) : null;
    } catch (e) {
        user = null;
    }
    const userId = user && (user.id || user.Id || user.ID);

    if (!userId) {
        document.getElementById("formDanhGia").style.display = "none";
    }
    // Đánh giá sao
    const stars = document.querySelectorAll('.wrap-rating .item-rating');
    const ratingInput = document.querySelector('.wrap-rating input[name="rating"]');
    stars.forEach((star, idx) => {
        star.addEventListener('click', function () {
            ratingInput.value = idx + 1;
            stars.forEach((s, i) => {
                s.classList.toggle('zmdi-star', i <= idx);
                s.classList.toggle('zmdi-star-outline', i > idx);
            });
        });
    });

    // Gửi đánh giá
    const reviewForm = document.querySelector('.tab-pane#reviews form.w-full');
    if (reviewForm) {
        reviewForm.addEventListener('submit', function (e) {
            e.preventDefault();
            const rating = ratingInput.value;
            const review = reviewForm.querySelector('textarea[name="review"]').value.trim();


            if (!rating || !review) {
                alert('Vui lòng nhập đánh giá và chọn số sao!');
                return;
            }

            // Lấy productId từ URL
            const productId = getProductId();





            console.log(userId);
            // (demo: để mặc định 1 nếu chưa login)

            let nguoiDungId = Number(userId),
                sanPhamId = Number(productId),
                rate = Number(rating),
                noiDung = review;


            try {
                const res = fetch(`http://localhost:5150/api/DanhGia/AddDanhGia?nguoiDungId=${nguoiDungId}&sanPhamId=${sanPhamId}&rate=${rate}&noiDung=${noiDung}`, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                });

                alert("Cảm ơn bạn đã đánh giá! ❤️");

                // Reset form
                reviewForm.reset();
                stars.forEach((s) => {
                    s.classList.remove('zmdi-star');
                    s.classList.add('zmdi-star-outline');
                });
                ratingInput.value = '';
                loadReviews(productId);

            } catch (error) {
                console.error(error);
                alert("Không thể gửi đánh giá. Vui lòng thử lại!");
            }

            // Reset form
            reviewForm.reset();
            stars.forEach((s) => {
                s.classList.remove('zmdi-star');
                s.classList.add('zmdi-star-outline');
            });
            ratingInput.value = '';
        });
    }
});

// -------------------
// Load danh sách đánh giá
// -------------------

async function loadReviews(productId) {
    const list = document.getElementById("review-list");

    try {
        const res = await fetch(`http://localhost:5150/api/DanhGia/reviews/${productId}`);
        const data = await res.json();

        list.innerHTML = ""; // xoá danh sách cũ

        data.forEach(r => {
            const stars = renderStars(r.rate);
            const avatar = r.avatar ? r.avatar : "images/avatar-01.jpg";

            const html = `
                <div class="flex-w flex-t p-b-68">
                    <div class="wrap-pic-s size-109 bor0 of-hidden m-r-18 m-t-6">
                        <img src="${avatar}" alt="AVATAR">
                    </div>

                    <div class="size-207">
                        <div class="flex-w flex-sb-m p-b-17">
                            <span class="mtext-107 cl2 p-r-20">
                                ${r.hoTen}
                            </span>

                            <span class="fs-18 cl11">
                                ${stars}
                            </span>
                        </div>

                        <p class="stext-102 cl6">
                            ${r.noiDung}
                        </p>

                        <span class="stext-102 cl9">
                            ${formatDate(r.ngayDanhGia)}
                        </span>
                    </div>
                </div>
            `;

            list.insertAdjacentHTML("beforeend", html);
        });

    } catch (err) {
        console.error("Lỗi tải đánh giá:", err);
    }
}

// render số sao của các đánh giá
function renderStars(rate) {
    let html = "";
    for (let i = 1; i <= 5; i++) {
        html += i <= rate
            ? `<i class="zmdi zmdi-star"></i>`
            : `<i class="zmdi zmdi-star-outline"></i>`;
    }
    return html;
}
// format ngày hiển thị
function formatDate(dateStr) {
    const d = new Date(dateStr);
    return d.toLocaleDateString("vi-VN");
}

loadReviews(getProductId()); // truyền ID sản phẩm thật

// ---------------------------
// RENDER DỮ LIỆU LÊN UI
// ---------------------------

// lấy id sản phẩm từ url
function getProductId() {
    const params = new URLSearchParams(window.location.search);
    return params.get("id");
}


// load dữ liệu sản phẩm 
document.addEventListener("DOMContentLoaded", () => {
    loadProductDetail();
});

async function loadProductDetail() {
    const id = getProductId();
    if (!id) return;

    try {
        const res = await fetch(`http://localhost:5150/api/DanhSachSanPham/${id}`);
        if (!res.ok) throw new Error("Không tìm thấy sản phẩm");

        const data = await res.json();
        renderProduct(data);

    } catch (err) {
        console.error("Lỗi:", err);
    }
}

function renderProduct(p) {

    // 1. Tên sản phẩm
    document.querySelector(".js-name-detail").textContent = p.tenSp;

    // 2. Giá
    document.querySelector(".mtext-106").textContent = p.gia.toLocaleString() + " VND";

    // 3. Mô tả
    document.querySelector("#description p").textContent = p.moTa;
    document.querySelector(".stext-102").textContent = p.moTa;

    // 4. Render Kích thước vào dropdown Size
    const sizeSelect = document.querySelector('.size-204 select[name="time"]');
    sizeSelect.innerHTML = `<option>Chọn kích cỡ</option>`;
    p.kichThuoc.forEach(kt => {
        sizeSelect.innerHTML += `<option>${kt.size}</option>`;
    });

    const $modal = $('.sec-product-detail');
    $modal.data('product-id', p.id);
    // ---- Cập nhật ảnh slider ----
    const $gallery = $modal.find('.slick3.gallery-lb');

    // Nếu slider đã init → unslick
    if ($gallery.hasClass('slick-initialized')) {
        $gallery.slick('unslick');
    }

    $gallery.empty(); // xóa ảnh cũ

    // Thêm ảnh mới
    p.anh.forEach(imgSrc => {
        const slide = `
                    <div class="item-slick3" data-thumb="/FrontendWeb/${imgSrc}">
                        <div class="wrap-pic-w pos-relative">
                            <img src="/FrontendWeb/${imgSrc}" alt="IMG-PRODUCT">
                            <a class="flex-c-m size-108 how-pos1 bor0 fs-16 cl10 bg0 hov-btn3 trans-04"
                               href="/FrontendWeb/${imgSrc}">
                                <i class="fa fa-expand"></i>
                            </a>
                        </div>
                    </div>
                `;
        $gallery.append(slide);
    });

    // Khởi tạo lại Slick slider modal
    $gallery.slick({
        slidesToShow: 1,
        slidesToScroll: 1,
        fade: true,
        infinite: true,
        autoplay: false,
        autoplaySpeed: 6000,

        arrows: true,
        appendArrows: $modal.find('.wrap-slick3-arrows'),
        prevArrow: '<button class="arrow-slick3 prev-slick3"><i class="fa fa-angle-left" aria-hidden="true"></i></button>',
        nextArrow: '<button class="arrow-slick3 next-slick3"><i class="fa fa-angle-right" aria-hidden="true"></i></button>',

        dots: true,
        appendDots: $modal.find('.wrap-slick3-dots'),
        dotsClass: 'slick3-dots',

        customPaging: function (slick, index) {
            var portrait = $(slick.$slides[index]).data('thumb');
            return `<img src="${portrait}"><div class="slick3-dot-overlay"></div>`;
        },
    });




    // 6. Cập nhật danh mục
    const categoriesText = document.querySelector(".size-302 .stext-107:last-child");
    if (categoriesText) {
        categoriesText.textContent = `Danh mục: ${p.danhMuc}`;
    }
}



