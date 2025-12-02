$(document).ready(function () {

    const userInfo = JSON.parse(localStorage.getItem("currentUser"));
    const userId = userInfo.id;
    
    const apiUrl = "http://localhost:5150/api/DatHang/LichSu/" + userId;

    loadLichSu();

    function loadLichSu() {
        $.ajax({
            url: apiUrl,
            method: "GET",
            dataType: "json",
            success: function (res) {

                $("#orderList").empty();
                console.log(res);

                if (res.length === 0) {
                    $("#orderList").html(`
                        <p class="stext-111 cl2">Bạn chưa có đơn hàng nào.</p>
                    `);
                    return;
                }

                res.forEach(order => {

                    let productHtml = "";

                    order.sanPhams.forEach(p => {
                        productHtml += `
                            <div class="order-product" style="
                                display:flex;
                                align-items:center;
                                padding:10px 0;
                                border-bottom:1px solid #eee;
                            ">
                                <img src="../image/${p.anh}" alt="" style="
                                    width:70px;
                                    height:70px;
                                    object-fit:cover;
                                    border-radius:6px;
                                    margin-right:15px;
                                ">

                                <div style="flex:1;">
                                    <div class="stext-102"><b>${p.tenSp}</b> (Size ${p.kichThuoc})</div>
                                    <div class="stext-111">SL: ${p.soLuong}</div>
                                    <div class="stext-111">${p.gia.toLocaleString()} ₫</div>
                                </div>

                                <div class="stext-111" style="font-weight:bold;">
                                    ${p.thanhTien.toLocaleString()} ₫
                                </div>
                            </div>
                        `;
                    });

                    const row = `
                        <div class="order-item" style="
                            border:1px solid #ddd; 
                            padding:15px; 
                            margin-bottom:15px; 
                            border-radius:8px;
                        ">

                            <!-- Header clickable -->
                            <div class="order-header" style="
                                cursor:pointer; 
                                display:flex; 
                                justify-content:space-between; 
                                align-items:center;
                            ">
                                <b>Đơn hàng #${order.donHangID}</b>
                                <span class="arrow">▼</span>
                            </div>

                            <!-- Chi tiết đơn hàng (ẩn) -->
                            <div class="order-detail" style="display:none; margin-top:15px;">

                                <div class="stext-111">
                                    Ngày đặt: ${new Date(order.ngayDat).toLocaleString()}
                                </div>
                                <div class="stext-111">
                                    Trạng thái: <b>${order.trangThai}</b>
                                </div>

                                <div class="order-products p-t-20">
                                    ${productHtml}
                                </div>

                                <div class="stext-111 p-t-15" style="margin-top:10px;">
                                    <b>Tổng tiền: ${order.tongTien.toLocaleString()} ₫</b>
                                </div>
                            </div>
                        </div>
                    `;

                    $("#orderList").append(row);
                });

                // Toggle chi tiết
                $(".order-header").off("click").on("click", function () {
                    const detail = $(this).next(".order-detail");
                    const arrow = $(this).find(".arrow");

                    detail.slideToggle(200);

                    arrow.text(arrow.text() === "▼" ? "▲" : "▼");
                });

            },

            error: function () {
                $("#orderList").html(`
                    <p class="stext-111 cl2">Không thể tải lịch sử đơn hàng.</p>
                `);
            }
        });
    }
});
