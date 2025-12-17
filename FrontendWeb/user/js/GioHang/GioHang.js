$(document).ready(function () {
    const raw = localStorage.getItem("currentUser");
    let user = null;
    try {
        user = raw ? JSON.parse(raw) : null;
    } catch (e) {
        user = null;
    }
    const userId = (user && (user.id || user.Id || user.ID)) || null;
    const nguoiDungId = userId; // ID người dùng đang đăng nhập
    const apiGioHang = `http://localhost:5150/api/giohang/chitiet/${nguoiDungId}`;
    const updateapiGioHang = `http://localhost:5150/api/giohang/update`;
    const delAllUrl = `http://localhost:5150/api/GioHang/DelAllItem/${nguoiDungId}`;
    const apiDatHang = "http://localhost:5150/api/giohang/DatHang";

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
                    `);
                    $(".header-cart-total.p-tb-40").text('Total: 0₫');
                    return;
                }

                res.items.forEach(item => {
                    const row = `
                        <li class="header-cart-item flex-w flex-t m-b-12">
						<div>
							<img style="width: 60px" src="../${item.anh}" alt="IMG">
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
                $(".header-cart-total.p-tb-40").text('Total: ' + res.tongTien.toLocaleString() + " ₫");
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
                    $(".icon-header-noti.js-show-cart").attr("data-notify", 0);
                    $(".table-shopping-cart").append(`
                        <tr class="table_row">
                            <td colspan="5" class="text-center p-t-40">
                                Giỏ hàng trống.
                            </td>
                        </tr>
                    `);
                    $(".subtotal-value").text("0 ₫");
                    return;
                }
                $(".icon-header-noti.js-show-cart").attr("data-notify", res.items.length);
                //item.anh
                res.items.forEach(item => {
                    const row = `
                        <tr class="table_row" 
                            data-sanpham="${item.sanPhamID}" 
                            data-kichthuoc="${item.kichThuocID}">
                            <td class="column-1">
                                <div class="DelItem how-itemcart1">
                                    <img src="../${item.anh}" alt="${item.tenSp}">
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
                            <td class="column-5 size">${item.kichThuoc}</td>
                            <td class="column-6 thanh-tien">${item.thanhTien.toLocaleString()} ₫</td>
                        </tr>`;
                    $(".table-shopping-cart").append(row);
                });

                $(".subtotal-value").text(res.tongTien.toLocaleString() + " ₫");
            },
            error: function (xhr) {
                console.error("❌ Lỗi tải giỏ hàng:", xhr);
            }
        });
    }

    // Gọi lần đầu
    loadCart();

    function formatCurrency(num) {
        return num.toLocaleString("vi-VN") + "₫";
    }

    function updateTotal() {
        let sum = 0;

        $(".table_row").each(function () {
            const totalText = $(this).find(".column-6").text().replace("₫", "").replace(/\./g, "").trim();
            const total = parseFloat(totalText);
            if (!isNaN(total)) sum += total;
        });

        $(".subtotal-value").text(formatCurrency(sum));
    }

    // ====== Sự kiện tăng giảm ======
    $(document).on("click", ".btn-num-product-up, .btn-num-product-down", function () {
        const row = $(this).closest(".table_row");
        const input = row.find(".num-product");
        const current = parseInt(input.val());
        const isUp = $(this).hasClass("btn-num-product-up");

        const newQty = isUp ? current + 1 : Math.max(1, current - 1);
        input.val(newQty);

        const priceText = row.find(".column-3").text().replace("₫", "").replace(/\./g, "").trim();
        const price = parseFloat(priceText);
        console.log(price);

        // ====== Cập nhật total dòng ngay lập tức ======
        const newTotal = (price * newQty);
        row.find(".column-6").text(formatCurrency(newTotal));

        // ====== Cập nhật subtotal ======
        updateTotal();

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
                //loadCart(); // reload lại giỏ
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
        const delItemUrl = `http://localhost:5150/api/GioHang/delItem/${nguoiDungId}/${sanPhamId}/${kichThuocId}`;
        console.log("Xoá");
        if (confirm("Bạn có muốn xoá sản phẩm này khỏi giỏ hàng?")) {
            $.ajax({
                url: delItemUrl,
                method: "DELETE",
                success: function () {
                    loadCart();
                    loadNavCart();
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
        const productId = $('.js-modal1').data('product-id') || $('.sec-product-detail').data('product-id');
        const kichThuocId = $('.js-modal1 select[name="time"]').val() || $('.sec-product-detail select[name="time"]').val(); // dropdown size
        const soLuong = parseInt($('.js-modal1 input[name="num-product"]').val() || $('.sec-product-detail input[name="num-product"]').val() || 1);
        console.log(productId, kichThuocId, soLuong);

        if (!userId) {
            alert("Bạn cần đăng nhập để thêm sản phẩm vào giỏ hàng!");
            return;
        }

        if (!kichThuocId || isNaN(kichThuocId)) {
            alert("Vui lòng chọn kích cỡ trước khi thêm vào giỏ hàng!");
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
            url: "http://localhost:5150/api/GioHang/AddToCart",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(data),
            success: function (res) {
                swal("Sản phẩm", "đã được thêm vào giỏ hàng thành công !", "success");
                loadCart();
            },
            error: function (xhr) {
                console.error(xhr);
                swal("Lỗi", "thêm vào giỏ hàng thất bại !", "error");
            }
        });
    });

    //Đặt hàng 
    document.getElementById("btnOpenOrder").onclick = function () {

        //Tính tông tiền
        $.get("http://localhost:5150/api/giohang/chitiet/" + nguoiDungId, function (res) {
            if (!res.items || res.items.length === 0) {
                alert("Vui lòng thêm sản phẩm vào giỏ hàng!!");
                return;
            }

            $("#tongTien").val(res.tongTien.toLocaleString() + " ₫");

            document.getElementById("orderModal").style.display = "flex";
        });
        $("#tenNguoiNhan").val('').prop("readonly", true);
        $("#soDienThoai").val('').prop("readonly", true);
        $("#diaChiNhan").val('').prop("readonly", true);
        //Load địa chỉ
        loadDiaChi();
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

    //function load địa chỉ
    function loadDiaChi() {
        $.get("http://localhost:5150/api/DiaChi/NguoiDung/" + nguoiDungId, function (res) {
            $("#diaChiDaLuu").empty().append(`
                <option value="">-- Chọn địa chỉ --</option>`);
            res.forEach(dc => {
                $("#diaChiDaLuu").append(`
                <option 
                    value="${dc.id}"
                    data-ten="${dc.tenNguoiNhan}"
                    data-sdt="${dc.sdt}"
                    data-diachi="${dc.diaChiCuThe}"
                >
                    ${dc.tenNguoiNhan} - ${dc.sdt}
                </option>
            `);
            });
        });

    }
    //Khi chọn địa chỉ tự động fill vào
    $(document).on("change", "#diaChiDaLuu", function () {
        let selected = $(this).find("option:selected");

        if (selected.val() === "") {
            // Không chọn gì → cho phép nhập tay
            $("#tenNguoiNhan, #soDienThoai, #diaChiNhan")
                .prop("readonly", false)
                .val("");
            return;
        }

        // Fill data
        $("#tenNguoiNhan").val(selected.data("ten")).prop("readonly", true);
        $("#soDienThoai").val(selected.data("sdt")).prop("readonly", true);
        $("#diaChiNhan").val(selected.data("diachi")).prop("readonly", true);
    });

    // Xác nhận đặt hàng
    $("#btnDatHang").click(function () {

        if ($("#tenNguoiNhan").val().trim() === "" ||
            $("#soDienThoai").val().trim() === "" ||
            $("#diaChiNhan").val().trim() === "") {

            alert("Vui lòng nhập đầy đủ thông tin!");
            return;
        }

        var diaChiNhanId = $("#diaChiDaLuu").val();
        if (!diaChiNhanId) {
            alert("Vui lòng chọn địa chỉ nhận hàng!");
            return;
        }

        const data = {
            nguoiDungId: nguoiDungId,
            diaChiId: diaChiNhanId,//Địa chỉ nhận
            ghiChu: $("#ghiChu").val(),
            phuongThucThanhToan: $("#phuongThuc").val()
        };

        $.ajax({
            url: "http://localhost:5150/api/DatHang/DatHang",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify(data),
            success: function (res) {
                // COD — hoàn tất đặt hàng và hiển thị thông báo
                console.log("✅ COD payment, order completed");

                document.getElementById("orderModal").style.display = "none";
                loadCart();

                if ($("#phuongThuc").val().trim() === "COD") {
                    alert("Đặt hàng thành công! Mã đơn: " + res.donHangId);
                    return;
                } else {
                    // Nếu khách chọn Banking(VNPay) — tạo URL thanh toán và redirect
                    const phuongThuc = data.phuongThucThanhToan;
                    console.log("✅ Đặt hàng thành công, phương thức:", phuongThuc, "Mã đơn:", res.donHangId);


                    // chuẩn bị payload cho tạo URL VNPay; dùng donHangId làm txn ref
                    const paymentData = {
                        orderType: String(res.donHangId),
                        amount: res.tongTien,
                        orderDescription: `Thanh toán đơn hàng #${res.donHangId}`,
                        name: $("#tenNguoiNhan").val() || ""
                    };

                    console.log("💳 Gửi request VNPay payment:", paymentData);


                    $.ajax({
                        url: "http://localhost:5150/api/ThanhToan/create-payment-url",
                        method: "POST",
                        contentType: "application/json",
                        data: JSON.stringify(paymentData),
                        success: function (payRes) {
                            console.log("✅ Nhận URL VNPay:", payRes);
                            if (payRes && payRes.paymentUrl) {
                                // đóng modal và chuyển hướng tới VNPay (không ghi alert)
                                document.getElementById("orderModal").style.display = "none";
                                console.log("🔄 Redirecting to VNPay...");
                                window.location.href = payRes.paymentUrl;
                            } else {
                                alert("Không tạo được URL thanh toán VNPay. Vui lòng thử lại.");
                                document.getElementById("orderModal").style.display = "none";
                                loadCart();
                            }
                        },
                        error: function (err) {
                            console.error("❌ Lỗi tạo URL VNPay:", err);
                            alert("Lỗi khi tạo URL VNPay. Vui lòng thử lại sau.");
                            document.getElementById("orderModal").style.display = "none";
                            loadCart();
                        }
                    });

                };

            },
            error: function (err) {
                alert("Lỗi đặt hàng!");
                console.log(err);
            }
        });



    });
});