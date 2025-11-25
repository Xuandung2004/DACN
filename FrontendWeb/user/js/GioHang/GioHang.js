$(document).ready(function () {
    const nguoiDungId = 11; // ID người dùng đang đăng nhập
    const apiUrl = `http://localhost:5150/api/giohang/chitiet/${nguoiDungId}`;
    const updateUrl = `http://localhost:5150/api/giohang/update`;
<<<<<<< Updated upstream
	const delAllUrl = `http://localhost:5150/api/GioHang/DelAllItem/${nguoiDungId}`;
=======
    const delAllUrl = `http://localhost:5150/api/GioHang/DelAllItem/${nguoiDungId}`;
>>>>>>> Stashed changes

    function loadNavCart() {
        $.ajax({
            url: apiUrl,
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
            url: apiUrl,
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
            url: updateUrl,
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
<<<<<<< Updated upstream
		const delItemUrl = `http://localhost:5150/api/GioHang/delItem/${nguoiDungId}/${sanPhamId}/${kichThuocId}`;
		console.log("Xoá");
		if(confirm("Bạn có muốn xoá sản phẩm này khỏi giỏ hàng?")){
			$.ajax({
				url: delItemUrl,
				method: "DELETE",
				success: function(){
					loadCart();
					console.log("Xoá thành công!");
				},
				error: function(err){
					console.log("Lỗi");
				}
			});
		}
	});
    $(document).on("click", ".js-show-cart", function(){
=======
        const delItemUrl = `http://localhost:5150/api/GioHang/delItem/${nguoiDungId}/${sanPhamId}/${kichThuocId}`;
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
>>>>>>> Stashed changes
        loadNavCart();
    });
});