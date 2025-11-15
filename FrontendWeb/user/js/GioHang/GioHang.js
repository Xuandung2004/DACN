$(document).ready(function () {
    const nguoiDungId = 4; // ID người dùng đang đăng nhập
    const apiGioHang = `https://localhost:7109/api/giohang/chitiet/${nguoiDungId}`;
    const updateapiGioHang = `https://localhost:7109/api/giohang/update`;
    const delAllUrl = `https://localhost:7109/api/GioHang/DelAllItem/${nguoiDungId}`;
    const apiDatHang = "https://localhost:7109/api/giohang/DatHang";

    function loadNavCart() {
        $.ajax({
            url: apiGioHang,
            method: "GET",
            dataType: "json",
            success: function (res) {
                console.log("Dữ liệu giỏ hàng", res);
                $(".header-cart-wrapitem .header-cart-item").remove();

                if (!res.items || res.items.length === 0) {
                    $(".header-cart-wrapitem").append(`
                        <li class="header-cart-item flex-w flex-t m-b-12">
                        <div class="header-cart-item-txt p-t-8">
                            <span class="header-cart-item-info">
                                Giỏ hàng trống!
                            </span>
                        </div>
                        </li>
                    `)
                    return;
                }

                res.items.forEach(item => {
                    const row = `
                        <li class="header-cart-item flex-w flex-t m-b-12">
						<div class="header-cart-item-img">
							<img src="${item.anh}" alt="IMG">
						</div>

						<div class="header-cart-item-txt p-t-8">
							<a href="#" class="header-cart-item-name m-b-18 hov-cl1 trans-04">
								${item.tenSp}
							</a>

							<span class="header-cart-item-info">
								${item.soLuong} * ${item.gia.toLocaleString()}
							</span>
						</div>
					</li>
                    `;
                    $(".header-cart-wrapitem").append(row);

                });
            },
            error: function (xhr) {
                console.error("❌ Lỗi tải giỏ hàng:", xhr);
            }
        });

    }
    loadNavCart();


    // ====== Hàm load giỏ hàng ======
    function loadCart() {
        $.ajax({
            url: apiGioHang,
            method: "GET",
            dataType: "json",
            success: function (res) {
                console.log("✅ Dữ liệu giỏ hàng:", res);
                $(".table-shopping-cart .table_row").remove();

                if (!res.items || res.items.length === 0) {
                    $(".table-shopping-cart").append(`
                        <tr class="table_row">
                            <td colspan="5" class="text-center p-t-40">
                                Giỏ hàng trống.
                            </td>
                        </tr>
                    `);
                    $(".mtext-110.cl2").text("0 ₫");
                    return;
                }
                //item.anh
                res.items.forEach(item => {
                    const row = `
                        <tr class="table_row" 
                            data-sanpham="${item.sanPhamID}" 
                            data-kichthuoc="${item.kichThuocID}">
                            <td class="column-1">
                                <div class="DelItem how-itemcart1">
                                    <img src="${item.anh}" alt="${item.tenSp}">
                                </div>
                            </td>
                            <td class="column-2">${item.tenSp}</td>
                            <td class="column-3">${item.gia.toLocaleString()} ₫</td>
                            <td class="column-4">
                                <div class="wrap-num-product flex-w m-l-auto m-r-0">
                                    <div class="btn-num-product-down cl8 hov-btn3 trans-04 flex-c-m">
                                        <i class="fs-16 zmdi zmdi-minus"></i>
                                    </div>

                                    <input class="mtext-104 cl3 txt-center num-product" 
                                           type="number" 
                                           value="${item.soLuong}" 
                                           min="1">

                                    <div class="btn-num-product-up cl8 hov-btn3 trans-04 flex-c-m">
                                        <i class="fs-16 zmdi zmdi-plus"></i>
                                    </div>
                                </div>
                            </td>
                            <td class="column-5 thanh-tien">${item.thanhTien.toLocaleString()} ₫</td>
                        </tr>`;
                    $(".table-shopping-cart").append(row);
                });

                $(".mtext-110.cl2").text(res.tongTien.toLocaleString() + " ₫");
            },
            error: function (xhr) {
                console.error("❌ Lỗi tải giỏ hàng:", xhr);
            }
        });
    }

    // Gọi lần đầu
    loadCart();

    // ====== Sự kiện tăng giảm ======
    $(document).on("click", ".btn-num-product-up, .btn-num-product-down", function () {
        const row = $(this).closest(".table_row");
        const input = row.find(".num-product");
        const current = parseInt(input.val());
        const isUp = $(this).hasClass("btn-num-product-up");

        const newQty = isUp ? current + 1 : Math.max(1, current - 1);
        input.val(newQty);

        const sanPhamId = row.data("sanpham");
        const kichThuocId = row.data("kichthuoc");

        // ====== Gửi request cập nhật ======
        $.ajax({
            url: updateapiGioHang,
            method: "PUT",
            contentType: "application/json",
            data: JSON.stringify({
                sanPhamId: sanPhamId,
                soLuong: newQty,
                nguoiDungId: nguoiDungId,
                kichThuocId: kichThuocId
            }),
            success: function () {
                console.log("✅ Cập nhật thành công sản phẩm", sanPhamId);
                loadCart(); // reload lại giỏ
            },
            error: function (xhr) {
                console.error("❌ Lỗi cập nhật:", xhr);
            }
        });
    });
    $(document).on("click", ".ClearCart", function () {
        if (confirm("Xoá toàn bộ giỏ hàng?")) {
            $.ajax({
                url: delAllUrl,
                method: "DELETE",
                success: function (res) {
                    loadCart();
                },
                error: function (err) {
                    console.log("Lỗi xoá");
                }
            });
        }
    });
    $(document).on("click", ".DelItem", function () {
        const row = $(this).closest("tr");
        const sanPhamId = row.data("sanpham");
        const kichThuocId = row.data("kichthuoc");
        const delItemUrl = `https://localhost:7109/api/GioHang/delItem/${nguoiDungId}/${sanPhamId}/${kichThuocId}`;
        console.log("Xoá");
        if (confirm("Bạn có muốn xoá sản phẩm này khỏi giỏ hàng?")) {
            $.ajax({
                url: delItemUrl,
                method: "DELETE",
                success: function () {
                    loadCart();
                    console.log("Xoá thành công!");
                },
                error: function (err) {
                    console.log("Lỗi");
                }
            });
        }
    });
    $(document).on("click", ".js-show-cart", function () {
        loadNavCart();
    });
    //add to cart
    $(document).on('click', '.js-addcart-detail', function () {
        const userId = nguoiDungId; // hoặc localStorage.getItem("userId")
        const productId = $('.js-modal1').data('product-id');
        const kichThuocId = $('.js-modal1 select[name="time"]').val(); // dropdown size
        const soLuong = parseInt($('.js-modal1 input[name="num-product"]').val() || 1);

        if (!userId) {
            alert("Bạn cần đăng nhập để thêm sản phẩm vào giỏ hàng!");
            return;
        }

        if (!kichThuocId) {
            alert("Vui lòng chọn size trước khi thêm vào giỏ hàng!");
            return;
        }

        const data = {
            NguoiDungId: parseInt(userId),
            SanPhamId: parseInt(productId),
            KichThuocId: parseInt(kichThuocId),
            SoLuong: soLuong
        };

        console.log("Add to cart:", data);

        $.ajax({
            url: "https://localhost:7109/api/GioHang/AddToCart",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(data),
            success: function (res) {
                swal("Sản phẩm", "đã được thêm vào giỏ hàng thành công !", "success");
            },
            error: function (xhr) {
                console.error(xhr);
                swal("Lỗi", "thêm vào giỏ hàng thất bại !", "error");
            }
        });
    });

    //Đặt hàng 
    document.getElementById("btnOpenOrder").onclick = function () {

        $.get("https://localhost:7109/api/giohang/chitiet/" + nguoiDungId, function (res) {

            $("#tongTien").val(res.tongTien.toLocaleString() + " ₫");

            document.getElementById("orderModal").style.display = "flex";
        });
    };

    // Đóng modal
    document.querySelector(".modal-close").onclick = function () {
        document.getElementById("orderModal").style.display = "none";
    };

    // Click ra ngoài để đóng
    window.onclick = function (e) {
        if (e.target.id === "orderModal") {
            document.getElementById("orderModal").style.display = "none";
        }
    };

    // Xác nhận đặt hàng
    $("#btnDatHang").click(function () {

        if ($("#tenNguoiNhan").val().trim() === "" ||
            $("#soDienThoai").val().trim() === "" ||
            $("#diaChiNhan").val().trim() === "") {

            alert("Vui lòng nhập đầy đủ thông tin!");
            return;
        }

        const data = {
            nguoiDungId: nguoiDungId,
            diaChiId: 3,//Địa chỉ nhận
            ghiChu: $("#ghiChu").val(),
            phuongThucThanhToan: $("#phuongThuc").val()
        };

        $.ajax({
            url: "https://localhost:7109/api/DatHang/DatHang",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify(data),
            success: function (res) {
                alert("Đặt hàng thành công! Mã đơn: " + res.donHangId);
                document.getElementById("orderModal").style.display = "none";
                loadCart();
            },
            error: function (err) {
                alert("Lỗi đặt hàng!");
                console.log(err);
            }
        });
    });
});